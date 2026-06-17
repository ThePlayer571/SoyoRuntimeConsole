using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class FloatParameterHandler : ParameterHandlerBase
    {
        public FloatParameterHandler([DisallowNull] string name) : base(name, "Float")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            var core = ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter);
            if (string.IsNullOrEmpty(core))
            {
                yield return "0";
                yield return "0.0";
            }
            else if (float.TryParse(core, out var result) && Mathf.Approximately(result, 0f))
            {
                if (core.Contains("."))
                {
                    yield return "0.0";
                }
                else
                {
                    yield return "0";
                }
            }
        }

        public override bool ShouldAdvance(string parameter)
        {
            return ParameterHandlerParsingUtility.HasTrailingDelimiter(parameter);
        }

        public override bool IsValid(string parameter)
        {
            return float.TryParse(ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter), out _);
        }

        public override bool TryParse(string parameter, out object value)
        {
            if (float.TryParse(ParameterHandlerParsingUtility.TrimTrailingDelimiter(parameter), out var result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

        public override bool IsInitialized => true;
    }
}