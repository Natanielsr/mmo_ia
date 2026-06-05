import type { HubConnection } from '@microsoft/signalr';
import type { MainScene } from '../../scenes/MainScene';

export class WorldEventHandler {
    private connection: HubConnection;
    private scene: MainScene;

    constructor(connection: HubConnection, scene: MainScene) {
        this.connection = connection;
        this.scene = scene;
    }

    register(): void {
        this.connection.on("WorldMap", (data: any) => {
            this.scene.worldMapLoaded({
                cols: data.cols ?? data.Cols ?? 0,
                rows: data.rows ?? data.Rows ?? 0,
                tiles: data.tiles ?? data.Tiles ?? [],
                biomes: data.biomes ?? data.Biomes ?? [],
            });
        });

        this.connection.on("ChunkLoaded", (data: any) => {
            this.scene.chunkLoaded({
                cx: data.cx ?? data.Cx ?? data.CX,
                cy: data.cy ?? data.Cy ?? data.CY,
                objects: (data.objects ?? data.Objects ?? []).map((obj: any) => ({
                    id: String(obj.id ?? obj.Id),
                    name: String(obj.name ?? obj.Name),
                    objectCode: String(obj.objectCode ?? obj.ObjectCode),
                    position: {
                        x: obj.position.x ?? obj.position.X,
                        y: obj.position.y ?? obj.position.Y
                    },
                    type: String(obj.type ?? obj.Type),
                    isPassable: Boolean(obj.isPassable ?? obj.IsPassable)
                }))
            });
        });
    }
}
