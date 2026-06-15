import { useEffect, useState } from 'react';
import { fetchUsers } from '../api/demoApi';
import type { DemoUser } from '../types/authDemo';

export function useUsers() {
  const [users, setUsers] = useState<DemoUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    fetchUsers()
      .then((data) => {
        if (active) {
          setUsers(data);
          setError(null);
          setLoading(false);
        }
      })
      .catch(() => {
        if (active) {
          setError('Backend ili baza nisu dostupni.');
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, []);

  return { users, loading, error };
}
