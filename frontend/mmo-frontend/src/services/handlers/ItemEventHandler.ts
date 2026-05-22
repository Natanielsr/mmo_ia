import type { HubConnection } from '@microsoft/signalr';
import type { MainScene } from '../../scenes/MainScene';
import type { useGameStore } from '../../stores/gameStore';

type Store = ReturnType<typeof useGameStore>;

export class ItemEventHandler {
    private connection: HubConnection;
    private scene: MainScene;
    private store: Store;

    constructor(connection: HubConnection, scene: MainScene, store: Store) {
        this.connection = connection;
        this.scene = scene;
        this.store = store;
    }

    register(): void {
        this.connection.on("SyncItems", (itemList: any[]) => {
            itemList.forEach(itemData => {
                this.scene.itemDropped(this.normalizeItem(itemData));
            });
        });

        this.connection.on("ItemDropped", (itemData: any) => {
            this.scene.itemDropped(this.normalizeItem(itemData));
        });

        this.connection.on("ItemPickedUp", (data: any) => {
            const normalizedData = {
                itemId: String(data.itemId ?? data.ItemId),
                playerId: String(data.playerId ?? data.PlayerId)
            };
            this.scene.itemPickedUp(normalizedData);

            const player = this.scene.getPlayer(normalizedData.playerId);
            if (player) {
                this.store.addLog(`${player.name} pegou um item!`);
            }
        });

        this.connection.on("InventoryUpdated", (items: any[]) => {
            const normalized = items.map(item => ({
                id: String(item.id ?? item.Id),
                name: String(item.name ?? item.Name),
                position: item.position ?? item.Position ?? { x: 0, y: 0 },
                type: String(item.type ?? item.Type),
                attackBonus: item.attackBonus ?? item.AttackBonus,
                defenseBonus: item.defenseBonus ?? item.DefenseBonus,
                slotIndex: item.slotIndex ?? item.SlotIndex,
                quantity: item.quantity ?? item.Quantity ?? 1,
            }));
            this.scene.inventoryUpdated(normalized);
        });

        this.connection.on("EquipmentUpdated", (slotData: any) => {
            const slots: Record<string, any> = {};
            for (const key of Object.keys(slotData)) {
                const item = slotData[key];
                slots[key] = item == null ? null : {
                    id: String(item.id ?? item.Id),
                    name: String(item.name ?? item.Name),
                    position: item.position ?? item.Position ?? { x: 0, y: 0 },
                    type: String(item.type ?? item.Type),
                    attackBonus: item.attackBonus ?? item.AttackBonus,
                    defenseBonus: item.defenseBonus ?? item.DefenseBonus,
                };
            }
            this.scene.equipmentUpdated(slots as any);
        });
    }

    private normalizeItem(itemData: any) {
        return {
            id: String(itemData.id ?? itemData.Id),
            name: String(itemData.name ?? itemData.Name),
            position: itemData.position ?? itemData.Position,
            type: String(itemData.type ?? itemData.Type)
        };
    }
}
