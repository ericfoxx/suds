using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace suds
{
    public class StatBlock
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Intellect { get; set; }
        public int Spirit { get; set; }
        
        public int PhysicalAttack { get; set; }
        public int MagicAttack { get; set; }
        public int PhysicalDefense { get; set; }
        public int MagicDefense { get; set; }
        public int CriticalChance { get; set; }
        public int FearChance { get; set; }

        public int LifeSteal { get; set; }
        public int SkillRefreshChance { get; set; }
        public int DodgeChance { get; set; }
        public int StunChance { get; set; }

        public StatBlock()
        {
            Health = MaxHealth = 10;
            
            Strength = Dexterity = Intellect = Spirit = 10;

            CalcDerivedAttributes();
        }

        public StatBlock(int maxH, int str, int dex, int intel, int spi)
        {
            Health = MaxHealth = maxH;
            
            Strength = str;
            Dexterity = dex;
            Intellect = intel;
            Spirit = spi;

            CalcDerivedAttributes();
        }

        public void CalcDerivedAttributes()
        {
            PhysicalAttack = PhysicalDefense = (int)Math.Ceiling((Strength + Dexterity) / 2.0);
            MagicAttack = MagicDefense = (int)Math.Ceiling((Intellect + Spirit) / 2.0);
            CriticalChance = (int)Math.Ceiling((Dexterity + Spirit) / 2.0);
            FearChance = (int)Math.Ceiling((Strength + Intellect) / 2.0);

            StunChance = Strength / 2;
            DodgeChance = Dexterity / 2;
            SkillRefreshChance = Intellect / 2;
            LifeSteal = Spirit / 2;
        }

        public void Display()
        {
            String.Format("Health:       {0,6}/{1,6}", Health.ToString(), MaxHealth.ToString()).Color(suds.Normal);
            String.Format("Strength:      {0,4} | Dexterity:  {1,4} | Physical Attack:  {2,4}", Strength.ToString(), Dexterity.ToString(), PhysicalAttack.ToString()).Color(suds.Normal);
            String.Format("Stun:          {0,4} | Dodge:      {1,4} | Physical Defense: {2,4}", StunChance.ToString(), DodgeChance.ToString(), PhysicalDefense.ToString()).Color(suds.Normal);
            String.Format("Intellect:     {0,4} | Spirit:     {1,4} | Magical Attack:   {2,4}", Intellect.ToString(), Spirit.ToString(), MagicAttack.ToString()).Color(suds.Normal);
            String.Format("Skill Refresh: {0,4} | Life Steal: {1,4} | Magical Defense:  {2,4}", SkillRefreshChance.ToString(), LifeSteal.ToString(), MagicDefense.ToString()).Color(suds.Normal);
        }
    }
    
    public class Player : IDescribable
    {
        public string Name { get; set; }
        public IOccupation Occupation { get; set; }
        public StatBlock Stats { get; set; }
        public IMob CurrentTarget { get; set; }
        public int XP { get; set; }
        public int Gold { get; set; }
        public List<Skill> Skills { get; set; }
        public Skill Skill1 { get; set; }
        public Skill Skill2 { get; set; }
        public Skill Skill3 { get; set; }
        public Skill Skill4 { get; set; }

        public Player()
        {
            Occupation = new Warrior(); ///TODO: implement more occupations!

            ///TODO: Create SetStats in Warrior class.

            Stats = new StatBlock(20, Dice.RollRange(12,17), Dice.RollRange(8, 10), Dice.RollRange(7, 10), Dice.RollRange(8, 10));

            //call Occupation.CanSmash() to see if this character can smash.
            Gold = 10;

            XP = 0;

            CurrentTarget = null;

            Skills = new List<Skill>();
        }

        public Player(string name)
            : this()
        {
            Name = name;
        }

        public void Describe()
        {
            "You are a fearsome warrior!".Color(suds.Normal);
            Stats.Display();
        }

        public void Die(string p)
        {
            p.Color(suds.Death);
            suds.Quit();
        }

        public void UseSkill1(Area area)
        {
            "Hi-ya!".Color(suds.Normal);
        }

        public void UseSkill2(Area area)
        {
            "Wa-chow!".Color(suds.Normal);
        }

        public void UseSkill3(Area area)
        {
            "You perform a spinning roundhouse!".Color(suds.Normal);
        }

        public void UseSkill4(Area area)
        {
            "You flip the table over. Wow!".Color(suds.Normal);
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

    public interface IMob : IDescribable
    {
        string GetName();
        void NoticePlayer();
        void MakeSound();
        void TakeDamage(int damage, bool IsCrit);
        void Die(bool IsCritOrOverkill, Room room);
        int GrantXP(bool IsCrit);
        StatBlock GetStatBlock();
        bool GetIsHostile();
        void SetIsHostile(bool flag);
        bool GetIsDead();
    }

    public class Rat : IMob
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public StatBlock Stats { get; set; }
        public bool IsHostile { get; set; }
        public bool IsDead { get; set; }

        public Rat()
        {
            Name = "rat" + suds.NextMobID;
            Stats = new StatBlock(Dice.RollRange(6,12), Dice.RollRange(3, 9), Dice.RollRange(3, 8), Dice.RollRange(3, 7), Dice.RollRange(1, 2));
            IsHostile = (Dice.RollRange(1,10) > 9) ? true : false; //10% chance 
            Description = String.Format("There is a filthy rat here ({0}).", Name);
            IsDead = false;
        }

        public string GetName()
        {
            return Name;
        }

        public void Describe()
        {
            if (IsDead)
            {
                "This rat is dead.".Color(suds.Death);
                return;
            }
            
            Description.Color(suds.Normal, false);
            if (Stats.Health / (float)Stats.MaxHealth <= 0.5) "It looks bloodied.".Color(suds.Normal, false);
            else if (Stats.Health / (float)Stats.MaxHealth <= 0.2) "It's near death!".Color(suds.Death, false);
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

        public void TakeDamage(int damage, bool IsCrit)
        {
            Stats.Health -= damage;
            if (IsCrit) "You deliver a powerful blow! ".Color(suds.Alert, false);
            String.Format("{0} recoils in pain. ", this.Name).Color(suds.Normal, false);
            if (Stats.Health / (float)Stats.MaxHealth <= 0.5) "It looks bloodied.".Color(suds.Alert, false);
            else if (Stats.Health / (float)Stats.MaxHealth <= 0.2) "It's near death!".Color(suds.Error, false);
            Console.WriteLine();
        }

        public void Die(bool IsCritOrOverkill, Room room)
        {
            var gold = Dice.RollRange(2, 6); //2-6 gold
            "The rat has been slain. ".Color(suds.Death, false);
            if (IsCritOrOverkill)
            {
                "It explodes!".Color(suds.Success);
                ///TODO: add an extra item on crit, with a better roll
                
                gold += 3;
            }
            ///TODO: add items to the room.

            String.Format("The rat drops {0} gold.",gold).Color(suds.Loot);
            room.gold += gold;
            IsDead = true;
            IsHostile = false;
        }

        public StatBlock GetStatBlock()
        {
            return Stats;
        }

        public int GrantXP(bool IsCrit)
        {
            return (IsCrit) ? 4 : 2;
        }

        public bool GetIsHostile()
        {
            return IsHostile;
        }

        public void SetIsHostile(bool flag)
        {
            IsHostile = flag;
        }

        public bool GetIsDead()
        {
            return IsDead;
        }
    }
}
