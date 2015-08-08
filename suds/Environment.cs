using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suds
{
    public class Room : IDescribable
    {
        //room intrinsics
        public string Name { get; set; }
        public int ID { get; set; }
        public Room North { get; set; }
        public Room East { get; set; }
        public Room South { get; set; }
        public Room West { get; set; }
        public Room Up { get; set; }
        public Room Down { get; set; }
        public Area ParentArea { get; set; }

        //room descriptions
        public string RoomDesc { get; set; }
        public bool Described { get; set; }
        //public string FloorDesc { get; set; }
        ///TODO: Add description types to Room class

        //room contents
        public List<IMob> Mobs { get; set; }
        public List<Item> Items { get; set; }
        public int gold { get; set; }
        ///TODO: Add more object types to rooms. Traps?

        public Room(string name)
        {
            Name = name;
            ID = suds.NextRoomID++;
            Described = false;
            Items = new List<Item>();
        }

        public Room(string name, string desc)
            : this(name)
        {
            RoomDesc = desc;
        }

        public Room(string name, string desc, Room n, Room e, Room s, Room w, Room u, Room d)
            : this(name, desc)
        {
            if (n != null) LinkRooms(n, suds.Directions.North);
            if (s != null) LinkRooms(s, suds.Directions.South);
            if (e != null) LinkRooms(e, suds.Directions.East);
            if (w != null) LinkRooms(w, suds.Directions.West);
            if (u != null) LinkRooms(u, suds.Directions.Up);
            if (d != null) LinkRooms(d, suds.Directions.Down);
        }

        /// TODO: Make 'EnterRoom' method in Room -- can handle starting room scripts?

        public void Describe()
        {
            String.Format("==={0}===", Name).Color(suds.Fancy);
            RoomDesc.Color(suds.Normal);

            var exits = "Exits: ";
            exits += (North != null) ? "North " : "";
            exits += (South != null) ? "South " : "";
            exits += (East != null) ? "East " : "";
            exits += (West != null) ? "West " : "";
            exits += (Up != null) ? "Up " : "";
            exits += (Down != null) ? "Down " : "";
            exits.Color(suds.Normal);

            if (Mobs != null) Mobs.ForEach(m => m.Describe());

            ///TODO: Describe items in Room
            if (Items.Count > 0) Items.ForEach(i => i.GetShortDescription());

            if (gold > 0) String.Format("There is {0} gold on the floor.", gold).Color(suds.Loot);

            if (!Described) Described = true;
        }

        

        public void LinkRooms(Room other, suds.Directions direction)
        {
            switch (direction)
            {
                case suds.Directions.North:
                    this.North = other;
                    other.South = this;
                    break;
                case suds.Directions.East:
                    this.East = other;
                    other.West = this;
                    break;
                case suds.Directions.South:
                    this.South = other;
                    other.North = this;
                    break;
                case suds.Directions.West:
                    this.West = other;
                    other.East = this;
                    break;
                case suds.Directions.Up:
                    this.Up = other;
                    other.Down = this;
                    break;
                case suds.Directions.Down:
                    this.Down = other;
                    other.Up = this;
                    break;
            }
        }

        public bool GetAnyHostiles()
        {
            if (Mobs == null) return false;
            var anyHostiles = Mobs.Any(m => m.GetIsHostile() && !m.GetIsStunned());
            return anyHostiles;
        }
    }

    public static class Combat
    {
        public static void GetTarget()
        {
            var room = Hero.CurrentRoom;
            if (room.Mobs != null && !room.Mobs.All(m => m.GetIsDead()))
            {
                Hero.CurrentTarget = (from m in room.Mobs
                                             where m.GetIsDead() == false
                                             orderby m.GetStatBlock().Health
                                             select m).First();
            }
            else
                Hero.CurrentTarget = null;
        }

        public static void AttackMob(Skill skill = null)
        {
            var playerStats = Hero.Stats;
            var playerAttack = playerStats.PhysicalAttack;
            
            var target = Hero.CurrentTarget;
            var targetStats = target.GetStatBlock();
            var targetDef = targetStats.PhysicalDefense;

            var procRoll = 0;
            
            //TODO: apply weapon and passive skill/xp-grid-buff dmgMods to dmgMod as well
            var dmgMod = 0;

            //apply skill effects
            if (skill != null)
            {
                var mods = skill.Modifiers;
                //say what it does
                String.Format("{0} ", skill.Sound).Color(suds.Normal, false);
                playerAttack += mods.PhyAtk;
                playerAttack.PctMod(mods.PhyAtkPct); // (from suds.PctMod extension method)
                dmgMod += mods.Dmg;
                dmgMod.PctMod(mods.DmgPct);
            }
            //roll for damage
            ///TODO: implement magical attacks
#if DEBUG
            String.Format("(pAtk:{0} tDef:{1}) ", playerAttack, targetDef).Color(suds.Normal, false);
#endif
            var roll = Dice.RollRange(1, playerAttack);
            if (roll > targetDef)
            {
                //hit! Compute damage (usually based on weapon and skill)
                var dmg = Dice.RollRange(1, roll + dmgMod - targetDef);
                
                //set skill timer
                if (skill != null) skill.Timer = skill.TimerMax;
                
                //crit calc
                // roll 1-100, if < critChance, CRIT!
                procRoll = Dice.RollPercent();
                var didCrit = (procRoll <= Hero.Stats.ScaledCritChance) ? true : false;
#if DEBUG
                String.Format("(critChance:{0} roll:{1}) ", Hero.Stats.ScaledCritChance, procRoll).Color(suds.Alert, false);
#endif
                if (didCrit)
                {
                    dmg *= 2;
                    "You deliver a powerful blow! ".Color(suds.Alert, false);
                }
                //steal life from mob
                procRoll = Dice.RollPercent();
                if (procRoll < Hero.Stats.ScaledLifeStealChance)
                {
                    var life = (int)Math.Ceiling(Hero.Stats.MaxHealth / 15.0);
                    dmg += life;
                    Hero.Stats.Health = Math.Min(Hero.Stats.MaxHealth, Hero.Stats.Health + life);
                    string.Format("You steal {0} life from your target! ", life).Color(suds.Magic, false);
                }

                //apply damage to mob
                target.TakeDamage(dmg, didCrit);
                
                //check mob life
                var h = target.GetStatBlock().Health;
                var mh = targetStats.MaxHealth;
                if (h <= 0)
                {
                    var didCritOrOverkill = (h <= -0.5 * mh);
                    target.Die(didCritOrOverkill);
                    Hero.XP += target.GrantXP(didCritOrOverkill);
                    //target.SetIsHostile(false);
                }
                //proc Fear & Stun
                procRoll = Dice.RollPercent();
                if (procRoll < Hero.Stats.ScaledFearChance)
                {
                    "Your target is so afraid of you, it stops attacking! ".Color(suds.Fancy, false);
                    target.SetIsHostile(false); //It's afraid, so it cowers.
                }
                procRoll = Dice.RollPercent();
                if (procRoll < Hero.Stats.ScaledStunChance)
                {
                    "You stun your target with your powerful attack! ".Color(suds.Fancy, false);
                    ///TODO: Add any appropriate stun (or other calc'd attrs) buffs to skills
                    target.SetStunned(Hero.Stats.StunDuration);
                }

                //Hero.CurrentTarget = target;
                if (!Hero.CurrentTarget.GetIsHostile() && !Hero.CurrentTarget.GetIsDead()) Hero.CurrentTarget.SetIsHostile(true);
            }
            else
            {
                String.Format("You miss. ({0} vs {1})", roll, targetDef).Color(suds.Error, false);
            }
            Console.WriteLine();

            
        }

        public static void MobsAttackPlayer()
        {
            var room = Hero.CurrentRoom;
            var pDef = Hero.Stats.PhysicalDefense;
            int i, cnt, mAtt, procRoll;
            for (i = 0, cnt = room.Mobs.Count; i < cnt; i++)
            {
                if (room.Mobs[i].GetIsHostile())
                {
                    String.Format("{0} attacks you. ", room.Mobs[i].GetName()).Color(suds.Error, false);
                    mAtt = room.Mobs[i].GetStatBlock().PhysicalAttack;
                    var attRoll = Dice.RollRange(1, mAtt);
                    if (attRoll > pDef)
                    {
                        procRoll = Dice.RollPercent();
                        if (procRoll < Hero.Stats.ScaledDodgeChance)
                        {
                            "You dodge gracefully! ".Color(suds.Fancy);
                        }
                        else
                        {
                            var dmg = Dice.RollRange(1, 3);
                            Hero.Stats.Health -= dmg;
                            "The attack hits!".Color(suds.Error);
                            if (Hero.Stats.Health <= 0) Hero.Die(String.Format("You have been slain by {0}.", room.Mobs[i].GetName()));
                        }
                    }
                    else
                    {
                        "The attack misses!".Color(suds.Normal);
                    }
                }
            }
        }
    }

    public class Area : IDescribable
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLoaded { get; set; }
        public List<Room> Rooms { get; set; }
        public Room CurrentRoom { get; set; }

        public Area(string name, string description)
        {
            ID = suds.NextAreaID++;
            Name = name;
            Description = description;

            IsLoaded = false;
        }

        public void Travel(Room targetRoom)
        {
            CurrentRoom = targetRoom;
            Hero.CurrentRoom = targetRoom;
            CurrentRoom.Describe();
        }

        public void Describe()
        {
            String.Format("{0}: {1}",Name,Description).Color(suds.Normal);
        }
    }
}
