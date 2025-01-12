using System;
using System.Net;
using System.Collections.Generic;
using Npgsql;
using MTCG.Models;
using MTCG.DataAccess;

namespace MTCG.Services
{
    public static class BattleService
    {
        private static Dictionary<string, string> MatchmakingQueue = new Dictionary<string, string>();

        public static string StartBattle(HttpListenerContext context)
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
                    // Fetch user's deck
                    var userDeck = FetchDeck(conn, username);
                    if (userDeck.Count != 4)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return "Error: You must have exactly 4 cards in your deck to battle.";
                    }

                    // Matchmaking
                    var opponent = FindOpponent(username);
                    if (opponent == null)
                    {
                        AddToMatchmakingQueue(username);
                        context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                        return "Waiting for an opponent...";
                    }

                    RemoveFromMatchmakingQueue(username);
                    RemoveFromMatchmakingQueue(opponent);

                    var opponentDeck = FetchDeck(conn, opponent);
                    if (opponentDeck.Count != 4)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return "Error: Opponent's deck is invalid.";
                    }

                    // Execute battle
                    string battleLog = ExecuteBattle(userDeck, opponentDeck, username, opponent, out bool userWon);

                    // Update stats and ELO
                    UpdateStatsAndElo(conn, username, opponent, userWon);

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    return battleLog;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting battle: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Error: Unable to start battle.";
            }
        }

        private static List<Card> FetchDeck(NpgsqlConnection conn, string username)
        {
            var deck = new List<Card>();
            using (var cmd = new NpgsqlCommand("SELECT cards.id, cards.name, cards.damage FROM deck_cards INNER JOIN cards ON deck_cards.card_id = cards.id INNER JOIN decks ON deck_cards.deck_id = decks.id INNER JOIN users ON decks.user_id = users.id WHERE users.username = @username", conn))
            {
                cmd.Parameters.AddWithValue("username", username);
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
            return deck;
        }

        private static string FindOpponent(string username)
        {
            foreach (var entry in MatchmakingQueue)
            {
                if (entry.Key != username)
                {
                    return entry.Key;
                }
            }
            return null;
        }

        private static void AddToMatchmakingQueue(string username)
        {
            if (!MatchmakingQueue.ContainsKey(username))
            {
                MatchmakingQueue[username] = DateTime.Now.ToString();
            }
        }

        private static void RemoveFromMatchmakingQueue(string username)
        {
            if (MatchmakingQueue.ContainsKey(username))
            {
                MatchmakingQueue.Remove(username);
            }
        }

        private static string ExecuteBattle(List<Card> userDeck, List<Card> opponentDeck, string username, string opponent, out bool userWon)
        {
            var battleLog = "Battle Log:\n";
            var userScore = 0;
            var opponentScore = 0;

            for (int i = 0; i < userDeck.Count; i++)
            {
                var userCard = userDeck[i];
                var opponentCard = opponentDeck[i];

                battleLog += $"Round {i + 1}: {userCard.Name} ({userCard.Damage}) vs {opponentCard.Name} ({opponentCard.Damage})\n";

                if (userCard.Damage > opponentCard.Damage)
                {
                    userScore++;
                    battleLog += "User wins the round\n";
                }
                else if (userCard.Damage < opponentCard.Damage)
                {
                    opponentScore++;
                    battleLog += "Opponent wins the round\n";
                }
                else
                {
                    battleLog += "Round is a draw\n";
                }
            }

            userWon = userScore > opponentScore;
            battleLog += userWon ? "User wins the battle!" : "Opponent wins the battle!";

            return battleLog;
        }

        private static void UpdateStatsAndElo(NpgsqlConnection conn, string username, string opponent, bool userWon)
        {
            var userEloChange = userWon ? 30 : -15;
            var opponentEloChange = userWon ? -15 : 30;

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = new NpgsqlCommand("UPDATE users SET elo = elo + @eloChange, games_played = games_played + 1, games_won = games_won + @gamesWon WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("eloChange", userEloChange);
                        cmd.Parameters.AddWithValue("gamesWon", userWon ? 1 : 0);
                        cmd.Parameters.AddWithValue("username", username);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand("UPDATE users SET elo = elo + @eloChange, games_played = games_played + 1, games_won = games_won + @gamesWon WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("eloChange", opponentEloChange);
                        cmd.Parameters.AddWithValue("gamesWon", userWon ? 0 : 1);
                        cmd.Parameters.AddWithValue("username", opponent);
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
    }
}