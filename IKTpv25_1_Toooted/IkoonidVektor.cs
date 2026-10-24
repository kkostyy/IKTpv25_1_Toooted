using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Vektorikoonid 16×16 ruudustikul. Ei sõltu süsteemi fontidest, seega ei teki kunagi
// "kastikesi" ja ikoonid on igas arvutis ühesugused.
public static partial class Ikoonid
{
    private static bool JoonistaVektor(Graphics g, string glyph, Rectangle r, Color varv, float suurus)
    {
        if (glyph.Length != 1) return false;
        Action<Graphics, Pen, Brush>? joonis = glyph[0] switch
        {
            '' => (g, p, b) => { g.DrawLine(p, 8, 2, 8, 14); g.DrawLine(p, 2, 8, 14, 8); },
            '' => (g, p, b) => { g.DrawLine(p, 3, 3, 13, 13); g.DrawLine(p, 13, 3, 3, 13); },
            '' => (g, p, b) => { Tapp(g, b, 3, 8, 1.2f); Tapp(g, b, 8, 8, 1.2f); Tapp(g, b, 13, 8, 1.2f); },
            '' => Hammasratas,
            '' => (g, p, b) =>
            {
                g.DrawEllipse(p, 3.5f, 2.5f, 5, 5);
                g.DrawArc(p, 1, 9.5f, 10, 10, 180, 180);
                g.DrawArc(p, 9.5f, 3.5f, 4, 4, 140, 290);
                g.DrawArc(p, 9f, 9.5f, 6.5f, 8, 220, 140);
            },
            '' => (g, p, b) =>
            {
                g.DrawRectangle(p, 2.5f, 5.5f, 11, 9);
                g.DrawArc(p, 5.5f, 1.5f, 5, 7, 180, 180);
            },
            '' => (g, p, b) =>
            {
                g.DrawEllipse(p, 5.5f, 1.5f, 9, 9);
                using var j = new Pen(p.Color, p.Width * 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                g.DrawLine(j, 6.6f, 9.4f, 2f, 14f);
            },
            '' => (g, p, b) =>
            {
                g.DrawLines(p, new[] { P(5, 4.5f), P(6.3f, 2.5f), P(9.7f, 2.5f), P(11, 4.5f) });
                Umar(g, p, 1.5f, 4.5f, 13, 9, 1.5f);
                g.DrawEllipse(p, 5.5f, 6.5f, 5, 5);
            },
            '' => (g, p, b) =>
            {
                g.DrawRectangle(p, 3, 7, 10, 7.5f);
                g.DrawArc(p, 5, 2, 6, 7, 180, 180);
                g.DrawLine(p, 5, 5.5f, 5, 7); g.DrawLine(p, 11, 5.5f, 11, 7);
                Tapp(g, b, 8, 10.7f, 1f);
            },
            '' => (g, p, b) => g.DrawPolygon(p, Taht()),
            '' => (g, p, b) => { g.FillPolygon(b, Taht()); g.DrawPolygon(p, Taht()); },
            '' => (g, p, b) => g.DrawLine(p, 2, 8, 14, 8),
            '' => (g, p, b) =>
            {
                g.DrawLine(p, 1.5f, 3.5f, 14.5f, 3.5f);
                g.DrawLines(p, new[] { P(6, 3.5f), P(6, 1.5f), P(10, 1.5f), P(10, 3.5f) });
                g.DrawLines(p, new[] { P(3.5f, 3.5f), P(4.5f, 14.5f), P(11.5f, 14.5f), P(12.5f, 3.5f) });
                g.DrawLine(p, 6.5f, 6, 6.5f, 12); g.DrawLine(p, 9.5f, 6, 9.5f, 12);
            },
            '' => (g, p, b) =>
            {
                g.DrawPolygon(p, new[] { P(1.5f, 1.5f), P(12, 1.5f), P(14.5f, 4), P(14.5f, 14.5f), P(1.5f, 14.5f) });
                g.DrawRectangle(p, 4.5f, 1.5f, 6, 4);
                g.DrawRectangle(p, 4, 9, 8, 5.5f);
            },
            '' => (g, p, b) => g.DrawLines(p, new[] { P(10.5f, 2), P(4.5f, 8), P(10.5f, 14) }),
            '' => (g, p, b) => g.DrawLines(p, new[] { P(5.5f, 2), P(11.5f, 8), P(5.5f, 14) }),
            '' => (g, p, b) =>
            {
                g.DrawEllipse(p, 5, 1.5f, 6, 6);
                g.DrawArc(p, 2, 9.5f, 12, 11, 180, 180);
            },
            '' => (g, p, b) =>
            {
                using var gp = new GraphicsPath();
                gp.AddArc(1.5f, 1.5f, 13, 13, 70, 300);
                gp.AddBezier(P(11.6f, 3.6f), P(9.5f, 7f), P(13.5f, 8f), P(10.3f, 14.2f));
                g.DrawPath(p, gp);
                Tapp(g, b, 5, 5.5f, 1.1f); Tapp(g, b, 8.3f, 4.2f, 1.1f); Tapp(g, b, 4.4f, 9.2f, 1.1f); Tapp(g, b, 7, 12, 1.1f);
            },
            '' => (g, p, b) =>
            {
                using var gp = new GraphicsPath();
                gp.AddBezier(P(0.8f, 8), P(4, 2.5f), P(12, 2.5f), P(15.2f, 8));
                gp.AddBezier(P(15.2f, 8), P(12, 13.5f), P(4, 13.5f), P(0.8f, 8));
                g.DrawPath(p, gp);
                g.DrawEllipse(p, 5.7f, 5.7f, 4.6f, 4.6f);
            },
            '' => (g, p, b) =>
            {
                g.DrawPolygon(p, new[] { P(8, 1.5f), P(14.5f, 4.5f), P(14.5f, 11.5f), P(8, 14.5f), P(1.5f, 11.5f), P(1.5f, 4.5f) });
                g.DrawLines(p, new[] { P(1.5f, 4.5f), P(8, 7.5f), P(14.5f, 4.5f) });
                g.DrawLine(p, 8, 7.5f, 8, 14.5f);
            },
            '' => (g, p, b) =>
            {
                g.DrawLines(p, new[] { P(0.5f, 2.5f), P(2.8f, 2.5f), P(4.8f, 11), P(13.5f, 11) });
                g.DrawLines(p, new[] { P(3.5f, 5), P(15, 5), P(13.6f, 9), P(4.4f, 9) });
                Tapp(g, b, 6, 13.7f, 1.3f); Tapp(g, b, 12, 13.7f, 1.3f);
            },
            '' => (g, p, b) =>
            {
                g.DrawArc(p, 2, 2.5f, 12, 12, 300, 300);
                g.DrawLine(p, 8, 1, 8, 8);
            },
            '' => (g, p, b) =>
            {
                g.DrawLines(p, new[] { P(1, 8), P(8, 1.5f), P(15, 8) });
                g.DrawLines(p, new[] { P(3, 6.5f), P(3, 14.5f), P(13, 14.5f), P(13, 6.5f) });
                g.DrawLines(p, new[] { P(6.5f, 14.5f), P(6.5f, 10), P(9.5f, 10), P(9.5f, 14.5f) });
            },
            '' => (g, p, b) =>
            {
                g.DrawArc(p, 2, 2, 12, 12, 200, 300);
                g.DrawLines(p, new[] { P(1.3f, 3.5f), P(2.4f, 6.4f), P(5.3f, 5.6f) });
                g.DrawLines(p, new[] { P(8, 4.5f), P(8, 8), P(10.5f, 9.5f) });
            },
            '' => (g, p, b) =>
            {
                g.DrawEllipse(p, 1.5f, 1.5f, 13, 13);
                g.DrawLines(p, new[] { P(8, 4), P(8, 8), P(11, 10) });
            },
            '' => (g, p, b) =>
            {
                g.DrawLines(p, new[] { P(7, 2), P(2, 2), P(2, 14), P(14, 14), P(14, 9) });
                g.DrawLine(p, 7.5f, 8.5f, 14, 2);
                g.DrawLines(p, new[] { P(9.5f, 2), P(14, 2), P(14, 6.5f) });
            },
            '' or '' => (g, p, b) =>
            {
                using var gp = new GraphicsPath();
                gp.AddBezier(P(2.5f, 13.5f), P(1.5f, 5), P(7, 2), P(14, 2));
                gp.AddBezier(P(14, 2), P(14.5f, 9), P(11, 14.5f), P(2.5f, 13.5f));
                g.DrawPath(p, gp);
                g.DrawLine(p, 2.5f, 13.5f, 10, 6);
            },
            '' => (g, p, b) =>
            {
                Umar(g, p, 1, 3, 14, 10, 1.5f);
                using var t = new Pen(p.Color, p.Width * 1.6f);
                g.DrawLine(t, 1, 6.2f, 15, 6.2f);
                g.DrawLine(p, 3, 10, 6.5f, 10);
            },
            '' => (g, p, b) =>
            {
                g.DrawPolygon(p, new[] { P(1.5f, 1.5f), P(8, 1.5f), P(14.5f, 8), P(8, 14.5f), P(1.5f, 8) });
                g.DrawEllipse(p, 4, 4, 2, 2);
            },
            '' => (g, p, b) =>
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = 3.5f + i * 4.5f;
                    Tapp(g, b, 2.5f, y, 1.1f);
                    g.DrawLine(p, 5.5f, y, 14.5f, y);
                }
            },
            '' => (g, p, b) =>
            {
                g.DrawEllipse(p, 2, 3.5f, 12, 12);
                g.DrawLine(p, 6.3f, 1, 9.7f, 1);
                g.DrawLine(p, 8, 1, 8, 3.5f);
                g.DrawLine(p, 8, 9.5f, 8, 6);
                g.DrawLine(p, 12.3f, 4, 13.6f, 5.3f);
            },
            '' => (g, p, b) =>
            {
                g.DrawEllipse(p, 1, 1, 14, 14);
                g.DrawLine(p, 8, 7, 8, 12);
                Tapp(g, b, 8, 4.6f, 1f);
            },
            '' => (g, p, b) =>
            {
                g.DrawLines(p, new[] { P(1.5f, 1.5f), P(1.5f, 14.5f), P(14.5f, 14.5f) });
                g.DrawLines(p, new[] { P(3.5f, 11.5f), P(6.5f, 7), P(9.5f, 9.5f), P(13.5f, 3.5f) });
            },
            '' => (g, p, b) =>
            {
                g.DrawLine(p, 1, 14.5f, 15, 14.5f);
                using var t = new Pen(p.Color, p.Width * 2f);
                g.DrawLine(t, 3.5f, 14, 3.5f, 9); g.DrawLine(t, 8, 14, 8, 3); g.DrawLine(t, 12.5f, 14, 12.5f, 6.5f);
            },
            '' => (g, p, b) =>
            {
                using var gp = new GraphicsPath();
                gp.AddBezier(P(8, 14), P(3, 10.5f), P(0.5f, 7.5f), P(1.5f, 4.3f));
                gp.AddBezier(P(1.5f, 4.3f), P(2.5f, 1.2f), P(6.5f, 1), P(8, 4.3f));
                gp.AddBezier(P(8, 4.3f), P(9.5f, 1), P(13.5f, 1.2f), P(14.5f, 4.3f));
                gp.AddBezier(P(14.5f, 4.3f), P(15.5f, 7.5f), P(13, 10.5f), P(8, 14));
                g.DrawPath(p, gp);
            },
            '' => (g, p, b) =>
            {
                g.DrawRectangle(p, 1.5f, 1.5f, 5.5f, 5.5f); g.DrawRectangle(p, 9, 1.5f, 5.5f, 5.5f);
                g.DrawRectangle(p, 1.5f, 9, 5.5f, 5.5f); g.DrawRectangle(p, 9, 9, 5.5f, 5.5f);
            },
            _ => null
        };
        if (joonis == null) return false;

