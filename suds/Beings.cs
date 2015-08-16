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
        {
            ScaledCritChance = CriticalChance.Scale();
            ScaledFearChance = FearChance.Scale();
            ScaledDodgeChance = DodgeChance.Scale();
            ScaledStunChance = StunChance.Scale();
            ScaledLifeStealChance = LifeSteal.Scale();
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
        public static List<Item> Items { get; set; }
        public static Item LeftHand { get; set; }
        public static Item RightHand { get; set; }
        public static Wielded WieldState { get; set; }

        public static List<Skill> Skills { get; set; }
        public static Skill Skill1 { get; set; }
        public static Skill Skill2 { get; set; }
        public static Skill Skill3 { get; set; }
        public static Skill Skill4 { get; set; }

        static Hero()
        {
            Occupation = new Warrior(); ///TODO: implement more occupations!
            Stats = new StatBlock(20, 20, 10, 6, 8);
            Gold = 10;
            XP = 0;
            CurrentTarget = null;
            Skills = new List<Skill>();
            Items = new List<Item>();
            WieldState = Wielded.None;
        }

        public static void Describe()
        {
            //TODO: Better player description system (items, occ, backstory/quests/title?)
            "You are a fearsome warrior!".Color(suds.Normal);
            if (WieldState == Wielded.TwoH) string.Format("You are wielding {0}.", Hero.LeftHand.Desc).Color(suds.Success);
            else string.Format("You are wielding {0} in your left hand and {1} in your right", (Hero.LeftHand == null) ? "nothing" : Hero.LeftHand.Desc, (Hero.RightHand == null) ? "nothing" : Hero.RightHand.Desc).Color(suds.Success);
            "You are carrying: ".Color(suds.Normal);
            if (Items.Count == 0) "Nothing.".Color(suds.Normal);
            else Items.ForEach(i => i.GetShortDescription());
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

        public static void Wield(Item item)
        {
            var weaponType = Runtime.ItemTypes.Single(i => String.Equals(i.Name, "Weapon"));
            if (item.Type != weaponType)
            {
                "Sorry, you cannot wield that.".Color(suds.Error);
                return;
            }
            if (item.SpecialProps.Count != 0 
                && item.SpecialProps.Any(
                    s => String.Equals(s.Name,"IsTwoHanded")))
            {
                if (item.SpecialProps.Single(
                        s => String.Equals(s.Name,"IsTwoHanded"))
                    .BoolVal == false)
                {
                    //one-handed -- check for empty hand
                    if (LeftHand == null) LeftHand = item;
                    else if (RightHand == null) RightHand = item;
                    else
                    {
                        //don't ask here -- they should figure it out by comparing
                        "Your hands are full. Please un-equip a weapon and try again.".Color(suds.Error);
                    }
                }
                else
                {
                    //two-handed
                    if (LeftHand == null && RightHand == null) LeftHand = RightHand = item;
                    else
                    {
                        "Your hands are full and this item is two-handed. Please un-equip your weapon(s) and try again.".Color(suds.Error);
                    }
                }
            }

            //Set Wielded state
            if (LeftHand != null && RightHand != null)
            {
                WieldState = (LeftHand.ID == RightHand.ID) ? Wielded.TwoH : Wielded.Dual;
            }
            else if (LeftHand == null || RightHand == null)
            {
                WieldState = (LeftHand != null) ? Wielded.LH : Wielded.RH;
            }
            else WieldState = Wielded.None;
        }

        public static void DescAttack(Skill skill = null)
        {
            if (skill != null)
            {
                String.Format("{0} ", skill.Sound).Color(suds.Normal, false);
                return;
            }
            switch (WieldState)
            {
                case Wielded.None:
                    "You swing bare-handed. ".Color(suds.Normal, false);
                    break;
                case Wielded.TwoH:
                case Wielded.LH:
                    LeftHand.ActionDesc.Color(suds.Normal, false);
                    break;
                case Wielded.RH:
                    RightHand.ActionDesc.Color(suds.Normal, false);
                    break;
                case Wielded.Dual:
                    String.Format("You swing with your {0} and {1}. ", LeftHand.Name, RightHand.Name).Color(suds.Normal, false);
                    break;
                default:
                    "YOUR WIELDSTATE IS UNDEF ".Color(suds.Error, false);
                    break;
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

    public enum Wielded
    {
        None,
        LH,
        RH,
        TwoH,
        Dual
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

    public class Mob : IMob
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int BaseXP { get; set; }

        public List<Item> Items { get; set; }
        public StatBlock Stats { get; set; }
        public int StunCounter { get; set; }

        public bool IsHostile { get; set; }
        public bool IsDead { get; set; }
                
        public string Desc { get; set; }
        public string DeadDesc { get; set; }
        public string HalfDeadDesc { get; set; }
        public string MostlyDeadDesc { get; set; }
        public string StunnedDesc { get; set; }
        public string PainDesc { get; set; }

        public string DeathDesc { get; set; }
        public string DeathCritDesc { get; set; }
        public string DropGoldDesc { get; set; }

        public Mob()
        {
            ID = suds.NextMobID++;
            Name = "mob";
            Stats = new StatBlock(Dice.RollRange(6,12), Dice.RollRange(3, 9), Dice.RollRange(3, 8), Dice.RollRange(3, 7), Dice.RollRange(1, 2));
            IsHostile = (Dice.RollRange(1,10) > 9) ? true : false; //10% chance 
            Items = new List<Item>();
            Desc = String.Format("There is a generic mob here: {0}.", GetName());
            DeadDesc = "There is something dead here.";
            HalfDeadDesc = "It is bloodied!";
            MostlyDeadDesc = "It is near death.";
            StunnedDesc = "It is stunned!";
            PainDesc = "It recoils in pain.";
            DeathDesc = "The creature has been slain.";
            DeathCritDesc = "It explodes!";
            DropGoldDesc = "The creature drops {0} gold";
            IsDead = false;
            BaseXP = 2;
            StunCounter = 0;
        }

        public string GetName()
        {
            return String.Format("{0}[{1}]", Name, ID);
        }

        public void Describe()
        {
            if (IsDead)
            {
                DeadDesc.Color(suds.Death);
                return;
            }
            
            string.Format("{0} ",Desc).Color(suds.Normal, false);
            if (Stats.Health / (float)Stats.MaxHealth <= 0.5) String.Format("{0} ", HalfDeadDesc).Color(suds.Alert, false);
            else if (Stats.Health / (float)Stats.MaxHealth <= 0.2) String.Format("{0} ", MostlyDeadDesc).Color(suds.Error, false);
            if (GetIsStunned()) String.Format("{0} ", StunnedDesc).Color(suds.Fancy, false);
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
            String.Format("{0} ", PainDesc).Color(suds.Normal, false);
            if (Stats.Health / (float)Stats.MaxHealth <= 0.5) String.Format("{0} ", HalfDeadDesc).Color(suds.Alert, false);
            else if (Stats.Health / (float)Stats.MaxHealth <= 0.2) String.Format("{0} ", MostlyDeadDesc).Color(suds.Error, false);
        }

        public void Die(bool IsCritOrOverkill)
        {
            var room = Hero.CurrentRoom;
            var gold = Dice.RollRange(2, 6); //2-6 gold
            String.Format("{0} ", DeathDesc).Color(suds.Death, false);
            if (IsCritOrOverkill)
            {
                String.Format("{0} ", DeathCritDesc).Color(suds.Success, false);
                ///TODO: add an extra item on crit, with a better roll
                
                gold *= 2;
            }
            //items
            foreach(var item in Items)
            {
                String.Format("{0} drops {1}. ", GetName(), item.Desc).Color(suds.Loot, false);
                Hero.CurrentRoom.Items.Add(item);
            }
            if (Items.Count > 0) Items.Clear();

            String.Format(DropGoldDesc,gold).Color(suds.Loot, false);
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
