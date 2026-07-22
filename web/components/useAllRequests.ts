'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import { leaveRequests, leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import type { LeaveRequestDto, LeaveRequestQuery, LeaveTypeDto, PagedResult } from '@/lib/types'

/** One page of every employee's requests + the leave-type catalog (for colour/flags).
 *  `onOverflow` fires when the requested page is past the end — e.g. the last row on the
 *  last page no longer matches the filter after a review. */
export function useAllRequests(query: LeaveRequestQuery, onOverflow?: (lastPage: number) => void) {
  const [data, setData] = useState<PagedResult<LeaveRequestDto> | null>(null)
  const [typeMap, setTypeMap] = useState<Map<number, LeaveTypeDto>>(new Map())
  const [loading, setLoading] = useState(true)

  const key = JSON.stringify(query)
  const stableQuery = useMemo(() => JSON.parse(key) as LeaveRequestQuery, [key])

  const load = useCallback(async () => {
    try {
      const [res, types] = await Promise.all([leaveRequests.all(stableQuery), leaveTypesApi.all()])
      setTypeMap(new Map(types.map((t) => [t.id, t])))
      if (res.items.length === 0 && res.page > 1) {
        onOverflow?.(Math.max(1, res.totalPages))
        return
      }
      setData(res)
    } catch {
      setData(null)
    } finally {
      setLoading(false)
    }
    // onOverflow is a render-scoped closure; re-running on it would loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [stableQuery])

  useEffect(() => {
    load()
  }, [load])

  /** Replace one request in place after a review action. */
  const patch = (updated: LeaveRequestDto) =>
    setData((d) =>
      d ? { ...d, items: d.items.map((r) => (r.id === updated.id ? updated : r)) } : d,
    )

  return { data, requests: data?.items ?? [], typeMap, loading, patch, reload: load }
}
