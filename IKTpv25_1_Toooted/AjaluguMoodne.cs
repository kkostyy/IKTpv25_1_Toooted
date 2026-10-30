using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Ostude ajalugu moodsas disainis (lõuend 3.1): vasakul valge kaart ostudega,
    // paremal valitud ostu tšekk "paberil" (sakiline alumine serv).
    public partial class OstudVorm
    {
        private TsekiEelvaade? tsekk;

        private void RakendaMoodne(Control nupud, Label readSilt)
        {
            ClientSize = new Size(1180, 720);
            BackColor = Teema.Taust;
            readSilt.Visible = false;
            read.Visible = false;

            var kaart = new RoundedPanel { Bounds = new Rectangle(16, 56, 770, 648), Radius = 16, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            foreach (var c in new[] { (Control)naitaTagastatud, ostud, nupud })
            {
                Controls.Remove(c);
                kaart.Controls.Add(c);
            }
            naitaTagastatud.Location = new Point(18, 16);
            naitaTagastatud.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ostud.Bounds = new Rectangle(14, 58, 742, 520);
            ostud.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            nupud.Bounds = new Rectangle(14, 588, 742, 50);
            nupud.BackColor = Teema.Pind;
            Controls.Add(kaart);

            tsekk = new TsekiEelvaade { Bounds = new Rectangle(802, 56, 362, 648), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right };
            Controls.Add(tsekk);
            ostud.SelectionChanged += (s, e) => NaitaTsekk();
        }

        private void NaitaTsekk()
        {
            if (tsekk == null) return;
            if (ValitudNr() is not int nr) { tsekk.Naita(null); return; }
            try
            {
                using var db = new ToodedContext();
                tsekk.Naita(db.Ostud.AsNoTracking().Include(o => o.Read).Include(o => o.Klient).FirstOrDefault(o => o.Id == nr));
            }
            catch { tsekk.Naita(null); }
        }
    }

    // Tšekk paberil: pood, aeg, read, soodustused, pant, KOKKU, makseviis
    public class TsekiEelvaade : Control
    {
        private Ost? ost;
        private double t = 1;

        public TsekiEelvaade()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public void Naita(Ost? uus)
        {
            if (uus?.Id == ost?.Id && uus != null) return;
            ost = uus;
            Animatsioon.Kaivita(this, 400, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var fp = new Font("Segoe UI", 14F, FontStyle.Bold);
            if (ost == null)
            {
                TextRenderer.DrawText(g, Keel.T("Vali vasakult ost, et näha tšekki."), new Font("Segoe UI", 10F), new Rectangle(0, 40, Width, 40), Teema.TekstNork, TextFormatFlags.HorizontalCenter);
                return;
            }
            TextRenderer.DrawText(g, Keel.F("Tšekk nr {0}", ost.Id), fp, new Point(4, 0), Teema.Tekst);

            // paber libiseb alt sisse
            float nihe = (float)((1 - t) * 24);
            var paber = new RectangleF(8, 40 + nihe, Width - 16, Height - 56);
            using (var tee = new GraphicsPath())
            {
                tee.AddLine(paber.X, paber.Bottom, paber.X, paber.Y + 12);
                tee.AddArc(paber.X, paber.Y, 24, 24, 180, 90);
                tee.AddArc(paber.Right - 24, paber.Y, 24, 24, 270, 90);
                tee.AddLine(paber.Right, paber.Y + 12, paber.Right, paber.Bottom);
                for (float x = paber.Right; x > paber.X; x -= 12)   // sakiline serv
                {
                    tee.AddLine(x, paber.Bottom, x - 6, paber.Bottom - 8);
                    tee.AddLine(x - 6, paber.Bottom - 8, Math.Max(paber.X, x - 12), paber.Bottom);
                }
                tee.CloseFigure();
                using (var vari = new SolidBrush(Color.FromArgb(20, 0, 0, 0))) { g.TranslateTransform(0, 6); g.FillPath(vari, tee); g.ResetTransform(); }
                using var b = new SolidBrush(Color.White);
                g.FillPath(b, tee);
            }

            var must = Color.FromArgb(30, 30, 30);
            var hall = Color.FromArgb(110, 116, 125);
            using var f = new Font("Consolas", 9F);
            using var fb = new Font("Consolas", 9F, FontStyle.Bold);
            using var fs = new Font("Consolas", 14F, FontStyle.Bold);
            float y = paber.Y + 22, x0 = paber.X + 20, x1 = paber.Right - 20;
            void Kesk(string s, Font font, Color c) { TextRenderer.DrawText(g, s, font, new Rectangle((int)paber.X, (int)y, (int)paber.Width, 18), c, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix); y += 18; }
            void Rida(string a, string b, Font font, Color c)
            {
                TextRenderer.DrawText(g, a, font, new Rectangle((int)x0, (int)y, (int)(x1 - x0 - 70), 18), c, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, b, font, new Rectangle((int)x0, (int)y, (int)(x1 - x0), 18), c, TextFormatFlags.Right | TextFormatFlags.NoPrefix);
                y += 18;
            }
            void Joon() { using var p = new Pen(Color.FromArgb(200, 200, 200)) { DashStyle = DashStyle.Dash }; g.DrawLine(p, x0, y + 6, x1, y + 6); y += 16; }

            Kesk(Arve.PoeNimi, fs, must);
            y += 4;
            Kesk(ost.Aeg.ToString("dd.MM.yyyy  HH:mm"), f, hall);
            if (ost.Klient != null) Kesk(Keel.F("KLIENT: {0}", ost.Klient.Kaardinumber), f, hall);
            if (ost.TarneAeg is DateTime ta) Kesk(Keel.T("Tarneaeg:") + " " + Tarne.AknaTekst(ta, ost.Aeg), fb, Color.FromArgb(160, 35, 110));
            Joon();
            foreach (var r in ost.Read)
            {
                if (y > paber.Bottom - 150) { Kesk("…", f, hall); break; }
                decimal tava = r.TavaHind is decimal th && th > r.Hind ? th : r.Hind;
                Rida(r.Toodenimetus, (tava * r.Kogus).ToString("0.00"), f, must);
                if (r.Kogus > 1) Rida($"  {r.Kogus} x {tava:0.00}", "", f, hall);
                if (tava > r.Hind) Rida("  " + (r.Soodus ?? Keel.T("Soodustus")), "-" + ((tava - r.Hind) * r.Kogus).ToString("0.00"), f, Color.FromArgb(20, 130, 70));
                if (r.Pant > 0) Rida("  " + Keel.T("Pakendi tagatisraha"), r.PantKokku.ToString("0.00"), f, hall);
            }
            if (ost.TarneHind > 0) Rida(Keel.T("Kohaletoimetamine"), ost.TarneHind.ToString("0.00"), f, must);
            if (ost.Soodustus > 0) Rida(Keel.T("Boonus kliendikaardilt"), "-" + ost.Soodustus.ToString("0.00"), f, Color.FromArgb(20, 130, 70));
            Joon();
            TextRenderer.DrawText(g, Keel.T("KOKKU"), fs, new Point((int)x0, (int)y), must);
            TextRenderer.DrawText(g, ost.Summa.ToString("0.00") + " €", fs, new Rectangle((int)x0, (int)y, (int)(x1 - x0), 26), must, TextFormatFlags.Right);
            y += 32;
            Kesk(Keel.T(ost.Makseviis == "Sularaha" ? "SULARAHA" : ost.Makseviis == MakseSeaded.Pangaulekanne ? "PANGAÜLEKANNE" : "KAARDIMAKSE"), fb, hall);
            if (ost.Tagastatud) Kesk(Keel.T("TAGASTATUD"), fb, Color.FromArgb(200, 30, 30));
            y += 6;
            Kesk(Keel.T("Aitäh ostu eest!"), f, hall);
        }
    }
}
