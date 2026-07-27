import { test as setup } from '@playwright/test'
import { EMPLOYEE_STATE } from '../playwright.config'
import { signInAndSaveState } from './helpers'

setup('authenticate as employee', async ({ page }) => {
  await signInAndSaveState(page, 'E2E_EMPLOYEE_EMAIL', 'E2E_EMPLOYEE_PASSWORD', EMPLOYEE_STATE)
})
