import { vi } from 'vitest';

function makeGraphics() {
    return {
        clear: vi.fn(),
        fillStyle: vi.fn(),
        fillRect: vi.fn(),
        lineStyle: vi.fn(),
        strokeRect: vi.fn(),
        setDepth: vi.fn().mockReturnThis(),
        destroy: vi.fn(),
        visible: true,
    };
}

function makeSprite() {
    const s = {
        alpha: 1 as number,
        scaleX: 1,
        scaleY: 1,
        texture: { key: 'mock' } as { key: string },
        setDisplaySize: vi.fn().mockReturnThis(),
        setDepth: vi.fn().mockReturnThis(),
        setTint: vi.fn().mockReturnThis(),
        clearTint: vi.fn().mockReturnThis(),
        setFrame: vi.fn().mockReturnThis(),
        setTexture(key: string, _frame?: number) { s.texture = { key }; return s; },
        setAngle: vi.fn().mockReturnThis(),
        setVisible: vi.fn().mockReturnThis(),
        displayWidth: 80,
        displayHeight: 80,
        setAlpha(a: number) { s.alpha = a; return s; },
        setInteractive: vi.fn().mockReturnThis(),
        on: vi.fn().mockReturnThis(),
        once: vi.fn().mockReturnThis(),
        play: vi.fn().mockReturnThis(),
        stop: vi.fn().mockReturnThis(),
        destroy: vi.fn(),
    };
    return s;
}

function makeText() {
    const t = {
        visible: true as boolean,
        _text: '' as string,
        setOrigin: vi.fn().mockReturnThis(),
        setDepth: vi.fn().mockReturnThis(),
        setText(str: string) { t._text = str; return t; },
        destroy: vi.fn(),
    };
    return t;
}

export function makeScene() {
    return {
        add: {
            sprite: vi.fn().mockImplementation(makeSprite),
            graphics: vi.fn().mockImplementation(makeGraphics),
            text: vi.fn().mockImplementation(makeText),
            existing: vi.fn(),
            particles: vi.fn().mockReturnValue({ setDepth: vi.fn(), explode: vi.fn() }),
        },
        tweens: {
            add: vi.fn(),
            killTweensOf: vi.fn(),
        },
        time: {
            delayedCall: vi.fn().mockReturnValue({ destroy: vi.fn() }),
        },
        cameras: {
            main: { startFollow: vi.fn() },
        },
    };
}

export type MockScene = ReturnType<typeof makeScene>;
