# SQL injection demo

Jednostavna full-stack aplikacija za demonstraciju SQL injection napada i obrane.

## Tehnologije

- Backend: ASP.NET Core Web API, MVC kontroleri, EF Core, SQL Server
- Frontend: React, TypeScript, Tailwind CSS, Vite
- Baza: SQL Server u Dockeru, samo demo korisnici

## Pokretanje

1. Pokreni bazu:

```bash
docker compose up -d
```

2. Pokreni backend:

```bash
dotnet run --project backend/src/SqlInjectionPresenter.Api/SqlInjectionPresenter.Api.csproj --urls http://localhost:5000
```

3. Pokreni frontend:

```bash
cd frontend
npm install
npm run dev
```

Baza sluša na lokalnom portu `14333`, a aplikacija je dostupna na `http://localhost:5173`.

## Demo unos

Za napad koristi:

```text
' OR 1=1 --
```

Ranjivi endpoint spaja tekst u SQL upit i prijava prolazi. Sigurni endpoint koristi EF upit s parametrima i isti unos ne prolazi.
