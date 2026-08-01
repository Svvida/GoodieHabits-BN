/**
 * GoodieHabits — Finance module API schema (for the mobile/front-end team).
 *
 * Hand-written from the backend DTOs/commands (Application/Finance/*). Types are stable;
 * the ROUTES block reflects the planned controllers (Phase 8) and may be finalized then.
 *
 * Serialization conventions (match the backend's System.Text.Json setup):
 *  - Enums serialize as STRINGS (e.g. "Income", "Monthly") via JsonStringEnumConverter.
 *  - `DateOnly` (transaction date) serializes as an ISO calendar date "YYYY-MM-DD" (no time, no TZ).
 *  - `DateTime` (audit fields) serializes as an ISO-8601 UTC timestamp.
 *  - C# `decimal` -> number; nullable value types -> `T | null`.
 *  - All endpoints are authenticated; the user is taken from the JWT (never sent in the body).
 */

// ─────────────────────────────── Enums ───────────────────────────────

export type FinanceTransactionType = 'Income' | 'Expense';
export type BudgetPeriod = 'Monthly' | 'Yearly';

// ──────────────────────────── Categories ─────────────────────────────

export interface FinanceCategoryDto {
  id: number;
  name: string;
  type: FinanceTransactionType;
  color: string | null;          // hex "#RRGGBB"
  icon: string | null;
  isSystem: boolean;             // seeded defaults: not editable/deletable by the user
  isSavings: boolean;            // descriptive tag; see note below
  parentCategoryId: number | null;
  subCategories: FinanceCategoryDto[]; // populated on the tree endpoint; empty otherwise
}

export interface CreateFinanceCategoryRequest {
  name: string;
  type: FinanceTransactionType;  // ignored when parentCategoryId is set (inherited from parent)
  parentCategoryId: number | null;
  color: string | null;
  icon: string | null;
  isSavings?: boolean;           // ignored when parentCategoryId is set (inherited from parent); defaults to false
}

export interface UpdateFinanceCategoryRequest {
  name: string;
  color: string | null;
  icon: string | null;
  isSavings?: boolean;           // only honoured on a main category; ignored on a sub (inherited)
}

/**
 * isSavings semantics
 * -------------------
 * A purely descriptive tag meaning "money set aside rather than consumed" (e.g. main "Savings"
 * with subs "Emergency fund", "House deposit"). The backend stores and returns it but does NOT
 * change any analytics: savings transactions are still counted in totalExpense / totalIncome,
 * category breakdowns and budget progress exactly as before. Presentation (e.g. showing a
 * separate "saved this month" figure, or netting savings out of spend) is the client's call.
 *
 * Inheritance: a sub-category always mirrors its parent's isSavings. Setting it on a main
 * cascades to all its subs; sending it for a sub is ignored. Consequence: a sub under a
 * *system* main can never be savings — create your own main for that. No seeded system
 * category is flagged as savings.
 *
 * Available on both Expense and Income categories (no server-side restriction).
 */

export interface DeleteFinanceCategoriesRequest {
  categoryIds: number[];         // bulk; all-or-nothing (see ROUTES note)
}

// ─────────────────────────── Transactions ────────────────────────────

export interface TransactionDto {
  id: number;
  type: FinanceTransactionType;
  amount: number;                // always > 0; sign implied by `type`
  categoryId: number | null;     // may reference a main or a sub category
  occurredOn: string;            // "YYYY-MM-DD"
  note: string | null;
  isPaid: boolean;               // see PAID STATUS below — descriptive only, moves no aggregate

  // ── Corrections (refunds / paybacks / reimbursements) ──
  correctsTransactionId: number | null; // set => this row IS a correction of that transaction
  correctedAmount: number;       // how much of `amount` came back; 0 when nothing did
  netAmount: number;             // amount - correctedAmount — RENDER THIS, not `amount`
  corrections: TransactionDto[]; // corrections raised against this row; always [] on a correction

  createdAt: string;             // ISO-8601 UTC
  updatedAt: string | null;
}

