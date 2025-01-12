using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using Npgsql;
using MTCG.Models;
using MTCG.DataAccess;
using Newtonsoft.Json;

namespace MTCG.Services
{
    public static class TradeService
    {
        public static string CreateTrade(HttpListenerContext context)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Error: Authorization header is missing or invalid.";
            }

            var token = authHeader.Substring("Bearer ".Length);
            var username = token.Replace("-mtcgToken", "");

            try
            {
                string body;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                var trade = JsonConvert.DeserializeObject<Trade>(body);

                if (trade == null || string.IsNullOrWhiteSpace(trade.CardToTrade) || trade.MinimumDamage <= 0 || string.IsNullOrWhiteSpace(trade.WantedType))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return "Error: Invalid trade details.";
                }

                using (var conn = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new NpgsqlCommand("INSERT INTO trades (id, user_id, card_to_trade, wanted_type, minimum_damage, created_at) VALUES (@id, (SELECT id FROM users WHERE username = @username), @card, @type, @damage, NOW())", conn))
                    {
                        cmd.Parameters.AddWithValue("id", trade.Id);
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("card", Guid.Parse(trade.CardToTrade));
                        cmd.Parameters.AddWithValue("type", trade.WantedType);
                        cmd.Parameters.AddWithValue("damage", trade.MinimumDamage);
                        cmd.ExecuteNonQuery();
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.Created;
                return "Trade created successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating trade: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to create trade.";
            }
        }

        public static string AcceptTrade(HttpListenerContext context, Guid tradeId)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Error: Authorization header is missing or invalid.";
            }

            var token = authHeader.Substring("Bearer ".Length);
            var username = token.Replace("-mtcgToken", "");

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {

                    // Fetch the trade details
                    string cardToTrade = null;
                    string wantedType = null;
                    float minimumDamage = 0;
                    int ownerId = 0;

                    using (var cmd = new NpgsqlCommand("SELECT * FROM trades WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", tradeId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                return "Error: Trade not found.";
                            }

                            cardToTrade = reader["card_to_trade"].ToString();
                            wantedType = reader["wanted_type"].ToString();
                            minimumDamage = Convert.ToSingle(reader["minimum_damage"]);
                            ownerId = Convert.ToInt32(reader["user_id"]);
                        } // Reader is disposed here
                    }

                    // Validate trade conditions
                    if (!ValidateTrade(conn, username, wantedType, minimumDamage))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return "Error: Trade conditions not met.";
                    }

                    // Perform the trade
                    ExecuteTrade(conn, ownerId, username, cardToTrade);

                    // Remove the trade entry
                    using (var deleteCmd = new NpgsqlCommand("DELETE FROM trades WHERE id = @id", conn))
                    {
                        deleteCmd.Parameters.AddWithValue("id", tradeId);
                        deleteCmd.ExecuteNonQuery();
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                return "Trade accepted successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting trade: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to accept trade.";
            }
        }
        private static bool ValidateTrade(NpgsqlConnection conn, string username, string wantedType, float minimumDamage)
        {
            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM cards INNER JOIN users ON cards.owner_id = users.id WHERE users.username = @username AND cards.damage >= @damage AND cards.type = @type", conn))
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("damage", minimumDamage);
                cmd.Parameters.AddWithValue("type", wantedType);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static void ExecuteTrade(NpgsqlConnection conn, int ownerId, string username, string cardToTrade)
        {
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Transfer cards
                    using (var cmd = new NpgsqlCommand("UPDATE cards SET owner_id = (SELECT id FROM users WHERE username = @username) WHERE id = @card", conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.Parameters.AddWithValue("card", cardToTrade);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static string GetAllTrades(HttpListenerContext context)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new NpgsqlCommand("SELECT id, card_to_trade, wanted_type, minimum_damage FROM trades", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var trades = new List<Trade>();
                            while (reader.Read())
                            {
                                trades.Add(new Trade
                                {
                                    Id = reader.GetGuid(0),
                                    CardToTrade = reader.GetGuid(1).ToString(),
                                    WantedType = reader.GetString(2),
                                    MinimumDamage = reader.GetFloat(3)
                                });
                            }
                            return JsonConvert.SerializeObject(trades);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching trades: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to fetch trades.";
            }
        }

        public static string DeleteTrade(HttpListenerContext context, Guid tradeId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new NpgsqlCommand("DELETE FROM trades WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", tradeId);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return "Error: Trade not found.";
                        }
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return "Trade deleted successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting trade: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to delete trade.";
            }
        }

    }
}