using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public enum ItemType
    {
        Generic,
        Potion,
        Healing,
        Weapon,
        Armor,
        Helmet,
        Shield,
        Legs,
        Boots
    }

    public interface IItem
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        int Value { get; }
        string TagName { get; }
        Position Position { get; set; }
        float Weight { get; }
        ItemType Type { get; }
        EquipmentSlot? Slot { get; }
        int Quantity { get; set; }
    }
}
