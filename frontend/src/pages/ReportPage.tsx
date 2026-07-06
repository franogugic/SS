export function ReportPage() {
  return (
    <div className="rounded-lg border border-stone-200 bg-white p-6 shadow-sm">
      <p className="text-sm font-semibold uppercase text-teal-700">SAST vs DAST</p>
      <h2 className="mt-2 text-xl font-semibold text-ink">Usporedni izvještaj</h2>
      <p className="mt-2 text-sm text-stone-600">
        Izvještaj generira ZAP scanner nakon DAST analize. Uspoređuje nalaze Semgrep (SAST) i OWASP ZAP (DAST) alata.
      </p>

      <div className="mt-6 overflow-hidden rounded-lg border border-stone-200" style={{ height: '75vh' }}>
        <iframe
          src="/sast-vs-dast-report.html"
          title="SAST vs DAST izvještaj"
          className="h-full w-full"
          style={{ border: 'none' }}
        />
      </div>
    </div>
  );
}
