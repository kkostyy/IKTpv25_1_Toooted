using System.Drawing.Drawing2D;
using SWF = System.Windows.Forms;

namespace IKTpv25_1_Toooted;

// Varjutab System.Windows.Forms.TextRenderer-i selles nimeruumis.
// GDI tekst ei arvesta Graphics-i pööret ega nihet (TranslateTransform / RotateTransform):
// pööratud kaartidel joonistus tekst valesse kohta. Kui teisendus on olemas, joonistame GDI+-ga.
public static class TextRenderer
{
    public static Size MeasureText(string? text, Font? font) => SWF.TextRenderer.MeasureText(text, font);
    public static Size MeasureText(string? text, Font? font, Size proposedSize) => SWF.TextRenderer.MeasureText(text, font, proposedSize);
    public static Size MeasureText(string? text, Font? font, Size proposedSize, TextFormatFlags flags) => SWF.TextRenderer.MeasureText(text, font, proposedSize, flags);
    public static Size MeasureText(IDeviceContext dc, string? text, Font? font) => SWF.TextRenderer.MeasureText(dc, text, font);
    public static Size MeasureText(IDeviceContext dc, string? text, Font? font, Size proposedSize, TextFormatFlags flags) => SWF.TextRenderer.MeasureText(dc, text, font, proposedSize, flags);

    public static void DrawText(IDeviceContext dc, string? text, Font? font, Point pt, Color foreColor) =>
        DrawText(dc, text, font, pt, foreColor, TextFormatFlags.Default);

    public static void DrawText(IDeviceContext dc, string? text, Font? font, Point pt, Color foreColor, TextFormatFlags flags)
    {
        if (dc is Graphics g && Teisendatud(g) && font != null)
        {
            var s = SWF.TextRenderer.MeasureText(text, font, Size.Empty, flags);
            Joonista(g, text, font, new Rectangle(pt, s), foreColor, flags);
            return;
        }
        SWF.TextRenderer.DrawText(dc, text, font, pt, foreColor, flags);
    }

    public static void DrawText(IDeviceContext dc, string? text, Font? font, Point pt, Color foreColor, Color backColor) =>
        SWF.TextRenderer.DrawText(dc, text, font, pt, foreColor, backColor);

    public static void DrawText(IDeviceContext dc, string? text, Font? font, Rectangle bounds, Color foreColor) =>
        DrawText(dc, text, font, bounds, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

    public static void DrawText(IDeviceContext dc, string? text, Font? font, Rectangle bounds, Color foreColor, TextFormatFlags flags)
    {
        if (dc is Graphics g && Teisendatud(g) && font != null)
        {
            Joonista(g, text, font, bounds, foreColor, flags);
            return;
        }
        SWF.TextRenderer.DrawText(dc, text, font, bounds, foreColor, flags);
    }

    public static void DrawText(IDeviceContext dc, string? text, Font? font, Rectangle bounds, Color foreColor, Color backColor, TextFormatFlags flags) =>
        SWF.TextRenderer.DrawText(dc, text, font, bounds, foreColor, backColor, flags);

    private static bool Teisendatud(Graphics g)
    {
        using var m = g.Transform;
        return !m.IsIdentity;
    }

    private static void Joonista(Graphics g, string? text, Font font, Rectangle r, Color c, TextFormatFlags flags)
    {
        if (string.IsNullOrEmpty(text)) return;
        using var sf = new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.None };
        sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        if ((flags & TextFormatFlags.WordBreak) == 0) sf.FormatFlags |= StringFormatFlags.NoWrap;
        sf.Alignment = (flags & TextFormatFlags.HorizontalCenter) != 0 ? StringAlignment.Center
                     : (flags & TextFormatFlags.Right) != 0 ? StringAlignment.Far : StringAlignment.Near;
        sf.LineAlignment = (flags & TextFormatFlags.VerticalCenter) != 0 ? StringAlignment.Center
                         : (flags & TextFormatFlags.Bottom) != 0 ? StringAlignment.Far : StringAlignment.Near;
        if ((flags & TextFormatFlags.EndEllipsis) != 0) sf.Trimming = StringTrimming.EllipsisCharacter;
        if ((flags & TextFormatFlags.NoPrefix) == 0) sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Hide;
        var vana = g.TextRenderingHint;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        using var b = new SolidBrush(c);
        // GDI jätab vasakule väikese varu – nihutame sama palju, et asukoht klapiks
        float varu = (flags & TextFormatFlags.NoPadding) != 0 ? 0 : font.Size * 0.25f;
        g.DrawString(text, font, b, new RectangleF(r.X + varu, r.Y, Math.Max(1, r.Width), Math.Max(1, r.Height)), sf);
        g.TextRenderingHint = vana;
    }
}
