using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class IntegerParameterHandler : ParameterHandlerBase
    {
        public IntegerParameterHandler([DisallowNull] string name) :
            base(name, "Integer")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            yield return "0";
        }

        public override bool ShouldAdvance(string parameter)
        {
            return parameter.EndsWith(' ');
        }

        public override bool IsValid(string parameter)
        {
            return int.TryParse(parameter, out _);
        }

        public override bool TryParse(string parameter, out object value)
        {
            if (int.TryParse(parameter, out int result))
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