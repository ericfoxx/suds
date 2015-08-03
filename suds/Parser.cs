using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace suds
{
    public static class Parser
    {
        public static List<string> Grammar = new List<string>
        {
            @"^(n)(?:orth)?",
            @"^(s)(?:outh)?",
            @"^(e)(?:ast)?",
            @"^(w)(?:est)?",
            @"^(d)(?:own)?",
            @"^(u)(?:p)?",
            @"^(l)(?:ook)?(?:\s+(\w+))?",
            @"^(a)(?:ttack)?(?:\s+(\w+))?",
            @"^(i)(?:nv)?(?:\s+(\w+))?",
            @"^(g)(?:rab|et)?(?:\s+(\w+))?",
            @"^(q)(?:uit)?",
            @"^(m)(?:e)?",
            @"^(h)(?:elp)?",
            @"^(1)(?:\s+(\w+))?",
            @"^(2)(?:\s+(\w+))?",
            @"^(3)(?:\s+(\w+))?",
            @"^(4)(?:\s+(\w+))?"
        };
        
        public static bool Parse(this string input) ///TODO: Parse - area
        {
            var area = Runtime.CurrentArea;
            var command = 'x';
            var blank = false;
            string args = null;
            var cont = true;
            //basic tests
            //emulate previous input if ENTER pressed:
            if (string.Equals(input, "∟"))
            {
                input = Runtime.prevInput;
                blank = true;
            }
            if (input.Length == 0 || input.Length >= 80) return false;
            //in the grammer?
            for (var idx = 0; idx < Grammar.Count() && cont; idx++)
            {
                var match = Regex.Match(input, Grammar[idx], RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    command = char.Parse(match.Groups[1].Value);
                    if (match.Groups[2] != null) args = match.Groups[2].Value;
                    cont = false;
                }
            }
            if (!blank) Runtime.prevInput = input;
            RunCommand(command, args);

            return true;
        }

        private static void RunCommand(char command, string args)
        {
            var area = Runtime.CurrentArea;
            switch (command)
            {
                case 'n':
                    "You walk north.".Color(suds.Normal);
                    var north = area.CurrentRoom.North;
                    if (north != null) area.Travel(north);
                    else "There does not appear to be an exit that way.".Color(suds.Error);
                    break;
                case 's':
                    "You walk south.".Color(suds.Normal);
                    var south = area.CurrentRoom.South;
                    if (south != null) area.Travel(south);
                    else "There does not appear to be an exit that way.".Color(suds.Error);
                    break;
                case 'e':
                    "You walk east.".Color(suds.Normal);
                    var east = area.CurrentRoom.East;
                    if (east != null) area.Travel(east);
                    else "There does not appear to be an exit that way.".Color(suds.Error);
                    break;
                case 'w':
                    "You walk west.".Color(suds.Normal);
                    var west = area.CurrentRoom.West;
                    if (west != null) area.Travel(west);
                    else "There does not appear to be an exit that way.".Color(suds.Error);
                    break;
                case 'd':
                    "You climb down.".Color(suds.Normal);
                    var down = area.CurrentRoom.Down;
                    if (down != null) area.Travel(down);
                    else "There does not appear to be an exit that way.".Color(suds.Error);
                    break;
                case 'u':
                    "You climb up.".Color(suds.Normal);
                    var up = area.CurrentRoom.Up;
                    if (up != null) area.Travel(up);
                    else "There does not appear to be an exit that way.".Color(suds.Error);
                    break;
                case 'q':
                    suds.Quit();
                    break;
                case 'l':
                    LookAt(args);
                    break;
                case 'm':
                    LookAt("me");
                    ///TODO: more 'me' description options
                    break;
                case 'h':
                    Help();
                    break;
                case 'a':
                    Attack(args);
                    Runtime.heartbeat = true;
                    break;
                case 'g':
                    Grab(args);
                    Runtime.heartbeat = true;
                    break;
                case '1':
                    Attack(args, Runtime.Hero.Skill1);
                    break;
                case '2':
                    Attack(args, Runtime.Hero.Skill2);
                    break;
                case '3':
                    Attack(args, Runtime.Hero.Skill3);
                    break;
                case '4':
                    Attack(args, Runtime.Hero.Skill4);
                    break;
                case 'i': ///TODO: Parser - Implement inventory case
                default:
                    "Not implemented yet. Good job picking though!".Color(suds.Error);
                    break;
            }
        }

        private static void Grab(string args)
        {
            var area = Runtime.CurrentArea;
            ///TODO: Disambiguate null arg 'grab'
            //for now, just grab gold
            var gold = area.CurrentRoom.gold;
            if (String.IsNullOrEmpty(args) && gold > 0)
            {
                if (Runtime.Hero.Gold >= 999999)
                    "You have too much gold. Spend some!".Color(suds.Error);
                else
                {
                    Runtime.Hero.Gold += gold;
                    area.CurrentRoom.gold = 0;
                    "You pick up the gold.".Color(suds.Loot);
                }
            }
        }

        private static void Attack(string args, Skill skill = null)
        {
            var room = Runtime.CurrentArea.CurrentRoom;
            var player = Runtime.Hero;
            var t = player.CurrentTarget;
            ///TODO: process Attack args
            if (t != null)
            {
                if (t.GetIsDead())
                {
                    Combat.GetTarget();
                }
                

            }
            else
            {
                Combat.GetTarget();
            }
            if (player.CurrentTarget != null)
            {
                Combat.AttackMob(skill);
            }
            else "There is nothing to attack here.".Color(suds.Error);
        }

        

        private static void LookAt(string args)
        {
            var area = Runtime.CurrentArea;
            if (args == null)
            {
                area.CurrentRoom.Describe();
                return;
            }
            //FindObject(args, area);

            //FOR NOW, just make it work for descriptions
            if (Regex.IsMatch(args, @"^m(e)?", RegexOptions.IgnoreCase)) area.CurrentRoom.player.Describe();
            
            ///TODO: Find way to match mob names on describe
            //area.CurrentRoom.mobs.ForEach(m => m.Describe());
            if (args.Equals("area",StringComparison.InvariantCultureIgnoreCase)) area.Describe();
        }

        //private static List<IDescribable> GetDescribables(Area area)
        //{
        //    var areaType = area.GetType();
        //    var areaProps = areaType.GetProperties().ToList();
        //    return (from p in areaProps
        //            where p is IDescribable
        //            select (IDescribable)p.GetValue(area)).ToList();
            //describableObjs.ForEach(o => o.Describe());
            ///BUG: I don't think GetDescribables is returning anything b/c the props themselves are indirect: classes that inherit IDescribable.
        //}

        //private void GetListOfDescs<Object>(List<Object> objects, out IList<IDescribable> objs) where Object : IDescribable
        //{
        //    objects = (from o in objects.OfType<IDescribable>() where o is IDescribable select (IDescribable)o).ToList();
        //}

        private static void FindObject(string args)
        {
            ///TODO: find scalable way to find objects and perform actions upon them
        }

        private static void Help()
        {
            ///TODO: Move Help to its own class.
            "This is where the help will go when I get around to it, and can make it more than 1,000 writeLines.".Color(suds.Normal);
        }
    }
}
