using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Item : IItem
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Value { get; }
        public string TagName { get; }
        public Position Position { get; set; }
        public float Weight { get; }
        public ItemType Type { get; }
        public virtual EquipmentSlot? Slot => null;
        public int Quantity { get; set; } = 1;

        public Item(string id, string name, float weight, Position position = null!,
                    ItemType type = ItemType.Generic,
                    string description = "", int value = 0, string tagName = "")
        {
            Id = id;
            Name = name;
            Weight = weight;
            Position = position ?? new Position(0, 0);
            Type = type;
            Description = description;
            Value = value;
            TagName = tagName;
        }
    }
}
