'use client'

import { useEffect, type ReactNode } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/state/auth'

export default function HrLayout({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (user && user.role !== 'HR') router.replace('/dashboard')
  }, [user, router])

  if (!user || user.role !== 'HR') return null
  return <>{children}</>
}
