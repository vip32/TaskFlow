
const { chromium, firefox, webkit, devices } = require('playwright');
const helpers = require('./lib/helpers');

// Extra headers from environment variables (if configured)
const __extraHeaders = helpers.getExtraHeadersFromEnv();

/**
 * Utility to merge environment headers into context options.
 * Use when creating contexts with raw Playwright API instead of helpers.createContext().
 * @param {Object} options - Context options
 * @returns {Object} Options with extraHTTPHeaders merged in
 */
function getContextOptionsWithHeaders(options = {}) {
  if (!__extraHeaders) return options;
  return {
    ...options,
    extraHTTPHeaders: {
      ...__extraHeaders,
      ...(options.extraHTTPHeaders || {})
    }
  };
}

(async () => {
  try {
    const browser=await chromium.launch({headless:true}); const page=await browser.newPage(); const errors=[]; page.on('console',m=>{if(m.type()==='error') errors.push(m.text());}); page.on('pageerror',e=>errors.push(e.message)); await page.goto('http://localhost:5003',{waitUntil:'networkidle'}); const hasAudio=await page.evaluate(()=> typeof window.taskflowAudio?.beep === 'function'); await page.getByRole('button',{name:'Start'}).click(); await page.waitForTimeout(500); await page.evaluate(()=>window.taskflowAudio.beep('finish')); await page.waitForTimeout(300); console.log('HAS_AUDIO_FN',hasAudio); console.log('ERRORS',JSON.stringify(errors)); await browser.close();
  } catch (error) {
    console.error('❌ Automation error:', error.message);
    if (error.stack) {
      console.error(error.stack);
    }
    process.exit(1);
  }
})();
