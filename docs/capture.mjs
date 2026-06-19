// Captures "field evidence" screenshots of the codex for the Phase Tracker.
// Drives the locally-installed Google Chrome via puppeteer-core (no download).
//   node docs/capture.mjs
import puppeteer from 'puppeteer-core';

const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const URL    = 'file:///Users/jameskelly/Development/LD57/docs/index.html';
const OUT    = '/Users/jameskelly/Development/LD57/docs/shots';

const TARGETS = [
  ['hero',         'p0_hero'],
  ['architecture', 'p0_map'],
  ['economy',      'p0_explorer'],
  ['porting',      'p0_porting'],
];

const browser = await puppeteer.launch({
  executablePath: CHROME,
  headless: true,
  args: ['--no-sandbox', '--hide-scrollbars', '--force-color-profile=srgb'],
});
const page = await browser.newPage();
await page.setViewport({ width: 1440, height: 880, deviceScaleFactor: 1.4 });
await page.goto(URL, { waitUntil: 'networkidle0' });
await page.evaluate(() => document.fonts && document.fonts.ready);
await page.evaluate(() => {
  document.documentElement.style.scrollBehavior = 'auto';
  document.querySelectorAll('.reveal').forEach(r => r.classList.add('in'));
});
await new Promise(r => setTimeout(r, 700));

for (const [id, name] of TARGETS) {
  await page.evaluate((id) => {
    const e = document.getElementById(id);
    const y = e.getBoundingClientRect().top + window.scrollY - 10;
    window.scrollTo(0, Math.max(0, y));
  }, id);
  await new Promise(r => setTimeout(r, 450));
  await page.screenshot({ path: `${OUT}/${name}.jpg`, type: 'jpeg', quality: 85 });
  console.log('captured', name);
}

await browser.close();
console.log('done');
