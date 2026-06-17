using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class BooleanParameterHandler : ParameterHandlerBase
    {
        public BooleanParameterHandler([DisallowNull] string name) : base(name, "Boolean")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            var query = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            if (string.IsNullOrEmpty(query))
            {
                yield return "true";
                yield return "false";
                yield break;
            }

            if ("true".StartsWith(query, StringComparison.OrdinalIgnoreCase))
            {
                yield return "true";
            }

            if ("false".StartsWith(query, StringComparison.OrdinalIgnoreCase))
            {
                yield return "false";
            }
        }

        public override bool ShouldAdvance(string parameter)
        {
            return ParameterHandlerParsingUtility.HasTrailingDelimiter(parameter);
        }

        public override bool IsValid(string parameter)
        {
            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            return core == "true" || core == "false";
        }

        public override bool TryParse(string parameter, out object value)
        {
            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            if (core == "true")
            {
                value = true;
                return true;
            }

            if (core == "false")
            {
                value = false;
                return true;
            }

            value = null;
            return false;
        }

        public override bool IsInitialized => true;
    }
}