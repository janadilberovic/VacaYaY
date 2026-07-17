'use client'

import type { ReactNode } from 'react'

export function Avatar({ text, size = 26 }: { text: string; size?: number }) {
  return (
    <span
      style={{
        width: size,
        height: size,
        borderRadius: '50%',
        background: 'var(--surface2)',
        color: 'var(--text2)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: size < 30 ? 10.5 : 12,
        fontWeight: 650,
        flexShrink: 0,
      }}
    >
      {text}
    </span>
  )
}

export function ListSkeleton({ rows = 3 }: { rows?: number }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
      {Array.from({ length: rows }).map((_, i) => (
        <div
          key={i}
          style={{
            height: 58,
            borderRadius: 12,
            background: 'var(--surface2)',
            animation: 'shimmer 1.2s infinite',
            animationDelay: `${i * 0.15}s`,
          }}
        />
      ))}
    </div>
  )
}

export function EmptyState({
  title,
  desc,
  icon = '○',
  action,
}: {
  title: string
  desc: string
  icon?: ReactNode
  action?: ReactNode
}) {
  return (
    <div
      style={{
        border: '1px dashed var(--border)',
        borderRadius: 14,
        padding: '44px 20px',
        textAlign: 'center',
        animation: 'fade .3s',
      }}
    >
      <div
        style={{
          width: 40,
          height: 40,
          borderRadius: '50%',
          background: 'var(--surface2)',
          margin: '0 auto 12px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--text3)',
          fontSize: 18,
        }}
      >
        {icon}
      </div>
      <div style={{ fontWeight: 600, marginBottom: 4 }}>{title}</div>
      <div style={{ color: 'var(--text3)', fontSize: 13, marginBottom: action ? 16 : 0 }}>{desc}</div>
      {action}
    </div>
  )
}

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div
      style={{
        background: 'var(--pill-rejected-bg)',
        color: 'var(--pill-rejected-fg)',
        borderRadius: 10,
        padding: '12px 14px',
        fontSize: 13,
      }}
    >
      {message}
    </div>
  )
}
