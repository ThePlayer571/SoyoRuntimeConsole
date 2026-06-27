using System;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Samples.QuickStart.ValueObjects;
using Soyo.SoyoRuntimeConsole.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Samples.QuickStart
{
    // 这里没有指定 TargetConsoleKey，意味着这些参数处理器适用于所有控制台
    public static class ParameterHandlers
    {
        // 定义参数处理器
        [ConsoleParameterHandler("GreetConfig", BracketType = BracketType.Braces)]
        public static GreetConfig GreetConfigHandler( // 函数名无所谓
            GreetStyle style,
            string[] words)
        {
            return new GreetConfig(style, words);
        }

        // 支持重载
        [ConsoleParameterHandler("GreetConfig")]
        public static GreetConfig GreetConfigHandler(GreetStyle style)
        {
            return new GreetConfig(style, Array.Empty<string>());
        }

        // 可以省略
        [ConsoleParameterHandler]
        public static GreetStyle GreetStyleHandler(
            GreetTone tone,
            bool withHandShake)
        {
            return new GreetStyle(tone, withHandShake);
        }
    }
}