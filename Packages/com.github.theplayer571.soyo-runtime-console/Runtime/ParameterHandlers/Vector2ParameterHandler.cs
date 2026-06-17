using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class Vector2ParameterHandler : ParameterHandlerBase
    {
        public Vector2ParameterHandler([DisallowNull] string name) : base(name, "Vector2")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(2, false);
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(2, true);
            }
        }

        public override bool ShouldAdvance(string parameter)
        {
            return ParameterHandlerParsingUtility.HasTrailingDelimiter(parameter)
                   && ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter).EndsWith(")");
        }

        public override bool IsValid(string parameter)
        {
            return TryParse(parameter, out _);
        }

        public override bool TryParse(string parameter, out object value)
        {
            if (!ParameterHandlerParsingUtility.TryParseBracketedComponents(parameter, 2, out var components)
                || !float.TryParse(components[0], out var x)
                || !float.TryParse(components[1], out var y))
            {
                value = null;
                return false;
            }

            value = new Vector2(x, y);
            return true;
        }

        public override bool IsInitialized => true;
    }
}