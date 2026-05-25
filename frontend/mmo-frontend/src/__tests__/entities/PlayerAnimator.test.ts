import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('phaser', () => ({
    default: {
        GameObjects: {
            Container: class {
                x = 0; y = 0;
                scene: unknown;
                constructor(scene: unknown, x = 0, y = 0) { this.scene = scene; this.x = x; this.y = y; }
                add(_c: unknown) { return this; }
                setDepth(_d: number) { return this; }
                bringToTop(_c: unknown) { return this; }
                destroy(_f?: boolean) {}
            },
        },
    },
}));

import { PlayerAnimator } from '../../entities/PlayerAnimator';
import { makeScene } from '../mocks/scene';
import type { MockScene } from '../mocks/scene';
import Phaser from 'phaser';

function makeAnimator() {
    const scene = makeScene() as unknown as Phaser.Scene & MockScene;
    const sprite = (scene as any).add.sprite(0, 0, 'hero', 0);
    const container = new Phaser.GameObjects.Container(scene, 0, 0);
    const bringToTopSpy = vi.spyOn(container, 'bringToTop');
    const animator = new PlayerAnimator(scene, sprite, container, () => false);
    return { animator, container, bringToTopSpy, scene };
}

describe('PlayerAnimator — shield render order', () => {
    it('bringToTop chamado com sprite Shield ao equipar Shield', () => {
        const { animator, bringToTopSpy } = makeAnimator();

        animator.setEquippedBodyOverlay('Shield', 'shield_wooden_walk', 'shield_wooden_slash');

        expect(bringToTopSpy).toHaveBeenCalledOnce();
    });

    it('shield fica no topo ao equipar outro slot depois do shield', () => {
        const { animator, bringToTopSpy } = makeAnimator();

        animator.setEquippedBodyOverlay('Shield', 'shield_wooden_walk', 'shield_wooden_slash');
        animator.setEquippedBodyOverlay('Chest', 'armor_leather_torso_walk', 'armor_leather_torso_slash');

        // deve chamar bringToTop duas vezes: uma ao equipar shield, outra ao equipar chest
        expect(bringToTopSpy).toHaveBeenCalledTimes(2);
    });

    it('não chama bringToTop quando shield não está equipado', () => {
        const { animator, bringToTopSpy } = makeAnimator();

        animator.setEquippedBodyOverlay('Chest', 'armor_leather_torso_walk', 'armor_leather_torso_slash');

        expect(bringToTopSpy).not.toHaveBeenCalled();
    });

    it('não chama bringToTop ao desequipar slot sem shield', () => {
        const { animator, bringToTopSpy } = makeAnimator();

        animator.setEquippedBodyOverlay('Chest', 'armor_leather_torso_walk', 'armor_leather_torso_slash');
        animator.setEquippedBodyOverlay('Chest', null, null);

        expect(bringToTopSpy).not.toHaveBeenCalled();
    });
});
