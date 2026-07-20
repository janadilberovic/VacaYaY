'use client'

import { useEffect, useState } from 'react'
import { leaveRequests } from './endpoints'

const cache = new Map<number, Promise<string[]>>()

function fetchYear(year: number): Promise<string[]> {
  let p = cache.get(year)
  if (!p) {
    p = leaveRequests.holidays(year).catch(() => [] as string[])
    cache.set(year, p)
  }
  return p
}

/** Public-holiday ISO dates for the given years, fetched from the API (the single
 *  source of truth) and cached per year. */
export function useHolidays(years: number[]): ReadonlySet<string> {
  const [days, setDays] = useState<ReadonlySet<string>>(new Set())
  const key = years.join(',')

  useEffect(() => {
    let active = true
    Promise.all(years.map(fetchYear)).then((lists) => {
      if (active) setDays(new Set(lists.flat()))
    })
    return () => {
      active = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key])

  return days
}
