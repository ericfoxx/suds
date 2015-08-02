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

        public int Resist { get; set; }
        public decimal ResistPct { get; set; }
        public int LifeSteal { get; set; }
        public decimal LifeStealPct { get; set; }

        public int StunChance { get; set; }
        public decimal StunChancePct { get; set; }
        public int StunDuration { get; set; }
        public decimal StunDurationPct { get; set; }
        public int Dodge { get; set; }
        public decimal DodgePct { get; set; }
    }

    public class Skill
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Sound { get; set; }

        public CombatModifiers Modifiers { get; set; }

        public Skill()
        {
            Modifiers = new CombatModifiers();
        }
    }
}
