import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('phaser', () => ({
    default: { GameObjects: { Container: class {}, Sprite: class {} } },
}));
vi.mock('../../managers/PlayerManager');
vi.mock('../../managers/MonsterManager');

import { CombatSystem } from '../../systems/CombatSystem';
import type { PlayerManager } from '../../managers/PlayerManager';
import type { MonsterManager } from '../../managers/MonsterManager';
import type { Monster } from '../../entities/Monster';
import type { Player } from '../../entities/Player';

function makePlayer(gx: number, gy: number): Partial<Player> {
    return { gridPosition: { x: gx, y: gy }, x: 0, y: 0 } as Partial<Player>;
}

function makeMonster(gx: number, gy: number, dead = false): Partial<Monster> {
    return { id: `m-${gx}-${gy}`, gridPosition: { x: gx, y: gy }, isDead: dead, x: 0, y: 0 } as Partial<Monster>;
}

function makeCombat(
    player: Partial<Player> | null,
    monsters: Record<string, Partial<Monster>>,
) {
    const playerManager = {
        getMyPlayer: vi.fn().mockReturnValue(player),
        getPlayer: vi.fn().mockReturnValue(null),
    } as unknown as PlayerManager;

    const monsterManager = {
        getMonstersDict: vi.fn().mockReturnValue(monsters),
        getMonster: vi.fn().mockReturnValue(null),
    } as unknown as MonsterManager;

    return new CombatSystem({} as never, playerManager, monsterManager);
}

describe('CombatSystem.attackNearestMonster', () => {
    let callback: (targetId: string) => void;

    beforeEach(() => {
        callback = vi.fn() as (targetId: string) => void;
    });

    it('não chama callback quando não há jogador', () => {
        const combat = makeCombat(null, {});
        combat.attackNearestMonster(callback);
        expect(callback).not.toHaveBeenCalled();
    });

    it('não chama callback quando não há monstros', () => {
        const combat = makeCombat(makePlayer(5, 5), {});
        combat.attackNearestMonster(callback);
        expect(callback).not.toHaveBeenCalled();
    });

    it('ataca monstro ao norte (dy=1)', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(5, 6),
        });
        combat.attackNearestMonster(callback);
        expect(callback).toHaveBeenCalledWith('m-5-6');
    });

    it('ataca monstro ao sul (dy=-1)', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(5, 4),
        });
        combat.attackNearestMonster(callback);
        expect(callback).toHaveBeenCalledWith('m-5-4');
    });

    it('ataca monstro a leste (dx=1)', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(6, 5),
        });
        combat.attackNearestMonster(callback);
        expect(callback).toHaveBeenCalledWith('m-6-5');
    });

    it('ataca monstro a oeste (dx=-1)', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(4, 5),
        });
        combat.attackNearestMonster(callback);
        expect(callback).toHaveBeenCalledWith('m-4-5');
    });

    it('ataca monstro na diagonal (dx=1, dy=1)', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(6, 6),
        });
        combat.attackNearestMonster(callback);
        expect(callback).toHaveBeenCalledWith('m-6-6');
    });

    it('não ataca monstro no mesmo tile (dx=0, dy=0)', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(5, 5),
        });
        combat.attackNearestMonster(callback);
        expect(callback).not.toHaveBeenCalled();
    });

    it('não ataca monstro a distância 2', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(7, 5),
        });
        combat.attackNearestMonster(callback);
        expect(callback).not.toHaveBeenCalled();
    });

    it('ignora monstros mortos', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            dead: makeMonster(6, 5, true),
        });
        combat.attackNearestMonster(callback);
        expect(callback).not.toHaveBeenCalled();
    });

    it('ignora monstros mortos mas ataca vivos na vizinhança', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            dead: makeMonster(6, 5, true),
            alive: makeMonster(5, 6),
        });
        combat.attackNearestMonster(callback);
        expect(callback).toHaveBeenCalledWith('m-5-6');
    });

    it('não chama callback quando callback é undefined', () => {
        const combat = makeCombat(makePlayer(5, 5), {
            a: makeMonster(6, 5),
        });
        expect(() => combat.attackNearestMonster(undefined)).not.toThrow();
    });
});

// ── Helpers para testes com cena ─────────────────────────────────────────────

function makeTextMock() {
    return { setOrigin: vi.fn().mockReturnThis(), setDepth: vi.fn().mockReturnThis(), destroy: vi.fn() };
}

function makeSceneMock() {
    return {
        add: { text: vi.fn().mockImplementation(makeTextMock) },
        tweens: { add: vi.fn() },
    };
}

function makeCombatWithScene(
    player: Partial<Player> | null,
    getPlayerImpl: (id: string) => Partial<Player> | null,
    monsters: Record<string, Partial<Monster>>,
    getMonsterImpl: (id: string) => Partial<Monster> | null,
) {
    const pm = {
        getMyPlayer: vi.fn().mockReturnValue(player),
        getPlayer: vi.fn().mockImplementation(getPlayerImpl),
    } as unknown as PlayerManager;

    const mm = {
        getMonstersDict: vi.fn().mockReturnValue(monsters),
        getMonster: vi.fn().mockImplementation(getMonsterImpl),
    } as unknown as MonsterManager;

    return new CombatSystem(makeSceneMock() as never, pm, mm);
}

