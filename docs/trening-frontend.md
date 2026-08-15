# Trening i suplementy — instrukcja dla zespołu FE

Nowy moduł backendu: **ćwiczenia, rutyny, sesje treningowe, suplementy i analityka**.
Typy TypeScript: [`docs/workouts-api-schema.ts`](./workouts-api-schema.ts).
Pełny kontrakt: [`docs/swagger.json`](./swagger.json). Decyzje i uzasadnienia: [`docs/workouts-module.md`](./workouts-module.md).

**TL;DR**
1. 🟢 30 nowych endpointów pod `/api/workouts/*` i `/api/supplements/*`. Nic istniejącego się nie zmieniło —
   **zero breaking changes**.
2. 🔴 Daty kalendarzowe (`performedOn`, `takenOn`, `date`) to `"YYYY-MM-DD"` i **muszą** być formatowane
   z lokalnego czasu urządzenia. `toISOString()` cofnie dzień połowie świata.
3. 🟡 `metricType` ćwiczenia decyduje, jakie kontrolki wyrenderować i co wysłać w serii. To jedyne pole,
   od którego zależy formularz logowania.
4. 🟡 Checkbox suplementu to **idempotentny `PUT`** — wysyłasz intencję, nie id wiersza. Dawka ad-hoc to
   osobny, **celowo powtarzalny** `POST`.
5. 🟡 Każdy zapis w sesji i każda zmiana suplementu zwraca **cały obiekt** (sesja / checklist dnia), więc
   repaint robisz z jednej odpowiedzi.

---

## 1. Daty: kalendarz kontra chwila

W module współistnieją dwa rodzaje czasu i **nie wolno ich mieszać**:

| Pole | Typ | Format | Znaczenie |
|---|---|---|---|
| `performedOn`, `takenOn`, `date`, `from`, `to`, `lastPerformedOn` | `DateOnly` | `"2026-08-15"` | dzień w **kalendarzu użytkownika** |
| `startedAt`, `completedAt`, `takenAt` | `DateTime` | `"2026-08-15T17:30:00Z"` | prawdziwa chwila UTC |

Powód jest ten sam co przy questach i finansach: backend nadpisuje strefę czasową użytkownika **przy każdym
odświeżeniu tokenu**. Gdyby „dzień treningu" był wyliczany z UTC, sesja przeskakiwałaby między dniami po
podróży.

**Wysyłanie.** Backend parsuje te pola jako `DateOnly` i przyjmuje **wyłącznie** `"YYYY-MM-DD"`. Pełny
timestamp = `400`.

```ts
// ✅ dobrze — lokalny dzień, który użytkownik widzi
const toIsoDate = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;

// ❌ źle — dla stref na wschód od UTC potrafi cofnąć o dzień
const wrong = d.toISOString().slice(0, 10);
```

`date-fns`: `format(d, 'yyyy-MM-dd')`. `dayjs`: `dayjs(d).format('YYYY-MM-DD')`. Oba operują lokalnie.

**Odbieranie.** `new Date("2026-08-15")` parsuje jako **północ UTC**, więc w strefach ujemnych pokaże 14
sierpnia. Trzymaj te pola jako string albo parsuj jawnie lokalnie:

```ts
const parseIsoDate = (s: string) => {
  const [y, m, d] = s.split('-').map(Number);
  return new Date(y, m - 1, d); // lokalna północ
};
```

**Możesz pominąć `performedOn` przy starcie sesji** — backend podstawi lokalne „dzisiaj" użytkownika z jego
profilu. Wysyłaj jawnie tylko wtedy, gdy wklepujesz trening z kartki.

---

## 2. `metricType` — z tego renderujesz formularz

Każde ćwiczenie ma `metricType`, który mówi, **jakie pomiary seria musi nieść**:

