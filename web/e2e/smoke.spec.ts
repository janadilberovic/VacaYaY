import { test, expect } from '@playwright/test'

// These assert the signed-out experience, so they opt out of the shared HR session.
test.use({ storageState: { cookies: [], origins: [] } })

test('unauthenticated root redirects to the login page', async ({ page }) => {
  await page.goto('/')
  await expect(page).toHaveURL(/\/login$/)
})

test('login page renders the sign-in card', async ({ page }) => {
  await page.goto('/login')
  await expect(page.getByText('Welcome back')).toBeVisible()
  await expect(page.getByLabel('Email')).toBeVisible()
  await expect(page.getByLabel('Password')).toBeVisible()
  await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible()
})

test('submitting empty credentials shows a validation message', async ({ page }) => {
  await page.goto('/login')
  await page.getByRole('button', { name: 'Sign in' }).click()
  await expect(page.getByText('Enter your email and password.')).toBeVisible()
})
