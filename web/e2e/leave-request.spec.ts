import { test, expect } from '@playwright/test'
import { pad, range } from '../lib/dates'

const MONTHS_AHEAD = 3

// The calendar disables days already covered by a pending/approved request, so a fixed range
// would only be selectable on the first run. Book a Monday–Tuesday far out and hand it back in
// afterEach, which keeps the suite re-runnable.
const monday = (() => {
  const now = new Date()
  const d = new Date(now.getFullYear(), now.getMonth() + MONTHS_AHEAD, 1)
  while (d.getDay() !== 1) d.setDate(d.getDate() + 1)
  return d
})()

const START_DAY = monday.getDate()
const END_DAY = START_DAY + 1
const MONTH_ISO = `${monday.getFullYear()}-${pad(monday.getMonth() + 1)}`
const START_ISO = `${MONTH_ISO}-${pad(START_DAY)}`
const END_ISO = `${MONTH_ISO}-${pad(END_DAY)}`

test.afterEach(async ({ page }) => {
  if (!page.url().startsWith('http')) return
  const token = await page.evaluate(() => localStorage.getItem('vacayay.token'))
  if (!token) return

  const headers = { Authorization: `Bearer ${token}` }
  const res = await page.request.get('/api/leave-requests/mine?pageSize=100&status=Pending', {
    headers,
  })
  const { items } = await res.json()
  for (const r of items) {
    if (r.startDate.startsWith(START_ISO)) {
      await page.request.post(`/api/leave-requests/${r.id}/cancel`, { headers })
    }
  }
})

test('a submitted request shows up in My Requests as pending', async ({ page }) => {
  await page.goto('/dashboard')
  await page.getByRole('button', { name: 'Request Leave', exact: true }).click()

  const modal = page.locator('.modal-card')
  await expect(modal).toBeVisible()

  await modal.getByRole('button', { name: /^Annual/ }).click()

  for (let i = 0; i < MONTHS_AHEAD; i++) {
    await modal.getByRole('button', { name: '›' }).click()
  }
  await modal.getByRole('button', { name: String(START_DAY), exact: true }).click()
  await modal.getByRole('button', { name: String(END_DAY), exact: true }).click()

  await modal.getByLabel(/Reason/).fill('Automated end-to-end check.')
  await modal.getByRole('button', { name: 'Submit request' }).click()

  await expect(page.getByText('Leave request submitted — pending HR review.')).toBeVisible()
  await expect(modal).toBeHidden()

  await page.getByRole('button', { name: 'My Requests' }).click()
  await expect(page).toHaveURL(/\/requests$/)
  await page.getByRole('button', { name: 'Pending', exact: true }).click()

  const row = page.locator('button').filter({ hasText: range(START_ISO, END_ISO) })
  await expect(row).toContainText('Pending')
  await expect(row).toContainText('Annual')
})

test('submitting an empty form reports the missing fields', async ({ page }) => {
  await page.goto('/dashboard')
  await page.getByRole('button', { name: 'Request Leave', exact: true }).click()

  const modal = page.locator('.modal-card')
  await modal.getByRole('button', { name: 'Submit request' }).click()

  await expect(modal.getByText('Pick a leave type.')).toBeVisible()
  await expect(modal.getByText('Start date is required.')).toBeVisible()
  await expect(modal).toBeVisible()
})
