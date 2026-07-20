'use client'

export interface ConfirmSpec {
  title: string
  message: string
  confirmLabel: string
  onConfirm: () => void
}

export function ConfirmDialog({ spec, onCancel }: { spec: ConfirmSpec; onCancel: () => void }) {
  return (
    <div className="modal-backdrop" style={{ alignItems: 'center', padding: 20, zIndex: 60 }}>
      <div className="modal-card" style={{ width: 400, padding: '22px 24px' }}>
        <div style={{ fontSize: 15.5, fontWeight: 700, marginBottom: 6 }}>{spec.title}</div>
        <div style={{ color: 'var(--text2)', fontSize: 13.5, marginBottom: 20 }}>{spec.message}</div>
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
          <button
            className="btn"
            style={{ background: 'none', border: '1px solid var(--border)', color: 'var(--text2)', padding: '8px 14px', fontSize: 13 }}
            onClick={onCancel}
          >
            Keep it
          </button>
          <button
            className="btn"
            style={{ background: 'var(--pill-rejected-fg)', color: '#fff', padding: '8px 16px', fontSize: 13 }}
            onClick={spec.onConfirm}
          >
            {spec.confirmLabel}
          </button>
        </div>
      </div>
    </div>
  )
}
