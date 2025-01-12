using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using MTCG.Services;

namespace MTCG.Controllers
{
    public class HttpServer
    {
        private readonly string _url;

        public HttpServer(string url)
        {
            _url = url;
        }

        public void Start()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(_url);
            listener.Start();

            Console.WriteLine($"Server listening at {_url}");

            while (true)
            {
                var context = listener.GetContextAsync();
                HandleRequestAsync(context);
            }
        }

        private async void HandleRequestAsync(Task<HttpListenerContext> contextTask)
        {
            try
            {
                var context = await contextTask;
                Console.WriteLine($"Request Method: {context.Request.HttpMethod}");
                Console.WriteLine($"Request URL: {context.Request.Url.AbsolutePath}");

                string responseText;

                // Process request and determine response
                responseText = ProcessRequest(context);

                // Write response
                await WriteResponseAsync(context, responseText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
            }
        }

        private string ProcessRequest(HttpListenerContext context)
        {
            string responseText;

            try
            {
                if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/users")
                {
                    responseText = UserService.RegisterUser(context);
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/sessions")
                {
                    responseText = UserService.LoginUser(context);
                }
                else if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/deck")
                {
                    responseText = DeckService.GetDeck(context);
                }
                else if (context.Request.HttpMethod == "PUT" && context.Request.Url.AbsolutePath == "/deck")
                {
                    responseText = DeckService.ConfigureDeck(context);
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/packages")
                {
                    responseText = PackageService.CreatePackage(context);
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/transactions/packages")
                {
                    responseText = PackageService.BuyPackage(context);
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/battles")
                {
                    responseText = BattleService.StartBattle(context);
                }
                else if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/stats")
                {
                    responseText = StatsService.GetStats(context);
                }
                else if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/scoreboard")
                {
                    responseText = StatsService.GetScoreboard(context);
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath == "/tradings")
                {
                    responseText = TradeService.CreateTrade(context);
                }
                else if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath == "/tradings")
                {
                    responseText = TradeService.GetAllTrades(context);
                }
                else if (context.Request.HttpMethod == "DELETE" && context.Request.Url.AbsolutePath.StartsWith("/tradings/"))
                {
                    var tradeId = context.Request.Url.AbsolutePath.Replace("/tradings/", "");
                    responseText = TradeService.DeleteTrade(context, Guid.Parse(tradeId));
                }
                else if (context.Request.HttpMethod == "POST" && context.Request.Url.AbsolutePath.StartsWith("/tradings/"))
                {
                    var tradeId = context.Request.Url.AbsolutePath.Replace("/tradings/", "");
                    responseText = TradeService.AcceptTrade(context, Guid.Parse(tradeId));
                }
                else
                {
                    responseText = "Endpoint not found.";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                responseText = "Internal server error.";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return responseText;
        }

        private async Task WriteResponseAsync(HttpListenerContext context, string responseText)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing response: {ex.Message}");
            }
        }
    }
}

