import { describe, it, expect } from 'vitest';
import {
    getBodyOverlayConfig,
    getAllBodyConfigs,
    BODY_SLOTS,
    type BodyOverlayConfig,
} from '../../../config/overlays';

// Espelho exato do enum C# EquipmentSlot no backend.
// Se o backend adicionar/renomear um slot, este array deve ser atualizado
// para que os testes abaixo detectem o conflito.
const BACKEND_EQUIPMENT_SLOTS = ['Weapon', 'Helmet', 'Chest', 'Legs', 'Boots', 'Shield'] as const;

// Slots que possuem overlay de body (exclui apenas Weapon, tratado separadamente)
const BACKEND_BODY_SLOTS = BACKEND_EQUIPMENT_SLOTS.filter(s => s !== 'Weapon');

describe('BODY_SLOTS — nomes de slot vs backend EquipmentSlot enum', () => {
    it('nenhum slot usa "Feet" — deve ser "Boots" para coincidir com o backend', () => {
        expect(BODY_SLOTS).not.toContain('Feet');
    });

    it('cada slot registrado existe no enum do backend', () => {
        for (const slot of BODY_SLOTS) {
            expect(BACKEND_EQUIPMENT_SLOTS).toContain(slot as typeof BACKEND_EQUIPMENT_SLOTS[number]);
        }
    });

    it('todos os slots de body do backend possuem um registry correspondente', () => {
        for (const slot of BACKEND_BODY_SLOTS) {
            expect(BODY_SLOTS).toContain(slot);
        }
    });
});

describe('getBodyOverlayConfig — lookup por slot + item', () => {
    it('retorna null para slot inexistente', () => {
        expect(getBodyOverlayConfig('Feet', 'iron-boots')).toBeNull();
    });

    it('retorna null para item inexistente no slot correto', () => {
        expect(getBodyOverlayConfig('Boots', 'item-inexistente')).toBeNull();
    });

    it('retorna config para plate-boots no slot Boots', () => {
        const cfg = getBodyOverlayConfig('Boots', 'plate-boots');
        expect(cfg).not.toBeNull();
    });

    it('retorna config para leather-vest no slot Chest', () => {
        const cfg = getBodyOverlayConfig('Chest', 'leather-vest');
        expect(cfg).not.toBeNull();
    });

    it('retorna config para leather-pants no slot Legs', () => {
        const cfg = getBodyOverlayConfig('Legs', 'leather-pants');
        expect(cfg).not.toBeNull();
    });

    it('retorna config para plate-helmet no slot Helmet', () => {
        const cfg = getBodyOverlayConfig('Helmet', 'plate-helmet');
        expect(cfg).not.toBeNull();
    });

    it('retorna config para wooden-shield no slot Shield', () => {
        const cfg = getBodyOverlayConfig('Shield', 'wooden-shield');
        expect(cfg).not.toBeNull();
    });
});

describe('getAllBodyConfigs — integridade estrutural de cada config', () => {
    const DIRECTIONS = ['north', 'west', 'south', 'east'] as const;

    function assertValidConfig(cfg: BodyOverlayConfig, label: string) {
        expect(cfg.walkTextureKey, `${label}: walkTextureKey vazio`).toBeTruthy();
        expect(cfg.slashTextureKey, `${label}: slashTextureKey vazio`).toBeTruthy();
        expect(cfg.walkAssetPath, `${label}: walkAssetPath vazio`).toBeTruthy();
        expect(cfg.slashAssetPath, `${label}: slashAssetPath vazio`).toBeTruthy();

        expect(cfg.frameWidth, `${label}: frameWidth inválido`).toBeGreaterThan(0);
        expect(cfg.frameHeight, `${label}: frameHeight inválido`).toBeGreaterThan(0);
        expect(cfg.walkFrameRate, `${label}: walkFrameRate inválido`).toBeGreaterThan(0);
        expect(cfg.slashFrameRate, `${label}: slashFrameRate inválido`).toBeGreaterThan(0);

        for (const dir of DIRECTIONS) {
            const [ws, we] = cfg.walkDirectionFrames[dir];
            expect(ws, `${label}: walkDirectionFrames.${dir} start inválido`).toBeGreaterThanOrEqual(0);
            expect(we, `${label}: walkDirectionFrames.${dir} end < start`).toBeGreaterThanOrEqual(ws);

            const [ss, se] = cfg.slashDirectionFrames[dir];
            expect(ss, `${label}: slashDirectionFrames.${dir} start inválido`).toBeGreaterThanOrEqual(0);
            expect(se, `${label}: slashDirectionFrames.${dir} end < start`).toBeGreaterThanOrEqual(ss);
        }
    }

    it('todas as configs têm campos obrigatórios válidos', () => {
        const configs = getAllBodyConfigs();
        expect(configs.length).toBeGreaterThan(0);
        for (const cfg of configs) {
            assertValidConfig(cfg, cfg.walkTextureKey);
        }
    });

    it('nenhum walkTextureKey é duplicado', () => {
        const keys = getAllBodyConfigs().map(c => c.walkTextureKey);
        expect(new Set(keys).size).toBe(keys.length);
    });

    it('nenhum slashTextureKey é duplicado', () => {
        const keys = getAllBodyConfigs().map(c => c.slashTextureKey);
        expect(new Set(keys).size).toBe(keys.length);
    });
});
