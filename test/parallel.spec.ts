import { test, expect, chromium } from '@playwright/test';
import path from 'path';
import fs from 'fs';
import { stringify } from 'csv-stringify/sync';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const MAX_RETRIES = 1;
const DROPDOWN_SELECTOR = '#electionType';

type ElectionSummary = {
  label: string;
  folder: string;
  affidavits: number;
  pages: number;
  failures: number;
};

test.describe.configure({ timeout: 43_200_000 }); // 12 hours

function logToCSV(candidateName: string, pageIndex: number, rowIndex: number, filePath: string, csvPath: string) {
  const record = {
    Candidate: candidateName,
    Page: pageIndex,
    Row: rowIndex,
    File: filePath
  };
  const csvLine = stringify([record], { header: false });
  fs.appendFileSync(csvPath, csvLine);
}

async function downloadAffidavitsOnPage(page, pageIndex, downloadDir, csvPath): Promise<{ success: number; failed: number }> {
  await page.waitForSelector('table tbody tr', { timeout: 60_000 });
  const rows = await page.locator('table tbody tr').all();
  console.log(`📄 Page ${pageIndex}: Found ${rows.length} candidates`);

  let success = 0;
  let failed = 0;

  for (let i = 0; i < rows.length; i++) {
    let attempt = 0;
    while (attempt < MAX_RETRIES) {
      try {
        const viewMoreLink = rows[i].locator('a:has-text("View more")').first();
        const [popup] = await Promise.all([
          page.waitForEvent('popup', { timeout: 30_000 }),
          viewMoreLink.click()
        ]);

        await popup.waitForLoadState('domcontentloaded', { timeout: 30_000 });
        const heading = await popup.locator('h4').first().innerText();
        const candidateName = heading.trim().replace(/[^\w\s.-]/g, '');
        console.log(`📄 Downloading affidavit for: ${candidateName}`);

        const downloadButton = popup.getByRole('button', { name: 'Download' }).last();
        await downloadButton.waitFor({ state: 'visible', timeout: 30_000 });

        const [download] = await Promise.all([
          popup.waitForEvent('download', { timeout: 30_000 }),
          downloadButton.click()
        ]);

        const safeName = candidateName.replace(/\s+/g, '_');
        const filePath = path.join(downloadDir, `${safeName}_Page${pageIndex}_Row${i + 1}.pdf`);
        await download.saveAs(filePath);

        console.log(`✅ Saved: ${filePath}`);
        logToCSV(candidateName, pageIndex, i + 1, filePath, csvPath);
        success++;

        await popup.close();
        await page.waitForTimeout(500);
        break;
      } catch (err) {
        attempt++;
        console.warn(`⚠️ Retry ${attempt} for candidate at row ${i + 1}:`, err);
        if (attempt === MAX_RETRIES) {
          console.error(`❌ Failed after ${MAX_RETRIES} attempts for row ${i + 1}`);
          failed++;
        }
      }
    }
  }

  return { success, failed };
}

async function processElectionType(value: string, label: string): Promise<ElectionSummary> {
  const browser = await chromium.launch();
  const context = await browser.newContext({ acceptDownloads: true });
  const page = await context.newPage();

  try {
    console.log(`🔄 Processing election type: ${label}`);
    await page.goto('https://affidavit.eci.gov.in/', { timeout: 60_000 });

    await page.selectOption(DROPDOWN_SELECTOR, value);
    const filterButton = page.getByRole('button', { name: 'Filter' });
    await expect(filterButton).toBeVisible({ timeout: 30_000 });

    await Promise.all([
      page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 60_000 }),
      filterButton.click()
    ]);

    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const folderName = `${label}_${timestamp}`;
    const downloadDir = path.join(__dirname, 'downloads', folderName);
    const csvPath = path.join(downloadDir, 'affidavits.csv');
    const summaryPath = path.join(downloadDir, 'summary.txt');

    fs.mkdirSync(downloadDir, { recursive: true });
    fs.writeFileSync(csvPath, 'Candidate,Page,Row,File\n');

    let pageIndex = 1;
    let totalSuccess = 0;
    let totalFailed = 0;

    while (true) {
      const { success, failed } = await downloadAffidavitsOnPage(page, pageIndex, downloadDir, csvPath);
      totalSuccess += success;
      totalFailed += failed;

      const nextLink = page.locator('a:has-text("Next »")');
      const isDisabled =
        (await nextLink.getAttribute('aria-disabled')) === 'true' ||
        (await nextLink.getAttribute('disabled')) !== null ||
        (await nextLink.evaluate(el => el.classList.contains('disabled')));

      if (isDisabled) break;

      await Promise.all([
        page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 60_000 }),
        nextLink.click()
      ]);

      pageIndex++;
    }

    const summary = `Election Type: ${label}
Total Affidavits Downloaded: ${totalSuccess}
Total Failures: ${totalFailed}
Pages Crawled: ${pageIndex}`;
    fs.writeFileSync(summaryPath, summary);
    console.log(`📊 Summary for ${label}:\n${summary}`);

    return {
      label,
      folder: folderName,
      affidavits: totalSuccess,
      pages: pageIndex,
      failures: totalFailed
    };
  } catch (err) {
    console.error(`❌ Error processing ${label}:`, err);
    return {
      label,
      folder: 'N/A',
      affidavits: 0,
      pages: 0,
      failures: 0
    };
  } finally {
    await browser.close();
  }
}

async function getElectionTypes(page): Promise<{ value: string, label: string }[]> {
  const options = await page.locator(`${DROPDOWN_SELECTOR} > option`).all();
  const result: { value: string; label: string }[] = [];
  for (const opt of options) {
    const value = await opt.getAttribute('value');
    const label = await opt.innerText();
    if (value && value.trim()) {
      result.push({ value, label: label.trim().replace(/\s+/g, '_') });
    }
  }
  return result;
}

test('📥 Parallel affidavit downloads with global summary', async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage();

  await page.goto('https://affidavit.eci.gov.in/', { timeout: 60_000 });

  const electionTypes = await getElectionTypes(page);
  await browser.close();

  const summaries = await Promise.all(
    electionTypes.map(({ value, label }) => processElectionType(value, label))
  );

  const totalAffidavits = summaries.reduce((sum, s) => sum + s.affidavits, 0);
  const totalPages = summaries.reduce((sum, s) => sum + s.pages, 0);
  const totalFailures = summaries.reduce((sum, s) => sum + s.failures, 0);

  const globalSummary = [
    `🗳 Election Types Processed: ${summaries.length}`,
    `📄 Total Pages Crawled: ${totalPages}`,
    `📥 Total Affidavits Downloaded: ${totalAffidavits}`,
    `❌ Total Failures: ${totalFailures}`,
    '',
    ...summaries.map(s =>
      `• ${s.label}: ${s.affidavits} downloaded, ${s.failures} failed, ${s.pages} pages → downloads/${s.folder}`
    )
  ].join('\n');

  const globalPath = path.join(__dirname, 'downloads', 'global_summary.txt');
  fs.writeFileSync(globalPath, globalSummary);
  console.log(`📊 Global Summary:\n${globalSummary}`);
});
