export type UserRole = 'Employee' | 'HR'

export type LeaveRequestStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'

export type LeaveColor = 'Gray' | 'Red' | 'Orange' | 'Yellow' | 'Green' | 'Blue' | 'Purple' | 'Pink'

export type LeaveTypeName =
  | 'Annual'
  | 'Sick'
  | 'Paid'
  | 'Unpaid'
  | 'Maternity'
  | 'Paternity'
  | 'Parental'
  | 'Bereavement'
  | 'Compassionate'
  | 'Marriage'
  | 'Study'
  | 'Sabbatical'
  | 'JuryDuty'
  | 'Military'
  | 'Religious'
  | 'Personal'

export interface AuthUser {
  id: number
  firstName: string
  lastName: string
  email: string
  role: UserRole
  department: string | null
  jobTitle: string | null
  daysOff: number
  profileImageUrl: string | null
}

export interface AuthResponse {
  accessToken: string | null
  expiresAtUtc: string | null
  mustChangePassword: boolean
  user: AuthUser | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface EmployeeQuery {
  page?: number
  pageSize?: number
  archived?: boolean
}

export type LeaveRequestSortField =
  'Default' | 'StartDate' | 'EndDate' | 'CreatedAt' | 'EmployeeName' | 'Status'

export interface LeaveRequestQuery {
  page?: number
  pageSize?: number
  status?: LeaveRequestStatus
  employeeId?: number
  leaveTypeName?: LeaveTypeName
  sortBy?: LeaveRequestSortField
  sortDescending?: boolean
}

export interface LeaveRequestSummary {
  totalCount: number
  countByStatus: Partial<Record<LeaveRequestStatus, number>>
  daysByType: Array<{ leaveTypeId: number; leaveTypeName: LeaveTypeName; workingDays: number }>
}

export interface EmployeeDto {
  id: number
  firstName: string
  lastName: string
  email: string
  role: UserRole
  department: string | null
  jobTitle: string | null
  hireDate: string | null
  employmentStartDate: string | null
  employmentEndDate: string | null
  daysOff: number
  profileImageUrl: string | null
  isActive: boolean
}

export interface CreateEmployeeRequest {
  firstName: string
  lastName: string
  email: string
  role: UserRole
  department: string | null
  jobTitle: string | null
  hireDate: string | null
  employmentStartDate: string | null
  employmentEndDate: string | null
  daysOff: number
}

export interface CreateEmployeeResponse {
  employee: EmployeeDto
  tempPassword: string
}

export interface LegacyEmployeeRosterItem {
  legacyId: number
  firstName: string
  lastName: string
  email: string
  department: string | null
  title: string | null
  hiredOn: string | null
  contractEnd: string | null
  daysOff: number
  alreadyImported: boolean
}

export interface ImportLegacyEmployeesResult {
  imported: number
  skipped: number
  notFound: number
  invalid: number
  importedEmployees: EmployeeDto[]
  roster: LegacyEmployeeRosterItem[]
}

export interface LeaveRequestDto {
  id: number
  employeeId: number
  employeeName: string
  leaveTypeId: number
  leaveTypeName: LeaveTypeName
  startDate: string
  endDate: string
  workingDays: number
  reason: string | null
  status: LeaveRequestStatus
  createdAt: string
  hrComment: string | null
  hrName: string | null
  reviewedAt: string | null
}

export interface CreateLeaveRequestRequest {
  leaveTypeId: number
  startDate: string
  endDate: string
  reason: string | null
}

export interface LeaveBalance {
  daysOff: number
  pendingDays: number
  remainingDays: number
}

export interface LeaveTypeDto {
  id: number
  name: LeaveTypeName
  color: LeaveColor | null
  isPaid: boolean
  countsAgainstBalance: boolean
}

export interface CreateLeaveTypeRequest {
  name: LeaveTypeName
  color: LeaveColor | null
  isPaid: boolean
  countsAgainstBalance: boolean
}

export interface UpdateLeaveTypeRequest {
  color: LeaveColor | null
  isPaid: boolean
  countsAgainstBalance: boolean
}
