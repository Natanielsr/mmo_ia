import { describe, it, expect, beforeEach, vi } from 'vitest';

const { mockJustDown } = vi.hoisted(() => ({
    mockJustDown: vi.fn(() => false),
}));

vi.mock('phaser', () => ({
    default: {
        Input: {
            Keyboard: {
                JustDown: mockJustDown,
                KeyCodes: {
                    W: 87, A: 65, S: 83, D: 68,
                    F3: 114, M: 77, I: 73, E: 69,
                },
            },
        },
        Scene: class {},
    },
}));

import { InputManager } from '../../managers/InputManager';

function makeScene() {
    const mkKey = () => ({ isDown: false });
    return {
        input: {
            keyboard: {
                enabled: true,
                removeCapture: vi.fn(),
                createCursorKeys: vi.fn().mockReturnValue({
                    up: mkKey(), down: mkKey(), left: mkKey(), right: mkKey(), space: mkKey(),
                }),
                addKeys: vi.fn().mockReturnValue({
                    up: mkKey(), down: mkKey(), left: mkKey(), right: mkKey(),
                }),
                addKey: vi.fn().mockImplementation(() => mkKey()),
            },
        },
    };
}

describe('InputManager — teclas de inventário e equipamento', () => {
    let manager: InputManager;

    beforeEach(() => {
        mockJustDown.mockReset();
        mockJustDown.mockReturnValue(false);
        manager = new InputManager(makeScene() as any);
        manager.setup();
    });

    it('isInventoryJustPressed — retorna true quando tecla I recém pressionada', () => {
        mockJustDown.mockReturnValue(true);
        expect(manager.isInventoryJustPressed()).toBe(true);
    });

    it('isEquipmentJustPressed — retorna true quando tecla E recém pressionada', () => {
        mockJustDown.mockReturnValue(true);
        expect(manager.isEquipmentJustPressed()).toBe(true);
    });

    it('isInventoryJustPressed — retorna false quando tecla não pressionada', () => {
        expect(manager.isInventoryJustPressed()).toBe(false);
    });

    it('isEquipmentJustPressed — retorna false quando tecla não pressionada', () => {
        expect(manager.isEquipmentJustPressed()).toBe(false);
    });
});
