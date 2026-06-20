using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// GreetStyle 的元组参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 HelloTone 枚举和 bool cute。
    /// 示例输入：(Shout, true)
    ///
    /// 本处理器可作为子处理器嵌入到更复杂的嵌套元组中，
    /// 以展示 TupleParameterHandler 的嵌套能力。
    /// </summary>
    public class GreetStyleHandler : TupleParameterHandler
    {
        public GreetStyleHandler()
            : base("style", "GreetStyle", BracketType.Parentheses,
                new EnumParameterHandler<HelloTone>("tone"),
                new BooleanParameterHandler("cute"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new GreetStyle((HelloTone)parts[0], (bool)parts[1]);
        }
    }
}
