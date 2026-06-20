namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 问候风格，由语气和是否可爱组合而成。
    /// 通常作为嵌套元组的子元素使用。
    /// </summary>
    public struct GreetStyle
    {
        public HelloTone Tone;
        public bool Cute;

        public GreetStyle(HelloTone tone, bool cute)
        {
            Tone = tone;
            Cute = cute;
        }

        public override string ToString()
        {
            var cuteStr = Cute ? "qwq" : "";
            return $"({Tone.ToDisplayString()}{cuteStr})";
        }
    }
}
