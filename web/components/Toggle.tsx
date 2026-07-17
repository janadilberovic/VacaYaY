'use client'

export function Toggle({ on, onToggle, label }: { on: boolean; onToggle: () => void; label: string }) {
  return (
    <div
      onClick={onToggle}
      style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', cursor: 'pointer' }}
    >
      <span style={{ fontSize: 13.5, fontWeight: 500 }}>{label}</span>
      <span
        style={{
          position: 'relative',
          width: 36,
          height: 20,
          borderRadius: 99,
          background: on ? 'var(--accent)' : 'var(--surface2)',
          transition: 'background .15s',
        }}
      >
        <span
          style={{
            position: 'absolute',
            top: 2,
            left: on ? 18 : 2,
            width: 16,
            height: 16,
            borderRadius: '50%',
            background: '#fff',
            transition: 'left .15s',
          }}
        />
      </span>
    </div>
  )
}
