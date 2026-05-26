import { describe, it, expect } from 'vitest'
import { getWeaponOverlayConfig } from '../../config/overlays/weaponOverlayRegistry'

describe('getWeaponOverlayConfig', () => {
    it('returns null for unknown weapon', () => {
        expect(getWeaponOverlayConfig('Unknown')).toBeNull()
    })

    describe('Dagger', () => {
        const cfg = getWeaponOverlayConfig('Dagger')

        it('exists', () => {
            expect(cfg).not.toBeNull()
        })

        it('has walk config', () => {
            expect(cfg?.walkTextureKey).toBe('weapon_dagger_walk')
            expect(cfg?.walkAssetPath).toBe('assets/walkcycle/WEAPON_dagger.png')
        })

        it('has slash config', () => {
            expect(cfg?.slashTextureKey).toBe('weapon_dagger_slash')
            expect(cfg?.slashAssetPath).toBe('assets/slash/WEAPON_dagger.png')
        })

        it('has correct frame size', () => {
            expect(cfg?.frameWidth).toBe(64)
            expect(cfg?.frameHeight).toBe(64)
        })

        it('has correct walk direction frames (LPC pattern: skip idle frame 0 per row)', () => {
            expect(cfg?.walkDirectionFrames).toEqual({
                north: [1, 8],
                west: [10, 17],
                south: [19, 26],
                east: [28, 35],
            })
        })

        it('has correct slash direction frames', () => {
            expect(cfg?.slashDirectionFrames).toEqual({
                north: [0, 5],
                west: [6, 11],
                south: [12, 17],
                east: [18, 23],
            })
        })

        it('has correct frame rates', () => {
            expect(cfg?.walkFrameRate).toBe(16)
            expect(cfg?.slashFrameRate).toBe(24)
        })
    })
})
