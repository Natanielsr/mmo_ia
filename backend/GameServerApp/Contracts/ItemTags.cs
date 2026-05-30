namespace GameServerApp.Contracts;

public static class ItemTags
{
    public const string Potion       = "potion";
    public const string Dagger       = "dagger";
    public const string LeatherVest  = "leather-vest";
    public const string IronHelmet   = "iron-helmet";
    public const string WoodenShield = "wooden-shield";
    public const string LeatherPants = "leather-pants";
    public const string IronBoots    = "iron-boots";
    public const string PlateArmor   = "plate-armor";
    public const string Cheese        = "cheese";
    public const string RawMeat       = "raw-meat";
    public const string MonsterMeat   = "monster-meat";

    public static class SpawnCode
    {
        public const string Potion = "item:" + ItemTags.Potion;
    }
}
