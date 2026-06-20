using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// GreetConfig 的元组参数处理器。使用花括号 {} 包裹，逗号分隔，
    /// 内部依次为 HelloTone 枚举和 bool cute。
    /// 示例输入：{Doubt, true}
    /// </summary>
    public class GreetConfigHandler : TupleParameterHandler
    {
        public GreetConfigHandler()
            : base("config", "GreetConfig", BracketType.Braces,
                new EnumParameterHandler<HelloTone>("tone"),
                new BooleanParameterHandler("cute"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new GreetConfig((HelloTone)parts[0], (bool)parts[1]);
        }
    }
}
