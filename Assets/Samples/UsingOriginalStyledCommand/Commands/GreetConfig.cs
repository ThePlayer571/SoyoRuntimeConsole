namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    /// <summary>
    /// 问候配置，由语调和是否可爱组合而成。
    /// </summary>
    public class GreetConfig
    {
        public HelloTone Tone;
        public bool Cute;

        public GreetConfig(HelloTone tone, bool cute)
        {
            Tone = tone;
            Cute = cute;
        }
    }
}
