import { test, expect } from '@playwright/test'
import { range } from '../lib/dates'
import { cancelIfPending, createPendingRequest, freeMonday } from './helpers'

let created: number | null = null

test.afterEach(async ({ page }) => {
  if (created === null || !page.url().startsWith('http')) return
  await cancelIfPending(page, created)
  created = null
})

test('an employee withdraws their own pending request', async ({ page }) => {
  await page.goto('/dashboard')
  const day = await freeMonday(page, 30)
  created = await createPendingRequest(page, day, 'Automated cancel check.')

  await page.goto(`/requests/${created}`)
  await expect(page.getByText(range(day, day), { exact: true })).toBeVisible()

  await page.getByRole('button', { name: 'Cancel request' }).click()

  const dialog = page.locator('.modal-card')
  await expect(dialog).toContainText('Cancel this request?')
  await dialog.getByRole('button', { name: 'Cancel request' }).click()

  await expect(page.getByText('Request cancelled.')).toBeVisible()
  await expect(page).toHaveURL(/\/requests$/)

  // Back on the detail page rather than hunting the row in the list: earlier runs leave their
  // own cancelled requests behind, so only the id identifies this one.
  await page.goto(`/requests/${created}`)
  await expect(page.getByText('Cancelled', { exact: true })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Cancel request' })).toHaveCount(0)
})

test('backing out of the confirm dialog leaves the request pending', async ({ page }) => {
  await page.goto('/dashboard')
  const day = await freeMonday(page, 40)
  created = await createPendingRequest(page, day, 'Automated cancel check.')

  await page.goto(`/requests/${created}`)
  await page.getByRole('button', { name: 'Cancel request' }).click()
  await page.getByRole('button', { name: 'Keep it' }).click()

  await expect(page.locator('.modal-card')).toHaveCount(0)
  await expect(page.getByText('Pending', { exact: true })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Cancel request' })).toBeVisible()
})
