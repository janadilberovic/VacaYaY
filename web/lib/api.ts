const TOKEN_KEY = 'vacayay.token'

export function getToken(): string | null {
  if (typeof window === 'undefined') return null
  return window.localStorage.getItem(TOKEN_KEY)
}

export function setToken(token: string | null) {
  if (typeof window === 'undefined') return
  if (token) window.localStorage.setItem(TOKEN_KEY, token)
  else window.localStorage.removeItem(TOKEN_KEY)
}

/** RFC7807 problem details, plus FluentValidation's `errors` bag on 400. */
export class ApiError extends Error {
  status: number
  detail?: string
  errors?: Record<string, string[]>

  constructor(status: number, title: string, detail?: string, errors?: Record<string, string[]>) {
    super(title)
    this.status = status
    this.detail = detail
    this.errors = errors
  }

  /** First validation message if present, else the problem title/detail. */
  get firstMessage(): string {
    if (this.errors) {
      const first = Object.values(this.errors)[0]
      if (first && first.length) return first[0]
    }
    return this.detail || this.message
  }
}

type Body =
  | { kind: 'json'; value: unknown }
  | { kind: 'form'; value: Record<string, string> }
  | { kind: 'none' }

async function request<T>(method: string, path: string, body: Body): Promise<T> {
  const headers: Record<string, string> = {}
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`

  let payload: BodyInit | undefined
  if (body.kind === 'json') {
    headers['Content-Type'] = 'application/json'
    payload = JSON.stringify(body.value)
  } else if (body.kind === 'form') {
    headers['Content-Type'] = 'application/x-www-form-urlencoded'
    payload = new URLSearchParams(body.value).toString()
  }

  const res = await fetch(`/api${path}`, { method, headers, body: payload })

  if (res.status === 204) return undefined as T

  const text = await res.text()
  const data = text ? JSON.parse(text) : null

  if (!res.ok) {
    const title = data?.title || res.statusText || 'Request failed'
    throw new ApiError(res.status, title, data?.detail, data?.errors)
  }

  return data as T
}

export const api = {
  get: <T>(path: string) => request<T>('GET', path, { kind: 'none' }),
  post: <T>(path: string, value?: unknown) =>
    request<T>('POST', path, value === undefined ? { kind: 'none' } : { kind: 'json', value }),
  put: <T>(path: string, value: unknown) => request<T>('PUT', path, { kind: 'json', value }),
  del: <T>(path: string) => request<T>('DELETE', path, { kind: 'none' }),
  postForm: <T>(path: string, value: Record<string, string>) =>
    request<T>('POST', path, { kind: 'form', value }),
  putForm: <T>(path: string, value: Record<string, string>) =>
    request<T>('PUT', path, { kind: 'form', value }),
}
