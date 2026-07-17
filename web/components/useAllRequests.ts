'use client'

import { useEffect, useState } from 'react'
import { leaveRequests, leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import type { LeaveRequestDto, LeaveTypeDto } from '@/lib/types'

export function useAllRequests() {
  const [requests, setRequests] = useState<LeaveRequestDto[]>([])
  const [typeMap, setTypeMap] = useState<Map<number, LeaveTypeDto>>(new Map())
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    Promise.all([leaveRequests.all(), leaveTypesApi.all()])
      .then(([reqs, types]) => {
        if (!active) return
        setRequests(reqs)
        setTypeMap(new Map(types.map((t) => [t.id, t])))
      })
      .catch(() => active && setRequests([]))
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [])

  /** Replace one request in place after a review action. */
  const patch = (updated: LeaveRequestDto) =>
    setRequests((list) => list.map((r) => (r.id === updated.id ? updated : r)))

  return { requests, typeMap, loading, patch }
}
