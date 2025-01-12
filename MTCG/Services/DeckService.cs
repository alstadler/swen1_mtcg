using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using Npgsql;
using MTCG.Models;

namespace MTCG.Services
{
    public static class DeckService
    {
        public static string GetDeck(HttpListenerContext context)
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
                using (var conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=postgres;Database=mtcg"))
                {
                    conn.Open();

                    List<Card> deck = new List<Card>();
                    using (var cmd = new NpgsqlCommand("SELECT cards.id, cards.name, cards.damage FROM deck_cards INNER JOIN cards ON deck_cards.card_id = cards.id INNER JOIN decks ON deck_cards.deck_id = decks.id INNER JOIN users ON decks.user_id = users.id WHERE users.username = @u", conn))
                    {
                        cmd.Parameters.AddWithValue("u", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                deck.Add(new Card
                                {
                                    Id = reader.GetGuid(0),
                                    Name = reader.GetString(1),
                                    Damage = reader.GetFloat(2)
                                });
                            }
                        }
                    }

                    return JsonSerializer.Serialize(deck);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting deck: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to fetch deck.";
            }
        }

        public static string ConfigureDeck(HttpListenerContext context)
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

                var cardIds = JsonSerializer.Deserialize<List<Guid>>(body);

                if (cardIds == null || cardIds.Count != 4)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return "Error: Deck must contain exactly 4 cards.";
                }

                using (var conn = new NpgsqlConnection("Host=localhost;Username=postgres;Password=postgres;Database=mtcg"))
                {
                    conn.Open();

                    // Clear existing deck
                    using (var cmd = new NpgsqlCommand("DELETE FROM deck_cards USING decks INNER JOIN users ON decks.user_id = users.id WHERE users.username = @u", conn))
                    {
                        cmd.Parameters.AddWithValue("u", username);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert new deck
                    foreach (var cardId in cardIds)
                    {
                        using (var cmd = new NpgsqlCommand("INSERT INTO deck_cards (deck_id, card_id) SELECT decks.id, @cardId FROM decks INNER JOIN users ON decks.user_id = users.id WHERE users.username = @u", conn))
                        {
                            cmd.Parameters.AddWithValue("u", username);
                            cmd.Parameters.AddWithValue("cardId", cardId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return "Deck successfully configured.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configuring deck: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to configure deck.";
            }
        }
    }
}
