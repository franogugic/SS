import { Clock, DatabaseZap, KeyRound, SearchCode, TriangleAlert } from 'lucide-react';

export type DemoPageId = 'login' | 'union' | 'error' | 'boolean' | 'time';
export type ScenarioPageId = Exclude<DemoPageId, 'login'>;

export interface DemoPageMeta {
  id: DemoPageId;
  title: string;
  shortTitle: string;
  description: string;
  icon: typeof KeyRound;
}

export interface ScenarioMeta extends DemoPageMeta {
  id: ScenarioPageId;
  payload: string;
  normalValue: string;
  queryLabel: string;
  queryPath: string;
  queryParam: string;
  endpoint: string;
  safeEndpoint: string;
  expectedResult: string;
  safeResult: string;
}

export const navigationItems: DemoPageMeta[] = [
  {
    id: 'login',
    title: 'Zaobilaženje prijave',
    shortTitle: 'Prijava',
    description: 'Ranjivi login upit dopušta da uvjet uvijek bude istinit.',
    icon: KeyRound
  },
  {
    id: 'union',
    title: 'UNION izvlačenje',
    shortTitle: 'UNION',
    description: 'Napadačev SELECT se spaja s rezultatom legitimnog upita.',
    icon: DatabaseZap
  },
  {
    id: 'error',
    title: 'Error-based napad',
    shortTitle: 'Greške',
    description: 'Poruka baze otkriva detalje o SQL upitu.',
    icon: TriangleAlert
  },
  {
    id: 'boolean',
    title: 'Blind boolean',
    shortTitle: 'Boolean',
    description: 'Istina se zaključuje iz različitog odgovora aplikacije.',
    icon: SearchCode
  },
  {
    id: 'time',
    title: 'Time-based blind',
    shortTitle: 'Vrijeme',
    description: 'Istina se zaključuje iz namjernog kašnjenja odgovora.',
    icon: Clock
  }
];

export const scenarios: Record<ScenarioPageId, ScenarioMeta> = {
  union: {
    id: 'union',
    title: 'UNION izvlačenje',
    shortTitle: 'UNION',
    description: 'Pretraga korisnika se zloupotrebljava tako da se rezultatu izvornog upita pridruže korisnici i lozinke iz tablice.',
    payload: "x%' UNION SELECT [Id], [Username], [Password], [FullName], [Role] FROM [Users] --",
    normalValue: 'admin',
    queryLabel: 'Pretraga korisnika',
    queryPath: '/korisnici',
    queryParam: 'search',
    endpoint: '/api/demo/union-attack',
    safeEndpoint: '/api/demo/union-safe',
    expectedResult: 'Ranjivi endpoint vraća sva tri korisnika i njihove lozinke.',
    safeResult: 'Sigurni endpoint tretira UNION payload kao tekst za pretragu i ne vraća podatke.',
    icon: DatabaseZap
  },
  error: {
    id: 'error',
    title: 'Error-based napad',
    shortTitle: 'Greške',
    description: 'Napadač namjerno lomi SQL sintaksu. Ako aplikacija prikaže poruku baze, ona može otkriti strukturu upita.',
    payload: "'",
    normalValue: 'admin',
    queryLabel: 'Dohvat korisnika po imenu',
    queryPath: '/korisnici/detalji',
    queryParam: 'username',
    endpoint: '/api/demo/error-attack',
    safeEndpoint: '/api/demo/error-safe',
    expectedResult: 'Rezultat prikazuje poruku baze o neispravno zatvorenom navodniku.',
    safeResult: 'Sigurni endpoint ne lomi SQL sintaksu jer je navodnik samo vrijednost parametra.',
    icon: TriangleAlert
  },
  boolean: {
    id: 'boolean',
    title: 'Blind boolean',
    shortTitle: 'Boolean',
    description: 'Payload postavlja pitanje postoji li korisnik kojem korisničko ime počinje slovom a. Ako je odgovor da, ranjivi uvjet postaje istinit i vraća korisnike.',
    payload: "x' OR EXISTS (SELECT 1 FROM [Users] WHERE [Username] LIKE 'a%') --",
    normalValue: 'admin',
    queryLabel: 'Provjera korisničkog imena',
    queryPath: '/korisnici/provjera',
    queryParam: 'username',
    endpoint: '/api/demo/blind-boolean-attack',
    safeEndpoint: '/api/demo/blind-boolean-safe',
    expectedResult: 'Ako postoji barem jedan username koji počinje s a, ranjivi endpoint vraća sve korisnike.',
    safeResult: 'Sigurni endpoint uspoređuje cijeli payload kao korisničko ime i ne izvršava EXISTS uvjet.',
    icon: SearchCode
  },
  time: {
    id: 'time',
    title: 'Time-based blind',
    shortTitle: 'Vrijeme',
    description: 'Payload ubacuje WAITFOR DELAY. Ako se uvjet ispuni, odgovor aplikacije kasni i napadač iz vremena odziva zaključuje da je pretpostavka točna.',
    payload: "admin'; IF EXISTS (SELECT 1 FROM [Users] WHERE [Username]='admin') WAITFOR DELAY '00:00:02'; --",
    normalValue: 'admin',
    queryLabel: 'Provjera korisničkog imena',
    queryPath: '/korisnici/spora-provjera',
    queryParam: 'username',
    endpoint: '/api/demo/time-based-attack',
    safeEndpoint: '/api/demo/time-based-safe',
    expectedResult: 'Ranjivi endpoint izvršava WAITFOR DELAY iz payloada i odgovor kasni oko dvije sekunde.',
    safeResult: 'Sigurni endpoint ne izvršava WAITFOR DELAY i odgovor dolazi bez namjernog kašnjenja.',
    icon: Clock
  }
};
