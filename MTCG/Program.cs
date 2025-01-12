using System;
using System.Threading;
using MTCG.Controllers;

namespace MTCG
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:10001/";
            var server = new HttpServer(url);

            Thread serverThread = new Thread(new ThreadStart(server.Start))
            {
                IsBackground = true
            };

            serverThread.Start();

            Console.WriteLine("Monster Trading Cards Game Server is running...");
            Console.WriteLine("Press Enter to stop the server.");

            Console.ReadLine();

            Console.WriteLine("Stopping server...");
        }
    }
}