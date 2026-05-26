import { describe, it, expect, beforeEach } from 'vitest';
import { vi } from 'vitest';

vi.mock('phaser', () => ({
    default: {
        GameObjects: {
            Container: class {
                x = 0; y = 0;
                scene: unknown;
                constructor(scene: unknown, x = 0, y = 0) { this.scene = scene; this.x = x; this.y = y; }
                add(_c: unknown) { return this; }
                bringToTop(_c: unknown) { return this; }
                setDepth(_d: number) { return this; }
                destroy(_f?: boolean) {}
            },
        },
    },
}));

import { PlayerManager } from '../../managers/PlayerManager';
import { makeScene } from '../mocks/scene';

const PLAYER_DATA = {
    id: 'p1',
    name: 'Hero',
    position: { x: 5, y: 5 },
    hp: 100,
    maxHp: 100,
    level: 1,
    experience: 0,
    isDead: false,
};

describe('PlayerManager — estado inicial', () => {
    it('myId é null antes de qualquer jogador entrar', () => {
        const pm = new PlayerManager(makeScene() as never);
        expect(pm.myId).toBeNull();
    });

    it('getMyPlayer retorna null inicialmente', () => {
        const pm = new PlayerManager(makeScene() as never);
        expect(pm.getMyPlayer()).toBeNull();
    });

    it('getPlayer retorna null para id desconhecido', () => {
        const pm = new PlayerManager(makeScene() as never);
        expect(pm.getPlayer('unknown')).toBeNull();
    });
});

describe('PlayerManager.updatePlayerPosition', () => {
    let pm: PlayerManager;
    beforeEach(() => { pm = new PlayerManager(makeScene() as never); });

    it('cria novo jogador e define myId quando isMe=true', () => {
        pm.updatePlayerPosition(PLAYER_DATA, true);
        expect(pm.myId).toBe('p1');
        expect(pm.getMyPlayer()).not.toBeNull();
    });

    it('não define myId quando isMe=false', () => {
        pm.updatePlayerPosition(PLAYER_DATA, false);
        expect(pm.myId).toBeNull();
    });

    it('getPlayer retorna o jogador criado', () => {
        pm.updatePlayerPosition(PLAYER_DATA, false);
        const p = pm.getPlayer('p1');
        expect(p).not.toBeNull();
        expect(p!.id).toBe('p1');
    });

    it('atualiza jogador existente sem criar duplicata', () => {
        pm.updatePlayerPosition(PLAYER_DATA, false);
        pm.updatePlayerPosition({ ...PLAYER_DATA, position: { x: 6, y: 6 } }, false);
        const dict = pm.getPlayersDict();
        expect(Object.keys(dict)).toHaveLength(1);
    });

    it('suporta casing flexível (Id e Position do backend)', () => {
        const flexData = { Id: 'p2', Name: 'Mage', Position: { X: 1, Y: 2 }, hp: 100, maxHp: 100 };
        pm.updatePlayerPosition(flexData, false);
        expect(pm.getPlayer('p2')).not.toBeNull();
    });

    it('é no-op quando position está ausente', () => {
        pm.updatePlayerPosition({ id: 'p99', name: 'Ghost' }, false);
        expect(pm.getPlayer('p99')).toBeNull();
    });
});

describe('PlayerManager.removePlayer', () => {
    it('remove jogador do dicionário', () => {
        const pm = new PlayerManager(makeScene() as never);
        pm.updatePlayerPosition(PLAYER_DATA, false);
        pm.removePlayer('p1');
        expect(pm.getPlayer('p1')).toBeNull();
    });

    it('é no-op para id inexistente', () => {
        const pm = new PlayerManager(makeScene() as never);
        expect(() => pm.removePlayer('nobody')).not.toThrow();
    });
});

describe('PlayerManager.updatePlayerStatus', () => {
    it('atualiza hp do jogador existente', () => {
        const pm = new PlayerManager(makeScene() as never);
        pm.updatePlayerPosition(PLAYER_DATA, false);
        pm.updatePlayerStatus({ id: 'p1', hp: 40, maxHp: 100, level: 1, experience: 0, isDead: false });
        expect(pm.getPlayer('p1')!.hp).toBe(40);
    });

    it('mantém hp existente quando statusData não tem campo hp', () => {
        const pm = new PlayerManager(makeScene() as never);
        pm.updatePlayerPosition(PLAYER_DATA, false);
        const hpAntes = pm.getPlayer('p1')!.hp;
        pm.updatePlayerStatus({ id: 'p1' }); // sem hp → fallback para player.hp
        expect(pm.getPlayer('p1')!.hp).toBe(hpAntes);
    });

    it('é no-op quando jogador não existe', () => {
        const pm = new PlayerManager(makeScene() as never);
        expect(() => pm.updatePlayerStatus({ id: 'ghost', hp: 0 })).not.toThrow();
    });
});

describe('PlayerManager.getMyPlayer — edge cases', () => {
    it('retorna null quando myId está definido mas jogador foi removido', () => {
        const pm = new PlayerManager(makeScene() as never);
        pm.updatePlayerPosition(PLAYER_DATA, true); // myId = 'p1'
        pm.removePlayer('p1');
        expect(pm.getMyPlayer()).toBeNull();
    });
});

describe('PlayerManager — weapon overlay ao spawnar', () => {
    it('equippedItems Weapon Dagger → cria weaponSprite com weapon_dagger_walk', () => {
        const scene = makeScene();
        const pm = new PlayerManager(scene as never);

        pm.updatePlayerPosition({ ...PLAYER_DATA, equippedItems: { Weapon: 'Dagger' } }, false);

        expect(scene.add.sprite).toHaveBeenCalledWith(0, 0, 'weapon_dagger_walk', expect.any(Number));
    });

    it('EquippedItems PascalCase também aplica overlay', () => {
        const scene = makeScene();
        const pm = new PlayerManager(scene as never);

        pm.updatePlayerPosition({ ...PLAYER_DATA, EquippedItems: { Weapon: 'Dagger' } }, false);

        expect(scene.add.sprite).toHaveBeenCalledWith(0, 0, 'weapon_dagger_walk', expect.any(Number));
    });

    it('sem equippedItems não cria sprite extra', () => {
        const scene = makeScene();
        const pm = new PlayerManager(scene as never);

        pm.updatePlayerPosition(PLAYER_DATA, false);

        const spriteCalls = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.calls;
        const weaponCall = spriteCalls.find((args: unknown[]) => String(args[2]).startsWith('weapon_'));
        expect(weaponCall).toBeUndefined();
    });

    it('nome de arma desconhecida não cria sprite overlay', () => {
        const scene = makeScene();
        const pm = new PlayerManager(scene as never);

        pm.updatePlayerPosition({ ...PLAYER_DATA, equippedItems: { Weapon: 'SwordOfUnknown' } }, false);

        const spriteCalls = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.calls;
        const weaponCall = spriteCalls.find((args: unknown[]) => String(args[2]).startsWith('weapon_'));
        expect(weaponCall).toBeUndefined();
    });
});
