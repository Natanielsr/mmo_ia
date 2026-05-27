import { describe, it, expect, vi } from 'vitest';

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

describe('PlayerAnimator.playTakeDamageFlash — overlays', () => {
    it('inclui apenas o sprite base quando não há overlays', () => {
        const { animator, scene } = makeAnimator();

        animator.playTakeDamageFlash();

        const { targets } = (scene.tweens.add as ReturnType<typeof vi.fn>).mock.calls[0][0];
        expect(targets).toHaveLength(1);
    });

    it('inclui overlay de corpo no tween quando equipado', () => {
        const { animator, scene } = makeAnimator();
        animator.setEquippedBodyOverlay('Chest', 'chest-walk', 'chest-slash');
        const overlaySprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;

        animator.playTakeDamageFlash();

        const { targets } = (scene.tweens.add as ReturnType<typeof vi.fn>).mock.calls[0][0];
        expect(targets).toContain(overlaySprite);
    });

    it('inclui weaponSprite no tween quando equipado', () => {
        const { animator, scene } = makeAnimator();
        animator.setEquippedWeaponOverlay('sword-walk', 'sword-slash');
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;

        animator.playTakeDamageFlash();

        const { targets } = (scene.tweens.add as ReturnType<typeof vi.fn>).mock.calls[0][0];
        expect(targets).toContain(weaponSprite);
    });

    it('inclui todos sprites com múltiplos overlays e arma', () => {
        const { animator, scene } = makeAnimator();
        animator.setEquippedBodyOverlay('Chest', 'chest-walk', null);
        animator.setEquippedBodyOverlay('Legs', 'legs-walk', null);
        animator.setEquippedWeaponOverlay('sword-walk', null);

        animator.playTakeDamageFlash();

        const { targets } = (scene.tweens.add as ReturnType<typeof vi.fn>).mock.calls[0][0];
        // base sprite + 2 body overlays + 1 weapon = 4
        expect(targets).toHaveLength(4);
    });

    it('onComplete limpa tint em todos os sprites quando vivo', () => {
        const { animator, scene } = makeAnimator();
        animator.setEquippedBodyOverlay('Chest', 'chest-walk', null);
        const overlaySprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        vi.spyOn(overlaySprite, 'clearTint');

        animator.playTakeDamageFlash();

        const { onComplete } = (scene.tweens.add as ReturnType<typeof vi.fn>).mock.calls[0][0];
        onComplete();

        expect(overlaySprite.clearTint).toHaveBeenCalled();
    });

    it('onComplete não limpa tint quando morto', () => {
        const scene = makeScene() as unknown as Phaser.Scene & MockScene;
        const sprite = (scene as any).add.sprite(0, 0, 'hero', 0);
        const container = new Phaser.GameObjects.Container(scene, 0, 0);
        const deadAnimator = new PlayerAnimator(scene, sprite, container, () => true);

        deadAnimator.setEquippedBodyOverlay('Chest', 'chest-walk', null);
        const overlaySprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        vi.spyOn(overlaySprite, 'clearTint');

        deadAnimator.playTakeDamageFlash();

        const { onComplete } = (scene.tweens.add as ReturnType<typeof vi.fn>).mock.calls[0][0];
        onComplete();

        expect(overlaySprite.clearTint).not.toHaveBeenCalled();
    });
});

describe('PlayerAnimator.playDieEffect — overlays', () => {
    it('aplica angle e tint cinza no overlay de corpo', () => {
        const { animator, scene } = makeAnimator();
        animator.setEquippedBodyOverlay('Chest', 'chest-walk', null);
        const overlaySprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        vi.spyOn(overlaySprite, 'setAngle');
        vi.spyOn(overlaySprite, 'setTint');

        animator.playDieEffect();

        expect(overlaySprite.setAngle).toHaveBeenCalledWith(90);
        expect(overlaySprite.setTint).toHaveBeenCalledWith(0x666666);
    });

    it('aplica angle e tint cinza na arma quando equipada', () => {
        const { animator, scene } = makeAnimator();
        animator.setEquippedWeaponOverlay('sword-walk', null);
        const weaponSprite = (scene.add.sprite as ReturnType<typeof vi.fn>).mock.results.at(-1)!.value;
        vi.spyOn(weaponSprite, 'setAngle');
        vi.spyOn(weaponSprite, 'setTint');

        animator.playDieEffect();

        expect(weaponSprite.setAngle).toHaveBeenCalledWith(90);
        expect(weaponSprite.setTint).toHaveBeenCalledWith(0x666666);
    });
});
