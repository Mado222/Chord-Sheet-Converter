using System.Drawing;

namespace WindControlLib
{
    public class ColoredText
    {
        public string Text { get; set; }
        public Color Color { get; set; }

        public ColoredText(string text, Color color)
        {
            Text = text;
            Color = color;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
