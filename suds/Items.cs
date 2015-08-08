using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace suds
{
    //interface IItem
    //{
    //    string Name;
    //    string Description;
    //    int Value;
    //    int BaseAttack; - better in IWeapon interface?
    //    bool IsRare;
    //    bool CanUse; - redundant with ItemType?
    //    bool IsTwoHanded; - redundant with WType?
    //    bool IsConsumable; - redundant with ItemType?
    //    int HealValue; - better as IConsumable interface?
    //    ItemType Type;
    //    WeaponType WType; - better as IWeapon interface?
    //    
    //}

    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public int BaseValue { get; set; }
        public ItemRarity Rarity { get; set; }
        public ItemType Type { get; set; }
        
        public List<ItemProp> SpecialProps { get; set; }
        public CombatModifiers CombatMods { get; set; }

        public Action AttackAction {get; set; }
        public Action ConsumeAction { get; set; }
        public Action ScriptAction { get; set; }
        public Action UseAction { get; set; }
        
        public Item ()
	    {
            ID = suds.NextItemID++;
            Rarity = ItemRarity.Trash;
            Type = new ItemType();
            SpecialProps = new List<ItemProp>();
            CombatMods = new CombatModifiers();
	    }

        public void GetShortDescription()
        {
            String.Format("{0}: {1} worth {2} gold", GetName(), Desc, GetValue()).Color(suds.Normal);
        }

        public string GetName()
        {
            return String.Format("{0}[{1}]", Name, ID);
        }

        public int GetValue()
        {
            int val;
            switch (Rarity)
            {
                case ItemRarity.Trash:
                    val = (int)Math.Ceiling(BaseValue * 0.5);
                    break;
                case ItemRarity.Common:
                    val = BaseValue;
                    break;
                case ItemRarity.Magical:
                    val = BaseValue * 2;
                    break;
                case ItemRarity.Rare:
                    val = BaseValue * 4;
                    break;
                case ItemRarity.Legendary:
                    val = BaseValue * 8;
                    break;
                case ItemRarity.Mythic:
                    val = BaseValue * 16;
                    break;
                default:
                    val = BaseValue;
                    break;
            };
            return val;
        }
    }

    public class ItemType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<ItemProp> BaseProps { get; set; }

        public ItemType()
        {
            ID = suds.NextItemTypeID++;
            BaseProps = new List<ItemProp>();
        }
    }

    public class ItemProp
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int? IntVal { get; set; }
        public int? IntMinVal { get; set; }
        public int? IntMaxVal { get; set; }
        public string StrVal { get; set; }

        public ItemProp()
        {
            ID = suds.NextItemPropID++;
        }

    }

    public enum ItemRarity
    {
        Trash,
        Common,
        Magical,
        Rare,
        Legendary,
        Mythic
    }
}
