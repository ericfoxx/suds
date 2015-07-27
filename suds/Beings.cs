using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace suds
{
    public class Player : IDescribable
    {
        public string Name { get; set; }

        public IOccupation Occupation { get; set; }

        public int Strength { get; private set; }

        public int Health { get; set; }

        public int MaxHealth { get; set; }

        public int Gold { get; set; }

        private Random rand;

        public Player()
        {
            rand = new Random();

            Strength = rand.Next(10) + 5;
            Occupation = new Warrior(); ///TODO: implement more occupations!
            Strength = Occupation.AlterStrength(Strength); //run only once! (ooh, or once every few level-ups...)
            Health = MaxHealth = 20;
            //call Occupation.CanSmash() to see if this character can smash.
            Gold = 10;
        }

        public Player(string name)
            : this()
        {
            Name = name;
        }

        void Describe()
        {
            ///TODO: Describe player
        }
    }

    public interface IOccupation
    {
        string SayOccName();

        int AlterStrength(int strength);

        bool CanSmash();
    }

    public class Warrior : IOccupation
    {
        public string SayOccName()
        {
            return "fearsome Warrior";
        }

        public int AlterStrength(int strength)
        {
            //warriors get +2 strength, with a minimum of 10.
            return (strength < 10) ? 12 : strength + 2;
        }

        public bool CanSmash()
        {
            return true;
        }
    }

    public interface IMob
    {
        void Describe();
        void NoticePlayer();
        void MakeSound();
        void Pain(bool IsCrit);
        void Die(bool IsCrit, Room room);
    }

    public class Rat : IMob, IDescribable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Strength { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }
        public bool IsHostile { get; set; }

        private Random rand;

        public Rat()
        {
            rand = new Random();

            Name = "rat" + suds.NextMobID;
            Strength = rand.Next(9) + 3; //3-12
            Health = MaxHealth = 10;
            IsHostile = (rand.NextDouble() >= 0.8) ? true : false; //20% chance 
            Description = String.Format("There is a filthy rat here ({0}).", Name);
        }

        public void Describe()
        {
            Description.Color(suds.Normal, false);
            if (Health / (float)MaxHealth <= 0.5) "It looks bloodied.".Color(suds.Normal, false);
            else if (Health / (float)MaxHealth <= 0.2) "It's near death!".Color(suds.Death, false);
            Console.WriteLine();
        }

        public void NoticePlayer()
        {
            throw new NotImplementedException();
        }

        public void MakeSound()
        {
            throw new NotImplementedException();
        }

        public void Pain(bool IsCrit)
        {
            if (IsCrit) "You deliver a powerful blow!".Color(suds.Alert, false);
            String.Format("{0} recoils in pain.", this.Name).Color(suds.Normal);
            
        }

        public void Die(bool IsCritOrOverkill, Room room)
        {
            var gold = rand.Next(5) + 2; //2-6 gold
            "{0} has been slain.".Color(suds.Death, false);
            if (IsCritOrOverkill)
            {
                "It explodes!".Color(suds.Success);
                ///TODO: add an extra item on crit, with a better roll
                
                gold += 3;
            }
            ///TODO: add items to the room.

            String.Format("The rat drops {0} gold.",gold).Color(suds.Loot);
            room.gold += gold;
            
        }
    }
}
