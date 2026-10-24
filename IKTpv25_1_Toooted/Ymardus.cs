using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted
{
    // Abifunktsioonid ümardatud nurkade jaoks
    public static class Ymardus
    {
        public static GraphicsPath Tee(Rectangle r, int radius) => Tee((RectangleF)r, radius);

        public static GraphicsPath Tee(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            if (Teema.Umarad && radius > 0) radius = Math.Max(radius * 2, radius + 10);   // "pehme" stiil
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static Color Segu(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
    }

    // Ümardatud nurkadega paneel ("kaart")
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 12;
        public Color FillColor { get; set; } = Color.White;
        public Color BorderColor { get; set; } = Color.FromArgb(210, 214, 220);

        public RoundedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            BackColor = Color.White;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Nurkade taha tuleb vanema taustavärv
            Color taust = Parent != null ? Parent.BackColor : SystemColors.Control;
            e.Graphics.Clear(taust);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            // Moodne: kaks korda suurem ümarus (väljad = "pill"), raam vaevu nähtav
            int raadius = Teema.OnMoodne ? Math.Min(Math.Max(Radius * 2, 16), Math.Min(Width, Height) / 2) : Radius;
            Color raam = Teema.OnMoodne ? Ymardus.Segu(BorderColor, FillColor, 0.55f) : BorderColor;
            using (var path = Ymardus.Tee(r, raadius))
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(raam))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
            base.OnPaint(e);
        }
    }

    // Ümardatud nurkadega nupp. Värvid tulevad teemast:
    // Peamine = aktsentvärvi nupp, Ohtlik = punane (nt Kustuta), muidu hele nupp.
    public class RoundedButton : Button
    {
        public int Radius { get; set; } = 8;
        public bool? Peamine { get; set; }
        public bool Ohtlik { get; set; }

        private bool _hover;
        private bool _down;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color taust = Parent != null ? Parent.BackColor : Teema.Taust;
            g.Clear(taust);

            // Designeris loodud nuppude puhul otsustame algse taustavärvi järgi
            bool peamine = Peamine ??= Teema.OnAktsent(BackColor);

            Color fill, border, tekst;
            if (Ohtlik)
            {
                fill = peamine ? Teema.Viga : Teema.Pind;
                border = Teema.Viga;
                tekst = peamine ? Color.White : Teema.Viga;
            }
            else if (peamine)
            {
                fill = Teema.Aktsent;
                border = Teema.Aktsent;
                tekst = Color.White;
            }
            else if (Teema.OnMoodne)
            {
                // Moodne: teisene nupp on hall "pill" ilma raamita
                fill = Teema.Paneel2;
                border = Teema.Paneel2;
                tekst = Teema.Tekst;
            }
            else
            {
                fill = Teema.Pind;
                border = Teema.Raam;
                tekst = Teema.Tekst;
            }

            if (!Enabled)
            {
                fill = Teema.Paneel2;
                border = Teema.Joon;
                tekst = Teema.TekstNork;
            }
            else if (_down)
            {
                fill = peamine || Ohtlik ? Ymardus.Segu(fill, Color.Black, 0.2f) : Ymardus.Segu(fill, Teema.Aktsent, 0.22f);
            }
            else if (_hover)
            {
                fill = peamine ? Ymardus.Segu(fill, Color.Black, 0.1f) : Ymardus.Segu(fill, Ohtlik ? Teema.Viga : Teema.Aktsent, 0.1f);
                if (!peamine) border = Ohtlik ? Teema.Viga : Teema.Aktsent;
            }

            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            int raadius = Radius;
            if (Teema.OnMoodne)
            {
                raadius = r.Height / 2;                                 // "pill"
                if (_down) r = Rectangle.Inflate(r, -3, -2);            // vajutus: nupp väheneb
                else if (_hover && Enabled)
                {
                    // hõljumisel pehme vari all
                    using var vari = new SolidBrush(Color.FromArgb(28, 0, 0, 0));
                    using var vt = Ymardus.Tee(new Rectangle(r.X + 2, r.Y + 3, r.Width - 4, r.Height - 2), raadius);
                    g.FillPath(vari, vt);
                    r = new Rectangle(r.X, r.Y, r.Width, r.Height - 2);
                }
            }
            using (var path = Ymardus.Tee(r, raadius))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            // Tekst keskel või vasakul (TextAlign = MiddleLeft, nt "Maksma")
            bool vasakul = TextAlign is ContentAlignment.MiddleLeft or ContentAlignment.TopLeft or ContentAlignment.BottomLeft;
            var tekstiAla = vasakul ? new Rectangle(16, 0, Width - 32, Height) : ClientRectangle;
            TextRenderer.DrawText(g, Text, Font, tekstiAla, tekst,
                (vasakul ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter) | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if (Focused && ShowFocusCues)
            {
                var f = new Rectangle(3, 3, Width - 7, Height - 7);
                using var fp = new Pen(peamine ? Color.FromArgb(160, Color.White) : Teema.Aktsent) { DashStyle = DashStyle.Dot };
                using var fpath = Ymardus.Tee(f, Math.Max(2, Radius - 3));
                g.DrawPath(fp, fpath);
            }
        }
    }
}
