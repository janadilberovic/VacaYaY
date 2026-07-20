'use client'

import { useLeaveModal } from '@/state/leaveModal'
import { useMyRequests } from '@/components/useMyRequests'
import { RequestList } from '@/components/RequestList'

export default function MyRequestsPage() {
  const { open } = useLeaveModal()
  const { requests, typeMap, loading } = useMyRequests()

  return (
    <>
      <div className="page-head">
        <div className="page-h">My requests</div>
        <button className="btn btn-primary" onClick={open}>＋ Request leave</button>
      </div>
      <RequestList requests={requests} typeMap={typeMap} loading={loading} />
    </>
  )
}
