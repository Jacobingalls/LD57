// Pixi render layer for the Mako loop. It is a *pure view* of MakoWorld: every frame
// it reads world state and syncs sprites — it never drives simulation. Orbs are drawn
// from a growable sprite pool in a single Container so Pixi batches the (mostly L1)
// cloud into few draw calls. Pixel-perfect: nearest-neighbour textures, no antialias.
import { Application, Assets, Sprite, Texture, TextureSource, Container, Graphics, Rectangle } from 'pixi.js';
import { SCENE, SIM, ORB } from '../data/tuning';
import type { MakoWorld } from './makoWorld';
import type { Collector } from './collector';

import orbL1Url from '../assets/orb_l1.png';
import orbL2Url from '../assets/orb_l2.png';
import orbL3Url from '../assets/orb_l3.png';
import collectorUrl from '../assets/collector.png';
import harvesterBgUrl from '../assets/harvester_bg.png';
import chargeSheetUrl from '../assets/harvester_charge_sheet.png';

const PPU = SIM.pixelsPerUnit; // texture import scale (16 px/unit)

// Default framing: show the shaft — orbs spawn in the pit (y≈0.85) and rise to the
// harvester/accelerator at the top (y≈9.9), with the collectors flanking (x≈-19..6).
// [minX, maxX, minY, maxY] in world units.
const FRAME = { minX: -21, maxX: 8, minY: -1, maxY: 16.5 };

// charge sheet: 10 frames of 55×31 at x = 1 + 56·i, y = 1.
const CHARGE_FRAMES = 10;
const CHARGE_FRAME = { w: 55, h: 31, x0: 1, y: 1, stride: 56 };

/** Orthographic depth camera: world units → screen pixels (Y up). */
export class Camera {
  cx = (FRAME.minX + FRAME.maxX) / 2;
  cy = (FRAME.minY + FRAME.maxY) / 2;
  ppu = 40;
  w = 800;
  h = 600;

  resize(w: number, h: number): void {
    this.w = w;
    this.h = h;
    this.ppu = Math.min(w / (FRAME.maxX - FRAME.minX), h / (FRAME.maxY - FRAME.minY));
  }
  sx(wx: number): number {
    return this.w / 2 + (wx - this.cx) * this.ppu;
  }
  sy(wy: number): number {
    return this.h / 2 - (wy - this.cy) * this.ppu;
  }
}

export class Renderer {
  readonly camera = new Camera();
  showZones = false;

  private readonly orbSprites: Sprite[] = [];
  private readonly orbLayer = new Container();
  private readonly beams = new Graphics();
  private readonly zones = new Graphics();
  readonly collectorSprites = new Map<Collector, Sprite>();
  harvesterHit!: Sprite;

  private texOrb!: Record<1 | 2 | 3, Texture>;
  private texCharge: Texture[] = [];
  private harvesterFg!: Sprite;
  private flashBlink = false;

  private constructor(
    readonly app: Application,
    private readonly world: MakoWorld,
  ) {}

  static async create(world: MakoWorld, container: HTMLElement): Promise<Renderer> {
    TextureSource.defaultOptions.scaleMode = 'nearest'; // pixel-perfect for every texture

    const app = new Application();
    await app.init({ background: 0x07080d, resizeTo: container, antialias: false, autoDensity: true });
    container.appendChild(app.canvas);

    // NOTE: Assets.load(array) returns a url-keyed record, so load individually.
    const [orb1, orb2, orb3, collectorTex, harvBg, chargeSheet] = await Promise.all([
      Assets.load<Texture>(orbL1Url),
      Assets.load<Texture>(orbL2Url),
      Assets.load<Texture>(orbL3Url),
      Assets.load<Texture>(collectorUrl),
      Assets.load<Texture>(harvesterBgUrl),
      Assets.load<Texture>(chargeSheetUrl),
    ]);

    const r = new Renderer(app, world);
    r.texOrb = { 1: orb1, 2: orb2, 3: orb3 };
    for (let i = 0; i < CHARGE_FRAMES; i++) {
      r.texCharge.push(
        new Texture({
          source: chargeSheet.source,
          frame: new Rectangle(CHARGE_FRAME.x0 + i * CHARGE_FRAME.stride, CHARGE_FRAME.y, CHARGE_FRAME.w, CHARGE_FRAME.h),
        }),
      );
    }
    r.build(collectorTex, harvBg);
    r.camera.resize(app.screen.width, app.screen.height);
    app.renderer.on('resize', (w: number, h: number) => r.camera.resize(w, h));
    return r;
  }

  private build(collectorTex: Texture, harvBg: Texture): void {
    const stage = this.app.stage;

    this.zones.alpha = 0.5;
    stage.addChild(this.zones);

    // collectors (all of them; visibility follows .active each frame)
    const spriteScale = 1; // set per-frame from camera ppu
    for (const c of [...this.world.manualCollectors, ...this.world.autoCollectors]) {
      const s = new Sprite(collectorTex);
      s.anchor.set(0.5);
      s.scale.set(spriteScale);
      s.eventMode = 'static';
      s.cursor = 'pointer';
      stage.addChild(s);
      this.collectorSprites.set(c, s);
    }

    stage.addChild(this.orbLayer); // orbs over collectors, under the harvester device + beams

    // harvester: background plate + animated charge foreground
    const bg = new Sprite(harvBg);
    bg.anchor.set(0.5);
    this.harvesterFg = new Sprite(this.texCharge[0]);
    this.harvesterFg.anchor.set(0.5);
    const harv = new Container();
    harv.addChild(bg, this.harvesterFg);
    stage.addChild(harv);
    this.harvesterContainer = harv;

    // invisible, generous hit target over the harvester for clicking
    this.harvesterHit = new Sprite(Texture.WHITE);
    this.harvesterHit.anchor.set(0.5);
    this.harvesterHit.alpha = 0;
    this.harvesterHit.eventMode = 'static';
    this.harvesterHit.cursor = 'pointer';
    stage.addChild(this.harvesterHit);

    stage.addChild(this.beams);
  }

