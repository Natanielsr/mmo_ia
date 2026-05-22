import type { HubConnection } from '@microsoft/signalr';
import type { MainScene } from '../../scenes/MainScene';
import type { MonsterData } from '../../types';
import type { useGameStore } from '../../stores/gameStore';

type Store = ReturnType<typeof useGameStore>;

export class MonsterEventHandler {
    private connection: HubConnection;
    private scene: MainScene;
    private store: Store;

    constructor(connection: HubConnection, scene: MainScene, store: Store) {
        this.connection = connection;
        this.scene = scene;
        this.store = store;
    }

    register(): void {
        this.connection.on("SyncMonsters", (monsterList: MonsterData[]) => {
            this.scene.syncMonsters(monsterList);
        });

        this.connection.on("MonsterSpawned", (monsterData: MonsterData) => {
            this.scene.monsterSpawned(monsterData);
        });

        this.connection.on("MonsterMoved", (monsterData: MonsterData) => {
            this.scene.monsterMoved(monsterData);
        });

        this.connection.on("MonsterDied", (monsterId: string) => {
            const monster = this.scene.getMonster(monsterId);
            this.scene.monsterDied(String(monsterId));
            this.store.addLog(`${monster?.name} foi derrotado!`);
        });

        this.connection.on("MonsterRemoved", (monsterId: string) => {
            this.scene.monsterRemoved(String(monsterId));
        });

        this.connection.on("MonsterDamaged", (data: any) => {
            this.scene.monsterDamaged({
                id: String(data.id ?? data.Id ?? data.monsterId),
                damage: Number(data.damage ?? data.Damage),
                currentHp: Number(data.currentHp ?? data.CurrentHp)
            });
        });
    }
}
