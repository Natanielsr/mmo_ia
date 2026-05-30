using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class PlayerInventory(int maxSlots = 20)
    {
        private readonly IItem?[] _slots = new IItem?[maxSlots];
        private readonly int _maxSlots = maxSlots;

        public bool AddItem(IItem item)
        {
            if (item == null) return false;
            if (item.Type == ItemType.Potion)
            {
                var existing = _slots.FirstOrDefault(s => s?.Type == ItemType.Potion);
                if (existing != null) { existing.Quantity++; return true; }
            }
            if (item.Type == ItemType.Food)
            {
                var existing = _slots.FirstOrDefault(s => s?.TagName == item.TagName);
                if (existing != null) { existing.Quantity++; return true; }
            }
            var freeIdx = Array.IndexOf(_slots, null);
            if (freeIdx < 0) return false;
            _slots[freeIdx] = item;
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            for (int i = 0; i < _maxSlots; i++)
                if (_slots[i]?.Id == itemId) { _slots[i] = null; return true; }
            return false;
        }

        public bool UseItem(string itemId, IPlayer player)
        {
            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i]?.Id != itemId) continue;

                if (_slots[i]!.Type == ItemType.Potion)
                {
                    player.Heal(20);
                    _slots[i]!.Quantity--;
                    if (_slots[i]!.Quantity <= 0) _slots[i] = null;
                    return true;
                }

                if (_slots[i]!.Type == ItemType.Food)
                {
                    var food = _slots[i] as Cheese;
                    player.ApplyHoT(_slots[i]!.TagName, food?.HealPerTick ?? 1, food?.TotalTicks ?? 10);
                    _slots[i]!.Quantity--;
                    if (_slots[i]!.Quantity <= 0) _slots[i] = null;
                    return true;
                }

                return false;
            }
            return false;
        }

        public bool DropItem(string itemId, Position targetPos)
        {
            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i]?.Id != itemId) continue;
                _slots[i]!.Position = targetPos;
                _slots[i] = null;
                return true;
            }
            return false;
        }

        public bool MoveItem(string itemId, int toIndex)
        {
            if (toIndex < 0 || toIndex >= _maxSlots) return false;
            int srcIdx = -1;
            for (int i = 0; i < _maxSlots; i++)
                if (_slots[i]?.Id == itemId) { srcIdx = i; break; }
            if (srcIdx < 0) return false;
            (_slots[toIndex], _slots[srcIdx]) = (_slots[srcIdx], _slots[toIndex]);
            return true;
        }

        public IReadOnlyList<IItem> GetItems()
        {
            var result = new List<IItem>();
            for (int i = 0; i < _maxSlots; i++)
                if (_slots[i] != null) result.Add(_slots[i]!);
            return result.AsReadOnly();
        }

        public IReadOnlyList<(int SlotIndex, IItem Item)> GetSlottedItems()
        {
            var result = new List<(int, IItem)>();
            for (int i = 0; i < _maxSlots; i++)
                if (_slots[i] != null) result.Add((i, _slots[i]!));
            return result.AsReadOnly();
        }
    }
}
