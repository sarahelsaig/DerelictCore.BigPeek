namespace DerelictCore.BigPeek.Models;

public record MagnificationInfo(
    float Width,
    float Height,
    int X,
    int Y,
    float MagnificationFactor = float.NaN)
{
    public MagnificationInfo(int width, int height, int x, int y)
        : this((float)width, height, x, y)
    {
    }

    /// <summary>
    /// Displays the positions and dimension in X11 geometry format (<c>WxH+X+Y</c>).
    /// </summary>
    /// <returns></returns>
    public string RectangleToString() => $"{Width}x{Height}{X:+0;-#}{Y:+0;-#}";
}