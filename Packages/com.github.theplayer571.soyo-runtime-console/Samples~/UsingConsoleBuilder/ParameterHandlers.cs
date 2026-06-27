using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Soyo.SoyoRuntimeConsole.ParameterHandlers;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingConsoleBuilder
{
    // 定义待解析的类
    public class MyNumber
    {
        public int X { get; set; }
    }

    public class MyNumberSon : MyNumber
    {
    }


    // 手动编写ParameterHandler
    public class MyNumberParameterHandler : SpaceSplitParameterHandlerBase
    {
        public MyNumberParameterHandler([DisallowNull] string name) : base(name, "MyNumber")
        {
        }

        public override IEnumerable<string> GetCandidates(string parameter)
        {
            yield return "100"; // 自定义：始终返回100
        }

        public override bool IsValid(string parameter)
        {
            return int.TryParse(parameter, out _);
        }

        public override object Parse(string parameter)
        {
            // 返回子类，兼容父类
            return new MyNumberSon { X = int.Parse(parameter) };
        }

        // 如果构造发生不正确就把这个置为false，平时注意要保持true，不然参数会被排除
        public override bool IsInitialized => true; 
    }
}