import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ItemEventHandler } from '../../services/handlers/ItemEventHandler';

function makeConnection() {
    const captured: Record<string, (...args: any[]) => void> = {};
    const connection = {
        on: vi.fn().mockImplementation((event: string, cb: (...args: any[]) => void) => {
            captured[event] = cb;
        }),
    };
    const emit = (event: string, ...args: any[]) => captured[event]?.(...args);
    return { connection, emit };
}

function makeScene() {
    return {
        itemDropped:     vi.fn(),
        itemPickedUp:    vi.fn(),
        inventoryUpdated: vi.fn(),
        equipmentUpdated: vi.fn(),
        getPlayer:       vi.fn().mockReturnValue(null),
    };
}

function makeStore() {
    return { addLog: vi.fn() };
}

const ALL_CATALOG_ITEMS = [
    { tagName: 'potion',        type: 'Potion',  quantity: 3 },
    { tagName: 'dagger',        type: 'Weapon',  attackBonus: 5 },
    { tagName: 'leather-vest',  type: 'Armor',   defenseBonus: 1 },
    { tagName: 'plate-armor',   type: 'Armor',   defenseBonus: 8 },
    { tagName: 'iron-helmet',   type: 'Helmet',  defenseBonus: 1 },
    { tagName: 'wooden-shield', type: 'Shield',  defenseBonus: 1 },
    { tagName: 'leather-pants', type: 'Legs',    defenseBonus: 1 },
    { tagName: 'iron-boots',    type: 'Boots',   defenseBonus: 1 },
];

describe('ItemEventHandler — normalizeItem', () => {
    let emit: (event: string, ...args: any[]) => void;
    let scene: ReturnType<typeof makeScene>;
    let handler: ItemEventHandler;

    beforeEach(() => {
        const conn = makeConnection();
        emit = conn.emit;
        scene = makeScene();
        handler = new ItemEventHandler(conn.connection as any, scene as any, makeStore() as any);
        handler.register();
    });

    describe('ItemDropped — payload PascalCase (formato do backend)', () => {
        it.each(ALL_CATALOG_ITEMS)(
            'preserva tagName "$tagName" (type=$type)',
            ({ tagName, type, ...extras }) => {
                emit('ItemDropped', {
                    Id: 'x', Name: 'Item', TagName: tagName,
                    Position: { x: 1, y: 1 }, Type: type, ...extras,
                });
                expect(scene.itemDropped).toHaveBeenCalledWith(
                    expect.objectContaining({ tagName }),
                );
            },
        );
    });

    describe('ItemDropped — payload camelCase', () => {
        it.each(ALL_CATALOG_ITEMS)(
            'preserva tagName "$tagName" via camelCase',
            ({ tagName, type, ...extras }) => {
                emit('ItemDropped', {
                    id: 'x', name: 'Item', tagName,
                    position: { x: 1, y: 1 }, type, ...extras,
                });
                expect(scene.itemDropped).toHaveBeenCalledWith(
                    expect.objectContaining({ tagName }),
                );
            },
        );
    });

    describe('SyncItems — todos os tipos do catálogo', () => {
        it('preserva tagName para todos os itens da lista', () => {
            const items = ALL_CATALOG_ITEMS.map((item, i) => ({
                Id: `id-${i}`, Name: 'X',
                TagName: item.tagName,
                Position: { x: i, y: 0 },
                Type: item.type,
            }));

            emit('SyncItems', items);

            expect(scene.itemDropped).toHaveBeenCalledTimes(ALL_CATALOG_ITEMS.length);
            ALL_CATALOG_ITEMS.forEach(({ tagName }) => {
                expect(scene.itemDropped).toHaveBeenCalledWith(
                    expect.objectContaining({ tagName }),
                );
            });
        });
    });

    describe('normalizeItem — campos extras', () => {
        it('mapeia attackBonus corretamente', () => {
            emit('ItemDropped', {
                TagName: 'dagger', Type: 'Weapon',
                Id: 'w1', Name: 'Dagger', Position: { x: 0, y: 0 },
                AttackBonus: 10,
            });
            expect(scene.itemDropped).toHaveBeenCalledWith(
                expect.objectContaining({ attackBonus: 10 }),
            );
        });

        it('mapeia defenseBonus corretamente', () => {
            emit('ItemDropped', {
                TagName: 'plate-armor', Type: 'Armor',
                Id: 'a1', Name: 'Plate Armor', Position: { x: 0, y: 0 },
                DefenseBonus: 8,
            });
            expect(scene.itemDropped).toHaveBeenCalledWith(
                expect.objectContaining({ defenseBonus: 8 }),
            );
        });

        it('quantity padrão é 1 quando ausente', () => {
            emit('ItemDropped', {
                TagName: 'dagger', Type: 'Weapon',
                Id: 'w1', Name: 'Dagger', Position: { x: 0, y: 0 },
            });
            expect(scene.itemDropped).toHaveBeenCalledWith(
                expect.objectContaining({ quantity: 1 }),
            );
        });

        it('tagName vazio quando ausente no payload', () => {
            emit('ItemDropped', {
                Id: 'x', Name: 'X', Position: { x: 0, y: 0 }, Type: 'Weapon',
            });
            expect(scene.itemDropped).toHaveBeenCalledWith(
                expect.objectContaining({ tagName: '' }),
            );
        });
    });
});
