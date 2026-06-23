using System.Linq;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ValueObjects;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ParameterHandlers
{
    // 继承CompositeParameterHandler，可以选择多种参数格式
    public class GreetConfigHandler : CompositeParameterHandler
    {
        public GreetConfigHandler(string name) : base(name, "GreetConfig",
            new GreetConfigFullHandler(name), new GreetConfigSimpleHandler(name))
        {
        }


        // 基于TupleParameterHandler的解析器
        private class GreetConfigFullHandler : TupleParameterHandler
        {
            public GreetConfigFullHandler(string name) : base(name, "GreetConfig", BracketType.Braces,
                new GreetStyleHandler("greetStyle"),
                new ArrayParameterHandler<string>("words", new StringParameterHandler("word")))
            {
            }

            public override object Parse(string parameter)
            {
                var paras = GetParsedSubParameters(parameter);

                var greetStyle = (GreetStyle)paras[0];
                var words = (object[])paras[1];

                return new GreetConfig(greetStyle, words.Select(w => (string)w).ToArray());
            }
        }

        // 简化的解析器
        private class GreetConfigSimpleHandler : TupleParameterHandler
        {
            public GreetConfigSimpleHandler(string name) : base(name, "GreetConfig", BracketType.Braces,
                new GreetStyleHandler("greetStyle"))
            {
            }

            public override object Parse(string parameter)
            {
                var paras = GetParsedSubParameters(parameter);

                var greetStyle = (GreetStyle)paras[0];

                return new GreetConfig(greetStyle, null);
            }
        }
    }
}