using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class BooleanParameterHandler : SpaceSplitParameterHandlerBase
    {
        public BooleanParameterHandler([DisallowNull] string name) : base(name, "Boolean")
        {
        }

        private string True => "true";
        private string False => "false";

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return True;
                yield return False;
                yield break;
            }

            parameter = parameter.Trim();

            if (True.StartsWith(parameter, StringComparison.OrdinalIgnoreCase))
            {
                yield return True;
            }

            if (False.StartsWith(parameter, StringComparison.OrdinalIgnoreCase))
            {
                yield return False;
            }
        }

        public override bool IsValid(string parameter)
        {
            parameter = parameter.Trim();
            return parameter == True || parameter == False;
        }

        public override object Parse(string parameter)
        {
            parameter = parameter.Trim();
            if (parameter == True)
            {
                return true;
            }

            if (parameter == False)
            {
                return false;
            }

            return null;
        }

        public override bool IsInitialized => true;
    }
}