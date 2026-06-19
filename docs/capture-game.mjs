// Captures "field evidence" screenshots of the RUNNING Phase 2 build (the Mako loop)
// for the Phase Tracker. Loads the self-contained docs/builds/phase2.html, drives it
// via the debug panel (which fires the real upgrade topics), lets the cloud build under
// the real rAF loop, then screenshots into docs/shots/.
//   node docs/capture-game.mjs
import puppeteer from 'puppeteer-core';

const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const URL = 'file:///Users/jameskelly/Development/LD57/docs/builds/phase2.html';
const OUT = '/Users/jameskelly/Development/LD57/docs/shots';

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

// Headless so rAF ticks reliably (a headed window can land on an inactive macOS Space
// and throttle rAF). Headless uses software GL, which holds 60fps up to a few hundred
// sprites — fine for these shots; the thousands-of-orbs perf is proven separately by the
// CPU step cost (~0.25ms / 2300 orbs).
const browser = await puppeteer.launch({
  executablePath: CHROME,
  headless: true,
  args: ['--no-sandbox', '--hide-scrollbars', '--force-color-profile=srgb', '--use-gl=angle', '--use-angle=swiftshader'],
});
const page = await browser.newPage();
await page.setViewport({ width: 1440, height: 810, deviceScaleFactor: 1.5 });
await page.goto(URL, { waitUntil: 'networkidle0' });
await page.waitForSelector('#btns button');
const stats = () => page.evaluate(() => document.querySelector('#stats').innerText.replace(/\s+/g, ' '));

// Click a debug button by its visible label (fires the real upgrade topic).
const click = (label, n = 1) =>
  page.evaluate(
    (label, n) => {
      const b = [...document.querySelectorAll('#btns button')].find((x) => x.textContent.startsWith(label));
      for (let i = 0; i < n; i++) b && b.click();
      return !!b;
    },
    label,
    n,
  );

// --- the loop in action: orbs rising, collectors beaming them in, Mako climbing ---
await click('Divine Guidance');
await click('Grow Crystal');
await click('Attenuate', 2);
await click('Amplify', 2);
await click('Luck', 5);
await click('+ Auto Collector', 4);
await click('Faster suck ×2', 2);
await click('+ Auto Harvester', 6);
await click('Burst +250 orbs'); // seed a real cloud so the collectors stay busy
await sleep(12000); // orbs rise → become collectable → collectors steadily reel them in
console.log('p2_loop:', await stats());
await page.screenshot({ path: `${OUT}/p2_loop.jpg`, type: 'jpeg', quality: 88 });

// --- a fuller cloud, still 60fps (kept ≤500 so software-GL capture stays honest) ---
await click('Burst +250 orbs');
await sleep(8000);
console.log('p2_cloud:', await stats());
await page.screenshot({ path: `${OUT}/p2_cloud.jpg`, type: 'jpeg', quality: 88 });

await browser.close();
console.log('done');
