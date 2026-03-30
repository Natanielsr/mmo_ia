import { HealingPotion } from '../entities/HealingPotion';
import type { ItemData } from '../types';

export class ItemManager {
    private scene: Phaser.Scene;
    private items: Map<string, HealingPotion> = new Map();

    constructor(scene: Phaser.Scene) {
        this.scene = scene;
    }

    public syncItem(itemData: ItemData): void {
        if (this.items.has(itemData.id)) {
            // Already exists, maybe update position if needed (items usually don't move)
            return;
        }

        if (itemData.type === 'Potion') {
            const potion = new HealingPotion(itemData, this.scene);
            this.items.set(itemData.id, potion);
        }
    }

    public removeItem(itemId: string): void {
        const item = this.items.get(itemId);
        if (item) {
            item.collect(); // Trigger animation
            this.items.delete(itemId);
        }
    }

    public clear(): void {
        this.items.forEach(item => item.destroy());
        this.items.clear();
    }
}
