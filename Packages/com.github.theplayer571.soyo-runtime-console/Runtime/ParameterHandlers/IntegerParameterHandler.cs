using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class IntegerParameterHandler : SpaceSplitParameterHandlerBase
    {
        public IntegerParameterHandler([DisallowNull] string name) : base(name, "Integer")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            parameter = parameter.Trim();
            if (string.IsNullOrEmpty(parameter) || int.TryParse(parameter, out int result) && result == 0)
            {
                yield return "0";
            }
        }

        public override bool IsValid(string parameter)
        {
            return int.TryParse(parameter.Trim(), out _);
        }

        public override object Parse(string parameter)
        {
            return int.Parse(parameter.Trim());
        }

        public override bool IsInitialized => true;
    }
}