// ── handleMonsterDamaged ──────────────────────────────────────────────────────

describe('CombatSystem.handleMonsterDamaged', () => {
    it('chama takeDamage no monstro correto', () => {
        const monster = { takeDamage: vi.fn(), x: 0, y: 0 } as unknown as Monster;
        const combat = makeCombatWithScene(null, () => null, {}, (id) => id === 'm1' ? monster : null);
        combat.handleMonsterDamaged({ id: 'm1', damage: 25, currentHp: 75 });
        expect(monster.takeDamage).toHaveBeenCalledWith(25);
    });

    it('é no-op quando monstro não existe', () => {
        const combat = makeCombatWithScene(null, () => null, {}, () => null);
        expect(() => combat.handleMonsterDamaged({ id: 'none', damage: 10, currentHp: 0 })).not.toThrow();
    });
});

// ── handlePlayerAttacked ──────────────────────────────────────────────────────

describe('CombatSystem.handlePlayerAttacked', () => {
    it('chama playAttackAnimation no atacante com posição do alvo', () => {
        const attacker = { playAttackAnimation: vi.fn(), takeDamage: vi.fn(), x: 0, y: 0 } as unknown as Player;
        const target   = { playAttackAnimation: vi.fn(), takeDamage: vi.fn(), x: 100, y: 50 } as unknown as Player;
        const getPlayer = (id: string) => id === 'att' ? attacker : id === 'tgt' ? target : null;
        const combat = makeCombatWithScene(null, getPlayer, {}, () => null);

        combat.handlePlayerAttacked({ attackerId: 'att', targetId: 'tgt', damage: 30 });

        expect(attacker.playAttackAnimation).toHaveBeenCalledWith(100, 50);
        expect(target.takeDamage).toHaveBeenCalledWith(30);
    });

    it('usa posição do monstro quando alvo é monstro (não player)', () => {
        const attacker = { playAttackAnimation: vi.fn(), takeDamage: vi.fn(), x: 0, y: 0 } as unknown as Player;
        const monster  = { takeDamage: vi.fn(), x: 200, y: 300 } as unknown as Monster;
        const getPlayer = (id: string) => id === 'att' ? attacker : null;
        const combat = makeCombatWithScene(null, getPlayer, {}, (id) => id === 'tgt' ? monster : null);

        combat.handlePlayerAttacked({ attackerId: 'att', targetId: 'tgt', damage: 15 });

        expect(attacker.playAttackAnimation).toHaveBeenCalledWith(200, 300);
    });

    it('usa posição do próprio atacante quando alvo não existe', () => {
        const attacker = { playAttackAnimation: vi.fn(), takeDamage: vi.fn(), x: 10, y: 20 } as unknown as Player;
        const getPlayer = (id: string) => id === 'att' ? attacker : null;
        const combat = makeCombatWithScene(null, getPlayer, {}, () => null);

        combat.handlePlayerAttacked({ attackerId: 'att', targetId: 'nobody', damage: 0 });

        expect(attacker.playAttackAnimation).toHaveBeenCalledWith(10, 20);
    });

    it('é no-op quando atacante não existe', () => {
        const combat = makeCombatWithScene(null, () => null, {}, () => null);
        expect(() => combat.handlePlayerAttacked({ attackerId: 'x', targetId: 'y', damage: 10 })).not.toThrow();
    });
});

// ── showDamageText — onComplete callback ─────────────────────────────────────

describe('CombatSystem.showDamageText — onComplete destrói o texto', () => {
    it('tween onComplete chama destroy no texto de dano', () => {
        const textMock = {
            setOrigin: vi.fn().mockReturnThis(),
            setDepth: vi.fn().mockReturnThis(),
            destroy: vi.fn(),
        };
        const scene = {
            add: { text: vi.fn().mockReturnValue(textMock) },
            tweens: {
                add: vi.fn().mockImplementation((cfg: { onComplete?: () => void }) => {
                    cfg.onComplete?.();
                }),
            },
        };
        const monster = { takeDamage: vi.fn(), x: 0, y: 0 } as unknown as Monster;
        const pm = { getMyPlayer: vi.fn(), getPlayer: vi.fn().mockReturnValue(null) } as unknown as PlayerManager;
        const mm = { getMonstersDict: vi.fn(), getMonster: vi.fn().mockReturnValue(monster) } as unknown as MonsterManager;
        const combat = new CombatSystem(scene as never, pm, mm);

        combat.handleMonsterDamaged({ id: 'x', damage: 10, currentHp: 90 });

        expect(textMock.destroy).toHaveBeenCalled();
    });
});
