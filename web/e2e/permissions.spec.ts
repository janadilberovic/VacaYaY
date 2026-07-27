import { test, expect } from '@playwright/test'
import { authHeaders } from './helpers'

test('the HR section is hidden from an employee', async ({ page }) => {
  await page.goto('/dashboard')

  await expect(page.getByRole('button', { name: 'My Requests' })).toBeVisible()
  await expect(page.getByText('Employee', { exact: true })).toBeVisible()

  await expect(page.getByRole('button', { name: 'HR Dashboard' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Employees' })).toHaveCount(0)
  await expect(page.getByRole('button', { name: 'Leave Types' })).toHaveCount(0)
})

test('an employee opening an HR page is sent back to the dashboard', async ({ page }) => {
  await page.goto('/hr/requests')

  await expect(page).toHaveURL(/\/dashboard$/)
  await expect(page.getByRole('button', { name: 'HR Dashboard' })).toHaveCount(0)
})

// Hiding the links is cosmetic — the guarantee is the API refusing the call, so assert that too.
test('the API refuses an HR-only endpoint for an employee', async ({ page }) => {
  await page.goto('/dashboard')
  const headers = await authHeaders(page)

  const all = await page.request.get('/api/leave-requests', { headers })
  expect(all.status()).toBe(403)

  const mine = await page.request.get('/api/leave-requests/mine', { headers })
  expect(mine.ok()).toBeTruthy()
})