| `metricType` | Wymagane w serii | Przykłady z biblioteki |
|---|---|---|
| `Reps` | `reps` | Pompki klasyczne, Podciąganie nachwytem |
| `RepsAndWeight` | `reps` + `weight` | Wyciskanie sztangi, Przysiad ze sztangą |
| `Time` | `durationSeconds` | Plank, Stanie na rękach, Skakanka |
| `Distance` | `distance` | Spacer farmera, Bear crawl |
| `DistanceAndTime` | `distance` + `durationSeconds` | Bieg, Rower, Wioślarz |

Dwie rzeczy, o które łatwo się potknąć:

- **Metryka mówi, co jest *wymagane*, nigdy czego *nie wolno*.** Możesz wysłać `weight` przy ćwiczeniu
  `Reps` — tak właśnie loguje się podciąganie z obciążeniem. Backend to zapisze i zwróci.
- **Brak wymaganego pomiaru to `400`** z komunikatem w stylu
  `"Weight is required for a 'RepsAndWeight' exercise."`. Warto go pokazać wprost.

⚠️ `metricType` jest typem wartościowym — **pominięcie go w requeście deserializuje się do `"Reps"`**, nie do
null. Zawsze wysyłaj jawnie.

⚠️ W sesji `metricType` i `exerciseName` to **snapshoty** zrobione w momencie dodania ćwiczenia. Zmiana nazwy
albo metryki w bibliotece **nie przepisuje historii** — renderuj stare treningi z pól wpisu sesji, nie
z aktualnej biblioteki. `exerciseId` bywa `null`, jeśli ćwiczenie zostało usunięte; wtedy po prostu nie
linkujesz do biblioteki.

---

## 3. Biblioteka ćwiczeń i rutyny

**Biblioteka** (`GET /api/workouts/exercises`) zwraca 101 ćwiczeń systemowych (`isSystem: true`) **plus**
własne użytkownika. Filtry: `muscleGroup`, `metricType`, `search`, `includeArchived`.

- Ćwiczenia systemowe są **tylko do odczytu** — edycja/usunięcie/archiwizacja zwraca **`403`**.
- Cudze zasoby zwracają **`404`** (nie 403 — nie zdradzamy, że istnieją).
- **Nazwy są unikalne w obrębie użytkownika.** Możesz mieć własne „Pompki klasyczne" obok systemowych.
- **Usunięcie ćwiczenia blokuje tylko rutyna** → `409`, a komunikat wymienia nazwy rutyn. Historia sesji
  nigdy nie blokuje. Gdy delete jest zablokowany, podpowiedz `PATCH /{id}/archived` — archiwum znika
  z pickera, ale zostaje w rutynach i historii.

