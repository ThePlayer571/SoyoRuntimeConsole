namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.Commands
{
    public static class HelloToneExtensions
    {
        public static string ToDisplayString(this HelloTone tone)
        {
            return tone switch
            {
                HelloTone.Normal => ".",
                HelloTone.Doubt => "?",
                HelloTone.Shout => "!",
                _ => tone.ToString()
            };
        }
    }
}