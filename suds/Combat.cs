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
            var playerAttack = playerStats.PhysicalAttack;
            var playerCrit = playerStats.CriticalChance;
            var playerFear = playerStats.FearChance;
            var playerLifeSteal = playerStats.LifeSteal;
            var playerStun = playerStats.StunChance;
            var playerStunDur = playerStats.StunDuration;

            var target = Hero.CurrentTarget;
            var targetStats = target.GetStatBlock();
            var targetDef = targetStats.PhysicalDefense;

            var procRoll = 0;

            //TODO: apply weapon and passive skill/xp-grid-buff dmgMods to dmgMod as well
            var dmgMod = 0;

            if (Hero.LeftHand != null)
            {
                var lh = Hero.LeftHand;
                playerAttack += lh.CombatMods.PhyAtk;
                playerAttack.PctMod(lh.CombatMods.PhyAtkPct);
                dmgMod += lh.CombatMods.Dmg;
                dmgMod.PctMod(lh.CombatMods.DmgPct);
                playerCrit += lh.CombatMods.Crit;
                playerCrit.PctMod(lh.CombatMods.CritPct);
                playerFear += lh.CombatMods.Fear;
                playerFear.PctMod(lh.CombatMods.FearPct);
                playerLifeSteal += lh.CombatMods.LifeSteal;
                playerLifeSteal.PctMod(lh.CombatMods.LifeStealPct);
                playerStun += lh.CombatMods.StunChance;
                playerStun.PctMod(lh.CombatMods.StunChancePct);
                playerStunDur += lh.CombatMods.StunDuration;
                playerStunDur.PctMod(lh.CombatMods.StunDurationPct);
            }
            if (Hero.RightHand != null && Hero.LeftHand.ID != Hero.RightHand.ID)
            {
                ///TODO: Implement scaling 2-hand damage nerf reduction skill for warriors?
                var rh = Hero.RightHand;
                playerAttack += rh.CombatMods.PhyAtk;
                playerAttack.PctMod(rh.CombatMods.PhyAtkPct);
                dmgMod += rh.CombatMods.Dmg;
                dmgMod.PctMod(rh.CombatMods.DmgPct);
                playerCrit += rh.CombatMods.Crit;
                playerCrit.PctMod(rh.CombatMods.CritPct);
                playerFear += rh.CombatMods.Fear;
                playerFear.PctMod(rh.CombatMods.FearPct);
                playerLifeSteal += rh.CombatMods.LifeSteal;
                playerLifeSteal.PctMod(rh.CombatMods.LifeStealPct);
                playerStun += rh.CombatMods.StunChance;
                playerStun.PctMod(rh.CombatMods.StunChancePct);
                playerStunDur += rh.CombatMods.StunDuration;
                playerStunDur.PctMod(rh.CombatMods.StunDurationPct);
            }

            

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
                playerCrit += mods.Crit;
                playerCrit.PctMod(mods.CritPct);
                playerFear += mods.Fear;
                playerFear.PctMod(mods.FearPct);
                playerLifeSteal += mods.LifeSteal;
                playerLifeSteal.PctMod(mods.LifeStealPct);
                playerStun += mods.StunChance;
                playerStun.PctMod(mods.StunChancePct);
                playerStunDur += mods.StunDuration;
                playerStunDur.PctMod(mods.StunDurationPct);
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
                var didCrit = (procRoll <= playerCrit.Scale()) ? true : false;
#if DEBUG
                String.Format("(critChance:{0} roll:{1}) ", playerCrit.Scale(), procRoll).Color(suds.Alert, false);
#endif
                if (didCrit)
                {
                    dmg *= 2;
                    "You deliver a powerful blow! ".Color(suds.Alert, false);
                }
                //steal life from mob
                procRoll = Dice.RollPercent();
                if (procRoll < playerLifeSteal.Scale())
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
                if (procRoll < playerFear.Scale())
                {
                    "Your target is so afraid of you, it stops attacking! ".Color(suds.Fancy, false);
                    target.SetIsHostile(false); //It's afraid, so it cowers.
                }
                procRoll = Dice.RollPercent();
                if (procRoll < playerStun.Scale())
                {
                    "You stun your target with your powerful attack! ".Color(suds.Fancy, false);
                    ///TODO: Add any appropriate stun (or other calc'd attrs) buffs to skills
                    target.SetStunned(playerStunDur);
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
}
