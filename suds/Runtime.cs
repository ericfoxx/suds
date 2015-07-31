using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace suds
{
    /// <summary>
    /// Runtime: manages state including:
    ///  - loading/unloading assets
    ///  - procedurally generating areas
    ///  - " items/doodads/flavor text
    /// </summary>
    public static class Runtime
    {
        private static bool quit { get; set; }
        public static string input { get; set; }
        public static Player Hero { get; set; }
        public static List<Area> Areas { get; set; }
        

        static Runtime()
        {
            quit = false;
            input = "";

            ///TODO: Manage player/world saving and loading

            Hero = new Player();
            "Welcome to the SUDS (Single-User Dungeon System)!".Color(suds.Normal);
            while (input.Length < 1 || input.Length > 15)
                input = "What is your name?".Ask(suds.Normal);
            Hero.Name = input;

            ///TODO: Choose occupation
            String.Format("Hello, {0}! You are a {1} with {2} Strength.", Hero.Name, Hero.Occupation.SayOccName(), Hero.Stats.Strength).Color(suds.Normal);
            if (Hero.Occupation.CanSmash()) "You can smash opponents!".Color(suds.Normal);

            Areas = new List<Area>();

            //ALL OTHER CODE GOES IN INIT or later.


        }

        /// <summary>
        /// Load all initial objects (starting area & its contents)
        /// </summary>
        public static void Init()
        {
            InitStartingArea();


        }

        public static void MainLoop()
        {
            var currentArea = Areas[0];
            
            while (!quit)
            {
                //prompt (including writing the player bar)
                input = suds.Prompt(currentArea.CurrentRoom);
                //parse input
                if (!Parser.Parse(input, currentArea))
                {
                    "Sorry, I can't understand that.".Color(suds.Error);
                }

                ///TODO: discriminate between world-mutating player actions and others (to prevent combat when looking, etc.)

                //mutate world
                //for example, combat, health regen, etc.
                if (currentArea.CurrentRoom.GetAnyHostiles())
                {
                    Combat.MobsAttackPlayer(currentArea.CurrentRoom);
                }

                //Handle player death after combat actions
                if (Hero.Stats.Health <= 0) Hero.Die("You have been slain.");
                //save state?


            }
        }

        private static void InitStartingArea()
        {
            var startingArea = new Area("Starting Area", "This is the area the player starts in.");

            var startingRoom = new Room("Starting Room", "This is the room you start in. It looks very normal.");
            startingRoom.gold = 2;
            var rat = new Rat();
            startingRoom.mobs = new List<IMob> { rat };
            startingRoom.player = Hero;
            startingArea.CurrentRoom = startingRoom;

            var secondRoom = new Room("Second Room", "This is the other one.");
            startingRoom.LinkRooms(secondRoom, suds.Directions.North);

            startingArea.Rooms = new List<Room>(){ startingRoom, secondRoom };
            startingArea.ContainsPlayer = true;
            startingArea.IsLoaded = true;

            Areas.Add(startingArea);

            startingRoom.Describe();
        }
    }

    public interface IDescribable
    {
        void Describe();
    }

    public static class Dice
    {
        // Let it be thread-safe
        private static ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(
         () => new Random());

        // Thread-safe non-skewed generator
        public static Random Generator
        {
            get
            {
                return s_Gen.Value;
            }
        }
        
        public static int RollRange(int min, int max)
        {
            return Generator.Next(min, max + 1);
        }

        public static bool CoinFlip()
        {
            return ((Generator.Next(0, 2) == 0) ? false : true);
        }
    }
}
