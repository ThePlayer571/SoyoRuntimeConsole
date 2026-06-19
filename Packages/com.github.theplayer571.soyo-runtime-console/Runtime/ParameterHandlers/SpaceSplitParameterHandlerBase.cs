using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Soyo.SoyoRuntimeConsole.ParameterHandlers
{
    public abstract class SpaceSplitParameterHandlerBase : ParameterHandlerBase
    {
        protected SpaceSplitParameterHandlerBase([CanBeNull] string name, [CanBeNull] string type) : base(name, type)
        {
        }

        protected SpaceSplitParameterHandlerBase(in IParameterHandler.Description description) : base(in description)
        {
        }


        public override bool ShouldAdvance(string parameter)
        {
            return !string.IsNullOrEmpty(parameter) && Normalize(parameter).EndsWith(' ');
        }

        protected string Normalize([DisallowNull] string parameter) =>
            ParameterHandlerParsingUtility.NormalizeSpaceSplitParameter(parameter);
    }
}