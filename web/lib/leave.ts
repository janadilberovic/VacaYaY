import type { LeaveColor, LeaveRequestStatus, LeaveTypeName } from './types'

export const COLOR_HEX: Record<LeaveColor, string> = {
  Gray: '#6b7280',
  Red: '#ef4444',
  Orange: '#f97316',
  Yellow: '#eab308',
  Green: '#22c55e',
  Blue: '#3b82f6',
  Purple: '#8b5cf6',
  Pink: '#ec4899',
}

export const COLOR_ORDER: LeaveColor[] = [
  'Gray',
  'Red',
  'Orange',
  'Yellow',
  'Green',
  'Blue',
  'Purple',
  'Pink',
]

export function colorHex(color: LeaveColor | null | undefined): string {
  return color ? COLOR_HEX[color] : COLOR_HEX.Gray
}

export const LEAVE_TYPE_NAMES: LeaveTypeName[] = [
  'Annual',
  'Sick',
  'Paid',
  'Unpaid',
  'Maternity',
  'Paternity',
  'Parental',
  'Bereavement',
  'Compassionate',
  'Marriage',
  'Study',
  'Sabbatical',
  'JuryDuty',
  'Military',
  'Religious',
  'Personal',
]

export function leaveTypeLabel(name: LeaveTypeName): string {
  return name.replace(/([a-z])([A-Z])/g, '$1 $2')
}

export const STATUS_LABEL: Record<LeaveRequestStatus, string> = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Cancelled: 'Cancelled',
}

/** Lowercase status keys the `--pill-*` CSS variables are named by. */
export function pillKey(status: LeaveRequestStatus): string {
  return status.toLowerCase()
}
