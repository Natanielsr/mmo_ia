class PhaserBase {
    scene: unknown = null;
    x: number = 0;
    y: number = 0;
    alpha: number = 1;
    scaleX: number = 1;
    scaleY: number = 1;
    visible: boolean = true;
    depth: number = 0;
    setDepth(_d: number) { return this; }
    setOrigin(_x?: number, _y?: number) { return this; }
    setDisplaySize(_w: number, _h: number) { return this; }
    setAlpha(_a: number) { return this; }
    setTint(_t: number) { return this; }
    clearTint() { return this; }
    setFrame(_f: number) { return this; }
    setTexture(_k: string, _f?: number) { return this; }
    setAngle(_a: number) { return this; }
    setInteractive() { return this; }
    on(_event: string, _fn: unknown) { return this; }
    once(_event: string, _fn: unknown) { return this; }
    play(_key: string, _ignoreIfPlaying?: boolean) { return this; }
    stop() { return this; }
    destroy() {}
    setText(_t: string) { return this; }
    setText2(_t: string) { return this; }
}

class MockContainer extends PhaserBase {
    constructor(_scene: unknown, _x?: number, _y?: number) { super(); }
    add(_children: unknown) { return this; }
    bringToTop(_child: unknown) { return this; }
}

class MockSprite extends PhaserBase {
    texture = { key: 'mock' };
    constructor(_scene: unknown, _x?: number, _y?: number, _texture?: string, _frame?: number) { super(); }
}

const phaser = {
    GameObjects: {
        Container: MockContainer,
        Sprite: MockSprite,
    },
    Scene: class {},
};

export default phaser;