/**
 * TRANSACTION CORRECTIONS
 * -----------------------
 * A correction is money coming *back* against an earlier transaction: a refund, a friend paying you back,
 * an employer reimbursing an expense — and the mirror direction too (refunding a client, returning overpaid
 * salary). It is modelled as a relation, not a type: the correction is itself a transaction pointing at its
 * parent via `correctsTransactionId`, and it INHERITS the parent's `type` and `categoryId`. The link is what
 * carries "the money went the other way", so you never send a type or a category when creating one.
 *
 * What this means for the client:
 *  - `GET /transactions` returns PARENTS ONLY. Corrections arrive nested in `corrections`, never as their own
 *    row, and `totalCount` counts parents only. A correction dated in a different month than its parent can
 *    therefore never be paged away from it.
 *  - Render `netAmount` everywhere you used to render `amount`. Any client-side total computed from `amount`
 *    will disagree with every server aggregate.
 *  - Analytics (summaries, breakdowns, trend, budget progress) already arrive NET. A correction nets against
 *    its PARENT'S period and category regardless of its own `occurredOn` — its own date is kept for the record
 *    and worth showing on the row, but it moves no money in its own month.
 *  - `0 <= correctedAmount <= amount` is enforced server-side, so no figure can go negative. Over-correcting is
 *    a 409; money received beyond the original amount is an ordinary Income transaction, not a correction.
 *  - One level deep: correcting a correction is a 409.
 *  - Editing a correction via `PUT /transactions/{id}` accepts amount/date/note. Echoing the inherited `type`
 *    and `categoryId` back unchanged is fine; actually changing either is a 409.
 *  - Deleting a correction restores the parent's net; deleting a parent removes its corrections with it.
 */

export interface AddCorrectionRequest {
  amount: number;                // > 0, and <= (parent.amount - parent.correctedAmount)
  occurredOn: string;            // "YYYY-MM-DD" — when the money came back (for the record only)
  note: string | null;
  // no `type`, no `categoryId` — both are inherited from the corrected transaction
}

/**
 * PAID STATUS
 * -----------
 * `isPaid` marks whether the money has actually moved yet — mainly meaningful for expenses dated today or
 * later ("the rent is logged, I haven't paid it"). Like `isSavings`, it is **pure metadata**: an unpaid
 * transaction is aggregated exactly as if it were already paid, in `totalIncome` / `totalExpense`, category
 * breakdowns, budget progress, the spending trend and `openingBalance` alike. A logged-but-unpaid bill is
 * money already spoken for, and excluding it would make the dashboard optimistic on precisely the days it
 * needs to be accurate. Rendering unpaid rows differently is the client's call.
 *
 *  - Omitting `isPaid` on create or update means **paid**, whatever the date — a date-dependent implicit
 *    default would surprise API clients. `PUT` is full replacement, so an omitted flag resets the row to paid;
 *    echo it back like `type` and `categoryId`.
 *  - `PATCH /transactions/{id}/paid-status` flips it without round-tripping the whole object.
 *  - Corrections are always `isPaid: true` — money that has come back has come back. Setting the paid status
 *    on a correction is a 409; set it on the transaction it corrects instead.
 *  - Rows materialized from a recurring template start `isPaid: false` when they are expenses, which is what
 *    gives you an "upcoming bills" view for free.
 */

export interface CreateTransactionRequest {
  type: FinanceTransactionType;
  amount: number;
  occurredOn: string;            // "YYYY-MM-DD" (past or future allowed)
  categoryId: number | null;
  note: string | null;
  isPaid?: boolean;              // omitted => true
}

export interface UpdateTransactionRequest {
  type: FinanceTransactionType;
  amount: number;
  occurredOn: string;
  categoryId: number | null;
  note: string | null;
  isPaid?: boolean;              // omitted => true (full replacement — echo it, don't drop it)
}

export interface UpdatePaidStatusRequest {
  isPaid: boolean;
}

