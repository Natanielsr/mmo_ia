using GameServerApp.Services.Combat;
using GameServerApp.Services.Items;
using GameServerApp.Services.Movement;
using GameServerApp.Services.Ranking;
using GameServerApp.Services.Repositories;
using GameServerApp.Services.World;
using GameServerApp.Services.World.WorldFormations;

namespace GameServer.Tests.Items
{
    public class ItemDefinitionRepositoryTests
    {
        private const string TestJson = """
            {
              "items": [
                {
                  "id": 1,
                  "tagName": "potion",
                  "name": "Healing Potion",
                  "description": "Restaura 20 de HP",
                  "value": 10,
                  "type": "Potion",
                  "weight": 0.5,
                  "healAmount": 20
                },
                {
                  "id": 2,
                  "tagName": "dagger",
                  "name": "Dagger",
                  "description": "Uma lâmina afiada",
                  "value": 25,
                  "type": "Weapon",
                  "weight": 1.5,
                  "attackBonus": 5
                }
              ]
            }
            """;

        private readonly ItemDefinitionRepository _sut = new(TestJson);

        [Fact]
        public void GetById_Returns_CorrectDefinition()
        {
            var def = _sut.GetById(1);

            Assert.NotNull(def);
            Assert.Equal("potion", def.TagName);
            Assert.Equal("Healing Potion", def.Name);
            Assert.Equal(10, def.Value);
            Assert.Equal("Potion", def.Type);
        }

        [Fact]
        public void GetById_Returns_Null_For_Unknown_Id()
        {
            var def = _sut.GetById(999);
            Assert.Null(def);
        }

        [Fact]
        public void GetByTagName_Returns_CorrectDefinition()
        {
            var def = _sut.GetByTagName("dagger");

            Assert.NotNull(def);
            Assert.Equal(2, def.Id);
            Assert.Equal(5, def.AttackBonus);
            Assert.Null(def.DefenseBonus);
        }

        [Fact]
        public void GetByTagName_Returns_Null_For_Unknown_TagName()
        {
            var def = _sut.GetByTagName("nonexistent");
            Assert.Null(def);
        }

        [Fact]
        public void GetAll_Returns_All_Definitions()
        {
            var all = _sut.GetAll();
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public void Constructor_Throws_On_Invalid_Json()
        {
            Assert.ThrowsAny<Exception>(() => new ItemDefinitionRepository("invalid json"));
        }
    }
}
