namespace GameServerApp.Contracts;

public static class ItemTags
{
    public const string Potion       = "potion";
    public const string Dagger       = "dagger";
    public const string LeatherVest  = "leather-vest";
    public const string IronHelmet   = "iron-helmet";
    public const string WoodenShield = "wooden-shield";
    public const string LeatherPants = "leather-pants";
    public const string PlateBoots   = "plate-boots";
    public const string ChainArmor   = "chain-armor";
    public const string KettleHat    = "kettle-hat";
    public const string LeatherShoes = "leather-shoes";
    public const string PlateLegs    = "plate-legs";
    public const string PurpleJacket = "purple-jacket";
    public const string RobLegs      = "rob-legs";
    public const string Robe         = "robe";
    public const string RobeHood     = "robe-hood";
    public const string WhiteShirt   = "white-shirt";
    public const string LeatherHat   = "leather-hat";
    public const string PlateArmor   = "plate-armor";
    public const string Cheese        = "cheese";
    public const string RawMeat       = "raw-meat";
    public const string MonsterMeat   = "monster-meat";

    public static class SpawnCode
    {
        public const string Potion = "item:" + ItemTags.Potion;
    }
}
