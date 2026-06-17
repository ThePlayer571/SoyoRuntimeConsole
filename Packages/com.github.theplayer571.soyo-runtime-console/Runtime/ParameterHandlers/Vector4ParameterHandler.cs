using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class Vector4ParameterHandler : ParameterHandlerBase
    {
        public Vector4ParameterHandler([DisallowNull] string name) : base(name, "Vector4")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(4, false);
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(4, true);
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
            if (!ParameterHandlerParsingUtility.TryParseBracketedComponents(parameter, 4, out var components)
                || !float.TryParse(components[0], out var x)
                || !float.TryParse(components[1], out var y)
                || !float.TryParse(components[2], out var z)
                || !float.TryParse(components[3], out var w))
            {
                value = null;
                return false;
            }

            value = new Vector4(x, y, z, w);
            return true;
        }

        public override bool IsInitialized => true;
    }
}