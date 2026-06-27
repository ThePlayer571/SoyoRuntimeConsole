using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ValueObjects;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ParameterHandlers
{
    // 继承TupleParameterHandler，用于组合多个参数
    public class GreetStyleHandler : TupleParameterHandler
    {
        public GreetStyleHandler(string name) : base(name, "GreetStyle", BracketType.Braces,
            new GreetToneHandler("greetTone"), new BooleanParameterHandler("withHandShake"))
        {
        }

        public override object Parse(string parameter)
        {
            var paras = GetParsedSubParameters(parameter);

            var greetTone = (GreetTone)paras[0];
            var withHandShake = (bool)paras[1];

            return new GreetStyle(greetTone, withHandShake);
        }
    }
}