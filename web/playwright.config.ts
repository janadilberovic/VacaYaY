import path from 'node:path'
import { defineConfig, devices } from '@playwright/test'
import dotenv from 'dotenv'

// `next dev` reads web/.env itself, but the Playwright runner does not — auth.setup.ts needs
// the E2E_* credentials from it. Real env vars (CI) win over the file.
dotenv.config({ path: path.resolve(__dirname, '.env'), quiet: true })

export const HR_STATE = 'e2e/.auth/hr.json'
export const EMPLOYEE_STATE = 'e2e/.auth/employee.json'

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  // Two sessions, two projects: the employee one is kept separate so a missing employee
  // account only fails permissions.spec.ts instead of blocking the whole suite.
  projects: [
    { name: 'setup', testMatch: /auth\.setup\.ts/ },
    { name: 'setup-employee', testMatch: /employee\.setup\.ts/ },
    {
      name: 'chromium',
      testIgnore: /permissions\.spec\.ts/,
      use: { ...devices['Desktop Chrome'], storageState: HR_STATE },
      dependencies: ['setup'],
    },
    {
      name: 'employee',
      testMatch: /permissions\.spec\.ts/,
      use: { ...devices['Desktop Chrome'], storageState: EMPLOYEE_STATE },
      dependencies: ['setup-employee'],
    },
  ],
  // The API serves no CORS headers, so next.config.mjs proxies /api/* to Kestrel — both
  // servers have to be up. Swagger's document is the readiness probe; it is Development-only.
  webServer: [
    {
      command: 'dotnet run --project ../src/VacaYAY.Api',
      url: 'http://localhost:5266/swagger/v1/swagger.json',
      reuseExistingServer: !process.env.CI,
      timeout: 180_000,
    },
    {
      command: 'npm run dev',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
  ],
})
