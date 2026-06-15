import type { ScenarioMeta } from '../data/scenarios';

interface ScenarioAttackPageProps {
  scenario: ScenarioMeta;
  value: string;
  busy: boolean;
  onChange: (value: string) => void;
  onUseNormalValue: () => void;
  onUsePayload: () => void;
  onRunVulnerable: (endpoint: string, payload: string) => void;
  onRunSafe: (endpoint: string, payload: string) => void;
}

export function ScenarioAttackPage({
  scenario,
  value,
  busy,
  onChange,
  onUseNormalValue,
  onUsePayload,
  onRunVulnerable,
  onRunSafe
}: ScenarioAttackPageProps) {
  const Icon = scenario.icon;
  const queryPreview = `${scenario.queryPath}?${scenario.queryParam}=${encodeURIComponent(value)}`;

  return (
    <section className="min-w-0 rounded-lg border border-stone-200 bg-white p-4 shadow-sm sm:p-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
        <span className="grid h-12 w-12 shrink-0 place-items-center rounded-md bg-teal-50 text-teal-700">
          <Icon className="h-6 w-6" aria-hidden="true" />
        </span>
        <div className="max-w-3xl">
          <p className="text-sm font-semibold uppercase text-teal-700">Scenarij napada</p>
          <h2 className="mt-1 text-2xl font-semibold text-ink">{scenario.title}</h2>
          <p className="mt-3 leading-7 text-stone-700">{scenario.description}</p>
        </div>
      </div>

      <div className="mt-6 grid min-w-0 gap-5 xl:grid-cols-[1fr_0.9fr]">
        <div className="min-w-0">
          <label className="grid gap-2">
            <span className="text-sm font-semibold text-stone-700">{scenario.queryLabel}</span>
            <textarea
              className="min-h-28 resize-y rounded-md border border-stone-300 px-3 py-3 font-mono text-sm leading-6 outline-none focus:border-teal-700 focus:ring-2 focus:ring-teal-100"
              value={value}
              onChange={(event) => onChange(event.target.value)}
            />
          </label>

          <div className="mt-3 rounded-md border border-stone-200 bg-stone-50 p-3">
            <p className="text-xs font-semibold uppercase text-stone-500">Simulirani query</p>
            <code className="mt-2 block break-all text-sm text-stone-800">{queryPreview}</code>
          </div>

          <div className="mt-3 grid gap-2 sm:flex sm:flex-wrap">
            <button
              className="rounded-md border border-stone-300 px-3 py-2 text-sm font-semibold hover:bg-stone-50"
              type="button"
              onClick={onUseNormalValue}
            >
              Normalan unos
            </button>
            <button
              className="rounded-md border border-stone-300 px-3 py-2 text-sm font-semibold hover:bg-stone-50"
              type="button"
              onClick={onUsePayload}
            >
              Ubaci payload
            </button>
          </div>
        </div>

        <div className="grid gap-3">
          <div className="rounded-md border border-rose-200 bg-rose-50 p-4">
            <p className="text-sm font-semibold text-rose-900">Bez zaštite</p>
            <p className="mt-2 text-sm leading-6 text-rose-900">{scenario.expectedResult}</p>
            <button
              className="mt-4 w-full rounded-md bg-rose-700 px-4 py-2 text-sm font-semibold text-white hover:bg-rose-800 disabled:opacity-60 sm:w-auto"
              type="button"
              disabled={busy}
              onClick={() => onRunVulnerable(scenario.endpoint, value)}
            >
              Pokreni ranjivo
            </button>
          </div>

          <div className="rounded-md border border-teal-200 bg-teal-50 p-4">
            <p className="text-sm font-semibold text-teal-900">Sa zaštitom</p>
            <p className="mt-2 text-sm leading-6 text-teal-900">{scenario.safeResult}</p>
            <button
              className="mt-4 w-full rounded-md bg-teal-700 px-4 py-2 text-sm font-semibold text-white hover:bg-teal-800 disabled:opacity-60 sm:w-auto"
              type="button"
              disabled={busy}
              onClick={() => onRunSafe(scenario.safeEndpoint, value)}
            >
              Pokreni sigurno
            </button>
          </div>
        </div>
      </div>
    </section>
  );
}
