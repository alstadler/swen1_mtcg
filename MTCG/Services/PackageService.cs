using System;
using System.Net;
using Npgsql;
using MTCG.Models;
using Newtonsoft.Json;
using MTCG.DataAccess;

namespace MTCG.Services
{
    public static class PackageService
    {
        public static string CreatePackage(HttpListenerContext context)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader == null || authHeader != "Bearer admin-mtcgToken")
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return "Error: Unauthorized.";
            }

            try
            {
                string body;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                var cards = JsonConvert.DeserializeObject<List<Card>>(body);
                if (cards == null || cards.Count != 5 || cards.Any(c => string.IsNullOrWhiteSpace(c.Type)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return "Error: A package must contain exactly 5 cards.";
                }

                using (var conn = DatabaseHelper.GetConnection())
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Insert the package
                            using (var cmd = new NpgsqlCommand("INSERT INTO packages DEFAULT VALUES RETURNING id", conn))
                            {
                                var packageId = (int)cmd.ExecuteScalar();

                                // Insert the cards
                                foreach (var card in cards)
                                {
                                    using (var cardCmd = new NpgsqlCommand("INSERT INTO cards (id, name, damage) VALUES (@id, @name, @damage)", conn))
                                    {
                                        cardCmd.Parameters.AddWithValue("id", card.Id);
                                        cardCmd.Parameters.AddWithValue("name", card.Name);
                                        cardCmd.Parameters.AddWithValue("damage", card.Damage);
                                        cardCmd.ExecuteNonQuery();
                                    }

                                    using (var packageCardCmd = new NpgsqlCommand("INSERT INTO package_cards (package_id, card_id) VALUES (@packageId, @cardId)", conn))
                                    {
                                        packageCardCmd.Parameters.AddWithValue("packageId", packageId);
                                        packageCardCmd.Parameters.AddWithValue("cardId", card.Id);
                                        packageCardCmd.ExecuteNonQuery();
                                    }
                                }
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

                context.Response.StatusCode = (int)HttpStatusCode.Created;
                return "Package created successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating package: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to create package.";
            }
        }

        public static string BuyPackage(HttpListenerContext context)
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
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check user's coins
                            using (var cmd = new NpgsqlCommand("SELECT coins FROM users WHERE username = @username", conn))
                            {
                                cmd.Parameters.AddWithValue("username", username);
                                var coins = (int)cmd.ExecuteScalar();

                                if (coins < 5)
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    return "Error: Not enough coins to buy a package.";
                                }
                            }

                            // Get available package
                            int packageId;
                            using (var cmd = new NpgsqlCommand("SELECT id FROM packages LIMIT 1", conn))
                            {
                                var result = cmd.ExecuteScalar();

                                if (result == null)
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                    return "Error: No packages available.";
                                }

                                packageId = (int)result;
                            }

                            // Assign cards to user and remove package
                            using (var cmd = new NpgsqlCommand("SELECT card_id FROM package_cards WHERE package_id = @packageId", conn))
                            {
                                cmd.Parameters.AddWithValue("packageId", packageId);
                                var cardIds = new List<Guid>();

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        cardIds.Add(reader.GetGuid(0));
                                    }
                                }

                                foreach (var cardId in cardIds)
                                {
                                    using (var assignCmd = new NpgsqlCommand("UPDATE cards SET owner_id = (SELECT id FROM users WHERE username = @username) WHERE id = @cardId", conn))
                                    {
                                        assignCmd.Parameters.AddWithValue("username", username);
                                        assignCmd.Parameters.AddWithValue("cardId", cardId);
                                        assignCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            using (var cmd = new NpgsqlCommand("DELETE FROM packages WHERE id = @packageId", conn))
                            {
                                cmd.Parameters.AddWithValue("packageId", packageId);
                                cmd.ExecuteNonQuery();
                            }

                            // Deduct coins
                            using (var cmd = new NpgsqlCommand("UPDATE users SET coins = coins - 5 WHERE username = @username", conn))
                            {
                                cmd.Parameters.AddWithValue("username", username);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            return "Package bought successfully.";
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error buying package: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to buy package.";
            }
        }

    }
}
