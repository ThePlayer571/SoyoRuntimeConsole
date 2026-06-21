using System.Text;

namespace Soyo.SoyoRuntimeConsole.Samples.UsingOriginalStyledCommand.ValueObjects
{
    public readonly struct GreetStyle
    {
        public GreetTone Tone { get; }
        public bool WithHandShake { get; }

        public GreetStyle(GreetTone tone, bool withHandShake)
        {
            Tone = tone;
            WithHandShake = withHandShake;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Hello");
            stringBuilder.Append(Tone.ToDisplayString());

            if (WithHandShake)
            {
                stringBuilder.Append("[HandShake]");
            }

            return stringBuilder.ToString();
        }
    }
}