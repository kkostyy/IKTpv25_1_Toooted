using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted
{
    // Wordi-laadne vahekaart (ribbon tab): valitud vaheleht sulandub tööriistaribaga
    public class RibbonTab : Control
    {
        private bool _valitud;
        private bool _hover;

        public bool Valitud
        {
            get { return _valitud; }
            set { _valitud = value; Invalidate(); }
        }

        public RibbonTab()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size = new Size(90, 30);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Teema.Taust);

            if (_valitud || _hover)
            {
                Color fill = _valitud ? Teema.Paneel : Teema.Paneel2;
                // Ümardatud ülanurgad: alumine osa läheb juhtelemendi ääre taha
                var r = new Rectangle(0, 0, Width - 1, Height + 10);
                using (var path = Ymardus.Tee(r, 9))
                using (var brush = new SolidBrush(fill))
                {
                    g.FillPath(brush, path);
                }
            }

            Color tekst = _valitud ? Teema.Teine : Teema.Tekst2;
            using (var f = new Font("Segoe UI", 9.5F, _valitud ? FontStyle.Bold : FontStyle.Regular))
            {
                TextRenderer.DrawText(g, Text, f, new Rectangle(0, 0, Width, Height - 2), tekst,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            if (_valitud)
            {
                using (var brush = new SolidBrush(Teema.Teine))
                {
                    g.FillRectangle(brush, 14, Height - 3, Width - 28, 3);
                }
            }
        }
    }
}
