using System.Data;
using Microsoft.Data.Sqlite;

namespace Esseti.Data
{
    public class DatabaseConfig
    {
        private const string ConnectionString = "Data Source=Data/esseti.db";

        public static IDbConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }
    }
}