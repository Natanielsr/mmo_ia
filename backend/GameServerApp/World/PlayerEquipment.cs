using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class PlayerEquipment
    {
        private readonly Dictionary<EquipmentSlot, IItem?> _slots = new();

        /// <summary>
        /// Equips an item into its designated slot. Returns any displaced item, or null if slot was empty.
        /// Returns null (no-op) if the item has no Slot (not equippable).
        /// </summary>
        public IItem? Equip(IItem item)
        {
            if (item.Slot == null) return null;

            var slot = item.Slot.Value;
            _slots.TryGetValue(slot, out var displaced);
            _slots[slot] = item;
            return displaced;
        }

        public IItem? Unequip(EquipmentSlot slot)
        {
            if (!_slots.TryGetValue(slot, out var item) || item == null) return null;
            _slots[slot] = null;
            return item;
        }

        public IItem? GetItem(EquipmentSlot slot)
            => _slots.TryGetValue(slot, out var item) ? item : null;

        public IReadOnlyDictionary<EquipmentSlot, IItem?> GetAllSlots() => _slots;

        public int GetAttackBonus()
            => _slots.Values.OfType<Weapon>().Sum(w => w.AttackBonus);

        public int GetDefenseBonus()
            => _slots.Values.OfType<IDefensiveItem>().Sum(d => d.DefenseBonus);
    }
}
