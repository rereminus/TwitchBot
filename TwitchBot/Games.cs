namespace TwitchBot
{
    public class Games
    {
        public Games() { }

        public static int Roll()
        {
            Random rnd = new Random();
            return rnd.Next(-1000, 1200);
        }
    }
}
