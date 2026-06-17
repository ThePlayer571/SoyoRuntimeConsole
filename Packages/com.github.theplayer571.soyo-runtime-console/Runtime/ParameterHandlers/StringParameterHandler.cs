using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class StringParameterHandler : ParameterHandlerBase
    {
        public StringParameterHandler([DisallowNull] string name) : base(name, "String")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return "\"\"";
            }
        }

        public override bool ShouldAdvance(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            if (!ParameterHandlerParsingUtility.HasTrailingDelimiter(parameter))
            {
                return false;
            }

            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            if (!string.IsNullOrEmpty(core) && core[0] == '"')
            {
                // If it's a quoted string, advance only when the quoted string is complete and
                // the user has typed the trailing delimiter.
                return ParameterHandlerParsingUtility.TryParseQuotedString(parameter, out _);
            }

            // For unquoted strings, trailing delimiter is enough to advance.
            return true;
        }

        public override bool IsValid(string parameter)
        {
            if (ParameterHandlerParsingUtility.TryParseQuotedString(parameter, out _))
            {
                return true;
            }

            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            return !string.IsNullOrEmpty(core) && core[0] != '"' && core.IndexOf('"') < 0;
        }

        public override bool TryParse(string parameter, out object value)
        {
            if (ParameterHandlerParsingUtility.TryParseQuotedString(parameter, out var quotedValue))
            {
                value = quotedValue;
                return true;
            }

            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            if (!string.IsNullOrEmpty(core) && core[0] != '"' && core.IndexOf('"') < 0)
            {
                value = core;
                return true;
            }

            value = null;
            return false;
        }

        public override bool IsInitialized => true;
    }
}