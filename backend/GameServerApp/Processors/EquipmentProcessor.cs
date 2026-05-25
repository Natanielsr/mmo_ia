using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Processors
{
    public class EquipmentProcessor : IEquipmentProcessor
    {
        private readonly IEquipmentManager _equipmentManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IWorldEvents _worldEvents;

        public EquipmentProcessor(
            IEquipmentManager equipmentManager,
            IInventoryManager inventoryManager,
            IWorldEvents worldEvents)
        {
            _equipmentManager = equipmentManager;
            _inventoryManager = inventoryManager;
            _worldEvents = worldEvents;
        }

        public bool ProcessEquipItem(IPlayer player, string itemId)
        {
            if (player.State == PlayerState.Dead) return false;

            var inv = _inventoryManager.GetInventory(player.Id);
            if (inv == null) return false;

            var item = inv.GetItems().FirstOrDefault(i => i.Id == itemId);
            if (item == null || item.Slot == null) return false;

            var eq = _equipmentManager.GetOrCreate(player.Id);
            inv.RemoveItem(itemId);
            var displaced = eq.Equip(item);
            if (displaced != null) inv.AddItem(displaced);

            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());

            _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
            {
                Id          = player.Id.ToString(),
                Hp          = player.Hp,
                MaxHp       = player.MaxHp,
                Level       = player.Level,
                Experience  = player.Experience,
                AttackPower = player.TotalAttackPower,
                Defense     = player.TotalDefense
            });
            _worldEvents.OnEquipmentUpdated(player.Id, eq.GetAllSlots());
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
            var equippedWeapon = eq.GetAllSlots().TryGetValue(EquipmentSlot.Weapon, out var w) ? w?.Name : null;
            _worldEvents.OnPlayerWeaponEquipped(player.Id.ToString(), equippedWeapon);
            var equippedArmor = eq.GetAllSlots().TryGetValue(EquipmentSlot.Chest, out var a) ? a?.Name : null;
            _worldEvents.OnPlayerArmorEquipped(player.Id.ToString(), equippedArmor);
            return true;
        }

        public bool ProcessUnequipItem(IPlayer player, EquipmentSlot slot)
        {
            var eq = _equipmentManager.GetEquipment(player.Id);
            if (eq == null) return false;

            var item = eq.Unequip(slot);
            if (item == null) return false;

            var inv = _inventoryManager.GetOrCreate(player.Id);
            inv.AddItem(item);

            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());

            _worldEvents.OnPlayerStatusUpdated(new PlayerStatusData
            {
                Id          = player.Id.ToString(),
                Hp          = player.Hp,
                MaxHp       = player.MaxHp,
                Level       = player.Level,
                Experience  = player.Experience,
                AttackPower = player.TotalAttackPower,
                Defense     = player.TotalDefense
            });
            _worldEvents.OnEquipmentUpdated(player.Id, eq.GetAllSlots());
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
            var currentWeapon = eq.GetAllSlots().TryGetValue(EquipmentSlot.Weapon, out var w) ? w?.Name : null;
            _worldEvents.OnPlayerWeaponEquipped(player.Id.ToString(), currentWeapon);
            var currentArmor = eq.GetAllSlots().TryGetValue(EquipmentSlot.Chest, out var a) ? a?.Name : null;
            _worldEvents.OnPlayerArmorEquipped(player.Id.ToString(), currentArmor);
            return true;
        }

        public bool ProcessMoveItemInInventory(IPlayer player, string itemId, int toIndex)
        {
            var inv = _inventoryManager.GetInventory(player.Id);
            if (inv == null) return false;
            if (!inv.MoveItem(itemId, toIndex)) return false;
            _worldEvents.OnInventoryUpdated(player.Id, inv.GetSlottedItems());
            return true;
        }
    }
}
