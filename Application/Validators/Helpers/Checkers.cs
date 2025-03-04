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

            // Allow necessary tags
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("a");
            sanitizer.AllowedTags.Add("blockquote");
            sanitizer.AllowedTags.Add("code");
            sanitizer.AllowedTags.Add("b");
            sanitizer.AllowedTags.Add("strong");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("em");
            sanitizer.AllowedTags.Add("u");
            sanitizer.AllowedTags.Add("s");
            sanitizer.AllowedTags.Add("del");
            sanitizer.AllowedTags.Add("h1");
            sanitizer.AllowedTags.Add("h2");
            sanitizer.AllowedTags.Add("h3");
            sanitizer.AllowedTags.Add("h4");
            sanitizer.AllowedTags.Add("h5");
            sanitizer.AllowedTags.Add("h6");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("ol");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("label");
            sanitizer.AllowedTags.Add("span");
            sanitizer.AllowedTags.Add("div");
            sanitizer.AllowedTags.Add("input");

            // Allow attributes for task list structure
            sanitizer.AllowedAttributes.Add("data-type");
            sanitizer.AllowedAttributes.Add("data-checked");

            // Allow only checkboxes (input type="checkbox")
            sanitizer.AllowedAttributes.Add("type");
            sanitizer.AllowedAttributes.Add("checked");

            sanitizer.RemovingTag += (sender, args) =>
            {
                // Allow <input type="checkbox"> but only inside <ul data-type="taskList">
                if (args.Tag.TagName == "input")
                {
                    var parent = args.Tag.Parent;
                    if (parent == null || parent.NodeName != "label") // Ensure it's inside <label>
                    {
                        args.Cancel = true; // Remove the <input> if it's outside a label
                    }
                }
            };

            // Prevent XSS via JavaScript URLs
            sanitizer.RemovingAttribute += (sender, args) =>
            {
                if (args.Tag.TagName == "a" && args.Attribute.Name == "href")
                {
                    string hrefValue = args.Attribute.Value.ToLower();
                    if (hrefValue.StartsWith("javascript:"))
                    {
                        args.Cancel = true; // Remove dangerous href values
                    }
                }
            };

            // Sanitize input
            string sanitized = sanitizer.Sanitize(input);

            // If sanitized version differs from original, it means input contained unsafe tags
            return sanitized == input;
        }
    }
}
