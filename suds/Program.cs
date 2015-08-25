using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///Single User Dungeon System
namespace suds
{
    class Program
    {
        static void Main(string[] args)
        {
            //Display-specific settings
            Console.Clear();
            Console.SetWindowSize(120, 40);
            Console.BufferWidth = 120;
            Console.BufferHeight = 40;

            //Init
            Runtime.Init();

            //MAIN LOOP

            Runtime.MainLoop();


            suds.Quit();
        }

        
    }

    

    

    public static class suds
    {
        public static ConsoleColor Fancy = ConsoleColor.Blue;
        public static ConsoleColor Normal = ConsoleColor.White;
        public static ConsoleColor Error = ConsoleColor.Red;
        public static ConsoleColor Alert = ConsoleColor.Yellow;
        public static ConsoleColor Success = ConsoleColor.Green;
        public static ConsoleColor Magic = ConsoleColor.Cyan;
        public static ConsoleColor Death = ConsoleColor.DarkRed;
        public static ConsoleColor Loot = ConsoleColor.Magenta;
        
        public enum Directions {
            North,
            South,
            East,
            West,
            Up,
            Down
        };

        //identifier counts
        public static int NextRoomID = 0;
        public static int NextMobID = 0;
        public static int NextAreaID = 0;

        public static int NextItemID = 0;
        public static int NextItemTypeID = 0;
        public static int NextSpecialPropID = 0;
        public static int NextXPNodeID = 0;

        public static string Prompt()
        {
            var health = Hero.Stats.Health / (float)Hero.Stats.MaxHealth;
            //12345678901234567890123456789012345678901234567890123456789012345678901234567890
            //<PlayerNameXXXX>-HP:HHH/MMM-SK:XXXXx-GP:XXXXXXX-
            "<".Color(suds.Normal, false);
            String.Format("{0,15}", Hero.Name).Color(suds.Fancy, false);
            String.Format(">-HP:").Color(suds.Normal, false);
            String.Format("{0:D3}",Hero.Stats.Health).Color(
                (health <= 0.5) 
                ? (health <= 0.2) ? suds.Death 
                    : suds.Alert
                : suds.Normal
                , false);
            String.Format("/{0:D3}",Hero.Stats.MaxHealth).Color(suds.Normal, false);
            Hero.GetSkillStatuses();
            String.Format("-GP:").Color(suds.Normal, false);
            String.Format("{0:D7}", Hero.Gold).Color(suds.Loot, false);
            String.Format("-XP:").Color(suds.Normal, false);
            String.Format("{0:D7}", Hero.XP).Color(suds.Fancy, false);
            Console.WriteLine();
            if (Hero.CurrentRoom.GetAnyHostiles()) return "!> ".Ask(suds.Alert, false);
            return ">> ".Ask(suds.Normal, false);
        }
        
        public static string Ask(this string promptText, ConsoleColor color, bool EOL = true)
        {
            String.Format("{0} ", promptText).Color(color, EOL);
            var ret = Console.ReadLine();
            if (ret == "") ret = "∟";
            return ret;
        }

        public static int Choose(this string promptText, List<string> list)
        {
            var cont = true;
            var choice = 0;
            while (cont)
            {
                Console.WriteLine(promptText);
                for (var i = 1; i <= list.Count; i++)
                {
                    Console.WriteLine("\t{0}: {1}", i, list[i-1]);
                }
                Console.WriteLine("Choose: [1-{0}] ", list.Count);
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    "Please enter a valid numerical value!".Color(suds.Error);
                }
                else if (choice > list.Count || choice < 1)
                {
                    String.Format("Please enter a number between 1 and {0}.", list.Count)
                        .Color(suds.Error);
                }
                else
                {
                    cont = false;
                }
            }
            return choice-1;
        }

        public static void Color(this string text, ConsoleColor color, bool EOL = true)
        {
            Console.ForegroundColor = color;
            if (!EOL)   Console.Write(text);
            else        Console.WriteLine(text);
            Console.ForegroundColor = suds.Normal;
        }

        public static void Quit()
        {
            //End
            Console.WriteLine();
            Console.WriteLine("==========");
            Console.WriteLine("Thank you for playing! Enjoy real life. (Press any key to exit.)");
            Console.Read();
            System.Environment.Exit(0);
        }

        public static void PctMod(this int stat, decimal pctMod)
        {
            if (pctMod != 0.0M)
                stat *= (int)Math.Ceiling(1.0M + pctMod);
        }

        public static int Scale(this int stat, int mod = 0)
        {
            // Wolfram Alpha: "plot (10x^0.3)/(0.2x^0.3+1) from x = -1 to x = 15"
            //(10x^0.3)/(0.2x^0.3+1)
            var n = stat + mod;
            var pow = Math.Pow((double)n, 0.3);
            var scaledStat = (int)Math.Floor((10 * pow) / (1 + 0.2 * pow));
            return scaledStat;
        }
    }
}


