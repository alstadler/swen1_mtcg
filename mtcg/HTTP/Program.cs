namespace MTCG.HTTP
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HTTPServer();
            server.Start();
        }
    }
}