export interface GetTransactionsQueryParams {
  from?: string;                 // "YYYY-MM-DD"
  to?: string;                   // "YYYY-MM-DD"
  type?: FinanceTransactionType;
  categoryId?: number;
  page: number;                  // 1-based
  pageSize: number;              // 1..100
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ────────────────────────────── Budgets ──────────────────────────────

export interface BudgetDto {
  id: number;
  categoryId: number | null;     // null => overall budget for the period
  period: BudgetPeriod;
  year: number;
  month: number | null;          // 1..12 for Monthly; null for Yearly
  limitAmount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateBudgetRequest {
  categoryId: number | null;
  period: BudgetPeriod;
  year: number;
  month: number | null;          // required for Monthly, omit for Yearly
  limitAmount: number;
}

export interface UpdateBudgetRequest {
  limitAmount: number;           // only the limit is mutable
}

export interface GetBudgetsQueryParams {
  year: number;
  month?: number;                // with a month: that month's monthly budgets + the year's yearly budgets
}

// ───────────────────────── Settings (Phase 7) ────────────────────────

export interface UpdateCurrencyRequest {
  currency: string;              // ISO-4217, e.g. "USD", "EUR", "PLN"
}

// ───────────────────────────── Analytics ─────────────────────────────

export interface CategoryBreakdownItemDto {
  categoryId: number | null;     // null => uncategorized
  categoryName: string | null;
  parentCategoryId: number | null; // roll sub-categories up to their main on the client
  amount: number;
  percentage: number;            // 0..100, share of the type's total
}

export interface MonthlySummaryDto {
  year: number;
  month: number;
  currency: string;
  totalIncome: number;
  totalExpense: number;
  net: number;
  /**
   * Money carried into this month from every earlier one — income minus expense, chained from the user's
   * first month (whose opening balance is 0). Gap months don't break the chain.
   *
   * SIGNED, deliberately not clamped at 0: an overspent history carries a negative forward. Clamping would
   * hide the number the user most needs ("−500", not "0 left") and, worse, break the chain — an erased
   * shortfall never reappears in any later month. Render negatives however you like; the value is honest.
   *
   * Unpaid transactions reduce it exactly like paid ones, per the `isPaid` note above.
   */
  openingBalance: number;
  expenseByCategory: CategoryBreakdownItemDto[];
  incomeByCategory: CategoryBreakdownItemDto[];
}

export interface MonthlyTotalsDto {
  month: number;                 // 1..12
  totalIncome: number;
  totalExpense: number;
  net: number;
}

export interface YearlySummaryDto {
  year: number;
  currency: string;
  totalIncome: number;
  totalExpense: number;
  net: number;
  months: MonthlyTotalsDto[];    // 12 entries, Jan..Dec
  expenseByCategory: CategoryBreakdownItemDto[];
  incomeByCategory: CategoryBreakdownItemDto[];
}

export interface CategoryBreakdownDto {
  type: FinanceTransactionType;
  year: number;
  month: number | null;          // null => whole year
  currency: string;
  total: number;
  items: CategoryBreakdownItemDto[];
}

export interface GetCategoryBreakdownQueryParams {
  type: FinanceTransactionType;
  period: BudgetPeriod;
  year: number;
  month?: number;                // required when period = "Monthly"
}

export interface BudgetProgressItemDto {
  budgetId: number;
  categoryId: number | null;     // null => overall budget
  categoryName: string | null;
  period: BudgetPeriod;
  year: number;
  month: number | null;
  limit: number;
  spent: number;                 // sum of expenses in the budget's scope/period
  remaining: number;             // limit - spent (may be negative)
  percentUsed: number;
  isOverBudget: boolean;
}

export interface GetBudgetProgressQueryParams {
  year: number;
  month?: number;
}

export interface SpendingTrendPointDto {
  year: number;
  month: number;
  income: number;
  expense: number;
  net: number;
}

export interface SpendingTrendDto {
  currency: string;
  points: SpendingTrendPointDto[]; // chronological
}

export interface GetSpendingTrendQueryParams {
  endYear: number;
  endMonth: number;              // 1..12
  months: number;                // 1..60, window ending at (endYear, endMonth)
}

// ──────────────────── Recurring transactions ─────────────────────────

/**
 * RECURRING TRANSACTIONS
 * ----------------------
 * A template the backend materializes into a real transaction once a month (rent, tuition, a subscription).
 * The template is not itself a transaction and appears in no aggregate.
 *
 *  - **A template never produces a row for the month it was created in.** The first one lands the following
 *    month. This is what keeps the "repeat monthly" tick on the add-transaction form correct: that flow posts
 *    the current month's transaction itself, and the generator must not duplicate it. If you add a template
 *    from the management screen and want this month covered too, post that first transaction yourself.
 *  - Materialized rows are **ordinary transactions** — editable, deletable, correctable. Expenses arrive
 *    `isPaid: false` (an "upcoming bills" view for free); income arrives paid.
 *  - **Deleting a row you didn't want does not bring it back.** The backend tracks how far generation has
 *    advanced, so a deleted month stays deleted.
 *  - Deleting a template keeps every transaction it already produced; they simply stop being linked to it.
 *    Deleting a materialized transaction does not touch the template.
 *  - `dayOfMonth` past the end of a short month clamps to its last day (31 -> 28/29/30), matching
 *    `remapOccurredOnToMonth`.
 *  - Pausing (`isActive: false`) and later resuming does **not** backfill the dormant months.
 *  - Generation runs as a catch-up sweep on API start, so a month may appear a little after it begins.
 */

export interface RecurringTransactionDto {
  id: number;
  type: FinanceTransactionType;
  categoryId: number | null;
  amount: number;
  note: string | null;
  dayOfMonth: number;            // 1..31
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateRecurringTransactionRequest {
  type: FinanceTransactionType;
  amount: number;
  dayOfMonth: number;            // 1..31
  categoryId: number | null;     // must match `type` if set
  note: string | null;
}

/** Partial update — omitted fields are left unchanged (unlike PUT /transactions, which fully replaces). */
export interface UpdateRecurringTransactionRequest {
  amount?: number;
  note?: string;                 // "" clears it; omit to leave it alone
  dayOfMonth?: number;
  isActive?: boolean;
}

// ────────────────────────── Planned routes ───────────────────────────
// All under /api/finance, all [Authorize]. Finalized in Phase 8.
export const FINANCE_ROUTES = {
  // Categories
  getCategoryTree: 'GET /api/finance/categories?type={Income|Expense}?',
  createCategory: 'POST /api/finance/categories',
  updateCategory: 'PUT /api/finance/categories/{id}',
  deleteCategories: 'DELETE /api/finance/categories', // body: DeleteFinanceCategoriesRequest (all-or-nothing)

  // Transactions
  getTransactions: 'GET /api/finance/transactions', // query: GetTransactionsQueryParams -> PagedResult<TransactionDto>
  getTransactionById: 'GET /api/finance/transactions/{id}',
  createTransaction: 'POST /api/finance/transactions',
  updateTransaction: 'PUT /api/finance/transactions/{id}',
  deleteTransaction: 'DELETE /api/finance/transactions/{id}',
  // body: AddCorrectionRequest -> TransactionDto of the CORRECTED PARENT (refreshed netAmount,
  // new correction already embedded), because corrections are never rendered as standalone rows.
  addCorrection: 'POST /api/finance/transactions/{id}/corrections',
  updatePaidStatus: 'PATCH /api/finance/transactions/{id}/paid-status', // body: UpdatePaidStatusRequest

  // Recurring transactions (templates)
  getRecurringTransactions: 'GET /api/finance/recurring-transactions',
  createRecurringTransaction: 'POST /api/finance/recurring-transactions',
  updateRecurringTransaction: 'PUT /api/finance/recurring-transactions/{id}', // partial body
  deleteRecurringTransaction: 'DELETE /api/finance/recurring-transactions/{id}',

  // Budgets
  getBudgets: 'GET /api/finance/budgets', // query: GetBudgetsQueryParams
  createBudget: 'POST /api/finance/budgets',
  updateBudget: 'PUT /api/finance/budgets/{id}',
  deleteBudget: 'DELETE /api/finance/budgets/{id}',

  // Analytics
  monthlySummary: 'GET /api/finance/analytics/monthly-summary?year&month',
  yearlySummary: 'GET /api/finance/analytics/yearly-summary?year',
  categoryBreakdown: 'GET /api/finance/analytics/category-breakdown', // GetCategoryBreakdownQueryParams
  budgetProgress: 'GET /api/finance/analytics/budget-progress?year&month?',
  spendingTrend: 'GET /api/finance/analytics/spending-trend', // GetSpendingTrendQueryParams

  // Settings
  getSettings: 'GET /api/finance/settings', // -> FinanceSettingsDto
  updateCurrency: 'PUT /api/finance/settings/currency',
} as const;

export interface FinanceSettingsDto {
  currency: string; // ISO-4217
}
