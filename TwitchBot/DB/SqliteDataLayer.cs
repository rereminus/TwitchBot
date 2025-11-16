using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;

namespace TwitchBot.DB
{
    public class SqliteDataLayer : IDisposable
    {
        private bool disposed = false;

        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}usersdata.db;";
        SqliteConnection connection;

        public SqliteDataLayer()
        {
            connection = new SqliteConnection(connectionString);
            connection.Open();

            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            command.CommandText = "CREATE TABLE IF NOT EXISTS Users(user_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, username TEXT NOT NULL UNIQUE, coins INTEGER NULL)";
            command.ExecuteNonQuery();
        }

        public void UpdateUsers(string username, int coins)
        {
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;
            try
            {
                command.CommandText = $"INSERT INTO Users (username, coins) VALUES ('{username}', {coins}) ON CONFLICT(username) DO " +
                    $"UPDATE SET coins = coins + EXCLUDED.coins; ";
                command.ExecuteNonQuery();
            }
            catch (Exception ex) 
            {
                Debug.WriteLine("Error adding info in database");
            }
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                connection.Close();
                disposed = true;
            }
        }

        ~SqliteDataLayer()
        {
            Dispose(false);
        }
    }
}
