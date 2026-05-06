namespace FermixAPI.Hints.Core.Utilities.Parser
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class Tags
    {
        private static readonly object Lock = new();

        private static readonly HashSet<string> AllTags = new()
        {
            "align", "allcaps", "alpha", "b", "color", "cspace", "font", "font-weight",
            "gradient", "i", "indent", "line-height", "line-indent", "link", "lowercase",
            "margin", "mark", "mspace", "nobr", "noparse", "page", "pos", "rotate", "s",
            "size", "smallcaps", "space", "sprite", "strikethrough", "style", "sub", "sup",
            "u", "uppercase", "voffset", "width",
        };

        public static bool IsTag(string tag)
        {
            return IsEndTag(tag) || IsStartTag(tag);
        }

        public static bool IsEndTag(string tag)
        {
            if (string.IsNullOrEmpty(tag.Trim()))
                return false;

            tag = tag.ToLower();

            lock (Lock)
            {
                if (tag.StartsWith("</"))
                {
                    string tagName = tag
                        .Substring(2, tag.Length - 3);

                    return AllTags.Contains(tagName);
                }

                return false;
            }
        }

        public static bool IsStartTag(string tag)
        {
            if (string.IsNullOrEmpty(tag.Trim()))
                return false;

            tag = tag.ToLower();

            lock (Lock)
            {
                if (tag.StartsWith("<") && !tag.StartsWith("</"))
                {
                    string tagName = tag
                        .Substring(1, tag.Length - 2)
                        .Split('=')
                        .First();

                    return AllTags.Contains(tagName);
                }

                return false;
            }
        }
    }
}