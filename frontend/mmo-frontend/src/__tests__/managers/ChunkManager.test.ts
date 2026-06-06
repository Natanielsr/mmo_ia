import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';

vi.mock('phaser', () => ({ default: { Scene: class {} } }));
vi.mock('../../config/constants', () => ({ GRID_SIZE: 32 }));
vi.mock('../../config/tileTypeRegistry', () => ({ TileType: { WaterFull: 0 } }));
vi.mock('../../config/terrainAssetMap', () => ({ getTerrainAssetEntry: vi.fn() }));

import { ChunkManager } from '../../managers/ChunkManager';
import type { ChunkData } from '../../dtos/world';

function makeScene() {
    const groupMock = { add: vi.fn(), destroy: vi.fn() };
    const rtMock = {
        destroy: vi.fn(), draw: vi.fn(),
        setOrigin: vi.fn(), setDepth: vi.fn(),
    };
    const spriteMock = { setDisplaySize: vi.fn(), setDepth: vi.fn() };
    const imgMock = {
        setOrigin: vi.fn(), setTexture: vi.fn(),
        setFlipY: vi.fn(), setDisplaySize: vi.fn(), destroy: vi.fn(),
    };
    return {
        add: {
            group: vi.fn().mockReturnValue(groupMock),
            renderTexture: vi.fn().mockReturnValue(rtMock),
            sprite: vi.fn().mockReturnValue(spriteMock),
        },
        make: { image: vi.fn().mockReturnValue(imgMock) },
    };
}

function chunk(cx: number, cy: number): ChunkData {
    return { cx, cy, objects: [] };
}

describe('ChunkManager — solicitação de chunks faltantes', () => {
    let onChunksNeeded: Mock<(coords: { cx: number; cy: number }[]) => void>;
    let scene: ReturnType<typeof makeScene>;
    let manager: ChunkManager;

    beforeEach(() => {
        onChunksNeeded = vi.fn();
        scene = makeScene();
        manager = new ChunkManager(scene as any, onChunksNeeded);
    });

    it('chama onChunksNeeded com coords visíveis não carregadas', () => {
        manager.update({ x: 0, y: 0 }, 1);

        expect(onChunksNeeded).toHaveBeenCalledTimes(1);
        const coords: { cx: number; cy: number }[] = onChunksNeeded.mock.calls[0][0];
        expect(coords).toHaveLength(9); // 3x3 ao redor de chunk (0,0)
        expect(coords).toContainEqual({ cx: 0, cy: 0 });
        expect(coords).toContainEqual({ cx: -1, cy: -1 });
        expect(coords).toContainEqual({ cx: 1, cy: 1 });
    });

    it('não inclui chunks já carregados na solicitação', () => {
        manager.handleChunkLoaded(chunk(0, 0));

        manager.update({ x: 0, y: 0 }, 1);

        const coords: { cx: number; cy: number }[] = onChunksNeeded.mock.calls[0][0];
        expect(coords).not.toContainEqual({ cx: 0, cy: 0 });
        expect(coords).toHaveLength(8);
    });

    it('não re-solicita chunks já pendentes', () => {
        manager.update({ x: 0, y: 0 }, 1); // adiciona 9 ao pending
        vi.clearAllMocks();

        manager.update({ x: 0, y: 0 }, 1); // todos já pendentes

        expect(onChunksNeeded).not.toHaveBeenCalled();
    });

    it('não chama onChunksNeeded quando não há chunks faltantes nem pendentes', () => {
        // Carrega todos os chunks visíveis
        for (let dx = -1; dx <= 1; dx++)
            for (let dy = -1; dy <= 1; dy++)
                manager.handleChunkLoaded(chunk(dx, dy));

        manager.update({ x: 0, y: 0 }, 1);

        expect(onChunksNeeded).not.toHaveBeenCalled();
    });

    it('re-solicita chunk após ser descarregado e player retornar', () => {
        // Passo 1: detecta chunk (0,0) faltando e pede
        manager.update({ x: 0, y: 0 }, 0); // radius 0 → só (0,0)
        expect(onChunksNeeded).toHaveBeenCalledWith([{ cx: 0, cy: 0 }]);
        vi.clearAllMocks();

        // Passo 2: chunk chega do server
        manager.handleChunkLoaded(chunk(0, 0));

        // Passo 3: player vai longe — chunkSize=16, posição 200 → chunkCoord 12
        // (0,0) está a distância 12 do player, safetyMargin+loadRadius=1, então é destruído
        manager.update({ x: 200, y: 200 }, 0);
        vi.clearAllMocks();

        // Passo 4: player volta — (0,0) não está em loadedChunks nem pendingRequests
        manager.update({ x: 0, y: 0 }, 0);

        expect(onChunksNeeded).toHaveBeenCalledWith(
            expect.arrayContaining([{ cx: 0, cy: 0 }])
        );
    });

    it('handleChunkLoaded remove chunk de pendingRequests', () => {
        manager.update({ x: 0, y: 0 }, 0); // (0,0) vai para pending
        vi.clearAllMocks();

        manager.handleChunkLoaded(chunk(0, 0)); // remove de pending, adiciona a loadedChunks

        manager.update({ x: 0, y: 0 }, 0); // (0,0) está em loadedChunks → não pede

        expect(onChunksNeeded).not.toHaveBeenCalled();
    });
});
