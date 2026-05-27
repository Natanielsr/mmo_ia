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
                instanceId, def.Name, def.Weight, position,
                healAmount: def.HealAmount ?? 20,
                description: def.Description, value: def.Value, tagName: def.TagName),

            "Weapon" => new Weapon(
                instanceId, def.Name, def.Weight, position,
                attackBonus: def.AttackBonus ?? 0,
                description: def.Description, value: def.Value, tagName: def.TagName),

            "Armor" => new Armor(
                instanceId, def.Name, def.Weight, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value, tagName: def.TagName),

            "Helmet" => new Helmet(
                instanceId, def.Name, def.Weight, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value, tagName: def.TagName),

            "Shield" => new Shield(
                instanceId, def.Name, def.Weight, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value, tagName: def.TagName),

            "Legs" => new Legs(
                instanceId, def.Name, def.Weight, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value, tagName: def.TagName),

            "Boots" => new Boots(
                instanceId, def.Name, def.Weight, position,
                defenseBonus: def.DefenseBonus ?? 0,
                description: def.Description, value: def.Value, tagName: def.TagName),

            _ => throw new InvalidOperationException($"Unknown item type: {def.Type}")
        };
    }
}
