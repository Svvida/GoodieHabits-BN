using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class FinanceCategoryConfiguration : IEntityTypeConfiguration<FinanceCategory>
    {
        public void Configure(EntityTypeBuilder<FinanceCategory> builder)
        {
            builder.ToTable("FinanceCategories");
            builder.HasKey(c => c.Id);

            builder.HasIndex(c => new { c.UserProfileId, c.ParentCategoryId });

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(FinanceCategory.NameMaxLength);

            builder.Property(c => c.Type)
                .IsRequired();

            builder.Property(c => c.Color)
                .HasMaxLength(7);

            builder.Property(c => c.Icon)
                .HasMaxLength(50);

            builder.Property(c => c.IsSystem)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.IsSavings)
                .IsRequired()
                .HasDefaultValue(false);

            // Self-reference: main -> sub (one level deep). Restrict so a parent with children can't be dropped.
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional owner (null => global system category). Manual delete on account wipe.
            builder.HasOne(c => c.UserProfile)
                .WithMany(u => u.FinanceCategories)
                .HasForeignKey(c => c.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            SeedSystemCategories(builder);
        }

        // System category taxonomy supplied by the front-end (docs/categories.txt). Names are Polish display
        // strings (no category i18n on the backend); icons are Ionicons outline names; sub-categories carry no
        // icon/color and inherit the parent's. Ids are assigned here and must stay stable across migrations:
        // expense mains 1-6, 8 and 9 (7 was a retired "Other", never reused), income mains 50-57, expense subs
        // 101-148 and 158-166.
        //
        // ⚠️ Seeded ids share one IDENTITY sequence with user-created categories, so the two can collide: ids
        // 149-157 are burned (150-153 are live user rows, the rest were handed out and freed) and must never be
        // seeded. The 2026-08-09 migration reseeds the identity to 100_000, which permanently splits the ranges:
        // **system categories below 100_000, user categories above it.** Explicitly inserting a lower id no
        // longer moves the counter (SQL Server only ever raises it), so new system categories can be numbered
        // freely from 167 up.
        //
        // "Spłata długów" (147) used to sit under "Finanse i Oszczędności", which made it inherit
        // IsSavings: true — but paying someone back isn't setting money aside, it's money leaving for good, so
        // it must not be netted out of spending the way the client treats IsSavings rows. It lives under
        // "Długi i Pożyczki" (8, IsSavings: false) instead, together with its mirror image "Pożyczki udzielone".
        //
        // Money lent out is deliberately IsSavings: false too, even though you expect it back: repayment is
        // recorded as a correction against the original expense, which nets it to zero on its own. Marking it
        // as savings would instead park an unrepaid loan under "set aside" forever — the one case where it is
        // certainly not.
        private static void SeedSystemCategories(EntityTypeBuilder<FinanceCategory> builder)
        {
            // --- Expense mains ---
            var mieszkanie = FinanceCategory.CreateSystemMain(1, "Mieszkanie", FinanceTransactionTypeEnum.Expense, "#1987EE", "home-outline");
            var transport = FinanceCategory.CreateSystemMain(2, "Transport", FinanceTransactionTypeEnum.Expense, "#F59E0B", "car-outline");
            var zdrowie = FinanceCategory.CreateSystemMain(3, "Życie i Zdrowie", FinanceTransactionTypeEnum.Expense, "#10B981", "heart-outline");
            var edukacja = FinanceCategory.CreateSystemMain(4, "Rozwój i Edukacja", FinanceTransactionTypeEnum.Expense, "#8B5CF6", "school-outline");
            var rozrywka = FinanceCategory.CreateSystemMain(5, "Rozrywka i Inne", FinanceTransactionTypeEnum.Expense, "#EC4899", "game-controller-outline");
            var oszczednosci = FinanceCategory.CreateSystemMain(6, "Finanse i Oszczędności", FinanceTransactionTypeEnum.Expense, "#14B8A6", "trending-up-outline", isSavings: true);
            var dlugi = FinanceCategory.CreateSystemMain(8, "Długi i Pożyczki", FinanceTransactionTypeEnum.Expense, "#EF4444", "swap-horizontal-outline");
            var podroze = FinanceCategory.CreateSystemMain(9, "Podróże i Wakacje", FinanceTransactionTypeEnum.Expense, "#0EA5E9", "airplane-outline");

            // --- Income mains (flat: no sub-categories) ---
            const string incomeColor = "#10B981";
            var wynagrodzenie = FinanceCategory.CreateSystemMain(50, "Wynagrodzenie", FinanceTransactionTypeEnum.Income, incomeColor, "briefcase-outline");
            var premia = FinanceCategory.CreateSystemMain(51, "Premia / Bonus", FinanceTransactionTypeEnum.Income, incomeColor, "gift-outline");
            var dzialalnosc = FinanceCategory.CreateSystemMain(52, "Działalność gosp.", FinanceTransactionTypeEnum.Income, incomeColor, "business-outline");
            var freelance = FinanceCategory.CreateSystemMain(53, "Freelance", FinanceTransactionTypeEnum.Income, incomeColor, "laptop-outline");
            var dochodPasywny = FinanceCategory.CreateSystemMain(54, "Dochód pasywny", FinanceTransactionTypeEnum.Income, incomeColor, "wallet-outline");
            var swiadczenia = FinanceCategory.CreateSystemMain(55, "Świadczenia", FinanceTransactionTypeEnum.Income, incomeColor, "shield-checkmark-outline");
            var zwrotPodatku = FinanceCategory.CreateSystemMain(56, "Zwrot podatku", FinanceTransactionTypeEnum.Income, incomeColor, "receipt-outline");
            var inneIncome = FinanceCategory.CreateSystemMain(57, "Inne", FinanceTransactionTypeEnum.Income, incomeColor, "ellipsis-horizontal-outline");

            builder.HasData(
                mieszkanie, transport, zdrowie, edukacja, rozrywka, oszczednosci, dlugi, podroze,
                wynagrodzenie, premia, dzialalnosc, freelance, dochodPasywny, swiadczenia, zwrotPodatku, inneIncome);

            // --- Expense subs (inherit parent icon/color; isSavings inherited from parent) ---
            builder.HasData(
                // Mieszkanie (1) -> 101..112
                FinanceCategory.CreateSystemSub(101, mieszkanie, "Czynsz / Rata kredytu"),
                FinanceCategory.CreateSystemSub(102, mieszkanie, "Czynsz administracyjny"),
                FinanceCategory.CreateSystemSub(103, mieszkanie, "Prąd"),
                FinanceCategory.CreateSystemSub(104, mieszkanie, "Woda i ścieki"),
                FinanceCategory.CreateSystemSub(105, mieszkanie, "Gaz"),
                FinanceCategory.CreateSystemSub(106, mieszkanie, "Ogrzewanie"),
                FinanceCategory.CreateSystemSub(107, mieszkanie, "Wywóz nieczystości"),
                FinanceCategory.CreateSystemSub(108, mieszkanie, "Internet / Wi-Fi"),
                FinanceCategory.CreateSystemSub(109, mieszkanie, "Telewizja"),
                FinanceCategory.CreateSystemSub(110, mieszkanie, "Ubezpieczenie nieruchomości"),
                FinanceCategory.CreateSystemSub(111, mieszkanie, "Serwis / Naprawy domowe"),
                FinanceCategory.CreateSystemSub(112, mieszkanie, "Środki czystości"),
                FinanceCategory.CreateSystemSub(165, mieszkanie, "Wyposażenie i meble"),

                // Transport (2) -> 113..121
                FinanceCategory.CreateSystemSub(113, transport, "Paliwo"),
                FinanceCategory.CreateSystemSub(114, transport, "Rata kredytu / leasingu"),
                FinanceCategory.CreateSystemSub(115, transport, "Ubezpieczenie OC/AC"),
                FinanceCategory.CreateSystemSub(116, transport, "Przegląd / Serwis"),
                FinanceCategory.CreateSystemSub(117, transport, "Opony"),
                FinanceCategory.CreateSystemSub(118, transport, "Komunikacja miejska / PKP"),
                FinanceCategory.CreateSystemSub(119, transport, "Taxi / Uber / Bolt"),
                FinanceCategory.CreateSystemSub(120, transport, "Parkingi / Autostrady"),
                FinanceCategory.CreateSystemSub(121, transport, "Akcesoria samochodowe"),

                // Życie i Zdrowie (3) -> 122..129
                FinanceCategory.CreateSystemSub(122, zdrowie, "Zakupy spożywcze"),
                FinanceCategory.CreateSystemSub(123, zdrowie, "Jedzenie na mieście"),
                FinanceCategory.CreateSystemSub(124, zdrowie, "Wizyty lekarskie"),
                FinanceCategory.CreateSystemSub(125, zdrowie, "Leki i suplementy"),
                FinanceCategory.CreateSystemSub(126, zdrowie, "Dentysta"),
                FinanceCategory.CreateSystemSub(127, zdrowie, "Siłownia / Karnet sportowy"),
                FinanceCategory.CreateSystemSub(128, zdrowie, "Kosmetyczka / Fryzjer"),
                FinanceCategory.CreateSystemSub(129, zdrowie, "Odzież i obuwie"),
                // Products, as opposed to the "Kosmetyczka / Fryzjer" services above it: toothpaste and
                // deodorant are neither household cleaning (112) nor groceries (122), which is where they
                // kept landing before this existed.
                FinanceCategory.CreateSystemSub(164, zdrowie, "Higiena i kosmetyki"),

                // Rozwój i Edukacja (4) -> 130..134
                FinanceCategory.CreateSystemSub(130, edukacja, "Czesne"),
                FinanceCategory.CreateSystemSub(131, edukacja, "Kursy online / Szkolenia"),
                FinanceCategory.CreateSystemSub(132, edukacja, "Książki / E-booki"),
                FinanceCategory.CreateSystemSub(133, edukacja, "Subskrypcje edukacyjne"),
                FinanceCategory.CreateSystemSub(134, edukacja, "Sprzęt edukacyjny"),

                // Rozrywka i Inne (5) -> 135..142
                FinanceCategory.CreateSystemSub(135, rozrywka, "Streaming (Netflix, Spotify...)"),
                FinanceCategory.CreateSystemSub(136, rozrywka, "Kino / Teatr / Koncerty"),
                FinanceCategory.CreateSystemSub(137, rozrywka, "Hobby i akcesoria"),
                FinanceCategory.CreateSystemSub(138, rozrywka, "Zwierzęta"),
                FinanceCategory.CreateSystemSub(139, rozrywka, "Dzieci"),
                FinanceCategory.CreateSystemSub(140, rozrywka, "Prezenty"),
                FinanceCategory.CreateSystemSub(141, rozrywka, "Wyjścia ze znajomymi"),
                FinanceCategory.CreateSystemSub(142, rozrywka, "Nieprzewidziane wydatki"),
                FinanceCategory.CreateSystemSub(166, rozrywka, "Elektronika i sprzęt"),

                // Finanse i Oszczędności (6, isSavings) -> 143..146
                FinanceCategory.CreateSystemSub(143, oszczednosci, "Poduszka finansowa"),
                FinanceCategory.CreateSystemSub(144, oszczednosci, "IKE / IKZE"),
                FinanceCategory.CreateSystemSub(145, oszczednosci, "Inwestycje (ETF / Giełda)"),
                FinanceCategory.CreateSystemSub(146, oszczednosci, "Oszczędności celowe"),

                // Długi i Pożyczki (8) -> 147..148
                FinanceCategory.CreateSystemSub(147, dlugi, "Spłata długów"),
                FinanceCategory.CreateSystemSub(148, dlugi, "Pożyczki udzielone"),

                // Podróże i Wakacje (9) -> 158..163. A trip is a main category rather than a sub of
                // "Rozrywka i Inne" because its cost is the sum of several kinds of spending (bed, transport,
                // food, tickets) and the number you actually want is the total for the trip.
                FinanceCategory.CreateSystemSub(158, podroze, "Noclegi"),
                FinanceCategory.CreateSystemSub(159, podroze, "Przejazdy / Loty"),
                FinanceCategory.CreateSystemSub(160, podroze, "Wynajem auta"),
                FinanceCategory.CreateSystemSub(161, podroze, "Jedzenie na wyjeździe"),
                FinanceCategory.CreateSystemSub(162, podroze, "Atrakcje i wycieczki"),
                FinanceCategory.CreateSystemSub(163, podroze, "Ubezpieczenie podróżne"));
        }
    }
}
