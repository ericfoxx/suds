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
        public Player player { get; set; }
        public List<IMob> mobs { get; set; }
        //public List<IItem> items { get; set; }
        public int gold { get; set; }
        ///TODO: Add more object types to rooms. Traps?

        public Room(string name)
        {
            Name = name;
            ID = suds.NextRoomID;
            suds.NextRoomID++;
            Described = false;
            ///TODO: Area = "Starting Area"; in Room ctor
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

        /// TODO: Make 'EnterRoom' method in Room

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

            if (mobs != null) mobs.ForEach(m => m.Describe());

            ///TODO: Describe items in Room

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
            if (mobs == null) return false;
            var anyHostiles = mobs.Any(m => m.GetIsHostile());
            return anyHostiles;
        }
    }

    public static class Combat
    {
        public static void GetTarget()
        {
            var room = Runtime.CurrentArea.CurrentRoom;
            if (room.mobs != null && !room.mobs.All(m => m.GetIsDead()))
            {
                room.player.CurrentTarget = (from m in room.mobs
                                             where m.GetIsDead() == false
                                             orderby m.GetStatBlock().Health
                                             select m).First();
            }
            else
                Runtime.Hero.CurrentTarget = null;
        }

        public static void AttackMob(Skill skill = null)
        {
            var player = Runtime.Hero;
            var playerStats = player.Stats;
            var playerAttack = playerStats.PhysicalAttack;
            
            var target = Runtime.Hero.CurrentTarget;
            var targetStats = target.GetStatBlock();
            var targetDef = targetStats.PhysicalDefense;
            
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
                var dmg = Dice.RollRange(1, roll - targetDef + dmgMod);
                ///TODO: roll critical after hit

                //crit calc
                // roll 1-100, if < critChance, CRIT!
                //(10x^0.3)/(0.2x^0.3+1)
                var pow = Math.Pow((double)playerStats.CriticalChance,0.3);
                var critChance = (int)Math.Floor((10 * pow)/(1 + 0.2 * pow));
                var critRoll = Dice.RollRange(1, 100);
                var didCrit = (critRoll <= critChance) ? true : false;
#if DEBUG
                String.Format("(critChance:{0} roll:{1}) ", critChance, critRoll).Color(suds.Alert, false);
#endif
                if (didCrit) "You deliver a powerful blow! ".Color(suds.Alert, false);
                
                //apply damage to mob
                target.TakeDamage(dmg, didCrit);
                
                //check mob life
                var h = target.GetStatBlock().Health;
                var mh = targetStats.MaxHealth;
                if (h <= 0)
                {
                    target.Die((h <= (-0.5 * mh ) ? true : false));
                    player.XP += target.GrantXP(didCrit);
                    target.SetIsHostile(false);
                }
                else if (!target.GetIsHostile()) target.SetIsHostile(true);
            }
            else
            {
                String.Format("You miss. ({0} vs {1})", roll, targetDef).Color(suds.Error);
            }

            
        }

        public static void MobsAttackPlayer()
        {
            var room = Runtime.CurrentArea.CurrentRoom;
            var player = Runtime.Hero;
            var pDef = player.Stats.PhysicalDefense;
            int i, cnt, mAtt;
            for (i = 0, cnt = room.mobs.Count; i < cnt; i++)
            {
                if (room.mobs[i].GetIsHostile())
                {
                    String.Format("{0} attacks you. ", room.mobs[i].GetName()).Color(suds.Error, false);
                    mAtt = room.mobs[i].GetStatBlock().PhysicalAttack;
                    if (Dice.RollRange(1,mAtt) > pDef)
                    {
                        var dmg = Dice.RollRange(1, 3);
                        player.Stats.Health -= dmg;
                        "The attack hits!".Color(suds.Error);
                        if (player.Stats.Health <= 0) player.Die(String.Format("You have been slain by {0}.", room.mobs[i].GetName()));
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
        public bool ContainsPlayer { get; set; }
        public Room CurrentRoom { get; set; }

        public Area(string name, string description)
        {
            ID = suds.NextAreaID;
            suds.NextAreaID++;
            Name = name;
            Description = description;

            IsLoaded = ContainsPlayer = false;
        }

        public void Travel(Room targetRoom)
        {
            var player = CurrentRoom.player;
            CurrentRoom = targetRoom;
            CurrentRoom.player = player;
            CurrentRoom.Describe();
        }

        public void Describe()
        {
            String.Format("{0}: {1}",Name,Description).Color(suds.Normal);
        }
    }
}
