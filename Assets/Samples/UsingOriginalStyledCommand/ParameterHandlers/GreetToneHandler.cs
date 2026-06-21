using Soyo.SoyoRuntimeConsole.ParameterHandlers;
using Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ParameterHandlers
{
    // 继承EnumParameterHandler，实现Enum参数
    // 也可以不建一个类，而是在每次使用时创建一个EnumParameterHandler
    public class GreetToneHandler : EnumParameterHandler<GreetTone>
    {
        public GreetToneHandler(string name) : base(name)
        {
        }
    }


}