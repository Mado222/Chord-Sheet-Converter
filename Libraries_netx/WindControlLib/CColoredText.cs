using System.Drawing;

namespace WindControlLib
{
    public class ColoredText(string? text, Color color)
    {
        public string Text { get; set; } = text ?? "";
        public Color Color { get; set; } = color;

        public override string ToString()
        {
            return Text;
        }
    }
}
