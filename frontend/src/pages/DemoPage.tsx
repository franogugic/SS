import { useEffect, useState } from 'react';
import { runSafeLogin, runScenario, runVulnerableLogin } from '../api/demoApi';
import { AppHeader } from '../components/AppHeader';
import { PageShell } from '../components/PageShell';
import { ResultPanel } from '../components/ResultPanel';
import { UserTable } from '../components/UserTable';
import { navigationItems, scenarios, type DemoPageId, type ScenarioPageId } from '../data/scenarios';
import { useUsers } from '../hooks/useUsers';
import type { DemoResult, LoginRequest, LoginResult } from '../types/authDemo';
import { LoginBypassPage } from './LoginBypassPage';
import { ScenarioAttackPage } from './ScenarioAttackPage';

const attackPayload: LoginRequest = {
  username: "' OR 1=1 --",
  password: 'nije_bitno'
};

const initialScenarioValues = {
  union: scenarios.union.payload,
  error: scenarios.error.payload,
  boolean: scenarios.boolean.payload,
  time: scenarios.time.payload
};

const scenarioPageIds: ScenarioPageId[] = ['union', 'error', 'boolean', 'time'];

function isDemoPageId(value: string | null): value is DemoPageId {
  return navigationItems.some((item) => item.id === value);
}

function isScenarioPageId(value: DemoPageId): value is ScenarioPageId {
  return scenarioPageIds.includes(value as ScenarioPageId);
}

function readStateFromUrl() {
  const params = new URLSearchParams(window.location.search);
  const pageParam = params.get('page');
  const activePage = isDemoPageId(pageParam) ? pageParam : 'login';
  const scenarioValues = { ...initialScenarioValues };

  if (isScenarioPageId(activePage)) {
    const scenario = scenarios[activePage];
    const queryValue = params.get(scenario.queryParam);

    if (queryValue !== null) {
      scenarioValues[activePage] = queryValue;
    }
  }

  return { activePage, scenarioValues };
}

function writeStateToUrl(page: DemoPageId, scenarioValues: Record<ScenarioPageId, string>) {
  const params = new URLSearchParams();
  params.set('page', page);

  if (isScenarioPageId(page)) {
    const scenario = scenarios[page];
    params.set(scenario.queryParam, scenarioValues[page]);
  }

  const query = params.toString();
  window.history.replaceState(null, '', `${window.location.pathname}?${query}`);
}

export function DemoPage() {
  const initialUrlState = readStateFromUrl();
  const { users, loading, error: usersError } = useUsers();
  const [activePage, setActivePage] = useState<DemoPageId>(initialUrlState.activePage);
  const [form, setForm] = useState<LoginRequest>({ username: 'admin', password: 'kriva_lozinka' });
  const [scenarioValues, setScenarioValues] = useState<Record<ScenarioPageId, string>>(initialUrlState.scenarioValues);
  const [vulnerableResult, setVulnerableResult] = useState<DemoResult | null>(null);
  const [safeResult, setSafeResult] = useState<DemoResult | null>(null);
  const [vulnerableError, setVulnerableError] = useState<string | null>(null);
  const [safeError, setSafeError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    writeStateToUrl(activePage, scenarioValues);
  }, [activePage, scenarioValues]);

  useEffect(() => {
    function handlePopState() {
      const nextState = readStateFromUrl();
      setActivePage(nextState.activePage);
      setScenarioValues(nextState.scenarioValues);
      clearResults();
    }

    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, []);

  function selectPage(page: DemoPageId) {
    setActivePage(page);
    clearResults();
  }

  function clearResults() {
    setVulnerableResult(null);
    setSafeResult(null);
    setVulnerableError(null);
    setSafeError(null);
  }

  async function executeLogin(action: (request: LoginRequest) => Promise<LoginResult>, mode: 'vulnerable' | 'safe') {
    setBusy(true);
    mode === 'vulnerable' ? setVulnerableError(null) : setSafeError(null);

    try {
      const response = await action(form);
      mode === 'vulnerable' ? setVulnerableResult(response) : setSafeResult(response);
    } catch {
      const message = 'Zahtjev nije uspio. Provjeri backend i bazu.';
      mode === 'vulnerable' ? setVulnerableError(message) : setSafeError(message);
    } finally {
      setBusy(false);
    }
  }

  async function executeScenario(endpoint: string, payload: string, mode: 'vulnerable' | 'safe') {
    setBusy(true);
    mode === 'vulnerable' ? setVulnerableError(null) : setSafeError(null);

    try {
      const response = await runScenario(endpoint, payload);
      mode === 'vulnerable' ? setVulnerableResult(response) : setSafeResult(response);
    } catch {
      const message = 'Scenarij nije uspio. Provjeri backend i bazu.';
      mode === 'vulnerable' ? setVulnerableError(message) : setSafeError(message);
    } finally {
      setBusy(false);
    }
  }

  function updateScenarioValue(page: ScenarioPageId, value: string) {
    setScenarioValues((current) => ({
      ...current,
      [page]: value
    }));
  }

  return (
    <PageShell>
      <AppHeader activePage={activePage} items={navigationItems} onSelect={selectPage} />

      <div className="grid gap-6 lg:grid-cols-[0.9fr_1.1fr]">
        {activePage === 'login' && (
          <LoginBypassPage
            form={form}
            busy={busy}
            onChange={setForm}
            onAttackPreset={() => setForm(attackPayload)}
            onRunVulnerable={(action) => executeLogin(action, 'vulnerable')}
            onRunSafe={(action) => executeLogin(action, 'safe')}
            vulnerableAction={runVulnerableLogin}
            safeAction={runSafeLogin}
          />
        )}

        {activePage !== 'login' && (
          <ScenarioAttackPage
            scenario={scenarios[activePage as ScenarioPageId]}
            value={scenarioValues[activePage as ScenarioPageId]}
            busy={busy}
            onChange={(value) => updateScenarioValue(activePage as ScenarioPageId, value)}
            onUseNormalValue={() =>
              updateScenarioValue(activePage as ScenarioPageId, scenarios[activePage as ScenarioPageId].normalValue)
            }
            onUsePayload={() =>
              updateScenarioValue(activePage as ScenarioPageId, scenarios[activePage as ScenarioPageId].payload)
            }
            onRunVulnerable={(endpoint, payload) => executeScenario(endpoint, payload, 'vulnerable')}
            onRunSafe={(endpoint, payload) => executeScenario(endpoint, payload, 'safe')}
          />
        )}

        <UserTable users={users} loading={loading} error={usersError} />
      </div>

      <div className="grid min-w-0 gap-6 xl:grid-cols-2">
        <ResultPanel
          title="Bez zaštite"
          emptyText="Pokreni ranjivi tok za prikaz rezultata."
          result={vulnerableResult}
          error={vulnerableError}
        />
        <ResultPanel
          title="Sa zaštitom"
          emptyText="Pokreni sigurni tok za prikaz rezultata."
          result={safeResult}
          error={safeError}
        />
      </div>
    </PageShell>
  );
}
