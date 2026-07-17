export type UserRole = 'Employee' | 'HR'

export type LeaveRequestStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'

export type LeaveColor =
  | 'Gray'
  | 'Red'
  | 'Orange'
  | 'Yellow'
  | 'Green'
  | 'Blue'
  | 'Purple'
  | 'Pink'

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
