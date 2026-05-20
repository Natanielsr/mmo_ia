import type { ItemData } from '../types';

const MAX_SLOTS = 20;

export class InventoryManager {
    private items: ItemData[] = [];

    public onUseItem?: (itemId: string) => void;
    public onDropItem?: (itemId: string, x: number, y: number) => void;
    public onEquipItem?: (itemId: string) => void;

    public addItem(item: ItemData): void {
        if (this.items.length >= MAX_SLOTS) return;
        this.items.push(item);
    }

    public removeItem(itemId: string): void {
        this.items = this.items.filter(i => i.id !== itemId);
    }

    public getItems(): ItemData[] {
        return [...this.items];
    }

    public useItem(itemId: string): void {
        const item = this.items.find(i => i.id === itemId);
        if (item && (item.type === 'Weapon' || item.type === 'Armor')) {
            this.onEquipItem?.(itemId);
        } else {
            this.onUseItem?.(itemId);
        }
    }

    public dropItem(itemId: string, x: number, y: number): void {
        this.onDropItem?.(itemId, x, y);
    }

    public onInventoryUpdated(items: ItemData[]): void {
        this.items = [...items];
    }

    public clear(): void {
        this.items = [];
    }
}
