using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public class StringParameterHandler : ParameterHandlerBase
    {
        public StringParameterHandler([DisallowNull] string name) : base(name, "String")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                yield return @"""";
            }
        }

        public override bool ShouldAdvance(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            parameter = ParameterHandlerParsingUtility.NormalizeSpaceSplitParameter(parameter);

            var quoted = parameter.StartsWith('"');

            if (quoted)
            {
                return parameter.Length >= 3 && parameter[^1] == ' ' && parameter[^2] == '"';
            }
            else
            {
                return parameter.EndsWith(' ');
            }
        }

        public override bool IsValid(string parameter)
        {
            parameter = parameter.Trim();
            var quoted = parameter.StartsWith('"');

            if (quoted)
            {
                return parameter.Length >= 2 && parameter[^1] == '"' && parameter.IndexOf('"', 1) < 0;
            }
            else
            {
                return true;
            }
        }

        public override object Parse(string parameter)
        {
            parameter = parameter.Trim();
            var quoted = parameter.StartsWith('"');

            if (quoted)
            {
                return parameter[1..^2];
            }
            else
            {
                return parameter;
            }
        }

        public override bool IsInitialized => true;
    }
}