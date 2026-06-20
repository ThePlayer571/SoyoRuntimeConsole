using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 问候格式参数处理器 —— 展示 Tuple(Tuple, simple) 的嵌套能力（混合括号类型）。
    /// 外层使用花括号，第一个子参数是 GreetStyleHandler（圆括号元组），
    /// 第二个子参数是浮点数 volume。
    ///
    /// 示例输入：{(Shout, true), 0.8}
    /// 解析结果：object[] { GreetStyle, float }
    /// </summary>
    public class GreetFormatHandler : TupleParameterHandler
    {
        public GreetFormatHandler()
            : base("format", "GreetFormat", BracketType.Braces,
                new GreetStyleHandler(),                   // (tone, cute)
                new FloatParameterHandler("volume"))       // float
        {
        }

        public override object Parse(string parameter)
        {
            return GetParsedSubParameters(parameter);
        }
    }
}
