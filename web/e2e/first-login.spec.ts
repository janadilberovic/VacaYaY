import { test, expect } from '@playwright/test'
import { hrToken } from './helpers'

// The whole point is the signed-out first login, so this file opts out of the shared HR session.
test.use({ storageState: { cookies: [], origins: [] } })

const NEW_PASSWORD = 'E2ePassw0rd'

let createdEmployeeId: number | null = null

test.afterEach(async ({ request }) => {
  if (createdEmployeeId === null) return
  await request.delete(`/api/employees/${createdEmployeeId}`, {
    headers: { Authorization: `Bearer ${hrToken()}` },
  })
  createdEmployeeId = null
})

test('a provisioned employee sets their password on first login', async ({ page, request }) => {
  // Employees are only ever soft-deleted and the address stays taken, so each run needs a
  // brand-new email — Create answers 409 on a duplicate.
  const email = `e2e-${Date.now()}@ingsoftware.com`

  const res = await request.post('/api/employees', {
    headers: { Authorization: `Bearer ${hrToken()}` },
    data: { firstName: 'E2E', lastName: 'Newcomer', email, role: 'Employee', daysOff: 20 },
  })
  expect(res.ok()).toBeTruthy()
  const { employee, tempPassword } = await res.json()
  createdEmployeeId = employee.id

  await page.goto('/login')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(tempPassword)
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page).toHaveURL(/\/change-password$/)
  await expect(page.getByText('Set a new password')).toBeVisible()

  await page.getByLabel('New password').fill(NEW_PASSWORD)
  await page.getByLabel('Confirm password').fill(NEW_PASSWORD)
  await page.getByRole('button', { name: 'Change password' }).click()

  await expect(page).toHaveURL(/\/dashboard$/)
  await expect(page.getByText('Password changed — welcome!')).toBeVisible()

  // Signing back in is what proves the new password was actually persisted.
  await page.getByRole('button', { name: 'Sign out' }).click()
  await expect(page).toHaveURL(/\/login$/)

  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(NEW_PASSWORD)
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page).toHaveURL(/\/dashboard$/)
})

test('the new password must satisfy the listed rules', async ({ page, request }) => {
  const email = `e2e-${Date.now()}-weak@ingsoftware.com`

  const res = await request.post('/api/employees', {
    headers: { Authorization: `Bearer ${hrToken()}` },
    data: { firstName: 'E2E', lastName: 'Weakling', email, role: 'Employee', daysOff: 20 },
  })
  expect(res.ok()).toBeTruthy()
  const { employee, tempPassword } = await res.json()
  createdEmployeeId = employee.id

  await page.goto('/login')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(tempPassword)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await expect(page).toHaveURL(/\/change-password$/)

  await page.getByLabel('New password').fill('short1')
  await page.getByLabel('Confirm password').fill('short1')
  await expect(page.getByRole('button', { name: 'Change password' })).toBeDisabled()

  await page.getByLabel('New password').fill(NEW_PASSWORD)
  await page.getByLabel('Confirm password').fill('E2ePassw0rdX')
  await expect(page.getByRole('button', { name: 'Change password' })).toBeDisabled()

  await page.getByLabel('Confirm password').fill(NEW_PASSWORD)
  await expect(page.getByRole('button', { name: 'Change password' })).toBeEnabled()
})
