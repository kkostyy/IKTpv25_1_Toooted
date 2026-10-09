using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IKTpv25_1_Toooted
{
    // Abifunktsioonid ümardatud nurkade jaoks
    public static class Ymardus
    {
        public static GraphicsPath Tee(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
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
        public Color BorderColor { get; set; } = UiStiil.Joon;

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
            using (var path = Ymardus.Tee(r, Radius))
            using (var brush = new SolidBrush(FillColor))
            using (var pen = new Pen(BorderColor))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
            base.OnPaint(e);
        }
    }

    // Ümardatud nurkadega nupp
    public class RoundedButton : Button
    {
        public int Radius { get; set; } = 8;
        private bool _hover;
        private bool _down;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
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
            Color taust = Parent != null ? Parent.BackColor : SystemColors.Control;
            g.Clear(taust);

            Color fill = BackColor;
            Color border = FlatAppearance.BorderColor;
            Color tekst = ForeColor;

            bool tume = BackColor.GetBrightness() < 0.6f;
            if (!Enabled)
            {
                fill = Color.FromArgb(238, 239, 241);
                border = Color.FromArgb(220, 223, 228);
                tekst = Color.FromArgb(160, 164, 170);
            }
            else if (_down)
            {
                fill = tume ? Ymardus.Segu(BackColor, Color.Black, 0.2f) : Ymardus.Segu(BackColor, UiStiil.Sinine, 0.16f);
            }
            else if (_hover)
            {
                fill = tume ? Ymardus.Segu(BackColor, Color.Black, 0.1f) : Ymardus.Segu(BackColor, UiStiil.Sinine, 0.08f);
                if (!tume) border = UiStiil.Sinine;
            }

            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = Ymardus.Tee(r, Radius))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, tekst,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
