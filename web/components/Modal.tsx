'use client'

import type { ReactNode } from 'react'

interface Props {
  onClose: () => void
  width: number
  align?: 'top' | 'center'
  children: ReactNode
}

/** Backdrop + centered card. Clicking the backdrop closes; clicks inside don't. */
export function Modal({ onClose, width, align = 'top', children }: Props) {
  return (
    <div
      className="modal-backdrop"
      style={{
        alignItems: align === 'center' ? 'center' : 'flex-start',
        padding: align === 'center' ? 20 : '8vh 20px',
      }}
      onClick={onClose}
    >
      <div className="modal-card" style={{ width }} onClick={(e) => e.stopPropagation()}>
        {children}
      </div>
    </div>
  )
}

export function ModalHeader({ title, onClose }: { title: string; onClose: () => void }) {
  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '20px 24px 0',
      }}
    >
      <div style={{ fontSize: 17, fontWeight: 700 }}>{title}</div>
      <button className="modal-x" onClick={onClose}>
        ✕
      </button>
    </div>
  )
}
