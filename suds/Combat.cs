using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suds
{
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
            StatBlock calcdStats = new StatBlock();

            var target = Hero.CurrentTarget;
            var targetStats = target.GetStatBlock();
            var targetDef = targetStats.PhysicalDefense;

            var procRoll = 0;

            //TODO: apply weapon and passive skill/xp-grid-buff dmgMods to dmgMod as well
            var dmgMod = 0;

            if (Hero.WieldState == Wielded.Dual || Hero.WieldState == Wielded.LH || Hero.WieldState == Wielded.TwoH)
            {
                var lh = Hero.LeftHand;
                calcdStats = playerStats.AddMods(lh.CombatMods);
                dmgMod += lh.CombatMods.Dmg;
                dmgMod.PctMod(lh.CombatMods.DmgPct);
            }
            if (Hero.WieldState == Wielded.RH || Hero.WieldState == Wielded.Dual)
            {
                ///TODO: Implement scaling 2-hand damage nerf reduction skill for warriors?
                var rh = Hero.RightHand;
                if (Hero.LeftHand == null) calcdStats = playerStats.AddMods(rh.CombatMods);
                else calcdStats = calcdStats.AddMods(rh.CombatMods);
                dmgMod += rh.CombatMods.Dmg;
                dmgMod.PctMod(rh.CombatMods.DmgPct);
            }

            

            //apply skill effects
            if (skill != null)
            {
                var mods = skill.Modifiers;
                calcdStats = calcdStats.AddMods(mods);
                dmgMod += mods.Dmg;
                dmgMod.PctMod(mods.DmgPct);
            }

            //Describe the attack
            Hero.DescAttack(skill);

            //roll for damage
            ///TODO: implement magical attacks
#if DEBUG
            String.Format("(pAtk:{0} tDef:{1}) ", calcdStats.PhysicalAttack, targetDef).Color(suds.Normal, false);
#endif
            var roll = Dice.RollRange(1, calcdStats.PhysicalAttack);
            if (roll > targetDef)
            {
                //hit! Compute damage (usually based on weapon and skill)
                var dmg = Dice.RollRange(1, roll + dmgMod - targetDef);

                //set skill timer
                if (skill != null) skill.Timer = skill.TimerMax;

                //crit calc
                // roll 1-100, if < critChance, CRIT!
                procRoll = Dice.RollPercent();
                var didCrit = (procRoll <= calcdStats.CriticalChance.Scale()) ? true : false;
#if DEBUG
                String.Format("(critChance:{0} roll:{1}) ", calcdStats.CriticalChance.Scale(), procRoll).Color(suds.Alert, false);
#endif
                if (didCrit)
                {
                    dmg *= 2;
                    "You deliver a powerful blow! ".Color(suds.Alert, false);
                }
                //steal life from mob
                procRoll = Dice.RollPercent();
                if (procRoll < calcdStats.LifeSteal.Scale())
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
                    Console.WriteLine();
                    return;
                }
                //proc Fear & Stun
                procRoll = Dice.RollPercent();
                if (procRoll < calcdStats.FearChance.Scale())
                {
                    "Your target is so afraid of you, it stops attacking! ".Color(suds.Fancy, false);
                    target.SetIsHostile(false); //It's afraid, so it cowers.
                }
                procRoll = Dice.RollPercent();
                if (procRoll < calcdStats.StunChance.Scale())
                {
                    "You stun your target with your powerful attack! ".Color(suds.Fancy, false);
                    target.SetStunned(calcdStats.StunDuration);
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
            var calcdStats = new StatBlock();

            if (Hero.WieldState == Wielded.Dual || Hero.WieldState == Wielded.LH || Hero.WieldState == Wielded.TwoH)
            {
                var lh = Hero.LeftHand;
                calcdStats = Hero.Stats.AddMods(lh.CombatMods);
            }
            if (Hero.WieldState == Wielded.RH || Hero.WieldState == Wielded.Dual)
            {
                ///TODO: Implement scaling 2-hand damage nerf reduction skill for warriors?
                var rh = Hero.RightHand;
                if (Hero.LeftHand == null) calcdStats = Hero.Stats.AddMods(rh.CombatMods);
                else calcdStats = calcdStats.AddMods(rh.CombatMods);
            }

            int i, cnt, mAtt, procRoll;
            for (i = 0, cnt = room.Mobs.Count; i < cnt; i++)
            {
                if (room.Mobs[i].GetIsHostile())
                {
                    String.Format("{0} attacks you. ", room.Mobs[i].GetName()).Color(suds.Error, false);
                    mAtt = room.Mobs[i].GetStatBlock().PhysicalAttack;
                    var attRoll = Dice.RollRange(1, mAtt);
                    if (attRoll > calcdStats.PhysicalDefense)
                    {
                        procRoll = Dice.RollPercent();
                        if (procRoll < calcdStats.DodgeChance.Scale(Hero.Boots.CombatMods.Dodge))
                        {
                            "You dodge gracefully! ".Color(suds.Fancy);
                        }
                        else
                        {
                            var dmg = (room.Mobs[i].GetStatBlock().BaseDamage / 3 ) + Dice.RollRange(1, 3);
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
}
