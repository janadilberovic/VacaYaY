import { test, expect, type Page } from '@playwright/test'
import { range } from '../lib/dates'
import { cancelIfPending, createPendingRequest, freeMonday } from './helpers'

let created: number | null = null

async function openPendingList(page: Page) {
  await page.goto('/hr/requests')
  await page.getByRole('button', { name: 'Pending', exact: true }).click()
  await page.locator('select').last().selectOption({ label: 'Submitted — newest' })
}

test.afterEach(async ({ page }) => {
  if (created === null || !page.url().startsWith('http')) return
  await cancelIfPending(page, created)
  created = null
})

test('HR approves a pending request', async ({ page }) => {
  await page.goto('/dashboard')
  const day = await freeMonday(page, 60)
  created = await createPendingRequest(page, day, 'Automated review check.')

  await openPendingList(page)

  const row = page.getByTestId(`request-row-${created}`)
  await expect(row).toContainText(range(day, day))

  // The glyph is the button's accessible name, so getByRole('button', { name: 'Approve' })
  // would not match here — the label only exists as a title attribute.
  await row.getByTitle('Approve').click()

  const modal = page.locator('.modal-card')
  await expect(modal).toContainText('Approve request')
  await modal.getByLabel(/HR comment/).fill('Enjoy the time off.')
  await modal.getByRole('button', { name: 'Approve', exact: true }).click()

  await expect(page.getByText('Request approved.')).toBeVisible()
  await expect(row).toContainText('Approved')
  await expect(row.getByTitle('Approve')).toHaveCount(0)
})

test('HR rejects a pending request with a comment', async ({ page }) => {
  await page.goto('/dashboard')
  const day = await freeMonday(page, 120)
  created = await createPendingRequest(page, day, 'Automated review check.')

  await openPendingList(page)

  const row = page.getByTestId(`request-row-${created}`)
  await row.getByTitle('Reject').click()

  const modal = page.locator('.modal-card')
  await expect(modal).toContainText('Reject request')
  await modal.getByLabel(/HR comment/).fill('Team is short-staffed that week.')
  await modal.getByRole('button', { name: 'Reject', exact: true }).click()

  await expect(page.getByText('Request rejected.')).toBeVisible()
  await expect(row).toContainText('Rejected')
  await expect(row).toContainText('Team is short-staffed that week.')
})
