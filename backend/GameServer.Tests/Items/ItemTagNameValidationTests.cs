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

    [Fact]
    public void Helmet_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Helmet("id", "Helmet", 1.2f, "", Pos, defenseBonus: 1));
    }

    [Fact]
    public void Shield_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Shield("id", "Shield", 2.5f, "", Pos, defenseBonus: 1));
    }

    [Fact]
    public void Legs_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Legs("id", "Pants", 1.0f, "", Pos, defenseBonus: 1));
    }

    [Fact]
    public void Boots_Constructor_Throws_When_TagName_Is_Empty()
    {
        Assert.Throws<ArgumentException>(() =>
            new Boots("id", "Boots", 1.5f, "", Pos, defenseBonus: 1));
    }

    [Fact]
    public void All_Catalog_Item_Types_Preserve_TagName()
    {
        Assert.Equal("potion",        new HealingPotion("id", "HP",           0.5f, "potion",        Pos).TagName);
        Assert.Equal("dagger",        new Weapon       ("id", "Dagger",       1.5f, "dagger",        Pos, attackBonus: 5).TagName);
        Assert.Equal("leather-vest",  new Armor        ("id", "Leather Vest", 2.0f, "leather-vest",  Pos, defenseBonus: 1).TagName);
        Assert.Equal("plate-armor",   new Armor        ("id", "Plate Armor",  5.0f, "plate-armor",   Pos, defenseBonus: 8).TagName);
        Assert.Equal("iron-helmet",   new Helmet       ("id", "Iron Helmet",  1.2f, "iron-helmet",   Pos, defenseBonus: 1).TagName);
        Assert.Equal("wooden-shield", new Shield       ("id", "Wooden Shield",2.5f, "wooden-shield", Pos, defenseBonus: 1).TagName);
        Assert.Equal("leather-pants", new Legs         ("id", "Leather Pants",1.0f, "leather-pants", Pos, defenseBonus: 1).TagName);
        Assert.Equal("iron-boots",    new Boots        ("id", "Iron Boots",   1.5f, "iron-boots",    Pos, defenseBonus: 1).TagName);
    }
}
