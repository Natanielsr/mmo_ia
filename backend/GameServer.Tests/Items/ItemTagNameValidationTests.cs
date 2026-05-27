using GameServerApp.Contracts.Types;
using GameServerApp.World;
using Xunit;

namespace GameServer.Tests.Items;

public class ItemTagNameValidationTests
{
    private static readonly Position Pos = new(1, 1);

    [Fact]
    public void Item_Constructor_Throws_When_TagName_Is_Null()
    {
        Assert.Throws<ArgumentException>(() =>
            new Item("id", "Name", 1f, null!));
    }

    [Fact]
    public void Item_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Item("id", "Name", 1f, ""));
    }

    [Fact]
    public void Item_Constructor_Throws_When_TagName_Is_Whitespace()
    {
        Assert.Throws<ArgumentException>(() =>
            new Item("id", "Name", 1f, "   "));
    }

    [Fact]
    public void Item_Constructor_Sets_TagName_When_Valid()
    {
        var item = new Item("id", "Name", 1f, "iron-sword");
        Assert.Equal("iron-sword", item.TagName);
    }

    [Fact]
    public void Weapon_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Weapon("id", "Sword", 2f, "", Pos, attackBonus: 5));
    }

    [Fact]
    public void HealingPotion_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new HealingPotion("id", "Potion", 0.5f, "", Pos));
    }

    [Fact]
    public void Armor_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Armor("id", "Vest", 3f, "", Pos, defenseBonus: 2));
    }
}
