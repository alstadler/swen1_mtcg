using System;
using Npgsql;

namespace MTCG.DataAccess
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mtcg";

        public static NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}