  private harvesterContainer!: Container;

  /** One render frame: sync every sprite to current world state. */
  syncFrame(): void {
    const cam = this.camera;
    const orbScale = cam.ppu / PPU;

    this.flashBlink = !this.flashBlink;
    this.syncOrbs(cam, orbScale);
    this.syncHarvester(cam, orbScale);
    this.syncCollectors(cam, orbScale);
    this.drawBeams(cam);
    this.drawZones(cam);
  }

  private syncOrbs(cam: Camera, orbScale: number): void {
    const orbs = this.world.orbs;
    for (let i = 0; i < orbs.length; i++) {
      let s = this.orbSprites[i];
      if (!s) {
        s = new Sprite();
        s.anchor.set(0.5);
        this.orbSprites.push(s);
        this.orbLayer.addChild(s);
      }
      const o = orbs[i];
      s.visible = true;
      s.texture = this.texOrb[o.level];
      s.x = cam.sx(o.x);
      s.y = cam.sy(o.y);

      const bright = o.accelerated || o.rawCollectable;
      // raw orbs are a dim plum cloud; accelerated/collectable orbs pop bright + larger.
      s.tint = bright ? 0xffffff : 0x9a5278;
      s.scale.set(bright ? orbScale * 1.3 : orbScale);
      s.alpha = o.flashing && this.flashBlink ? 0.25 : 1; // FlashRoutine blink
    }
    for (let i = orbs.length; i < this.orbSprites.length; i++) this.orbSprites[i].visible = false;
  }

  private syncHarvester(cam: Camera, scale: number): void {
    const h = SCENE.harvester;
    const sx = cam.sx(h.x);
    const sy = cam.sy(h.y);
    this.harvesterContainer.position.set(sx, sy);
    this.harvesterContainer.scale.set(scale);
    // charge 0→9 maps to frames 0→9
    this.harvesterFg.texture = this.texCharge[Math.min(this.world.harvester.chargeState, CHARGE_FRAMES - 1)];
    this.harvesterHit.position.set(sx, sy);
    this.harvesterHit.width = 3.5 * cam.ppu; // ~ harvester collider footprint
    this.harvesterHit.height = 2.5 * cam.ppu;
  }

  private syncCollectors(cam: Camera, scale: number): void {
    for (const [c, s] of this.collectorSprites) {
      s.visible = c.active;
      if (!c.active) continue;
      s.x = cam.sx(c.x);
      s.y = cam.sy(c.y);
      s.scale.set(scale);
      // tint the tip when actively sucking, to read the manual/auto cadence
      s.tint = c.sucking ? 0xbfefff : 0xffffff;
    }
  }

  private drawBeams(cam: Camera): void {
    const g = this.beams;
    g.clear();

    // collector → target orb beams
    for (const c of this.world.activeCollectors) {
      const t = c.currentTarget;
      if (!t) continue;
      g.moveTo(cam.sx(c.drawTargetX), cam.sy(c.drawTargetY));
      g.lineTo(cam.sx(t.x), cam.sy(t.y));
      g.stroke({ width: c.lasering ? 3 : 1.5, color: 0x9fe8ff, alpha: c.lasering ? 0.95 : 0.55 });
    }

    // harvester summoning beam: channels from the device down into the pit (spawn point)
    if (this.world.harvester.discharging) {
      const h = SCENE.harvester;
      g.moveTo(cam.sx(h.x), cam.sy(h.y));
      g.lineTo(cam.sx(h.x), cam.sy(SCENE.spawnPoint.y));
      g.stroke({ width: 3, color: 0xff8fd0, alpha: 0.9 });
    }
  }

  private drawZones(cam: Camera): void {
    const g = this.zones;
    g.clear();
    if (!this.showZones) return;

    const f = SCENE.field;
    g.rect(cam.sx(f.x - f.halfW), cam.sy(f.y + f.halfH), f.halfW * 2 * cam.ppu, f.halfH * 2 * cam.ppu);
    g.fill({ color: 0x3a6ea5, alpha: 0.06 });
    g.stroke({ width: 1, color: 0x3a6ea5, alpha: 0.3 });

    const a = SCENE.accelerator;
    g.rect(cam.sx(a.x - a.halfW), cam.sy(a.y + a.halfH), a.halfW * 2 * cam.ppu, a.halfH * 2 * cam.ppu);
    g.stroke({ width: 1, color: 0xffd166, alpha: 0.6 });

    for (const c of this.world.activeCollectors) {
      g.circle(cam.sx(c.drawTargetX), cam.sy(c.drawTargetY), ORB.collider.radius * cam.ppu * 3);
      g.stroke({ width: 1, color: 0x9fe8ff, alpha: 0.5 });
    }
  }
}
