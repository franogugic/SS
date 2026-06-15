import { LoginDemoPanel } from '../components/LoginDemoPanel';
import type { LoginRequest, LoginResult } from '../types/authDemo';

interface LoginBypassPageProps {
  form: LoginRequest;
  busy: boolean;
  onChange: (form: LoginRequest) => void;
  onAttackPreset: () => void;
  onRunVulnerable: (action: (request: LoginRequest) => Promise<LoginResult>) => void;
  onRunSafe: (action: (request: LoginRequest) => Promise<LoginResult>) => void;
  vulnerableAction: (request: LoginRequest) => Promise<LoginResult>;
  safeAction: (request: LoginRequest) => Promise<LoginResult>;
}

export function LoginBypassPage({
  form,
  busy,
  onChange,
  onAttackPreset,
  onRunVulnerable,
  onRunSafe,
  vulnerableAction,
  safeAction
}: LoginBypassPageProps) {
  return (
    <LoginDemoPanel
      form={form}
      busy={busy}
      variant="attack"
      onChange={onChange}
      onAttackPreset={onAttackPreset}
      onVulnerableLogin={() => onRunVulnerable(vulnerableAction)}
      onSafeLogin={() => onRunSafe(safeAction)}
    />
  );
}