        // Fondi suurus punktides → ikooni küljepikkus pikslites (nagu fondiglüüf)
        float kylg = suurus * g.DpiX / 72f;
        float k = kylg / 16f;
        var olek = g.Save();
        try
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TranslateTransform(r.X + (r.Width - kylg) / 2f, r.Y + (r.Height - kylg) / 2f);
            g.ScaleTransform(k, k);
            // Joon on umbes 1.3 px paksune (väikestel suurustel mitte õhem kui 1 px)
            float paksus = Math.Max(1.05f, 1.3f * Math.Max(1f, k * 0.9f)) / k;
            using var p = new Pen(varv, paksus) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var b = new SolidBrush(varv);
            joonis(g, p, b);
        }
        finally { g.Restore(olek); }
        return true;
    }

    private static PointF P(float x, float y) => new(x, y);

    private static void Tapp(Graphics g, Brush b, float x, float y, float r) => g.FillEllipse(b, x - r, y - r, r * 2, r * 2);

    private static void Umar(Graphics g, Pen p, float x, float y, float w, float h, float r)
    {
        using var gp = new GraphicsPath();
        gp.AddArc(x, y, r * 2, r * 2, 180, 90);
        gp.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
        gp.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
        gp.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
        gp.CloseFigure();
        g.DrawPath(p, gp);
    }

    private static PointF[] Taht()
    {
        var t = new PointF[10];
        for (int i = 0; i < 10; i++)
        {
            double a = -Math.PI / 2 + i * Math.PI / 5;
            float rr = i % 2 == 0 ? 7f : 2.9f;
            t[i] = new PointF(8 + rr * (float)Math.Cos(a), 8.6f + rr * (float)Math.Sin(a));
        }
        return t;
    }

    private static void Hammasratas(Graphics g, Pen p, Brush b)
    {
        var pts = new List<PointF>();
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f;
            foreach (var (nurk, rr) in new[] { (a - 15f, 5.2f), (a - 8f, 7f), (a + 8f, 7f), (a + 15f, 5.2f) })
            {
                double rad = nurk * Math.PI / 180;
                pts.Add(new PointF(8 + rr * (float)Math.Cos(rad), 8 + rr * (float)Math.Sin(rad)));
            }
        }
        g.DrawPolygon(p, pts.ToArray());
        g.DrawEllipse(p, 5.6f, 5.6f, 4.8f, 4.8f);
    }
}
