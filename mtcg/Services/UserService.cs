using System;
using System.IO;
using System.Net;
using System.Text.Json;
using Npgsql;
using MTCG.Models;

namespace MTCG.Services
{
    public static class UserService
    {
        public static string RegisterUser(HttpListenerContext context)
        {
            try
            {
                string body;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                var user = JsonSerializer.Deserialize<User>(body);

                using (var conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=postgres;Database=mtcg"))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand("INSERT INTO users (username, password, coins) VALUES (@u, @p, 20)", conn))
                    {
                        cmd.Parameters.AddWithValue("u", user.Username);
                        cmd.Parameters.AddWithValue("p", user.Password);
                        cmd.ExecuteNonQuery();
                    }
                }

                context.Response.StatusCode = (int)HttpStatusCode.Created;
                return "User successfully registered.";
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                return "Error: User already exists.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering user: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to register user.";
            }
        }

        public static string LoginUser(HttpListenerContext context)
        {
            try
            {
                string body;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                var user = JsonSerializer.Deserialize<User>(body);

                using (var conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=postgres;Database=mtcg"))
                {
                    conn.Open();

                    string dbPassword;
                    using (var cmd = new NpgsqlCommand("SELECT password FROM users WHERE username = @u", conn))
                    {
                        cmd.Parameters.AddWithValue("u", user.Username);
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            return "Error: Invalid username or password.";
                        }

                        dbPassword = result.ToString();
                    }

                    if (dbPassword != user.Password)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return "Error: Invalid username or password.";
                    }
                }

                return $"{user.Username}-mtcgToken";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging in user: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to login user.";
            }
        }
    }
}
