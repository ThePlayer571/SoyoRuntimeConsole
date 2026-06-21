namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ValueObjects
{
    public enum GreetTone
    {
        Formal,
        Casual,
        Enthusiastic
    }

    public static class GreetToneExtensions
    {
        public static string ToDisplayString(this GreetTone tone)
        {
            return tone switch
            {
                GreetTone.Formal => ".",
                GreetTone.Casual => "~",
                GreetTone.Enthusiastic => "!",
                _ => tone.ToString()
            };
        }
    }
}