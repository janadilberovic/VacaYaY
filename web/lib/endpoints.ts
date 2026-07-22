import { api } from './api'
import type {
  AuthResponse,
  CreateEmployeeRequest,
  CreateEmployeeResponse,
  CreateLeaveRequestRequest,
  CreateLeaveTypeRequest,
  EmployeeDto,
  EmployeeQuery,
  ImportLegacyEmployeesResult,
  LeaveBalance,
  LeaveRequestDto,
  LeaveRequestQuery,
  LeaveRequestSummary,
  LeaveTypeDto,
  LegacyEmployeeRosterItem,
  PagedResult,
  UpdateLeaveTypeRequest,
} from './types'

export const auth = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/auth/login', { email, password }),
  changePassword: (body: {
    email: string
    currentPassword: string
    newPassword: string
    confirmNewPassword: string
  }) => api.post<AuthResponse>('/auth/change-password', body),
  logout: () => api.post<void>('/auth/logout'),
}

const leaveRequestQuery = (q: LeaveRequestQuery) => {
  const params = new URLSearchParams()
  if (q.page !== undefined) params.set('page', String(q.page))
  if (q.pageSize !== undefined) params.set('pageSize', String(q.pageSize))
  if (q.status) params.set('status', q.status)
  if (q.employeeId !== undefined) params.set('employeeId', String(q.employeeId))
  if (q.leaveTypeName) params.set('leaveTypeName', q.leaveTypeName)
  if (q.sortBy) params.set('sortBy', q.sortBy)
  if (q.sortDescending !== undefined) params.set('sortDescending', String(q.sortDescending))
  const qs = params.toString()
  return qs ? `?${qs}` : ''
}

export const leaveRequests = {
  mine: (q: LeaveRequestQuery = {}) =>
    api.get<PagedResult<LeaveRequestDto>>(`/leave-requests/mine${leaveRequestQuery(q)}`),
  all: (q: LeaveRequestQuery = {}) =>
    api.get<PagedResult<LeaveRequestDto>>(`/leave-requests${leaveRequestQuery(q)}`),
  summary: () => api.get<LeaveRequestSummary>('/leave-requests/summary'),
  balance: () => api.get<LeaveBalance>('/leave-requests/balance'),
  byId: (id: number) => api.get<LeaveRequestDto>(`/leave-requests/${id}`),
  create: (body: CreateLeaveRequestRequest) => api.post<LeaveRequestDto>('/leave-requests', body),
  holidays: (year: number) => api.get<string[]>(`/leave-requests/holidays?year=${year}`),
  approve: (id: number, hrComment: string | null) =>
    api.post<LeaveRequestDto>(`/leave-requests/${id}/approve`, { hrComment }),
  reject: (id: number, hrComment: string | null) =>
    api.post<LeaveRequestDto>(`/leave-requests/${id}/reject`, { hrComment }),
  cancel: (id: number) => api.post<LeaveRequestDto>(`/leave-requests/${id}/cancel`),
}

export const employees = {
  list: (q: EmployeeQuery = {}) => {
    const params = new URLSearchParams()
    if (q.page !== undefined) params.set('page', String(q.page))
    if (q.pageSize !== undefined) params.set('pageSize', String(q.pageSize))
    if (q.archived) params.set('archived', 'true')
    const qs = params.toString()
    return api.get<PagedResult<EmployeeDto>>(`/employees${qs ? `?${qs}` : ''}`)
  },
  me: () => api.get<EmployeeDto>('/employees/me'),
  create: (body: CreateEmployeeRequest) => api.post<CreateEmployeeResponse>('/employees', body),
  archive: (id: number) => api.del<void>(`/employees/${id}`),
  restore: (id: number) => api.post<EmployeeDto>(`/employees/${id}/restore`),
  legacyRoster: () => api.get<LegacyEmployeeRosterItem[]>('/employees/legacy'),
  importLegacy: (legacyIds: number[]) =>
    api.post<ImportLegacyEmployeesResult>('/employees/import-legacy', { legacyIds }),
}

// Leave-type create/update are [FromForm] on the API — send urlencoded, not JSON.
const ltForm = (body: CreateLeaveTypeRequest | UpdateLeaveTypeRequest): Record<string, string> => {
  const form: Record<string, string> = {
    IsPaid: String(body.isPaid),
    CountsAgainstBalance: String(body.countsAgainstBalance),
  }
  if (body.color) form.Color = body.color
  if ('name' in body) form.Name = body.name
  return form
}

export const leaveTypes = {
  all: () => api.get<LeaveTypeDto[]>('/leave-types'),
  create: (body: CreateLeaveTypeRequest) =>
    api.postForm<LeaveTypeDto>('/leave-types', ltForm(body)),
  update: (id: number, body: UpdateLeaveTypeRequest) =>
    api.putForm<LeaveTypeDto>(`/leave-types/${id}`, ltForm(body)),
  archive: (id: number) => api.del<void>(`/leave-types/${id}`),
  restore: (id: number) => api.post<LeaveTypeDto>(`/leave-types/${id}/restore`),
}
