using System;
using System.Text;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    internal static class ParameterHandlerParsingUtility
    {
        // Returns true when the provided parameter string ends with the delimiter used to separate
        // parameters (a single space). This is used by handlers to decide whether the user has
        // finished typing the current parameter when parsing incrementally.
        internal static bool HasTrailingDelimiter(string parameter)
        {
            return !string.IsNullOrEmpty(parameter) && parameter.EndsWith(' ');
        }

        // Removes a single trailing delimiter (space) if present. Handlers should call this before
        // attempting to parse the logical value of the parameter so that trailing spaces don't
        // interfere with parsing routines.
        internal static string TrimTrailingDelimiter(string parameter)
        {
            return HasTrailingDelimiter(parameter) ? parameter[..^1] : parameter;
        }

        // Attempts to parse a quoted string literal from the given parameter.
        // Rules:
        // - A valid quoted string must start and end with double quotes (").
        // - A single trailing delimiter (space) is allowed and ignored.
        // - The backslash character (\\) can be used to escape characters including the quote.
        // - Returns true and outputs the unescaped inner string when parsing succeeds; otherwise false.
        internal static bool TryParseQuotedString(string parameter, out string value)
        {
            value = null;

            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            var core = TrimTrailingDelimiter(parameter);
            if (core.Length < 2 || core[0] != '"' || core[^1] != '"')
            {
                return false;
            }

            var builder = new StringBuilder(core.Length - 2);
            var escaping = false;

            for (var i = 1; i < core.Length - 1; i++)
            {
                var ch = core[i];

                if (escaping)
                {
                    builder.Append(ch);
                    escaping = false;
                }
                else if (ch == '\\')
                {
                    // Enter escape mode: next character is taken literally.
                    escaping = true;
                }
                else if (ch == '"')
                {
                    // Unescaped quote inside content is invalid
                    return false;
                }
                else
                {
                    builder.Append(ch);
                }
            }

            // Trailing backslash with nothing to escape is invalid
            if (escaping)
            {
                return false;
            }

            value = builder.ToString();
            return true;
        }

        // Parses a parenthesis-wrapped, comma-separated list of components like "(x, y, z)".
        // - `expectedCount` enforces how many comma-separated components must be present.
        // - A single trailing space is allowed and ignored.
        // - Each component is trimmed of whitespace; empty components cause parsing to fail.
        // On success, fills `components` with the raw component strings (trimmed) and returns true.
        internal static bool TryParseBracketedComponents(string parameter, int expectedCount, out string[] components)
        {
            components = null;

            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            var core = TrimTrailingDelimiter(parameter);
            if (core.Length < 2 || core[0] != '(' || core[^1] != ')')
            {
                return false;
            }

            var inner = core.Substring(1, core.Length - 2);
            var parts = inner.Split(',');
            if (parts.Length != expectedCount)
            {
                return false;
            }

            components = new string[expectedCount];
            for (var i = 0; i < expectedCount; i++)
            {
                var component = parts[i].Trim();
                if (component.Length == 0)
                {
                    return false;
                }

                components[i] = component;
            }

            return true;
        }

        // Helper to build sample zero vectors used as completion candidates, e.g. "(0, 0)" or "(0.0, 0.0)".
        internal static string BuildZeroVectorCandidate(int componentCount, bool decimalComponent)
        {
            var component = decimalComponent ? "0.0" : "0";
            var components = new string[componentCount];
            for (var i = 0; i < componentCount; i++)
            {
                components[i] = component;
            }

            return $"({string.Join(", ", components)})";
        }
    }
}
