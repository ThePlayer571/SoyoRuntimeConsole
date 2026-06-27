    using System.Text;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ValueObjects
{
    public readonly struct GreetConfig              
    {
        public GreetStyle Style { get; }
        public string[] Words { get; }
        
        public GreetConfig(GreetStyle style, string[] words)
        {
            Style = style;
            Words = words;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Style.ToString());
            if (Words != null && Words.Length > 0)
            {
                stringBuilder.Append(" - ");
                stringBuilder.Append(string.Join(", ", Words));
            }

            return stringBuilder.ToString();
        }
    }
}