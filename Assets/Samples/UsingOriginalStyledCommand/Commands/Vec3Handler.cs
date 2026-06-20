using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// Vec3 的复合参数处理器。支持两种输入格式：
    /// (int x, int y) → bool flag 默认为 false；
    /// (int x, int y, bool flag) → 明确指定 flag。
    /// 示例输入：(1, 2) 或 (1, 2, true)
    /// </summary>
    public class Vec3Handler : CompositeParameterHandler
    {
        public Vec3Handler()
            : base("vec3", "Vec3",
                new Vec3ShortHandler(),
                new Vec3FullHandler())
        {
        }

        /// <summary>
        /// (int, int) → Vec3(x, y, false)
        /// </summary>
        private class Vec3ShortHandler : TupleParameterHandler
        {
            public Vec3ShortHandler()
                : base("vec3", "Vec3", BracketType.Parentheses,
                    new IntegerParameterHandler("x"),
                    new IntegerParameterHandler("y"))
            {
            }

            public override object Parse(string parameter)
            {
                var parts = GetParsedSubParameters(parameter);
                return new Vec3((int)parts[0], (int)parts[1], false);
            }
        }

        /// <summary>
        /// (int, int, bool) → Vec3(x, y, flag)
        /// </summary>
        private class Vec3FullHandler : TupleParameterHandler
        {
            public Vec3FullHandler()
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
}
