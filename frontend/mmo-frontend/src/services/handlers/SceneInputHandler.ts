import type { HubConnection } from '@microsoft/signalr';
import type { MainScene } from '../../scenes/MainScene';

export class SceneInputHandler {
    private connection: HubConnection;
    private scene: MainScene;

    constructor(connection: HubConnection, scene: MainScene) {
        this.connection = connection;
        this.scene = scene;
    }

    register(): void {
        this.scene.onRequestMove = (direction: string) => {
            this.connection.invoke("RequestMove", direction).catch(console.error);
        };
        this.scene.onAttackMonster = (targetId: string) => {
            this.connection.invoke("RequestAttack", targetId).catch(console.error);
        };
        this.scene.onRequestUseItem = (itemId: string) => {
            this.connection.invoke("RequestUseItem", itemId).catch(console.error);
        };
        this.scene.onRequestDropItem = (itemId: string, x: number, y: number) => {
            this.connection.invoke("RequestDropItem", itemId, x, y).catch(console.error);
        };
        this.scene.onRequestEquipItem = (itemId: string) => {
            this.connection.invoke("RequestEquipItem", itemId).catch(console.error);
        };
        this.scene.onRequestUnequipItem = (slot: string) => {
            this.connection.invoke("RequestUnequipItem", slot).catch(console.error);
        };
        this.scene.onRequestMoveItemInInventory = (itemId: string, toIndex: number) => {
            this.connection.invoke("RequestMoveItemInInventory", itemId, toIndex).catch(console.error);
        };
    }
}
