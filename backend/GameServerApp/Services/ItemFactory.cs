using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.World;

namespace GameServerApp.Services;

public class ItemFactory : IItemFactory
{
    public IItem Create(ItemDefinition def, string instanceId, Position position)
    {
        return def.Type switch
        {
            "Potion" => new HealingPotion(
                instanceId, def.Name, def.Weight, def.TagName, position,
                healAmount: def.HealAmount ?? 20,
                description: def.Description, value: def.Value),

            "Food" => new Cheese(
                instanceId, def.Name, def.Weight, def.TagName, position,
                healAmount: def.HealAmount ?? 10,
                description: def.Description, value: def.Value),

            "Weapon" => new Weapon(
                instanceId, def.Name, def.Weight, def.TagName, position,
                attackBonus: def.AttackBonus ?? 0,
                description: def.Description, value: def.Value),

            "Armor" => new Armor(
                instanceId, def.Name, def.Weight, def.TagName, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value),

            "Helmet" => new Helmet(
                instanceId, def.Name, def.Weight, def.TagName, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value),

            "Shield" => new Shield(
                instanceId, def.Name, def.Weight, def.TagName, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value),

            "Legs" => new Legs(
                instanceId, def.Name, def.Weight, def.TagName, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value),

            "Boots" => new Boots(
                instanceId, def.Name, def.Weight, def.TagName, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value),

            _ => throw new InvalidOperationException($"Unknown item type: {def.Type}")
        };
    }
}
