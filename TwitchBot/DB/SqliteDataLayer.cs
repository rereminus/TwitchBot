using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace TwitchBot.DB
{
    public class SqliteDataLayer : IDisposable
    {
        private bool disposed = false;
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        string connectionString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}usersdata.db;";
        SqliteConnection connection;

        public SqliteDataLayer()
        {
            connection = new SqliteConnection(connectionString);
            connection.Open();

            SqliteCommand command = new();
            command.Connection = connection;
            command.CommandText = "CREATE TABLE IF NOT EXISTS Users(user_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, username TEXT NOT NULL UNIQUE, balance FLOAT NULL)";
            command.ExecuteNonQuery();
        }

        public void UpdateUsers(string username, double coins)
        {
            SqliteCommand command = new();
            command.Connection = connection;
            try
            {
                command.CommandText = $"INSERT INTO Users (username, balance) VALUES ('{username}', {coins}) ON CONFLICT(username) DO " +
                    $"UPDATE SET balance = balance + EXCLUDED.balance; ";
                command.ExecuteNonQuery();
                _logger.Info("user checked");
            }
            catch (SqliteException sqlex)
            {
                _logger.Error("Error adding info in database", sqlex);
            }
            catch (Exception ex) 
            {
                _logger.Error("Unknown error", ex);
            }
        }

        public float GetBalance(string username)
        {
            SqliteCommand command = new();
            command.Connection = connection;

            try
            {
                command.CommandText = $"SELECT balance FROM Users WHERE username = '{username}'";
                string scalar = command.ExecuteScalar().ToString();
                //TODO: проверка command.ExecuteScalar() на null
                if (float.TryParse(scalar, out float balance))
                {
                    return balance;
                }
                else
                {
                    _logger.Warn($"Error getting balance, command result: '{scalar}'");
                    return 0;
                }
               
                
            }
            catch (SqliteException sqlex)
            {
                _logger.Error($"Error getting balance, code: {sqlex.ErrorCode}");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"User is not in the database: {ex.ToString()}");
                return 0;
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
