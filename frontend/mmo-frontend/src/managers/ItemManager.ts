import { HealingPotion } from '../entities/items/HealingPotion';
import { WorldCheese } from '../entities/items/WorldCheese';
import { WorldWeapon } from '../entities/items/WorldWeapon';
import { WorldArmor } from '../entities/items/WorldArmor';
import { WorldHelmet } from '../entities/items/WorldHelmet';
import { WorldShield } from '../entities/items/WorldShield';
import { WorldLegs } from '../entities/items/WorldLegs';
import { WorldBoots } from '../entities/items/WorldBoots';
import type { IWorldItem } from '../entities/items/IWorldItem';
import type { ItemData } from '../types';

export class ItemManager {
    private scene: Phaser.Scene;
    private items: Map<string, IWorldItem> = new Map();

    constructor(scene: Phaser.Scene) {
        this.scene = scene;
    }

    public syncItem(itemData: ItemData): void {
        if (this.items.has(itemData.id)) return;

        let item: IWorldItem | null = null;

        if (itemData.type === 'Potion') {
            item = new HealingPotion(itemData, this.scene);
        } else if (itemData.type === 'Food') {
            item = new WorldCheese(itemData, this.scene);
        } else if (itemData.type === 'Weapon') {
            item = new WorldWeapon(itemData, this.scene);
        } else if (itemData.type === 'Armor') {
            item = new WorldArmor(itemData, this.scene);
        } else if (itemData.type === 'Helmet') {
            item = new WorldHelmet(itemData, this.scene);
        } else if (itemData.type === 'Shield') {
            item = new WorldShield(itemData, this.scene);
        } else if (itemData.type === 'Legs') {
            item = new WorldLegs(itemData, this.scene);
        } else if (itemData.type === 'Boots') {
            item = new WorldBoots(itemData, this.scene);
        }

        if (item) {
            this.items.set(itemData.id, item);
        }
    }

    public removeItem(itemId: string): void {
        const item = this.items.get(itemId);
        if (item) {
            item.collect();
            this.items.delete(itemId);
        }
    }

    public clear(): void {
        this.items.forEach(item => item.destroy());
        this.items.clear();
    }

    public getItemsDict(): Map<string, IWorldItem> {
        return this.items;
    }
}
