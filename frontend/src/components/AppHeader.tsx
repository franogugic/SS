import type { DemoPageId, DemoPageMeta } from '../data/scenarios';

interface AppHeaderProps {
  activePage: DemoPageId;
  items: DemoPageMeta[];
  onSelect: (page: DemoPageId) => void;
}

export function AppHeader({ activePage, items, onSelect }: AppHeaderProps) {
  return (
    <header className="min-w-0 rounded-lg border border-stone-200 bg-white p-4 shadow-sm sm:p-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p className="text-sm font-semibold uppercase text-teal-700">SQL injection demo</p>
          <h1 className="mt-2 text-2xl font-semibold text-ink sm:text-3xl">Napadi i obrana nad demo bazom korisnika</h1>
          <p className="mt-2 max-w-3xl text-sm leading-6 text-stone-700">
            Odaberi vrstu napada, pokreni pripremljeni payload i usporedi rezultat s parametriziranim upitom.
          </p>
        </div>
      </div>

      <nav className="mt-5 grid grid-cols-2 gap-2 sm:flex sm:overflow-x-auto sm:pb-1" aria-label="Vrste SQL injection demo scenarija">
        {items.map((item) => {
          const Icon = item.icon;
          const active = item.id === activePage;

          return (
            <button
              key={item.id}
              className={
                active
                  ? 'inline-flex min-h-10 items-center justify-center gap-2 rounded-md bg-ink px-3 py-2 text-sm font-semibold text-white sm:shrink-0'
                  : 'inline-flex min-h-10 items-center justify-center gap-2 rounded-md border border-stone-200 px-3 py-2 text-sm font-semibold text-stone-700 hover:bg-stone-50 sm:shrink-0'
              }
              type="button"
              onClick={() => onSelect(item.id)}
            >
              <Icon className="h-4 w-4" aria-hidden="true" />
              {item.shortTitle}
            </button>
          );
        })}
      </nav>
    </header>
  );
}
