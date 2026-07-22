'use client'

// Server paging means the page count is unbounded — show a sliding window, not every page.
function pageWindow(current: number, total: number, size = 7): number[] {
  const start = Math.max(1, Math.min(current - Math.floor(size / 2), total - size + 1))
  const end = Math.min(total, start + size - 1)
  return Array.from({ length: end - start + 1 }, (_, i) => start + i)
}

export function Pagination({
  page,
  pageSize,
  pageCount,
  totalCount,
  onPage,
}: {
  page: number
  pageSize: number
  pageCount: number
  totalCount: number
  onPage: (p: number) => void
}) {
  if (pageCount <= 1) return null

  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        marginTop: 14,
      }}
    >
      <div style={{ color: 'var(--text3)', fontSize: 12.5 }}>
        {(page - 1) * pageSize + 1}–{Math.min(page * pageSize, totalCount)} of {totalCount}
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
        <button
          className="btn btn-ghost"
          style={{ width: 30, height: 30, padding: 0 }}
          disabled={page <= 1}
          onClick={() => onPage(page - 1)}
        >
          ‹
        </button>
        {pageWindow(page, pageCount).map((p) => {
          const on = p === page
          return (
            <button
              key={p}
              onClick={() => onPage(p)}
              style={{
                minWidth: 30,
                height: 30,
                border: `1px solid ${on ? 'var(--text)' : 'var(--border)'}`,
                borderRadius: 8,
                background: on ? 'var(--text)' : 'var(--surface)',
                color: on ? 'var(--bg)' : 'var(--text2)',
                cursor: 'pointer',
                fontSize: 12.5,
                fontWeight: 650,
              }}
            >
              {p}
            </button>
          )
        })}
        <button
          className="btn btn-ghost"
          style={{ width: 30, height: 30, padding: 0 }}
          disabled={page >= pageCount}
          onClick={() => onPage(page + 1)}
        >
          ›
        </button>
      </div>
    </div>
  )
}
