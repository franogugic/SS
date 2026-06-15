import type { DemoUser } from '../types/authDemo';

interface UserTableProps {
  users: DemoUser[];
  loading: boolean;
  error: string | null;
}

export function UserTable({ users, loading, error }: UserTableProps) {
  return (
    <section className="min-w-0 rounded-lg border border-stone-200 bg-white p-4 shadow-sm sm:p-6">
      <h2 className="text-xl font-semibold">Korisnici u bazi</h2>
      <p className="mt-2 text-sm leading-6 text-stone-600">
        Baza za demo sadrži samo korisnike. Lozinke su vidljive namjerno, da se jasno pokaže što ranjivi upit može otkriti.
      </p>

      <div className="mt-5">
        {loading && <p className="text-sm text-stone-600">Učitavanje korisnika...</p>}
        {error && <p className="text-sm font-medium text-rose-700">{error}</p>}
        {!loading && !error && (
          <>
            <div className="grid gap-3 md:hidden">
              {users.map((user) => (
                <article key={user.id} className="rounded-md border border-stone-200 p-3 text-sm">
                  <div className="flex items-center justify-between gap-3">
                    <p className="font-semibold">{user.username}</p>
                    <span className="rounded-full bg-stone-100 px-2 py-1 text-xs font-medium">{user.role}</span>
                  </div>
                  <p className="mt-2 text-stone-700">{user.fullName}</p>
                  <p className="mt-2 break-all font-mono text-xs text-stone-600">lozinka: {user.password}</p>
                </article>
              ))}
            </div>

            <div className="hidden overflow-x-auto md:block">
              <table className="w-full min-w-[640px] border-collapse text-left text-sm">
                <thead>
                  <tr className="border-b border-stone-200 text-stone-500">
                    <th className="py-2 pr-3 font-medium">ID</th>
                    <th className="py-2 pr-3 font-medium">Korisničko ime</th>
                    <th className="py-2 pr-3 font-medium">Lozinka</th>
                    <th className="py-2 pr-3 font-medium">Ime</th>
                    <th className="py-2 pr-3 font-medium">Uloga</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((user) => (
                    <tr key={user.id} className="border-b border-stone-100">
                      <td className="py-3 pr-3">{user.id}</td>
                      <td className="py-3 pr-3 font-semibold">{user.username}</td>
                      <td className="py-3 pr-3 font-mono text-xs">{user.password}</td>
                      <td className="py-3 pr-3">{user.fullName}</td>
                      <td className="py-3 pr-3">{user.role}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </div>
    </section>
  );
}
