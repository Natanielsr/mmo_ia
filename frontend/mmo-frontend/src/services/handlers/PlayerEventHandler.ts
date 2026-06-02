import type { HubConnection } from '@microsoft/signalr';
import type { MainScene } from '../../scenes/MainScene';
import type { PlayerData, AttackData, RankingEntryData } from '../../types';
import { gameContainer } from '../../ui';
import type { useGameStore } from '../../stores/gameStore';

type Store = ReturnType<typeof useGameStore>;

export class PlayerEventHandler {
    private connection: HubConnection;
    private scene: MainScene;
    private game: Phaser.Game;
    private store: Store;

    constructor(connection: HubConnection, scene: MainScene, game: Phaser.Game, store: Store) {
        this.connection = connection;
        this.scene = scene;
        this.game = game;
        this.store = store;
    }

    register(): void {
        this.connection.on("Joined", (playerData: any) => {
            this.store.setLoggedIn(playerData.name);
            this.store.addLog(`Entrou como ${playerData.name}!`);
            gameContainer?.classList.remove('hidden');

            const container = document.getElementById('phaser-game');
            if (container) {
                this.game.scale.resize(container.clientWidth, container.clientHeight);
            }

            this.scene.updatePlayerPosition(playerData, true);
            this.scene.openDefaultPanels();
        });

        this.connection.on("PlayerHpChanged", (hpData: any) => {
            this.scene.updatePlayerStatus(hpData);
            const myPlayer = this.scene.getMyPlayer();
            if (myPlayer && String(hpData.id ?? hpData.Id) === String(myPlayer.id)) {
                this.store.updateCharacterStatus({
                    hp: hpData.hp ?? hpData.Hp,
                    maxHp: hpData.maxHp ?? hpData.MaxHp,
                });
            }
        });

        this.connection.on("PlayerHealed", (hpData: any) => {
            const id = String(hpData.id ?? hpData.Id);
            this.scene.getPlayer(id)?.playHealEffect();
        });

        this.connection.on("HealingConsumed", (data: any) => {
            const id = String(data.playerId ?? data.PlayerId);
            this.scene.getPlayer(id)?.playCrunchEffect();
        });

        this.connection.on("PlayerStatusUpdated", (statusData: any) => {
            this.scene.updatePlayerStatus(statusData);
            const myPlayer = this.scene.getMyPlayer();
            if (myPlayer && String(statusData.id ?? statusData.Id) === String(myPlayer.id)) {
                this.store.updateCharacterStatus({
                    hp: statusData.hp ?? statusData.Hp,
                    maxHp: statusData.maxHp ?? statusData.MaxHp,
                    level: statusData.level ?? statusData.Level,
                    experience: statusData.experience ?? statusData.Experience,
                    attackPower: statusData.attackPower ?? statusData.AttackPower,
                    defense: statusData.defense ?? statusData.Defense,
                });
            }
        });

        this.connection.on("SyncPlayers", (playerList: any[]) => {
            playerList.forEach(p => {
                if (p.position && (p.position.x !== undefined || p.position.X !== undefined)) {
                    this.scene.updatePlayerPosition(p);
                }
            });
        });

        this.connection.on("SyncPlayerStatuses", (statusList: any[]) => {
            statusList.forEach(s => {
                this.scene.updatePlayerStatus(s);
                const myPlayer = this.scene.getMyPlayer();
                if (myPlayer && String(s.id ?? s.Id) === String(myPlayer.id)) {
                    this.store.updateCharacterStatus({
                        hp: s.hp ?? s.Hp,
                        maxHp: s.maxHp ?? s.MaxHp,
                        level: s.level ?? s.Level,
                        experience: s.experience ?? s.Experience,
                        attackPower: s.attackPower ?? s.AttackPower,
                        defense: s.defense ?? s.Defense,
                    });
                }
            });
        });

        this.connection.on("PlayerJoined", (playerPosData: PlayerData) => {
            this.store.addLog(`${playerPosData.name} entrou no mundo!`);
            if (playerPosData.position.x !== undefined && playerPosData.position.y !== undefined) {
                this.scene.updatePlayerPosition(playerPosData);
            }
        });

        this.connection.on("PlayerMoved", (playerData: any) => {
            this.scene.updatePlayerPosition(playerData);
        });

        this.connection.on("PlayerLeft", (playerId: string) => {
            this.store.addLog(`${playerId} saiu do mundo.`);
            this.scene.removePlayer(playerId);
        });

        this.connection.on("PlayerAttacked", (data: any) => {
            const attackData: AttackData = {
                attackerId: String(data.attackerId ?? data.AttackerId),
                attackerName: String(data.attackerName ?? data.AttackerName),
                targetId: String(data.targetId ?? data.TargetId),
                targetName: String(data.targetName ?? data.TargetName),
                damage: Number(data.damage ?? data.Damage)
            };

            this.scene.playerAttacked(attackData);
            this.store.addLog(`${attackData.attackerName} atacou ${attackData.targetName} causando ${attackData.damage} de dano!`);

            const myPlayer = this.scene.getMyPlayer();
            if (myPlayer && attackData.targetId === String(myPlayer.id)) {
                this.store.updateCharacterStatus({ hp: myPlayer.hp, maxHp: myPlayer.maxHp });
            }
        });

        this.connection.on("PlayerDied", (playerId: string) => {
            const player = this.scene.getPlayer(playerId);
            const playerName = player ? player.name : `Jogador ${playerId}`;

            this.scene.playerDied(playerId);
            this.store.addLog(`${playerName} morreu!`, true);

            if (this.scene.getMyPlayer()?.id === playerId.toString()) {
                this.store.updateCharacterStatus({ hp: 0, isDead: true });
            }
        });

        this.connection.on("PlayerItemEquipped", (data: { playerId: string; slot: string; itemName: string | null }) => {
            const playerId = data.playerId ?? (data as any).PlayerId;
            const slot     = data.slot     ?? (data as any).Slot;
            const itemName = data.itemName ?? (data as any).ItemName ?? null;
            this.scene.playerItemEquipped(playerId, slot, itemName);
        });

        this.connection.on("RankingUpdated", (entries: RankingEntryData[]) => {
            this.store.updateRanking(entries);
        });
    }
}
