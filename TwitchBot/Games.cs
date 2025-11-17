using Microsoft.AspNetCore.Routing.Constraints;

namespace TwitchBot
{
    public class Games
    {
        public Games() { }

        public static double Roll()
        {
            Random rnd = new Random();
            //return rnd.Next(-1000, 1200);

            int chance = rnd.Next(1, 101);
            double result = 0;

            switch (chance)
            {
                //BIG LOSE 5%
                case > 0 and <= 5:
                    result = rnd.Next(-1000, -500);
                    break;
                //STANDART LOSE 32%
                case > 5 and <= 37:
                    result = rnd.Next(-500, -100);
                    break;
                //MICRO LOSE 7%
                case > 37 and <= 44:
                    result = rnd.Next(-100, 0);
                    break;
                //SMALL WIN 15%
                case > 44 and <= 59:
                    result = rnd.Next(0, 100);
                    break;
                //STANDART WIN 35%
                case > 59 and <= 94:
                    result = rnd.Next(100, 300);
                    break;
                //BIG WIN 5%
                case > 94 and <= 99:
                    result = rnd.Next(300, 700);
                    break;
                //MEGA WIN 1%
                case 100:
                    result = rnd.Next(700, 10000);
                    break;
                default:
                    break;
            }
            return result + rnd.NextDouble(); 
        }
    }
}
