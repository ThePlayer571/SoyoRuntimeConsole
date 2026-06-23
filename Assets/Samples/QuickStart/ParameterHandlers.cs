using System;
using Soyo.SoyoRuntimeConsole.Attributes;
using Soyo.SoyoRuntimeConsole.Samples.QuickStart.ValueObjects;

namespace Soyo.SoyoRuntimeConsole.Samples.QuickStart
{
    public static class ParameterHandlers
    {
        // 定义参数处理器
        [ConsoleParameterHandler("GreetConfig")]
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