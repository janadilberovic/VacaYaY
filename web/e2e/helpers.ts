import { readFileSync } from 'node:fs'
import path from 'node:path'
import { expect, type Page } from '@playwright/test'
import { pad } from '../lib/dates'
import { HR_STATE } from '../playwright.config'

/** The HR bearer token straight out of the saved session, for tests that run signed out and
 *  still need HR to arrange something first. */
export function hrToken(): string {
  const state = JSON.parse(readFileSync(path.resolve(__dirname, '..', HR_STATE), 'utf8'))
  for (const origin of state.origins ?? []) {
    const entry = origin.localStorage?.find((e: { name: string }) => e.name === 'vacayay.token')
    if (entry) return entry.value
  }
  throw new Error(`No vacayay.token found in ${HR_STATE}`)
}

export async function signInAndSaveState(
  page: Page,
  emailVar: string,
  passwordVar: string,
  statePath: string,
) {
  const email = process.env[emailVar]
  const password = process.env[passwordVar]
  if (!email || !password) {
    throw new Error(`Missing ${emailVar} / ${passwordVar} — see web/.env.example`)
  }

  await page.goto('/login')
  await page.getByLabel('Email').fill(email)
  await page.getByLabel('Password').fill(password)
  await page.getByRole('button', { name: 'Sign in' }).click()

  // An account still on its TempPassword lands on /change-password instead, so E2E accounts
  // must already have a real password set.
  await expect(page).toHaveURL(/\/dashboard$/)

  await page.context().storageState({ path: statePath })
}

export async function authHeaders(page: Page) {
  const token = await page.evaluate(() => localStorage.getItem('vacayay.token'))
  return { Authorization: `Bearer ${token}` }
}

function mondayAfter(daysFromNow: number): string {
  const d = new Date()
  d.setDate(d.getDate() + daysFromNow)
  while (d.getDay() !== 1) d.setDate(d.getDate() + 1)
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

/** Approving is irreversible, so runs leave Approved requests behind — and the API rejects a
 *  range overlapping one. Walk forward a week at a time to the first day this employee has not
 *  already booked, which keeps the suite runnable indefinitely. */
export async function freeMonday(page: Page, weeksOut: number): Promise<string> {
  const headers = await authHeaders(page)
  // pageSize caps at 100, so sort the furthest-out requests first — those are the ones this
  // suite books, and the ones a new booking could collide with.
  const res = await page.request.get(
    '/api/leave-requests/mine?pageSize=100&sortBy=StartDate&sortDescending=true',
    { headers },
  )
  expect(res.ok()).toBeTruthy()
  const { items } = await res.json()
  const taken = new Set<string>(
    items
      .filter((r: { status: string }) => r.status === 'Pending' || r.status === 'Approved')
      .map((r: { startDate: string }) => r.startDate.slice(0, 10)),
  )

  let offset = weeksOut * 7
  while (taken.has(mondayAfter(offset))) offset += 7
  return mondayAfter(offset)
}

/** Arrange a one-day pending request through the API. Sick leave doesn't count against the
 *  balance, so repeated runs can't drain the account. */
export async function createPendingRequest(
  page: Page,
  day: string,
  reason: string,
): Promise<number> {
  const headers = await authHeaders(page)
  const types = await (await page.request.get('/api/leave-types', { headers })).json()
  const sick = types.find((t: { name: string }) => t.name === 'Sick')

  const res = await page.request.post('/api/leave-requests', {
    headers,
    data: { leaveTypeId: sick.id, startDate: day, endDate: day, reason },
  })
  expect(res.ok()).toBeTruthy()

  return (await res.json()).id as number
}

export async function cancelIfPending(page: Page, id: number) {
  const headers = await authHeaders(page)
  const res = await page.request.get(`/api/leave-requests/${id}`, { headers })
  if (res.ok() && (await res.json()).status === 'Pending') {
    await page.request.post(`/api/leave-requests/${id}/cancel`, { headers })
  }
}
