using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using TwitchBot.DB;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Interfaces;

namespace TwitchBot
{
    public class Bot : BackgroundService
    {
        SqliteDataLayer sqliteDataLayer;
        private HttpClient _httpClient;
        HttpListener _listener;
        AuthSettings authSettings;
        string settingsPath = $"{AppDomain.CurrentDomain.BaseDirectory}AuthSettings.json";

        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly TwitchLib.Client.TwitchClient _client;

        public Bot()
        {
            _client = new TwitchLib.Client.TwitchClient();
            _httpClient = new HttpClient();
            _listener = new HttpListener();
            sqliteDataLayer = new SqliteDataLayer();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(settingsPath))
                {
                    /*AuthSettings authSettings = new()
                    {
                        ClientId = "example",
                        ClientSecret = "example",
                        Username = "example"
                    };

                    string jsonString = JsonSerializer.Serialize(authSettings);
                    File.WriteAllText(settingsPath, jsonString);*/

                    throw new Exception("Settings File is mssing");
                }
                else
                {
                    ConnectionCredentials credentials;
                    authSettings = JsonSerializer.Deserialize<AuthSettings>(File.ReadAllText(settingsPath));

                    _listener.Prefixes.Add("http://localhost:7030/");
                    _listener.Start();

                    string authUrl = $"https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={authSettings.ClientId}&redirect_uri=http://localhost:7030&scope=chat:read+chat:edit";
                    var psi = new ProcessStartInfo(authUrl)
                    {
                        UseShellExecute = true
                    };

                    Process.Start(psi);

                    HttpListenerContext context = await _listener.GetContextAsync();
                    string code = context.Request.QueryString["code"];

                    Dictionary<string, string> values = new()
                    {
                        {"client_id", authSettings.ClientId},
                        {"client_secret", authSettings.ClientSecret },
                        {"code", code},
                        {"grant_type", "authorization_code" },
                        {"redirect_uri", "http://localhost:7030" }
                     };

                    var content = new FormUrlEncodedContent(values);

                    var response = await _httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    using (JsonDocument document = JsonDocument.Parse(responseString))
                    {
                        JsonElement root = document.RootElement;
                        if (root.TryGetProperty("access_token", out JsonElement access_tokenElement))
                        {
                            credentials = new ConnectionCredentials(authSettings.Username, access_tokenElement.GetString());
                            _client.Initialize(credentials);

                            _client.OnConnected += Client_OnConnected;
                            _client.OnJoinedChannel += Client_OnJoinedChannel;
                            _client.OnMessageReceived += Client_OnMessageReceived;
                            _client.OnChatCommandReceived += Client_OnChatCommandReceived;

                            await _client.ConnectAsync();
                        }
                        else
                        {
                            throw new Exception("Error receiving token. Bot is not running.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

            await Task.Delay(1000, cancellationToken);
            await base.StartAsync(cancellationToken);

            _logger.Info("Bot has started.");

            _ = Every10SecAsync(authSettings.Username, cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(_client.IsConnected)
                    _logger.Info("Bot is alive at: {time}", DateTimeOffset.Now);
                else
                {
                    _logger.Info("Bot down at: {time}", DateTimeOffset.Now);
                    break;
                }
                    
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.Info("Bot is stopping.");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Bot is stopping...");

            await _client.DisconnectAsync();
            _listener.Stop();
            _listener.Close();
            _httpClient.Dispose();
            sqliteDataLayer.Dispose();

            await base.StopAsync(cancellationToken);

            _logger.Info("Bot has stopped.");
        }

        async Task Client_OnConnected(object? sender, OnConnectedEventArgs e)
        {
            await _client.JoinChannelAsync(authSettings.Username);
        }

        async Task Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
        {
            _logger.Info($"Connected to {e.Channel}");
            await _client.SendMessageAsync(e.Channel, "buh");
        }

        async Task Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            _logger.Info($"{e.ChatMessage.Username} # {e.ChatMessage.Channel}: {e.ChatMessage.Message}");
        }

        async Task Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
        {
            if(e.Command.Name == "roll")
            {
                int result = Games.Roll();
                _logger.Info($"rolled: {result}");
                sqliteDataLayer.UpdateUsers(e.ChatMessage.Username, result);

                await _client.SendMessageAsync(e.ChatMessage.Channel, $"Выигрыш {result}!");
            }
            _logger.Info($"{e.Command}");
        }

        async Task Every10SecAsync(string channel, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _client.SendMessageAsync(channel, "10 сек");
                _logger.Info($"{channel}: 10 сек");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }
}
