'use client'

import { useEffect, useMemo, useState } from 'react'
import { Modal, ModalHeader } from './Modal'
import { Calendar } from './Calendar'
import { ErrorBanner } from './ui'
import { useToast } from '@/state/toast'
import { useLeaveModal } from '@/state/leaveModal'
import { leaveRequests, leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import { colorHex, leaveTypeLabel } from '@/lib/leave'
import {
  eachDayISO,
  estimateWorkingDays,
  fmt,
  isoDate,
  range,
  todayISO,
  workingDaysNoun,
} from '@/lib/dates'
import { useHolidays } from '@/lib/holidays'
import type { LeaveBalance, LeaveTypeDto } from '@/lib/types'

interface Errors {
  type?: string
  start?: string
  end?: string
  form?: string
}

export function RequestLeaveModal() {
  const { toast } = useToast()
  const { isOpen, close, notifyCreated } = useLeaveModal()

  const [types, setTypes] = useState<LeaveTypeDto[]>([])
  const [typeId, setTypeId] = useState<number | null>(null)
  const [start, setStart] = useState('')
  const [end, setEnd] = useState('')
  const [reason, setReason] = useState('')
  const [errs, setErrs] = useState<Errors>({})
  const [submitting, setSubmitting] = useState(false)
  const [bookedDays, setBookedDays] = useState<Set<string>>(new Set())
  const [balance, setBalance] = useState<LeaveBalance | null>(null)

  useEffect(() => {
    if (!isOpen) return
    setTypeId(null)
    setStart('')
    setEnd('')
    setReason('')
    setErrs({})
    setSubmitting(false)
    setBookedDays(new Set())
    setBalance(null)
    leaveRequests
      .balance()
      .then(setBalance)
      .catch(() => setBalance(null))
    leaveTypesApi
      .all()
      .then(setTypes)
      .catch(() => setTypes([]))
    leaveRequests
      .mine({ pageSize: 100 })
      .then((reqs) => {
        const days = new Set<string>()
        for (const r of reqs.items) {
          if (r.status === 'Pending' || r.status === 'Approved') {
            for (const iso of eachDayISO(isoDate(r.startDate), isoDate(r.endDate))) days.add(iso)
          }
        }
        setBookedDays(days)
      })
      .catch(() => setBookedDays(new Set()))
  }, [isOpen])

  const selected = useMemo(() => types.find((t) => t.id === typeId) ?? null, [types, typeId])
  const previewYears = useMemo(() => {
    if (!start) return []
    const from = Number(start.slice(0, 4))
    const to = Number((end || start).slice(0, 4))
    const years: number[] = []
    for (let y = from; y <= to; y++) years.push(y)
    return years
  }, [start, end])
  const holidays = useHolidays(previewYears)
  const previewDays =
    start && end && end >= start ? estimateWorkingDays(start, end, holidays) : null
  // Provisional warning only — CreateAsync in LeaveRequestService is the authoritative check.
  // remainingDays comes from the API and already nets off pending requests.
  const overBalance =
    previewDays !== null &&
    !!selected?.countsAgainstBalance &&
    !!balance &&
    previewDays > balance.remainingDays

  if (!isOpen) return null

  function pickDay(iso: string) {
    setErrs((e) => ({ ...e, start: undefined, end: undefined, form: undefined }))
    if (!start || (start && end)) {
      setStart(iso)
      setEnd('')
    } else if (iso < start) {
      setStart(iso)
    } else {
      setEnd(iso)
    }
  }

  function validate(): Errors {
    const today = todayISO()
    const e: Errors = {}
    if (!typeId) e.type = 'Pick a leave type.'
    if (!start) e.start = 'Start date is required.'
    else if (start < today) e.start = 'Start date can’t be in the past.'
    if (!end) e.end = 'End date is required.'
    else if (start && end < start) e.end = 'End date must be on or after the start date.'
    return e
  }

  async function submit() {
    const found = validate()
    if (Object.keys(found).length) {
      setErrs(found)
      return
    }
    setSubmitting(true)
    setErrs({})
    try {
      await leaveRequests.create({
        leaveTypeId: typeId!,
        startDate: start,
        endDate: end,
        reason: reason.trim() || null,
      })
      toast('Leave request submitted — pending HR review.')
      notifyCreated()
      close()
    } catch (err) {
      if (err instanceof ApiError) {
        setErrs({ form: err.firstMessage })
      } else {
        setErrs({ form: 'Something went wrong. Please try again.' })
      }
      setSubmitting(false)
    }
  }

  const rangeSummary = start ? (end ? range(start, end) : `${fmt(start)} → …`) : 'Select dates'

  return (
    <Modal onClose={close} width={560}>
      <ModalHeader title="Request leave" onClose={close} />
      <div style={{ padding: '18px 24px', display: 'flex', flexDirection: 'column', gap: 18 }}>
        <div>
          <div style={{ fontSize: 12.5, fontWeight: 600, marginBottom: 8 }}>Leave type</div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 8 }}>
            {types.map((t) => {
              const active = typeId === t.id
              const badge = `${t.isPaid ? 'Paid' : 'Unpaid'}${t.countsAgainstBalance ? ' · Balance' : ''}`
              return (
                <button
                  key={t.id}
                  onClick={() => {
                    setTypeId(t.id)
                    setErrs((e) => ({ ...e, type: undefined }))
                  }}
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 9,
                    padding: '10px 12px',
                    borderRadius: 10,
                    cursor: 'pointer',
                    textAlign: 'left',
                    background: active ? 'var(--surface2)' : 'var(--surface)',
                    border: `1px solid ${active ? 'var(--accent)' : 'var(--border)'}`,
                  }}
                >
                  <span
                    style={{
                      width: 9,
                      height: 9,
                      borderRadius: '50%',
                      background: colorHex(t.color),
                      flexShrink: 0,
                    }}
                  />
                  <span style={{ fontSize: 13, fontWeight: 600, color: 'var(--text)', flex: 1 }}>
                    {leaveTypeLabel(t.name)}
                  </span>
                  <span style={{ fontSize: 10.5, fontWeight: 600, color: 'var(--text3)' }}>
                    {badge}
                  </span>
                </button>
              )
            })}
          </div>
          {errs.type && (
            <div className="err" style={{ marginTop: 6 }}>
              {errs.type}
            </div>
          )}
        </div>

        <div>
          <div
            style={{
              display: 'flex',
              alignItems: 'baseline',
              justifyContent: 'space-between',
              marginBottom: 8,
            }}
          >
            <div style={{ fontSize: 12.5, fontWeight: 600 }}>Dates</div>
            <div
              style={{
                fontSize: 12.5,
                fontWeight: 600,
                color: start ? 'var(--text)' : 'var(--text3)',
              }}
            >
              {rangeSummary}
            </div>
          </div>
          {errs.start && (
            <div className="err" style={{ marginTop: 0, marginBottom: 6 }}>
              {errs.start}
            </div>
          )}
          {errs.end && (
            <div className="err" style={{ marginTop: 0, marginBottom: 6 }}>
              {errs.end}
            </div>
          )}
          <Calendar start={start} end={end} onSelectDay={pickDay} bookedDays={bookedDays} />
        </div>

        {previewDays !== null && (
          <div
            style={{
              background: 'var(--surface2)',
              borderRadius: 10,
              padding: '12px 14px',
              fontSize: 13,
              display: 'flex',
              alignItems: 'center',
              gap: 10,
            }}
          >
            <span style={{ fontWeight: 700, fontSize: 14 }}>{previewDays}</span>
            <span style={{ color: 'var(--text2)' }}>
              {workingDaysNoun(previewDays)} — excludes weekends &amp; public holidays
            </span>
          </div>
        )}
        {overBalance && (
          <div
            style={{
              background: 'var(--pill-pending-bg)',
              color: 'var(--pill-pending-fg)',
              borderRadius: 10,
              padding: '10px 14px',
              fontSize: 12.5,
            }}
          >
            This exceeds your remaining balance of {balance!.remainingDays} days
            {balance!.pendingDays > 0 &&
              ` (${balance!.pendingDays} already held by pending requests)`}
            .
          </div>
        )}

        <div>
          <label htmlFor="f-reason" className="field-label">
            Reason <span style={{ color: 'var(--text3)', fontWeight: 500 }}>(optional)</span>
          </label>
          <textarea
            id="f-reason"
            rows={3}
            className="textarea"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="Add a short note for HR…"
          />
        </div>

        {errs.form && <ErrorBanner message={errs.form} />}
      </div>
      <div className="modal-foot">
        <button
          className="btn"
          style={{
            background: 'none',
            border: '1px solid var(--border)',
            color: 'var(--text2)',
            padding: '9px 16px',
            fontSize: 13,
          }}
          onClick={close}
        >
          Cancel
        </button>
        <button
          className="btn btn-primary"
          style={{ padding: '9px 20px', fontSize: 13 }}
          disabled={submitting}
          onClick={submit}
        >
          {submitting ? 'Submitting…' : 'Submit request'}
        </button>
      </div>
    </Modal>
  )
}
