using Microsoft.AspNetCore.SignalR;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServer.Infrastructure.SignalR;

public partial class SignalREventEmitter
{
    public void OnMonsterSpawned(MonsterData monsterData)
    {
        _hubContext?.Clients.All.SendAsync("MonsterSpawned", monsterData);
    }

    public void OnMonsterMoved(MonsterData monsterData)
    {
        _hubContext?.Clients.All.SendAsync("MonsterMoved", monsterData);
    }

    public void OnMonsterDied(string monsterId)
    {
        _hubContext?.Clients.All.SendAsync("MonsterDied", monsterId);
    }

    public void OnMonsterDamaged(string monsterId, int damage, int currentHp)
    {
        _hubContext?.Clients.All.SendAsync("MonsterDamaged", new
        {
            MonsterId = monsterId,
            Damage = damage,
            CurrentHp = currentHp
        });
    }
}