using System.Text;
using System.Text.RegularExpressions;

namespace DeluxeNET.Services
{
    /// <summary>
    /// Allowlist HTML sanitizer for player pages. No external dependencies.
    /// Strips &lt;script&gt;, &lt;iframe&gt;, event handlers (onclick etc.),
    /// javascript:/data: URLs, and any tag not on the allowlist. Safe tags keep
    /// a small set of harmless attributes.
    /// </summary>
    public static class HtmlSanitizer
    {
        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "div", "span", "h1", "h2", "h3", "h4", "h5", "h6",
            "b", "i", "u", "s", "strong", "em", "a", "img",
            "ul", "ol", "li", "br", "hr", "blockquote", "pre", "code",
            "table", "thead", "tbody", "tr", "th", "td",
            "font", "center", "marquee", "blink"
        };

        private static readonly HashSet<string> VoidTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "br", "hr", "img"
        };

        // Attributes allowed per tag (lowercase). "style" is allowed but scrubbed.
        private static readonly HashSet<string> GlobalAttrs = new(StringComparer.OrdinalIgnoreCase)
        {
            "style", "align", "color", "size", "face"
        };

        private static readonly Regex TagRegex = new(
            @"<(/?)\s*([a-zA-Z][a-zA-Z0-9]*)\s*((?:[^'""<>]+|'[^']*'|""[^""]*"")*?)(/?)>",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex AttrRegex = new(
            @"([a-zA-Z][a-zA-Z0-9\-]*)\s*=\s*(?:'([^']*)'|""([^""]*)""|([^\s>]+))",
            RegexOptions.Compiled);

        private static readonly Regex StyleUnsafe = new(
            @"expression\s*\(|javascript\s*:|vbscript\s*:|behaviour\s*:|url\s*\(\s*['""]?\s*data\s*:",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EventHandlerAttr = new(
            @"^on[a-z]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Sanitize(string? html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // 1. Remove script/style blocks and their contents entirely.
            var s = Regex.Replace(html, @"<script\b[^>]*>.*?</script\s*>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            s = Regex.Replace(s, @"<style\b[^>]*>.*?</style\s*>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            s = Regex.Replace(s, @"<iframe\b[^>]*>.*?</iframe\s*>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            s = Regex.Replace(s, @"<object\b[^>]*>.*?</object\s*>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            s = Regex.Replace(s, @"<embed\b[^>]*>.*?</embed\s*>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            s = Regex.Replace(s, @"<form\b[^>]*>.*?</form\s*>", "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 2. Walk tags, keeping only allowlisted ones with safe attributes.
            var sb = new StringBuilder(s.Length);
            int last = 0;
            foreach (Match m in TagRegex.Matches(s))
            {
                sb.Append(s, last, m.Index - last);
                last = m.Index + m.Length;

                var closing = m.Groups[1].Value == "/";
                var tag = m.Groups[2].Value.ToLowerInvariant();
                var attrText = m.Groups[3].Value;
                var selfClose = m.Groups[4].Value == "/";

                if (!AllowedTags.Contains(tag))
                    continue; // drop tag but keep inner text

                if (closing)
                {
                    if (!VoidTags.Contains(tag))
                        sb.Append("</").Append(tag).Append('>');
                    continue;
                }

                sb.Append('<').Append(tag);
                AppendSafeAttrs(sb, tag, attrText);
                if (selfClose || VoidTags.Contains(tag))
                    sb.Append(" /");
                sb.Append('>');
            }
            sb.Append(s, last, s.Length - last);

            return sb.ToString();
        }

        private static void AppendSafeAttrs(StringBuilder sb, string tag, string attrText)
        {
            foreach (Match m in AttrRegex.Matches(attrText))
            {
                var name = m.Groups[1].Value.ToLowerInvariant();
                var value = m.Groups[2].Success ? m.Groups[2].Value
                          : m.Groups[3].Success ? m.Groups[3].Value
                          : m.Groups[4].Value;

                if (EventHandlerAttr.IsMatch(name))
                    continue; // onclick, onload, onerror, ...

                bool allowed = GlobalAttrs.Contains(name);
                if (tag == "a" && (name == "href" || name == "title" || name == "target")) allowed = true;
                if (tag == "img" && (name == "src" || name == "alt" || name == "width" || name == "height" || name == "title")) allowed = true;
                if (tag == "font" && name == "size") allowed = true;
                if (!allowed) continue;

                if (name == "style")
                {
                    value = ScrubStyle(value);
                    if (string.IsNullOrWhiteSpace(value)) continue;
                }

                if ((name == "href" || name == "src") && !IsSafeUrl(value))
                    continue;

                // Re-encode the value to avoid attribute injection.
                value = value.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;");
                sb.Append(' ').Append(name).Append("=\"").Append(value).Append('"');
            }
        }

        private static string ScrubStyle(string style)
        {
            if (StyleUnsafe.IsMatch(style)) return string.Empty;
            // Drop any declaration containing url( except plain http(s) images — simplest: drop url() entirely.
            var parts = style.Split(';');
            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                var p = part.Trim();
                if (p.Length == 0) continue;
                if (p.IndexOf("url(", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (StyleUnsafe.IsMatch(p)) continue;
                // Allow only simple property: value pairs.
                if (!Regex.IsMatch(p, @"^[a-z\-]+\s*:\s*[^;{}]+$", RegexOptions.IgnoreCase)) continue;
                if (sb.Length > 0) sb.Append("; ");
                sb.Append(p);
            }
            return sb.ToString();
        }

        private static bool IsSafeUrl(string url)
        {
            var u = url.Trim().ToLowerInvariant();
            if (u.StartsWith("javascript:") || u.StartsWith("vbscript:") || u.StartsWith("data:"))
                return false;
            return true; // http(s), relative, mailto, etc. are fine
        }
    }
}
