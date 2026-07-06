import type { DemoUser, LoginRequest, LoginResult, ScenarioResult } from '../types/authDemo';

const apiBaseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:8081';

export async function fetchUsers(): Promise<DemoUser[]> {
  const response = await fetch(`${apiBaseUrl}/api/demo/users`);

  if (!response.ok) {
    throw new Error('Korisnici se ne mogu dohvatiti.');
  }

  return response.json();
}

export async function runVulnerableLogin(request: LoginRequest): Promise<LoginResult> {
  return postLogin('/api/demo/vulnerable-login', request);
}

export async function runSafeLogin(request: LoginRequest): Promise<LoginResult> {
  return postLogin('/api/demo/safe-login', request);
}

export async function runScenario(path: string, payload: string): Promise<ScenarioResult> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ payload })
  });

  if (!response.ok) {
    throw new Error('Demo scenarij nije uspio.');
  }

  return response.json();
}

async function postLogin(path: string, request: LoginRequest): Promise<LoginResult> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(request)
  });

  if (!response.ok) {
    throw new Error('Demo zahtjev nije uspio.');
  }

  return response.json();
}
