using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 多重问候配置参数处理器 —— 展示 Tuple(Tuple, Tuple) 的嵌套能力。
    /// 外层使用方括号，两个子参数均为 GreetStyleHandler（各自是圆括号元组）。
    /// 这是两个 Tuple 作为另一个 Tuple 的直接子级的完整演示。
    ///
    /// 示例输入：[(Shout, true), (Normal, false)]
    /// 解析结果：object[] { GreetStyle, GreetStyle }
    /// </summary>
    public class MultiGreetConfigHandler : TupleParameterHandler
    {
        public MultiGreetConfigHandler()
            : base("multi_config", "MultiGreetConfig", BracketType.Brackets,
                new GreetStyleHandler(),    // (tone, cute) — 第一个问候风格
                new GreetStyleHandler())    // (tone, cute) — 第二个问候风格
        {
        }

        public override object Parse(string parameter)
        {
            return GetParsedSubParameters(parameter);
        }
    }
}
