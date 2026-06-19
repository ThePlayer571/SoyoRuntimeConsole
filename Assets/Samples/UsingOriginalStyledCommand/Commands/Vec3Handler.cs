using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// Vec3 的复合参数处理器。使用圆括号 () 包裹，逗号分隔，
    /// 内部依次为 int x、int y、bool flag。
    /// 示例输入：(1, 2, true)
    /// </summary>
    public class Vec3Handler : CompositeParameterHandler
    {
        public Vec3Handler()
            : base("vec3", "Vec3", BracketType.Parentheses,
                new IntegerParameterHandler("x"),
                new IntegerParameterHandler("y"),
                new BooleanParameterHandler("flag"))
        {
        }

        public override object Parse(string parameter)
        {
            var parts = GetParsedSubParameters(parameter);
            return new Vec3((int)parts[0], (int)parts[1], (bool)parts[2]);
        }
    }
}
