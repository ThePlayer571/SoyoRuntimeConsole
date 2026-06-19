using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class FloatParameterHandler : SpaceSplitParameterHandlerBase
    {
        public FloatParameterHandler([DisallowNull] string name) : base(name, "Float")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return "0";
                yield return "0.0";
                yield break;
            }

            parameter = parameter.Trim();
            if (float.TryParse(parameter, out var result) && Mathf.Approximately(result, 0f))
            {
                if (parameter.Contains("."))
                {
                    yield return "0.0";
                }
                else
                {
                    yield return "0";
                }
            }
        }

        public override bool IsValid(string parameter)
        {
            return float.TryParse(parameter.Trim(), out _);
        }

        public override object Parse(string parameter)
        {
            return float.Parse(parameter.Trim());
        }

        public override bool IsInitialized => true;
    }
}