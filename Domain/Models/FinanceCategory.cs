using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Models
{
    /// <summary>
    /// A finance category, optionally nested one level deep (main -> sub).
    /// System categories (<see cref="IsSystem"/>) are seeded globally and have no owner
    /// (<see cref="UserProfileId"/> is null); user categories are owned by a single profile.
    /// </summary>
    public class FinanceCategory : EntityBase
    {
        public const int NameMaxLength = 50;

        public int Id { get; set; }
        public int? UserProfileId { get; private set; }      // null => system/seeded (global)
        public int? ParentCategoryId { get; private set; }   // null => main category
        public string Name { get; private set; } = null!;
        public FinanceTransactionTypeEnum Type { get; private set; }
        public string? Color { get; private set; }
        public string? Icon { get; private set; }
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Marks the category as holding money set aside rather than consumed (e.g. "Savings" -> "Emergency fund").
        /// Descriptive only: analytics totals treat savings transactions like any other, so the client decides how
        /// to present them. Sub-categories always inherit this from their parent.
        /// </summary>
        public bool IsSavings { get; private set; }

        public UserProfile? UserProfile { get; set; }
        public FinanceCategory? ParentCategory { get; set; }
        public ICollection<FinanceCategory> SubCategories { get; set; } = [];
        public ICollection<FinanceTransaction> Transactions { get; set; } = [];

        public bool IsMain => ParentCategoryId is null;

        protected FinanceCategory() { }

        private FinanceCategory(
            int? userProfileId,
            int? parentCategoryId,
            string name,
            FinanceTransactionTypeEnum type,
            string? color,
            string? icon,
            bool isSystem,
            bool isSavings)
        {
            ValidateName(name);
            ValidateColor(color);

            UserProfileId = userProfileId;
            ParentCategoryId = parentCategoryId;
            Name = name.Trim();
            Type = type;
            Color = color;
            Icon = icon;
            IsSystem = isSystem;
            IsSavings = isSavings;
        }

        public static FinanceCategory CreateMain(
            int userProfileId,
            string name,
            FinanceTransactionTypeEnum type,
            string? color = null,
            string? icon = null,
            bool isSavings = false)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            return new FinanceCategory(userProfileId, null, name, type, color, icon, isSystem: false, isSavings);
        }

        /// <summary>Creates a sub-category. <c>Type</c> and <c>IsSavings</c> are inherited from the parent.</summary>
        public static FinanceCategory CreateSub(
            int userProfileId,
            FinanceCategory parent,
            string name,
            string? color = null,
            string? icon = null)
        {
            if (userProfileId <= 0)
                throw new InvalidArgumentException("UserProfileId must be greater than zero.");

            ArgumentNullException.ThrowIfNull(parent);

            if (!parent.IsMain)
                throw new InvalidArgumentException("Categories can only be nested one level deep.");

            // A sub may be attached to the user's own main category or to a shared system category.
            if (parent.UserProfileId is not null && parent.UserProfileId != userProfileId)
                throw new ForbiddenException("Cannot create a subcategory under a category owned by another user.");

            return new FinanceCategory(userProfileId, parent.Id, name, parent.Type, color, icon, isSystem: false, parent.IsSavings);
        }

        /// <summary>Factory for seeding global system categories (main level). Id is explicit for stable seed data.</summary>
        public static FinanceCategory CreateSystemMain(
            int id,
            string name,
            FinanceTransactionTypeEnum type,
            string? color = null,
            string? icon = null,
            bool isSavings = false)
            => new(null, null, name, type, color, icon, isSystem: true, isSavings) { Id = id };

        /// <summary>Factory for seeding global system categories (sub level). Id is explicit for stable seed data.</summary>
        public static FinanceCategory CreateSystemSub(
            int id,
            FinanceCategory parent,
            string name,
            string? color = null,
            string? icon = null)
        {
            ArgumentNullException.ThrowIfNull(parent);

            if (!parent.IsMain)
                throw new InvalidArgumentException("Categories can only be nested one level deep.");

            return new FinanceCategory(null, parent.Id, name, parent.Type, color, icon, isSystem: true, parent.IsSavings) { Id = id };
        }

        public void Rename(string name)
        {
            ValidateName(name);
            Name = name.Trim();
        }

        public void UpdateColor(string? color)
        {
            ValidateColor(color);
            Color = color;
        }

        public void UpdateIcon(string? icon) => Icon = icon;

        /// <summary>
        /// Flags the category as savings. Sub-categories inherit this from their parent, so callers changing a main
        /// must apply the same value to its sub-categories to keep the tree consistent.
        /// </summary>
        public void UpdateIsSavings(bool isSavings) => IsSavings = isSavings;

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidArgumentException("Category name cannot be null or whitespace.");
            if (name.Trim().Length > NameMaxLength)
                throw new InvalidArgumentException($"Category name cannot exceed {NameMaxLength} characters.");
        }

        private static void ValidateColor(string? color)
        {
            if (color is null)
                return;
            if (color.Length != 7 || color[0] != '#')
                throw new InvalidArgumentException("Color must be a valid hex color code (e.g. #RRGGBB).");
        }
    }
}
