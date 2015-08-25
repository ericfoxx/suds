using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suds
{
    public static class Experience
    {
        public static IOccupation HeroOccupation { get; set; }
        
        //serialize and deserialize from a static json data file 
        //containing the definitions of the nodes
        //see: http://roy-t.nl/index.php/2010/04/12/serialization-blob-ini-xml-and-json/

        //serialize from a MASTER and decorate the output
        //via a savefile specific to the character

    }

    public class ExpNode
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int XPCost { get; set; }
        public bool InitLocked { get; set; }
        public bool CurrLocked { get; set; }
        public int InitCharges { get; set; }
        public int CurrCharges { get; set; }
        public CombatModifiers Mods { get; set; }
        public SpecialProp SpecialProp { get; set; }
    }
}
