namespace GameServerApp.Dtos;

public record MonsterDefinition(
    int Id,
    string TagName,
    string Name,
    int Hp,
    int Attack,
    int Xp);
