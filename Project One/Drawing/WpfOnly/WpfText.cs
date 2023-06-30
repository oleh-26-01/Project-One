using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace Project_One.Drawing.WpfOnly;

public class WpfText : WpfAbstract
{
    private readonly TextBlock _textBlock;

    public WpfText(string text, Vector2 position, float fontSize = 12)
    {
        _textBlock = WpfObjects.TextBlock(text, position.X, position.Y, fontSize);
        Objects.Add(_textBlock);
    }

    public void SetText(string text)
    {
        _textBlock.Text = text;
    }

    public void SetPosition(Vector2 position)
    {
        _textBlock.Margin = new Thickness(position.X, position.Y, 0, 0);
    }
}