**Rutyna** = jeden gotowy trening do odpalenia („Push A"), nie plan tygodniowy.

⚠️ **Tablica `exercises` nie ma pola `order` — pozycja w tablicy JEST kolejnością.** Dotyczy rutyn i sesji.
`PUT /routines/{id}` to **pełne zastąpienie**: czego nie wyślesz, to znika. Pusta tablica jest legalna.

Usunięcie rutyny **nie kasuje** sesji z niej wykonanych — tracą tylko `routineId`.

---

## 4. Sesja treningowa

Przepływ, który obsługuje Twój ekran „Start sesji":

```
GET  /api/workouts/sessions/active          → wznowić czy zacząć nową?
POST /api/workouts/sessions                 { routineId } albo { name }
     … logowanie serii …
POST /api/workouts/sessions/{id}/finish
```

- ⚠️ **`GET /active` zwraca `204 No Content` z pustym ciałem**, gdy nic nie trwa — **nie** wywołuj na tym
  `response.json()`, bo rzuci wyjątkiem. To normalny stan, nie błąd (dlatego nie 404):

  ```ts
  const res = await fetch('/api/workouts/sessions/active', { headers });
  const active = res.status === 204 ? null : await res.json();
  ```

  Axios zwróci tu `data: ""` — sprawdzaj `status === 204`, nie prawdziwość `data`.
- **Druga równoległa sesja to `409`**, a komunikat zawiera id aktywnej. Możesz od razu zaproponować
  „Wznów trening" bez dodatkowego zapytania.
- Start z `routineId` **kopiuje** ćwiczenia i cele z rutyny. Bez `routineId` musisz podać `name`.
- **Każdy mutujący endpoint sesji zwraca całą `WorkoutSessionDto`** z przeliczonym `totals` — repaint robisz
  z jednej odpowiedzi, nie sklejasz cząstkowych update'ów.

### Dwie ścieżki logowania — wybierz albo mieszaj

| | Kiedy | Endpoint |
|---|---|---|
| **Na żywo** | jest zasięg, użytkownik klika seria po serii | `POST /sessions/{id}/exercises/{entryId}/sets` |
| **Hurtowo** | siłownia bez zasięgu, sync na koniec | `PUT /sessions/{id}/log` |

Obie piszą **te same wiersze** i są wymienne.

⚠️ `PUT /log` to **pełne zastąpienie całego drzewa** ćwiczeń i serii. Dlatego jest bezpieczny przy retry —
wysłanie tego samego payloadu dwa razy zostawia ten sam log, nie podwojony. **Pusta tablica czyści log**, więc
zawsze wysyłaj całe drzewo, nigdy delta.

Numeracja serii (`setNumber`, 1..n bez dziur) i kolejność (`order`) są po stronie serwera — po skasowaniu
serii reszta jest przenumerowana i dostajesz to w odpowiedzi.

`completedAt` w serii jest opcjonalny (domyślnie „teraz"). Przy odtwarzaniu sesji zalogowanej offline wyślij
prawdziwe znaczniki, żeby nie skleiły się wszystkie na moment synchronizacji.

- `finish` → status `Completed`, liczy się do analityki.
- `abandon` → status `Abandoned`, **nie** liczy się do analityki i nie odpala gamifikacji.
- Sesję można edytować także po zakończeniu (poprawki wpisów).

Każda seria dostaje `estimatedOneRepMax` (Epley, liczony serwerowo, `null` gdy się nie da) — jedna liczba dla
wszystkich klientów.

---

## 5. Suplementy

Model: **suplement** (co) → **sloty** (kiedy i ile) → **intake** (odhaczenie).

Magnez rano i wieczorem to **jeden suplement z dwoma slotami**, nie dwa suplementy. Jednostka (`unit`) siedzi
na suplemencie i obowiązuje wszystkie jego sloty i dawki.

### Checklist dnia obsługuje oba ekrany

```
GET /api/supplements/checklist?date=2026-08-15
GET /api/supplements/checklist?date=2026-08-15&timing=PreWorkout&timing=PostWorkout
```

Pierwsze wywołanie to samodzielny ekran suplementów. Drugie — panel w treningu. **To jest cała integracja
między modułami**; nie ma osobnego zasobu suplementów podpiętego pod sesję, bo plan musi działać też w dzień
nietreningowy. Przy odhaczaniu z ekranu treningu dorzuć `workoutSessionId`.

Odpowiedź ma dwie listy:
- `items` — zaplanowane dawki (tylko aktywne suplementy), każda z `taken` / `takenAt` / `takenAmount`,
- `adHoc` — dawki poza planem tego dnia (mogą dotyczyć też suplementów nieaktywnych).

### Checkbox: idempotentny `PUT`, nie POST/DELETE

```ts
PUT /api/supplements/intakes
{ supplementId, slotId, date, taken: true, amount?, workoutSessionId? }
```

Wysyłasz **intencję**, nie id wiersza — nie musisz go trzymać. Odhaczenie dwa razy zostawia jedną dawkę,
odznaczenie czegoś nigdy nieodhaczonego to ciche nic. Telefon klika szybko, offline i czasem dwa razy;
w bazie stoi za tym unikalny indeks `(slot, dzień)`.

`amount` jest opcjonalny — domyślnie wchodzi zaplanowana ilość ze slotu.

### Dawka ad-hoc: `POST`, celowo powtarzalny

```ts
POST /api/supplements/intakes
{ supplementId, date, amount?, workoutSessionId? }
```

„Wziąłem dziś jeszcze jedną" to prawdziwe zdarzenie — dwa POST-y to dwie dawki, i tak ma być. Cofasz przez
`DELETE /api/supplements/intakes/{id}`, co jest też **jedyną** drogą usunięcia dawki ad-hoc (nie ma slotu do
odznaczenia).

**Wszystkie mutacje dawek zwracają cały `SupplementChecklistDto` dla tego dnia.**

### Usuwanie

- Suplement z zalogowanymi dawkami → **`409`**, komunikat podpowiada deaktywację
  (`PATCH /supplements/{id}/active`). Deaktywacja zdejmuje go z checklisty, ale zostawia całą historię.
- Slot usuwa się normalnie; dawki wzięte przeciw niemu **zostają** i stają się ad-hoc.

---

## 6. Analityka — co liczy się, a co nie

```
GET /api/workouts/analytics/summary?from=&to=
GET /api/workouts/analytics/exercise-history?exerciseId=&from=&to=
GET /api/workouts/analytics/personal-records
GET /api/supplements/analytics/adherence?from=&to=
```

Zasady, które warto odzwierciedlić w UI:

- **Liczą się tylko sesje `Completed`.** Porzucone i trwające są pomijane wszędzie.
- **Rozgrzewki (`setType: "Warmup"`) są wyłączone ze wszystkich sum** i z rekordów.
- **`personal-records` celowo NIE ma szacowanego 1RM.** Wzór Epleya ma wyjątki (pojedyncze powtórzenie,
  odcięcie przy bardzo wysokich), których nie da się wiernie przełożyć na SQL, a rekord zawyżony o 3% jest
  gorszy niż jego brak. Tę liczbę masz w `exercise-history` (`bestEstimatedOneRepMax`) i na każdej serii
  w szczegółach sesji.
- **`adherence`: `rate: null` to „brak danych", nie 0%.** Nie koloruj tego na czerwono.
  Dzień wchodzi do mianownika dopiero gdy **w pełni minął** w kalendarzu użytkownika albo gdy coś już tego
  dnia wzięto — dzięki temu otwarcie apki o 9:00 nie pokazuje „1/3 planu przepadło".
  `rate` **nie jest ucinany na 100** — nadprogramowe dawki to fakt, nie błąd.
  Dawki ad-hoc **nie** wliczają się do licznika (nie były zaplanowane).

---

## 7. Ustawienia jednostki wagi

```
GET /api/workouts/settings            → { weightUnit, supportedWeightUnits }
PUT /api/workouts/settings/weight-unit  { weightUnit: "kg" | "lb" }
```

⚠️ Zmiana jednostki **reinterpretuje** zapisane ciężary, **nie przelicza ich**. Dokładnie ta sama zasada co
przy walucie w finansach. Jeśli chcesz ostrzec użytkownika przed przełączeniem — to jest moment.
Lista wspieranych jednostek przychodzi w odpowiedzi, nie hardkoduj jej.

---

## 8. Kody błędów, na które warto reagować

| Kod | Kiedy |
|---|---|
| `400` | brakujący pomiar dla metryki, przekroczone limity, zła data |
| `403` | próba edycji/usunięcia ćwiczenia systemowego |
| `404` | cudzy zasób albo nieistniejący; też: seria/wpis spoza tej sesji |
| `409` | druga aktywna sesja (id w komunikacie) · usunięcie ćwiczenia używanego w rutynie (nazwy rutyn w komunikacie) · usunięcie suplementu z historią · duplikat nazwy |

Komunikaty z `409` są pisane pod pokazanie użytkownikowi — zawierają konkrety (id sesji, nazwy rutyn).
