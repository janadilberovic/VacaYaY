'use client'

import { useRouter, usePathname } from 'next/navigation'
import { useAuth } from '@/state/auth'
import { useTheme } from '@/state/theme'
import { useLeaveModal } from '@/state/leaveModal'
import { initials } from '@/lib/format'

interface NavItem {
  label: string
  href?: string
  onClick?: () => void
}

export function Sidebar() {
  const { user, logout } = useAuth()
  const { theme, toggle } = useTheme()
  const leaveModal = useLeaveModal()
  const router = useRouter()
  const pathname = usePathname()

  const main: NavItem[] = [
    { label: 'Dashboard', href: '/dashboard' },
    { label: 'Request Leave', onClick: leaveModal.open },
    { label: 'My Requests', href: '/requests' },
    { label: 'Profile', href: '/profile' },
  ]

  const hr: NavItem[] = [
    { label: 'HR Dashboard', href: '/hr/dashboard' },
    { label: 'Requests', href: '/hr/requests' },
    { label: 'Employees', href: '/hr/employees' },
    { label: 'Leave Types', href: '/hr/leave-types' },
  ]

  const isHr = user?.role === 'HR'
  const roleLabel = isHr ? 'HR Manager' : 'Employee'

  function renderNav(items: NavItem[]) {
    return items.map((it) => {
      const active = !!it.href && (pathname === it.href || pathname.startsWith(it.href + '/'))
      return (
        <button
          key={it.label}
          onClick={() => (it.href ? router.push(it.href) : it.onClick?.())}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            padding: '8px 10px',
            border: 'none',
            borderRadius: 8,
            cursor: 'pointer',
            textAlign: 'left',
            fontSize: 13.5,
            fontWeight: 500,
            background: active ? 'var(--surface2)' : 'transparent',
            color: active ? 'var(--text)' : 'var(--text2)',
          }}
        >
          <span
            style={{
              width: 6,
              height: 6,
              borderRadius: '50%',
              background: active ? 'var(--accent)' : 'var(--border)',
            }}
          />
          {it.label}
        </button>
      )
    })
  }

  return (
    <div
      style={{
        width: 236,
        flexShrink: 0,
        display: 'flex',
        flexDirection: 'column',
        background: 'var(--surface)',
        borderRight: '1px solid var(--border)',
      }}
    >
      <div style={{ padding: '20px 20px 14px', fontSize: 17, fontWeight: 700, letterSpacing: '-0.02em' }}>
        Vaca<span style={{ color: 'var(--accent)' }}>YAY</span>
      </div>

      <div style={{ padding: '6px 12px', display: 'flex', flexDirection: 'column', gap: 2 }}>
        {renderNav(main)}
      </div>

      {isHr && (
        <>
          <div
            style={{
              padding: '14px 22px 4px',
              fontSize: 11,
              fontWeight: 600,
              letterSpacing: '.08em',
              textTransform: 'uppercase',
              color: 'var(--text3)',
            }}
          >
            HR
          </div>
          <div style={{ padding: '2px 12px', display: 'flex', flexDirection: 'column', gap: 2 }}>
            {renderNav(hr)}
          </div>
        </>
      )}

      <div
        style={{
          marginTop: 'auto',
          padding: 12,
          display: 'flex',
          flexDirection: 'column',
          gap: 8,
          borderTop: '1px solid var(--border)',
        }}
      >
        <button
          onClick={logout}
          style={{
            background: 'none',
            border: 'none',
            color: 'var(--text3)',
            fontSize: 12,
            cursor: 'pointer',
            textAlign: 'left',
            padding: '2px 10px',
          }}
        >
          Sign out
        </button>
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            padding: '8px 10px',
            borderRadius: 10,
            background: 'var(--surface2)',
          }}
        >
          <div
            style={{
              width: 30,
              height: 30,
              borderRadius: '50%',
              background: 'var(--accent)',
              color: '#fff',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: 12,
              fontWeight: 600,
              flexShrink: 0,
            }}
          >
            {user ? initials(user.firstName, user.lastName) : '—'}
          </div>
          <div style={{ minWidth: 0, flex: 1 }}>
            <div
              style={{
                fontSize: 13,
                fontWeight: 600,
                whiteSpace: 'nowrap',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }}
            >
              {user ? `${user.firstName} ${user.lastName}` : ''}
            </div>
            <div style={{ fontSize: 11.5, color: 'var(--text3)' }}>{roleLabel}</div>
          </div>
          <button
            onClick={toggle}
            title="Toggle theme"
            style={{
              border: '1px solid var(--border)',
              background: 'var(--surface)',
              color: 'var(--text2)',
              borderRadius: 7,
              padding: '4px 8px',
              fontSize: 11,
              fontWeight: 600,
              cursor: 'pointer',
            }}
          >
            {theme === 'light' ? 'Dark' : 'Light'}
          </button>
        </div>
      </div>
    </div>
  )
}
