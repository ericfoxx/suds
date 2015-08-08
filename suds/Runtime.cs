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
        public static string prevInput { get; set; }
        public static List<ItemType> ItemTypes { get; set; }
        public static List<Skill> Skills { get; set; }
        public static List<Area> Areas { get; set; }
        public static bool heartbeat { get; set; }
        

        static Runtime()
        {
            quit = false;
            input = prevInput = "";
            heartbeat = false;

            ///TODO: Manage player/world saving and loading

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
            InitItemTypes();
            InitStartingArea();
            InitSkills();

        }

        public static void MainLoop()
        {
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
                if (heartbeat){

                    if ( Hero.CurrentRoom.GetAnyHostiles())
                    {
                        Combat.MobsAttackPlayer();
                    }
                    Hero.DecSkillTimers();
                    Hero.HealthRegen();
                    Hero.CurrentRoom.Mobs
                        .Where(m => !m.GetIsDead() && m.GetIsStunned()).ToList()
                        .ForEach(m => m.DecStunCounter());
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
            var rat = new Mob();
            startingRoom.Mobs = new List<IMob> { rat };
            var sword = new Item
            {
                Name = "Sword",
                Desc = "a simple sword",
                BaseValue = 10,
                Rarity = ItemRarity.Common,
                Type = ItemTypes.First(s => string.Equals(s.Name,"Sword")),
                CombatMods = {
                    PhyAtk = 3,
                    Dmg = 2
                },
                AttackAction = delegate() { "You strike with the sword! ".Color(suds.Normal, false); }
            };
            rat.Items.Add(sword);
            Hero.CurrentRoom = startingRoom;
            startingArea.CurrentRoom = startingRoom;

            var secondRoom = new Room("Second Room", "This is the other one.");
            startingRoom.LinkRooms(secondRoom, suds.Directions.North);

            startingArea.Rooms = new List<Room>(){ startingRoom, secondRoom };
            startingArea.IsLoaded = true;

            Areas.Add(startingArea);
            Hero.CurrentArea = Areas[0];

            startingRoom.Describe();
        }

        internal static void InitItemTypes()
        {
            ItemTypes = new List<ItemType>{
                new ItemType{
                    Name = "Sword"
                },
                new ItemType{
                    Name = "Potion"
                }
            };
        }

        internal static void InitSkills()
        {
            var skills = new List<Skill>{
                new Skill{
                    Name = "Bash", ShortName = "BASH ",
                    Description = "A powerful strike with a greater stun chance.",
                    Sound = "You execute a powerful bash!",
                    TimerMax = 4,
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
                    TimerMax = 5,
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
                    TimerMax = 6,
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
                    TimerMax = 8,
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
                    TimerMax = 11,
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

        internal static void DebugGenerate(string args)
        {
            if (string.IsNullOrWhiteSpace(args) || string.Equals(args, "rat", StringComparison.InvariantCultureIgnoreCase))
            {
                var rat = new Mob();
                Hero.CurrentRoom.Mobs.Add(rat);
                "A rat suddenly appears in the room!".Color(suds.Magic);
            }
            else if (string.Equals(args, "ratking", StringComparison.InvariantCultureIgnoreCase))
            {
                var ratKing = new Mob
                {
                    Stats = new StatBlock(15,17,12,5,5),
                    IsHostile = true,
                    BaseXP = 20,
                    Desc = "There is a giant mass of horrible rats here -- a *RAT KING!*",
                    HalfDeadDesc = "The rat king is bloodied",
                    MostlyDeadDesc = "The rat king is near death!",
                    StunnedDesc = "The rat king is stunned!",
                    PainDesc = "The rat king recoils in pain.",
                    DeathDesc = "The pile of rats quivers and falls apart, finally silenced forever.",
                    DeathCritDesc = "Your final mighty blow sends it flying apart!",
                    DropGoldDesc = "The rat king drops {0} gold"
                };
                ratKing.Name = "TheRatKing";
                Hero.CurrentRoom.Mobs.Add(ratKing);
                "Out of the corner of your eye, you notice a horrible writhing mass of rats appear - it's a *Rat King*!".Color(suds.Magic);
            }
            
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

        public static int RollPercent()
        {
            return Generator.Next(1, 101);
        }

        public static bool CoinFlip()
        {
            return ((Generator.Next(0, 2) == 0) ? false : true);
        }

        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Generator.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
