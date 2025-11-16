namespace TwitchBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHostedService<Bot>();
            var app = builder.Build();

            app.Run();
        }
    }
}
