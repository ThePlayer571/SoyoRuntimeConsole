using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class Vector3IntParameterHandler : ParameterHandlerBase
    {
        public Vector3IntParameterHandler([DisallowNull] string name) : base(name, "Vector3Int")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return ParameterHandlerParsingUtility.BuildZeroVectorCandidate(3, false);
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
                || !int.TryParse(components[0], out var x)
                || !int.TryParse(components[1], out var y)
                || !int.TryParse(components[2], out var z))
            {
                value = null;
                return false;
            }

            value = new Vector3Int(x, y, z);
            return true;
        }

        public override bool IsInitialized => true;
    }
}