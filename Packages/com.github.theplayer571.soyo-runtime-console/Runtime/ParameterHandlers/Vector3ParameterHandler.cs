using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class Vector3ParameterHandler : ParameterHandlerBase
    {
        public Vector3ParameterHandler([DisallowNull] string name) : base(name, "Vector3")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(3, false);
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(3, true);
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
            if (!ParameterHandlerParsingUtility.TryParseBracketedComponents(parameter, 3, out var components)
                || !float.TryParse(components[0], out var x)
                || !float.TryParse(components[1], out var y)
                || !float.TryParse(components[2], out var z))
            {
                value = null;
                return false;
            }

            value = new Vector3(x, y, z);
            return true;
        }

        public override bool IsInitialized => true;
    }
}

