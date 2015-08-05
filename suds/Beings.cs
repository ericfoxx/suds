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
        public int HealthRegen { get; set; }

        public int LifeSteal { get; set; }
        public int SkillRefreshChance { get; set; }
        public int DodgeChance { get; set; }
        public int StunChance { get; set; }
        public int StunDuration { get; set; }

        public int ScaledCritChance { get; set; }
        public int ScaledFearChance { get; set; }
        public int ScaledDodgeChance { get; set; }
        public int ScaledStunChance { get; set; }
        public int ScaledLifeStealChance { get; set; }

        public StatBlock()
        {
            Health = MaxHealth = 10;
            
            Strength = Dexterity = Intellect = Spirit = 10;

            CalcDerivedAttributes();
            CalcScaledAttributes();
        }

        public StatBlock(int maxH, int str, int dex, int intel, int spi)
        {
            Health = MaxHealth = maxH;
            
            Strength = str;
            Dexterity = dex;
            Intellect = intel;
            Spirit = spi;

            CalcDerivedAttributes();
            CalcScaledAttributes();
        }

        public void CalcDerivedAttributes()
        {
            PhysicalAttack = PhysicalDefense = (int)Math.Ceiling((Strength + Dexterity) / 2.0);
            MagicAttack = MagicDefense = (int)Math.Ceiling((Intellect + Spirit) / 2.0);
            CriticalChance = (int)Math.Ceiling((Dexterity + Spirit) / 2.0);
            FearChance = (int)Math.Ceiling((Strength + Intellect) / 2.0);
            HealthRegen = (int)Math.Floor((Strength + Spirit) / 2.0);

            StunDuration = 2; //Because heartbeat happens on same step, we waste one

            StunChance = Strength / 2;
            DodgeChance = Dexterity / 2;
            SkillRefreshChance = Intellect / 2;
            LifeSteal = Spirit / 2;
        }

        public void CalcScaledAttributes()
        {// Wolfram Alpha: "plot (10x^0.3)/(0.2x^0.3+1) from x = -1 to x = 15"
            //(10x^0.3)/(0.2x^0.3+1)
            ScaledCritChance = Scale(CriticalChance);
            ScaledFearChance = Scale(FearChance);
            ScaledDodgeChance = Scale(DodgeChance);
            ScaledStunChance = Scale(StunChance);
            ScaledLifeStealChance = Scale(LifeSteal);
        }

        private int Scale(int stat)
        {
            var pow = Math.Pow((double)stat, 0.3);
            var scaledStat = (int)Math.Floor((10 * pow) / (1 + 0.2 * pow));
            return scaledStat;
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
    
    public static class Hero
    {
        public static string Name { get; set; }
        public static IOccupation Occupation { get; set; }
        public static StatBlock Stats { get; set; }
        public static Room CurrentRoom { get; set; }
        public static Area CurrentArea { get; set; }
        public static IMob CurrentTarget { get; set; }
        public static int XP { get; set; }
        public static int Gold { get; set; }
        public static List<Skill> Skills { get; set; }
        public static Skill Skill1 { get; set; }
        public static Skill Skill2 { get; set; }
        public static Skill Skill3 { get; set; }
        public static Skill Skill4 { get; set; }

        static Hero()
        {
            Occupation = new Warrior(); ///TODO: implement more occupations!
            Stats = new StatBlock(20, Dice.RollRange(12,17), Dice.RollRange(8, 10), Dice.RollRange(7, 10), Dice.RollRange(8, 10));
            Gold = 10;
            XP = 0;
            CurrentTarget = null;
            Skills = new List<Skill>();
        }

        public static void Describe()
        {
            //TODO: Better player description system (items, occ, backstory/quests/title?)
            "You are a fearsome warrior!".Color(suds.Normal);
            Stats.Display();
        }

        public static void Die(string p)
        {
            p.Color(suds.Death);
            suds.Quit();
        }

        public static void GetSkillStatuses()
        {
            "-SK1:".Color(suds.Normal, false);
            if (Skill1 != null)
            {
                String.Format("{0,5}:{1:D2}", Skill1.ShortName, Skill1.Timer).Color(suds.Normal, false);
            }
            else
            {
                "-----:00".Color(suds.Normal, false);
            }
            "-SK2:".Color(suds.Normal, false);
            if (Skill2 != null)
            {
                String.Format("{0,5}:{1:D2}", Skill2.ShortName, Skill2.Timer).Color(suds.Normal, false);
            }
            else
            {
                "-----:00".Color(suds.Normal, false);
            }
            "-SK3:".Color(suds.Normal, false);
            if (Skill3 != null)
            {
                String.Format("{0,5}:{1:D2}", Skill3.ShortName, Skill3.Timer).Color(suds.Normal, false);
            }
            else
            {
                "-----:00".Color(suds.Normal, false);
            }
            "-SK4:".Color(suds.Normal, false);
            if (Skill4 != null)
            {
                String.Format("{0,5}:{1:llD2}", Skill4.ShortName, Skill4.Timer).Color(suds.Normal, false);
            }
            else
            {
                "-----:00".Color(suds.Normal, false);
            }
        }

        public static void DecSkillTimers()
        {
            //roll for skill refresh & choose a random non-zero timer to set to zero
            var skills = new List<Skill> { Skill1, Skill2, Skill3, Skill4 };
            if (skills.Count(s => s != null && s.Timer > 0) == 0) return; //no skills to dec or refresh
            Skill skill;
            var roll = Dice.RollPercent();
            if (roll < Hero.Stats.SkillRefreshChance)
            {
                skills = new List<Skill> { Skill1, Skill2, Skill3, Skill4 };
                Dice.Shuffle(skills);
                skill = skills.First(s => s != null && s.Timer != 0);
                if (skill != null)
                {
                    String.Format("You feel energized as {0} refreshes!", skill.Name).Color(suds.Magic);
                    skill.Timer = 0;
                }
            }
            //check and see if any skill is on a timer, then dec by one (stopping at 0)
            skills.Where(s => s != null && s.Timer > 0).ToList().ForEach(s => s.Timer--);
        }

        internal static void HealthRegen()
        {
            if (Stats.Health < Stats.MaxHealth)
            {
                var regen = (int)Math.Floor(Stats.MaxHealth / (decimal)Stats.HealthRegen);
                Stats.Health = Math.Min(Stats.MaxHealth, Stats.Health + regen);
            }
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
        void Die(bool IsCritOrOverkill);
        int GrantXP(bool IsCrit);
        StatBlock GetStatBlock();
        bool GetIsHostile();
        void SetIsHostile(bool flag);
        bool GetIsDead();
        void SetStunned(int duration);
        bool GetIsStunned();
        void DecStunCounter();
    }

    public class Rat : IMob
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public StatBlock Stats { get; set; }
        public bool IsHostile { get; set; }
        public bool IsDead { get; set; }
        public int BaseXP { get; set; }
        public int StunCounter { get; set; }

        public Rat()
        {
            ID = suds.NextMobID;
            Name = "rat" + ID;
            suds.NextMobID++;
            Stats = new StatBlock(Dice.RollRange(6,12), Dice.RollRange(3, 9), Dice.RollRange(3, 8), Dice.RollRange(3, 7), Dice.RollRange(1, 2));
            IsHostile = (Dice.RollRange(1,10) > 9) ? true : false; //10% chance 
            Description = String.Format("There is a filthy rat here ({0}).", Name);
            IsDead = false;
            BaseXP = 2;
            StunCounter = 0;
        }

        public string GetName()
        {
            return Name;
        }

        public void Describe()
        {
            if (IsDead)
            {
                "There is a dead rat here.".Color(suds.Death);
                return;
            }
            
            Description.Color(suds.Normal, false);
            if (Stats.Health / (float)Stats.MaxHealth <= 0.5) "It looks bloodied. ".Color(suds.Alert, false);
            else if (Stats.Health / (float)Stats.MaxHealth <= 0.2) "It's near death! ".Color(suds.Error, false);
            if (GetIsStunned()) "It is stunned! ".Color(suds.Fancy, false);
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
            String.Format("{0} recoils in pain. ", this.Name).Color(suds.Normal, false);
            if (Stats.Health / (float)Stats.MaxHealth <= 0.5) "It looks bloodied.".Color(suds.Alert, false);
            else if (Stats.Health / (float)Stats.MaxHealth <= 0.2) "It's near death!".Color(suds.Error, false);
        }

        public void Die(bool IsCritOrOverkill)
        {
            var room = Hero.CurrentRoom;
            var gold = Dice.RollRange(2, 6); //2-6 gold
            "The rat has been slain. ".Color(suds.Death, false);
            if (IsCritOrOverkill)
            {
                "It explodes! ".Color(suds.Success, false);
                ///TODO: add an extra item on crit, with a better roll
                
                gold += 3;
            }
            ///TODO: add items to the room.

            String.Format("The rat drops {0} gold.",gold).Color(suds.Loot, false);
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
            return BaseXP * ((IsCrit) ? 2 : 1);
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

        public void SetStunned(int duration)
        {
            StunCounter += duration;
        }

        public bool GetIsStunned()
        {
            return (StunCounter > 0);
        }

        public void DecStunCounter()
        {
            if (StunCounter <= 1) StunCounter = 0;
            else StunCounter--;
        }
    }
}
