using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suds
{
    //This includes, but is not limited to:
    //  * Skills
    //  * Feats (?)
    //  * XP Grid (name?)

    
    public class CombatModifiers
    {
        public int PhyAtk { get; set; }
        public decimal PhyAtkPct { get; set; }
        public int PhyDef { get; set; }
        public decimal PhyDefPct { get; set; }

        public int MagAtk { get; set; }
        public decimal MagAtkPct { get; set; }
        public int MagDef { get; set; }
        public decimal MagDefPct { get; set; }

        public int Dmg { get; set; }
        public decimal DmgPct { get; set; }

        public int Crit { get; set; }
        public decimal CritPct { get; set; }

        public int Fear { get; set; }
        public decimal FearPct { get; set; }

        public int Resist { get; set; }
        public decimal ResistPct { get; set; }

        public int HealthRegen { get; set; }
        public decimal HealthRegenPct { get; set; }
        
        public int LifeSteal { get; set; }
        public decimal LifeStealPct { get; set; }

        public int SkillRefresh { get; set; }
        public decimal SkillRefreshPct { get; set; }

        public int StunChance { get; set; }
        public decimal StunChancePct { get; set; }
        
        public int StunDuration { get; set; }
        public decimal StunDurationPct { get; set; }
        
        public int Dodge { get; set; }
        public decimal DodgePct { get; set; }

        public void Describe()
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.PropertyType.Name.Equals("Int32"))
                {
                    int val = (int)prop.GetValue(this, null);
                    if (val != 0) String.Format("{0}: {1}", prop.Name, val).Color(suds.Normal);
                }
                else if (prop.PropertyType.Name.Equals("Decimal"))
                {
                  var val = (decimal)prop.GetValue(this, null);
                  if (val != 0.0m) String.Format("{0}: {1}", prop.Name, val).Color(suds.Normal);
                }
            }
        }

        public CombatModifiers Add(CombatModifiers other)
        {
            var ret = new CombatModifiers();
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.PropertyType.Name.Equals("Int32"))
                {
                    var val = (int)prop.GetValue(this, null);
                    var oVal = (int)prop.GetValue(other, null);
                    prop.SetValue(ret, val + oVal);
                }
                else if (prop.PropertyType.Name.Equals("Decimal"))
                {
                    var val = (decimal)prop.GetValue(this, null);
                    var oVal = (decimal)prop.GetValue(other, null);
                    prop.SetValue(ret, val + oVal);
                }
            }
            return ret;
        }
    }

    public class Skill
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Sound { get; set; }
        public int Timer { get; set; }
        public int TimerMax { get; set; }

        public CombatModifiers Modifiers { get; set; }

        public Skill()
        {
            Modifiers = new CombatModifiers();
            Timer = TimerMax = 0;
        }
    }
}
