import * as signalR from '@microsoft/signalr';
import { MainScene } from './MainScene';
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

        // --- Eventos do SignalR ---
        this.connection.on("Joined", (playerData: any) => {
            this.mainScene?.loadMap();
            addLog(`Entraste como ${playerData.name}!`);
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

        this.connection.on("PlayerLeft", (playerId: number) => {
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

        this.connection.on("PlayerDied", (playerId: number) => {
            const player = this.mainScene?.getPlayer(playerId);
            const playerName = player ? player.name : `Jogador ${playerId}`;

            this.mainScene?.playerDied(playerId);
            addLog(`${playerName} morreu!`, "error");

            if (this.mainScene?.myId === playerId.toString()) {
                updateUIHealthBar(0, 100);
            }
        });

        // --- Eventos de Monstros ---
        this.connection.on("SyncMonsters", (monsterList: MonsterData[]) => {
            this.mainScene?.syncMonsters(monsterList);
            addLog(`Sincronizados ${monsterList.length} monstros no mundo.`, "info");
        });

        this.connection.on("MonsterSpawned", (monsterData: MonsterData) => {
            this.mainScene?.monsterSpawned(monsterData);
            addLog(`Monstro ${monsterData.name} apareceu!`, "info");
        });

        this.connection.on("MonsterMoved", (monsterData: MonsterData) => {
            this.mainScene?.monsterMoved(monsterData);
        });

        this.connection.on("MonsterDied", (monsterId: number) => {
            this.mainScene?.monsterDied(monsterId);
            addLog(`Monstro ID ${monsterId} foi derrotado!`, "success");
        });

        this.connection.on("MonsterDamaged", (data: { monsterId: number; damage: number; currentHp: number }) => {
            this.mainScene?.monsterDamaged(data);
            addLog(`Monstro ID ${data.monsterId} sofreu ${data.damage} de dano!`, "info");
        });
    }

    public invoke(methodName: string, ...args: any[]): Promise<any> {
        return this.connection.invoke(methodName, ...args);
    }
}
