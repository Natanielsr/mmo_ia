import * as signalR from '@microsoft/signalr';
import { MainScene } from '../scenes/MainScene';
import type { PlayerPosData, AttackData, MonsterData } from '../types';
import { addLog, updateUIHealthBar, overlay, gameContainer, errorBanner, btnJoin } from '../ui';

export class SignalRService {
    private connection: signalR.HubConnection;
    private mainScene: MainScene | null = null;
    private game: Phaser.Game | null = null;

    constructor(url: string = "http://localhost:5258/gamehub") {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(url)
            .withAutomaticReconnect()
            .build();
    }

    public async start(): Promise<void> {
        try {
            if (btnJoin) btnJoin.disabled = true;
            await this.connection.start();
            console.log("Conectado ao SignalR!");
            if (errorBanner) errorBanner.classList.add('hidden');
            if (btnJoin) btnJoin.disabled = false;
        } catch (err) {
            addLog("Não foi possível conectar ao servidor. A tentar novamente...", "error");
            if (errorBanner) errorBanner.classList.remove('hidden');
            setTimeout(() => this.start(), 5000);
        }
    }

    public registerEvents(mainScene: MainScene, game: Phaser.Game): void {
        this.mainScene = mainScene;
        this.game = game;

        // Conecta o evento de input do Phaser ao envio do SignalR
        this.mainScene.onRequestMove = (direction: string) => {
            this.invoke("RequestMove", direction).catch(console.error);
        };
        this.mainScene.onAttackMonster = (targetId: string) => {
            this.attackMonster(targetId).catch(console.error);
        };

        // --- Eventos do SignalR ---
        this.connection.on("Joined", (playerData: any) => {
            addLog(`Entrou como ${playerData.name}!`);
            overlay?.classList.add('hidden');
            gameContainer?.classList.remove('hidden');

            // Força o redimensionamento do canvas
            const container = document.getElementById('phaser-game');
            if (container && this.game) {
                this.game.scale.resize(container.clientWidth, container.clientHeight);
            }

            this.mainScene?.updatePlayerPosition(playerData, true);
        });

        this.connection.on("PlayerStatusUpdated", (statusData: any) => {
            this.mainScene?.updatePlayerStatus(statusData);
            const myPlayer = this.mainScene?.getMyPlayer();
            if (myPlayer && String(statusData.id ?? statusData.Id) === String(myPlayer.id)) {
                updateUIHealthBar(statusData.hp ?? statusData.Hp, statusData.maxHp ?? statusData.MaxHp);
            }
        });

        this.connection.on("SyncPlayers", (playerList: any[]) => {
            playerList.forEach(p => {
                if (p.position && (p.position.x !== undefined || p.position.X !== undefined)) {
                    this.mainScene?.updatePlayerPosition(p);
                }
            });
        });

        this.connection.on("SyncPlayerStatuses", (statusList: any[]) => {
            statusList.forEach(s => {
                this.mainScene?.updatePlayerStatus(s);
                const myPlayer = this.mainScene?.getMyPlayer();
                if (myPlayer && String(s.id ?? s.Id) === String(myPlayer.id)) {
                    updateUIHealthBar(s.hp ?? s.Hp, s.maxHp ?? s.MaxHp);
                }
            });
        });

        this.connection.on("PlayerJoined", (playerPosData: PlayerPosData) => {
            addLog(`${playerPosData.name} entrou no mundo!`);
            if (playerPosData.position.x !== undefined && playerPosData.position.y !== undefined) {
                this.mainScene?.updatePlayerPosition(playerPosData);
            }
        });

        this.connection.on("PlayerMoved", (playerData: any) => {
            this.mainScene?.updatePlayerPosition(playerData);
        });

        this.connection.on("PlayerLeft", (playerId: string) => {
            addLog(`${playerId} saiu do mundo.`);
            this.mainScene?.removePlayer(playerId);
        });

        this.connection.on("PlayerAttacked", (data: any) => {
            const attackData: AttackData = {
                attackerId: String(data.attackerId ?? data.AttackerId),
                attackerName: String(data.attackerName ?? data.AttackerName),
                targetId: String(data.targetId ?? data.TargetId),
                targetName: String(data.targetName ?? data.TargetName),
                damage: Number(data.damage ?? data.Damage)
            };

            this.mainScene?.playerAttacked(attackData);
            addLog(`${attackData.attackerName} atacou ${attackData.targetName} causando ${attackData.damage} de dano!`);

            const myPlayer = this.mainScene?.getMyPlayer();
            if (myPlayer) {
                if (attackData.targetId === String(myPlayer.id)) {
                    updateUIHealthBar(myPlayer.hp, myPlayer.maxHp);
                }
            }
        });

        this.connection.on("PlayerDied", (playerId: string) => {
            const player = this.mainScene?.getPlayer(playerId);
            const playerName = player ? player.name : `Jogador ${playerId}`;

            this.mainScene?.playerDied(playerId);
            addLog(`${playerName} morreu!`, "error");

            if (this.mainScene?.getMyPlayer()?.id === playerId.toString()) {
                updateUIHealthBar(0, 100);
            }
        });

        // --- Eventos de Monstros ---
        this.connection.on("SyncMonsters", (monsterList: MonsterData[]) => {
            this.mainScene?.syncMonsters(monsterList);
        });

        this.connection.on("MonsterSpawned", (monsterData: MonsterData) => {
            this.mainScene?.monsterSpawned(monsterData);
            addLog(`Monstro ${monsterData.name} apareceu!`, "info");
        });

        this.connection.on("MonsterMoved", (monsterData: MonsterData) => {
            this.mainScene?.monsterMoved(monsterData);
        });

        this.connection.on("MonsterDied", (monsterId: string) => {
            const idStr = String(monsterId);
            this.mainScene?.monsterDied(idStr);
            const monster = this.mainScene?.getMonster(monsterId);
            addLog(`${monster?.name} foi derrotado!`, "success");
        });

        this.connection.on("MonsterRemoved", (monsterId: string) => {
            const idStr = String(monsterId);
            this.mainScene?.monsterRemoved(idStr);
        });

        this.connection.on("MonsterDamaged", (data: any) => {
            const normalizedData = {
                id: String(data.id ?? data.Id ?? data.monsterId),
                damage: Number(data.damage ?? data.Damage),
                currentHp: Number(data.currentHp ?? data.CurrentHp)
            };
            this.mainScene?.monsterDamaged(normalizedData);
        });

        this.connection.on("SyncItems", (itemList: any[]) => {
            itemList.forEach(itemData => {
                const normalizedItem = {
                    id: String(itemData.id ?? itemData.Id),
                    name: String(itemData.name ?? itemData.Name),
                    position: itemData.position ?? itemData.Position,
                    type: String(itemData.type ?? itemData.Type)
                };
                this.mainScene?.itemDropped(normalizedItem);
            });
        });

        this.connection.on("ItemDropped", (itemData: any) => {
            const normalizedItem = {
                id: String(itemData.id ?? itemData.Id),
                name: String(itemData.name ?? itemData.Name),
                position: itemData.position ?? itemData.Position,
                type: String(itemData.type ?? itemData.Type)
            };
            this.mainScene?.itemDropped(normalizedItem);
            addLog(`Um item apareceu no mapa: ${normalizedItem.name}!`, "info");
        });

        this.connection.on("ItemPickedUp", (data: any) => {
            const normalizedData = {
                itemId: String(data.itemId ?? data.ItemId),
                playerId: String(data.playerId ?? data.PlayerId)
            };
            this.mainScene?.itemPickedUp(normalizedData);

            const player = this.mainScene?.getPlayer(normalizedData.playerId);
            if (player) {
                addLog(`${player.name} pegou um item!`, "success");
            }
        });

        this.connection.on("ChunkLoaded", (data: any) => {
            const normalizedData = {
                cx: data.cx ?? data.Cx ?? data.CX,
                cy: data.cy ?? data.Cy ?? data.CY,
                objects: (data.objects ?? data.Objects ?? []).map((obj: any) => ({
                    id: String(obj.id ?? obj.Id),
                    name: String(obj.name ?? obj.Name),
                    objectCode: String(obj.objectCode ?? obj.ObjectCode),
                    position: {
                        x: obj.position.x ?? obj.position.X,
                        y: obj.position.y ?? obj.position.Y
                    },
                    type: String(obj.type ?? obj.Type),
                    isPassable: Boolean(obj.isPassable ?? obj.IsPassable)
                }))
            };
            this.mainScene?.chunkLoaded(normalizedData);
        });
    }

    public async attackMonster(targetId: string): Promise<void> {
        await this.invoke("RequestAttack", targetId);
    }

    public invoke(methodName: string, ...args: any[]): Promise<any> {
        return this.connection.invoke(methodName, ...args);
    }
}
