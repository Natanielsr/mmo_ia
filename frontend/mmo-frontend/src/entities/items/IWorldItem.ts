export interface IWorldItem {
    id: string;
    collect(): void;
    destroy(): void;
}
