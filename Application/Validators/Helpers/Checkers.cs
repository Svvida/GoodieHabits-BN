using System.Globalization;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace Application.Validators.Helpers
{
    internal static class Checkers
    {
        internal static bool IsEmail(string login) =>
            Regex.IsMatch(login, @"^(?!.*\.\.)[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        internal static bool IsSingleEmoji(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var enumerator = StringInfo.GetTextElementEnumerator(input);
            int count = 0;

            while (enumerator.MoveNext())
            {
                count++;
                if (count > 1)
                    return false;
            }

            return count == 1;
        }

        internal static bool IsSafeHtml(string input)
        {
            var sanitizer = new HtmlSanitizer();

            string[] allowedTags = { "a", "blockquote", "code", "b", "strong", "i", "em", "u", "s", "del",
                                     "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "p", "label",
                                     "span", "div", "input" };

            string[] allowedAttributes = { "data-type", "data-checked", "type", "checked" };

            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedTags.UnionWith(allowedTags);
            sanitizer.AllowedAttributes.UnionWith(allowedAttributes);

            // Event to ensure input checkboxes are inside labels
            sanitizer.RemovingTag += (sender, args) =>
            {
                if (args.Tag.TagName == "input" && (args.Tag.Parent == null || args.Tag.Parent.NodeName != "label"))
                {
                    args.Cancel = true; // Remove <input> if not inside <label>
                }
            };

            // Prevent JavaScript-based XSS in href attributes
            sanitizer.RemovingAttribute += (sender, args) =>
            {
                if (args.Tag.TagName == "a" && args.Attribute.Name == "href" &&
                    args.Attribute.Value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    args.Cancel = true;
                }
            };

            // Sanitize input and compare. If sanitized version differs from original, it means input contained unsafe tags
            return sanitizer.Sanitize(input) == input;
        }
    }
}
