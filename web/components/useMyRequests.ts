'use client'

import { useCallback, useEffect, useState } from 'react'
import { leaveRequests, leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import { useLeaveModal } from '@/state/leaveModal'
import type { LeaveRequestDto, LeaveTypeDto } from '@/lib/types'

export interface MyRequestsData {
  requests: LeaveRequestDto[]
  typeMap: Map<number, LeaveTypeDto>
  loading: boolean
  reload: () => void
}

/** Loads the current user's requests + the leave-type catalog (for colour/flags),
 *  refetching whenever a new request is submitted via the shared modal. */
export function useMyRequests(): MyRequestsData {
  const { refreshKey } = useLeaveModal()
  const [requests, setRequests] = useState<LeaveRequestDto[]>([])
  const [typeMap, setTypeMap] = useState<Map<number, LeaveTypeDto>>(new Map())
  const [loading, setLoading] = useState(true)
  const [tick, setTick] = useState(0)

  const reload = useCallback(() => setTick((t) => t + 1), [])

  useEffect(() => {
    let active = true
    setLoading(true)
    Promise.all([leaveRequests.mine(), leaveTypesApi.all()])
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
  }, [refreshKey, tick])

  return { requests, typeMap, loading, reload }
}
