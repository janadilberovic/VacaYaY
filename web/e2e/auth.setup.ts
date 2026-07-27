import { test as setup } from '@playwright/test'
import { HR_STATE } from '../playwright.config'
import { signInAndSaveState } from './helpers'

setup('authenticate as HR', async ({ page }) => {
  await signInAndSaveState(page, 'E2E_HR_EMAIL', 'E2E_HR_PASSWORD', HR_STATE)
})
