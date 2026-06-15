export interface DemoUser {
  id: number;
  username: string;
  password: string;
  fullName: string;
  role: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResult {
  success: boolean;
  mode: string;
  message: string;
  executedSql: string;
  users: DemoUser[];
}

export interface ScenarioResult {
  success: boolean;
  scenario: string;
  message: string;
  executedSql: string;
  databaseMessage: string | null;
  elapsedMilliseconds: number;
  users: DemoUser[];
}

export type DemoResult = LoginResult | ScenarioResult;
