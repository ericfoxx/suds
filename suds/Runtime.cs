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
        public static List<Skill> Skills { get; set; }
        public static List<Area> Areas { get; set; }
        public static Area CurrentArea { get; set; }
        public static bool heartbeat { get; set; }
        

        static Runtime()
        {
            quit = false;
            input = "";
            heartbeat = false;

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
            InitSkills();

        }

        public static void MainLoop()
        {
            CurrentArea = Areas[0];
            
            while (!quit)
            {
                //prompt (including writing the player bar)
                input = suds.Prompt();
                //parse input
                if (!Parser.Parse(input))
                {
                    "Sorry, I can't understand that.".Color(suds.Error);
                }

                ///TODO: discriminate between world-mutating player actions and others (to prevent combat when looking, etc.)

                //mutate world
                //for example, combat, health regen, etc.
                if (heartbeat && CurrentArea.CurrentRoom.GetAnyHostiles())
                {
                    Combat.MobsAttackPlayer();
                }

                //Handle player death after combat actions
                if (Hero.Stats.Health <= 0) Hero.Die("You have been slain.");
                //save state?

                heartbeat = false;
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

        internal static void InitSkills()
        {
            var skills = new List<Skill>{
                new Skill{
                    Name = "Bash", ShortName = "BASH ",
                    Description = "A powerful strike with a greater stun chance.",
                    Sound = "You execute a powerful bash!",
                    Modifiers = {
                        PhyAtk = 5,
                        Dmg = 5,
                        StunChance = 5,
                        StunDuration = 1                        
                    }
                },
                new Skill{
                    Name = "CorpseShield", ShortName = "CORSH",
                    Description = "Use a corpse in the room to increase your defenses.",
                    Sound = "You throw a corpse over your shoulder for protection. Yuck.",
                    Modifiers = {
                        PhyDef = 25,
                        MagDef = 25,
                        Resist = 5
                    }
                },
                new Skill{
                    Name = "BodySlam", ShortName = "BSLAM",
                    Description = "A huge slam that targets the entire room.",
                    Sound = "The whole room shakes with your huge body slam!",
                    Modifiers = {
                        PhyAtk = 50,
                        Dmg = 100,
                        StunChance = 20,
                        StunDuration = 2
                    }
                },
                new Skill{
                    Name = "TrueStrike", ShortName = "TRUES",
                    Description = "A cunning lunge that delivers incredible damage.",
                    Sound = "You surge forward, wreathed in power, delivering the True Strike!",
                    Modifiers = {
                        PhyAtk = 125,
                        PhyAtkPct = 0.25M,
                        Dmg = 500,
                        DmgPct = 0.5M,
                        StunChancePct = .1M,
                        StunDurationPct = .5M
                    }
                },
                new Skill{
                    Name = "AlphaForm", ShortName = "ALPHA",
                    Description = "A cataclysm of physical prowess!",
                    Sound = "You deliver death to all as you assume the ALPHA FORM!",
                    Modifiers = {
                        PhyAtk = 500,
                        PhyAtkPct = 0.5M,
                        Dmg = 1250,
                        DmgPct = 1.0M,
                        StunChancePct = .2M,
                        StunDurationPct = 1.0M
                    }
                }
            };
            Skills = skills;
            
            //init our warrior with Bash.
            var bash = Skills.First(s => string.Equals(s.ShortName,"BASH "));
            Hero.Skills.Add(bash);
            Hero.Skill1 = bash;
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
