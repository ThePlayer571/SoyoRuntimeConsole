using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class Vector2IntParameterHandler : ParameterHandlerBase
    {
        public Vector2IntParameterHandler([DisallowNull] string name) : base(name, "Vector2Int")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(2, false);
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
                || !int.TryParse(components[0], out var x)
                || !int.TryParse(components[1], out var y))
            {
                value = null;
                return false;
            }

            value = new Vector2Int(x, y);
            return true;
        }

        public override bool IsInitialized => true;
    }
}