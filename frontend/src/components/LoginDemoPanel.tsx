import { ShieldCheck, ShieldOff } from 'lucide-react';
import type { LoginRequest } from '../types/authDemo';

interface LoginDemoPanelProps {
  form: LoginRequest;
  busy: boolean;
  variant?: 'attack';
  onChange: (form: LoginRequest) => void;
  onAttackPreset: () => void;
  onVulnerableLogin: () => void;
  onSafeLogin: () => void;
}

export function LoginDemoPanel({
  form,
  busy,
  onChange,
  onAttackPreset,
  onVulnerableLogin,
  onSafeLogin
}: LoginDemoPanelProps) {
  return (
    <section className="min-w-0 rounded-lg border border-stone-200 bg-white p-4 shadow-sm sm:p-6">
      <p className="text-sm font-semibold uppercase text-rose-700">Scenarij napada</p>
      <h2 className="mt-2 text-2xl font-semibold text-ink">Zaobilaženje prijave</h2>
      <p className="mt-3 leading-7 text-stone-700">
        Ranjivi login spaja korisničko ime i lozinku direktno u SQL tekst. Payload može promijeniti WHERE uvjet tako da prijava prođe bez ispravne lozinke.
      </p>

      <div className="mt-6 grid gap-4">
        <label className="grid gap-2">
          <span className="text-sm font-medium text-stone-700">Korisničko ime</span>
          <input
            className="h-11 rounded-md border border-stone-300 px-3 outline-none focus:border-teal-700 focus:ring-2 focus:ring-teal-100"
            value={form.username}
            onChange={(event) => onChange({ ...form, username: event.target.value })}
          />
        </label>

        <label className="grid gap-2">
          <span className="text-sm font-medium text-stone-700">Lozinka</span>
          <input
            className="h-11 rounded-md border border-stone-300 px-3 outline-none focus:border-teal-700 focus:ring-2 focus:ring-teal-100"
            value={form.password}
            onChange={(event) => onChange({ ...form, password: event.target.value })}
          />
        </label>
      </div>

      <div className="mt-6 grid gap-3 sm:flex sm:flex-wrap">
        <button
          className="rounded-md border border-stone-300 px-4 py-2 text-sm font-semibold hover:bg-stone-50"
          type="button"
          onClick={onAttackPreset}
        >
          Ubaci napad
        </button>
        <button
          className="inline-flex items-center justify-center gap-2 rounded-md bg-rose-700 px-4 py-2 text-sm font-semibold text-white hover:bg-rose-800 disabled:opacity-60"
          type="button"
          disabled={busy}
          onClick={onVulnerableLogin}
        >
          <ShieldOff className="h-4 w-4" aria-hidden="true" />
          Bez zaštite
        </button>
        <button
          className="inline-flex items-center justify-center gap-2 rounded-md bg-teal-700 px-4 py-2 text-sm font-semibold text-white hover:bg-teal-800 disabled:opacity-60"
          type="button"
          disabled={busy}
          onClick={onSafeLogin}
        >
          <ShieldCheck className="h-4 w-4" aria-hidden="true" />
          Sa zaštitom
        </button>
      </div>
    </section>
  );
}
