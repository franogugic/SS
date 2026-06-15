import type { DemoResult } from '../types/authDemo';

interface ResultPanelProps {
  title: string;
  emptyText: string;
  result: DemoResult | null;
  error: string | null;
}

export function ResultPanel({ title: panelTitle, emptyText, result, error }: ResultPanelProps) {
  const title = result && 'scenario' in result ? result.scenario : result?.mode;

  return (
    <section className="min-w-0 rounded-lg border border-stone-200 bg-white p-4 shadow-sm sm:p-5">
      <h2 className="text-lg font-semibold sm:text-xl">{panelTitle}</h2>

      {error && <p className="mt-4 rounded-md bg-rose-50 p-3 text-sm font-medium text-rose-800">{error}</p>}

      {!error && !result && (
        <p className="mt-4 text-sm text-stone-600">{emptyText}</p>
      )}

      {result && (
        <div className="mt-5 grid min-w-0 gap-5">
          <div className={result.success ? 'rounded-md bg-emerald-50 p-4 text-emerald-900' : 'rounded-md bg-stone-100 p-4 text-stone-800'}>
            <p className="text-sm font-semibold">{title}</p>
            <p className="mt-1">{result.message}</p>
            {'elapsedMilliseconds' in result && (
              <p className="mt-2 text-sm">Vrijeme odziva: {result.elapsedMilliseconds} ms</p>
            )}
          </div>

          <div>
            <p className="mb-2 text-sm font-semibold text-stone-700">SQL koji se prikazuje u demu</p>
            <pre className="max-w-full overflow-x-auto whitespace-pre-wrap break-words rounded-md bg-stone-950 p-3 text-xs leading-5 text-teal-100 sm:p-4 sm:text-sm sm:leading-6">
              <code>{result.executedSql}</code>
            </pre>
          </div>

          {'databaseMessage' in result && result.databaseMessage && (
            <div>
              <p className="mb-2 text-sm font-semibold text-stone-700">Odgovor aplikacije / baze</p>
              <pre className="max-w-full overflow-x-auto whitespace-pre-wrap break-words rounded-md bg-amber-50 p-3 text-xs leading-5 text-amber-950 sm:p-4 sm:text-sm sm:leading-6">
                <code>{result.databaseMessage}</code>
              </pre>
            </div>
          )}

          <div>
            <p className="mb-2 text-sm font-semibold text-stone-700">Vraćeni korisnici</p>
            {result.users.length === 0 ? (
              <p className="text-sm text-stone-600">Nema korisnika.</p>
            ) : (
              <div className="grid gap-2">
                {result.users.map((user) => (
                  <div key={user.id} className="min-w-0 rounded-md border border-stone-200 p-3 text-sm">
                    <span className="font-semibold">{user.username}</span> - lozinka:{' '}
                    <span className="break-all font-mono text-xs">{user.password}</span> - {user.fullName}, {user.role}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </section>
  );
}
