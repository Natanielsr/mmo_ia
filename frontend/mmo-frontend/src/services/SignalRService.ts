import * as signalR from '@microsoft/signalr';
import { MainScene } from '../scenes/MainScene';
import { useGameStore } from '../stores/gameStore';
import { SceneInputHandler } from './handlers/SceneInputHandler';
import { PlayerEventHandler } from './handlers/PlayerEventHandler';
import { MonsterEventHandler } from './handlers/MonsterEventHandler';
import { ItemEventHandler } from './handlers/ItemEventHandler';
import { WorldEventHandler } from './handlers/WorldEventHandler';

export class SignalRService {
    private connection: signalR.HubConnection;

    constructor(url: string = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/gamehub` : "http://localhost:5258/gamehub") {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(url)
            .withAutomaticReconnect()
            .build();
    }

    public async start(): Promise<void> {
        const store = useGameStore();
        store.setServerMessage("A estabelecer ligação...", true);

        const wakeUpTimer = setTimeout(() => {
            store.setServerMessage("Conectando ao servidor, por favor aguarde...", true);
        }, 3000);

        try {
            await this.connection.start();
            clearTimeout(wakeUpTimer);
            console.log("Conectado ao SignalR!");
            store.setConnectionError(false);
            store.setServerMessage("Ligado!", false);
        } catch (err) {
            clearTimeout(wakeUpTimer);
            store.addLog("Não foi possível conectar ao servidor. A tentar novamente...", true);
            store.setConnectionError(true);
            store.setServerMessage("Erro de ligação. A tentar novamente...", true);

            setTimeout(() => this.start(), 5000);
        }
    }

    public registerEvents(mainScene: MainScene, game: Phaser.Game): void {
        const store = useGameStore();
        const conn = this.connection;

        new SceneInputHandler(conn, mainScene).register();
        new PlayerEventHandler(conn, mainScene, game, store).register();
        new MonsterEventHandler(conn, mainScene, store).register();
        new ItemEventHandler(conn, mainScene, store).register();
        new WorldEventHandler(conn, mainScene).register();
    }

    public invoke(methodName: string, ...args: any[]): Promise<any> {
        return this.connection.invoke(methodName, ...args);
    }
}
