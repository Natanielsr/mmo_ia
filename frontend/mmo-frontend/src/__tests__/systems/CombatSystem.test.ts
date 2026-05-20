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
    let callback: ReturnType<typeof vi.fn>;

    beforeEach(() => {
        callback = vi.fn();
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
