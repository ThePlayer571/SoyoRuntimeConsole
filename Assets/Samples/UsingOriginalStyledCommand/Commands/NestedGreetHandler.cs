using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 嵌套问候参数处理器 —— 展示 Tuple(Tuple, simple) 的嵌套能力。
    /// 外层使用圆括号，第一个子参数是 GreetStyleHandler（本身是一个圆括号元组），
    /// 第二个子参数是整数 repeat。
    ///
    /// 示例输入：((Normal, false), 3)
    /// 解析结果：object[] { GreetStyle, int }
    /// </summary>
    public class NestedGreetHandler : TupleParameterHandler
    {
        public NestedGreetHandler()
            : base("nested_greet", "NestedGreet", BracketType.Parentheses,
                new GreetStyleHandler(),                    // (tone, cute)
                new IntegerParameterHandler("repeat"))      // int
        {
        }

        public override object Parse(string parameter)
        {
            return GetParsedSubParameters(parameter);
        }
    }
}
