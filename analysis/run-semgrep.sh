#!/usr/bin/env bash
# Pokretanje Semgrep SAST analize za SQL Injection Presenter
# Pokretanje: bash analysis/run-semgrep.sh

#sluzi da zaustavimo skriptu ako ijedna naredba vrati gresku -> -e
#zasutavlja ako koristimo nedefiniranu varijablu -> u
# i o nam je da ako idena naredba u piplineau padne da ce cijela skripta pas
set -euo pipefail

#ide u root u slucaju pokretanja iz nekog drugog repoa
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

#kreira novi direktory za rezultate anlaize
# ovo -p je da ne baci gresku ako vec postoji folder s tim imenom da samo zanemari ovo
OUTPUT_DIR="analysis/semgrep-results"
mkdir -p "$OUTPUT_DIR"

#ispisi obicni da bude lipse
echo "=== Semgrep SAST analiza ==="
echo "Target : backend/src/SqlInjectionPresenter.Api/"
echo "Output : $OUTPUT_DIR/"
echo ""

echo "[1/3] Custom SQLi pravila (FromSqlRaw + CommandText)..."
#ovo je prvi scenarij
    #config je da koristi nasa custom rpavila koja smo napisalu u rules foleru tocnije sqli-csharp.yml
    #include je da se fokusiramo samo na .cs datoteke
    #json je output foramt i di ce se spremit taj JSON fajl
    #i ovaj url je od proejkta kojeg triba skentirat 2>/dev/null || true je da ignorira greske i nastavi dalje
semgrep \
  --config analysis/rules/sqli-csharp.yml \
  --include "*.cs" \
  --json \
  --output "$OUTPUT_DIR/custom-sqli.json" \
  backend/src/SqlInjectionPresenter.Api/ 2>/dev/null || true

# ── 2. Semgrep community C# security skup ────────────────────────────────────
echo "[2/3] Semgrep community C# security pravila..."
semgrep \
  --config "r/csharp.lang.security" \
  --include "*.cs" \
  --json \
  --output "$OUTPUT_DIR/community-sqli.json" \
  backend/src/SqlInjectionPresenter.Api/ 2>/dev/null || true

# ── 3. Rezultati ─────────────────────────────────────────────────────────────
echo "[3/3] Rezultati:"
echo ""

python3 << 'PYEOF'
import json, os

# ucitavanje JSON rezultata iz datoteka i uzima samo RESULTS polje
def load(path):
    if not os.path.exists(path): return []
    with open(path) as f:
        return json.load(f).get("results", [])

custom    = load("analysis/semgrep-results/custom-sqli.json")
community = load("analysis/semgrep-results/community-sqli.json")



# Dedupliciraj, da nam ne bi istu gresku oba scenarija ucitala
seen = set()
all_findings = []
for f in custom + community:
    key = (f["path"], f["start"]["line"], f["check_id"])
    if key not in seen:
        seen.add(key)
        all_findings.append(f)

# Kategoriziraj
def is_sqli(f):
    keywords = ["sql", "injection", "sqli", "query", "commandtext", "fromsql"]
    text = f["check_id"].lower() + f.get("extra",{}).get("message","").lower()
    return any(k in text for k in keywords)

def is_second_order(f):
    path = f["path"]
    line = f["start"]["line"]
    if not os.path.exists(path):
        return False
    with open(path) as src:
        lines = src.readlines()
    # Traži tag u bloku iznad nalaza (do 20 linija)
    start = max(0, line - 20)
    block = "".join(lines[start:line])
    return "semgrep-tag: second-order-sqli" in block

sqli_findings  = [f for f in all_findings if is_sqli(f)]
second_order   = [f for f in sqli_findings if is_second_order(f)]
direct_sqli    = [f for f in sqli_findings if not is_second_order(f)]

print(f"  Ukupno nalaza    : {len(all_findings)}")
print(f"  SQL Injection    : {len(sqli_findings)}")
print(f"    - Direktni SQLi: {len(direct_sqli)}")
print(f"    - Second-order : {len(second_order)}")
print()

if sqli_findings:
    print("─── SQL INJECTION NALAZI ───────────────────────────────────────────────────")
    for i, f in enumerate(sqli_findings, 1):
        # Izvuci korisne informacije iz nalaza
        path  = f["path"].replace("backend/src/SqlInjectionPresenter.Api/", "")
        line  = f["start"]["line"]
        rule  = f["check_id"].split(".")[-1]
        msg   = f.get("extra", {}).get("message", "").strip()
        sev   = f.get("extra", {}).get("severity", "?")
        cwe   = f.get("extra", {}).get("metadata", {}).get("cwe", "?")
        owasp = f.get("extra", {}).get("metadata", {}).get("owasp", "?")
        # Oznaci je li second-order
        tip = "SECOND-ORDER" if is_second_order(f) else "DIREKTNI"

        print(f"  Nalaz #{i} [{tip}]")
        print(f"  {'─' * 40}")
        print(f"  Ozbiljnost : {sev}")
        print(f"  Datoteka   : {path}:{line}")
        print(f"  Pravilo    : {rule}")
        print(f"  CWE        : {cwe}")
        print(f"  OWASP      : {owasp}")
        print(f"  Opis       : {msg}")
        # Prikazi stvarni kod iz datoteke ako je dostupan
        if os.path.exists(f["path"]):
            with open(f["path"]) as src:
                sve_linije = src.readlines()
            kod = sve_linije[line - 1].strip()
            print(f"  Kod        : {kod}")
        print()

print("─── SAŽETAK ────────────────────────────────────────────────────────────────")
print(f"  Ukupno SQL injection ranjivosti : {len(sqli_findings)}")
print(f"  - Direktnih                     : {len(direct_sqli)}")
print(f"  - Second-order                  : {len(second_order)}")
print()
print("  SAST PREDNOST: Semgrep detektira ranjivost statički iz koda bez pokretanja")
print("  aplikacije. Second-order je pronađen jer Semgrep vidi FromSqlRaw s")
print("  varijabilnim argumentom, neovisno odakle dolaze podaci.")
print()
PYEOF

echo ""
echo "=== Analiza završena ==="
echo "JSON detalji: analysis/semgrep-results/"
