import type { ReactNode } from 'react';

interface PageShellProps {
  children: ReactNode;
}

export function PageShell({ children }: PageShellProps) {
  return (
    <main className="min-h-screen bg-paper text-ink">
      <div className="mx-auto flex w-full max-w-7xl flex-col gap-5 px-3 py-4 sm:gap-7 sm:px-6 sm:py-6 lg:gap-8 lg:px-8">
        {children}
      </div>
    </main>
  );
}
