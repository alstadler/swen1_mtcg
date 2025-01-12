using System;
using System.Net;
using Npgsql;
using System.Text.Json;
using MTCG.DataAccess;

namespace MTCG.Services
{
    public static class StatsService
    {
        public static string GetStats(HttpListenerContext context)
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
                    using (var cmd = new NpgsqlCommand("SELECT games_played, games_won, elo FROM users WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var stats = new
                                {
                                    GamesPlayed = reader.GetInt32(0),
                                    GamesWon = reader.GetInt32(1),
                                    Elo = reader.GetInt32(2)
                                };

                                return JsonSerializer.Serialize(stats);
                            }
                            else
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                return "Error: User stats not found.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stats: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to fetch stats.";
            }
        }

        public static string GetScoreboard(HttpListenerContext context)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    using (var cmd = new NpgsqlCommand("SELECT username, elo, games_won FROM users ORDER BY elo DESC LIMIT 10", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var scoreboard = new List<object>();

                            while (reader.Read())
                            {
                                scoreboard.Add(new
                                {
                                    Username = reader.GetString(0),
                                    Elo = reader.GetInt32(1),
                                    GamesWon = reader.GetInt32(2)
                                });
                            }

                            return JsonSerializer.Serialize(scoreboard);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching scoreboard: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to fetch scoreboard.";
            }
        }
    }
}
