using System.Diagnostics;

namespace IKTpv25_1_Toooted;

// Lihtne animatsioon: taimer (10 ms) + Stopwatch, samm(t) saab väärtuse 0 → 1.
// Kui Windowsis on efektid välja lülitatud ("vähenda liikumist"), hüpatakse kohe lõppu.
public static class Animatsioon
{
    // Kõik üleminekud kestavad kaks korda kauem (pehmem, vähem "hüplik")
    public const double Kordaja = 2.0;

    // Windowsi taimeri täpsus 1 ms (vaikimisi ~15,6 ms) – kaadreid tuleb ~100 sekundis, mitte ~64
    [System.Runtime.InteropServices.DllImport("winmm.dll")] private static extern uint timeBeginPeriod(uint ms);
    private static bool tapsus;
    private static void Tapsus()
    {
        if (tapsus) return;
        tapsus = true;
        try { timeBeginPeriod(1); } catch { }
    }

    // Sujuv lõpp (ease-out cubic)
    public static double Sujuv(double t) => 1 - Math.Pow(1 - Math.Clamp(t, 0, 1), 3);

    // "Pop": läheb natuke üle ja tuleb tagasi (back-out)
    public static double Pop(double t)
    {
        t = Math.Clamp(t, 0, 1);
        const double c1 = 1.70158, c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }

    public static bool Lubatud => SystemInformation.UIEffectsEnabled;

    // Käivitab animatsiooni; tagastab taimeri (Stop() peatab). Omaniku sulgemisel peatub ise.
    public static System.Windows.Forms.Timer? Kaivita(Control omanik, int ms, Action<double> samm, Action? valmis = null, int viivitus = 0)
    {
        if (!Lubatud || ms <= 0)
        {
            samm(1);
            valmis?.Invoke();
            return null;
        }

        Tapsus();
        ms = (int)(ms * Kordaja);
        viivitus = (int)(viivitus * Kordaja);
        var taimer = new System.Windows.Forms.Timer { Interval = 10 };
        var kell = Stopwatch.StartNew();
        void Peata(object? s, EventArgs e) { taimer.Stop(); taimer.Dispose(); }
        omanik.Disposed += Peata;
        taimer.Tick += (s, e) =>
        {
            if (omanik.IsDisposed) { taimer.Stop(); return; }
            double t = (kell.ElapsedMilliseconds - viivitus) / (double)ms;
            if (t < 0) return;
            if (t >= 1)
            {
                taimer.Stop();
                omanik.Disposed -= Peata;
                taimer.Dispose();
                samm(1);
                valmis?.Invoke();
                return;
            }
            samm(t);
        };
        samm(0);
        taimer.Start();
        return taimer;
    }

    // Lõputu tsükkel (vilkuv punkt, kingituse kõikumine): faas(t) saab sekundid algusest
    public static System.Windows.Forms.Timer? Tsykkel(Control omanik, Action<double> faas, int intervall = 30)
    {
        if (!Lubatud) { faas(0); return null; }
        Tapsus();
        var taimer = new System.Windows.Forms.Timer { Interval = Math.Max(10, intervall / 2) };   // 2× sagedamini = sujuvam
        var kell = Stopwatch.StartNew();
        taimer.Tick += (s, e) =>
        {
            if (omanik.IsDisposed) { taimer.Stop(); taimer.Dispose(); return; }
            if (omanik.Visible) faas(kell.Elapsed.TotalSeconds);
        };
        omanik.Disposed += (s, e) => { taimer.Stop(); taimer.Dispose(); };
        taimer.Start();
        return taimer;
    }
}

// Isejoonistatud elemendi sujuv ilmumine: OnPaint lõpus kutsu Kata(g) – element tuleb esile
// taustavärvist (läbipaistvus). Mitu elementi järjest: viivitus = järjekord × 60 ms.
public sealed class Ilmumine
{
    private readonly Control c;
    public double T { get; private set; }

    public Ilmumine(Control c, int viivitus = 0)
    {
        this.c = c;
        if (!Animatsioon.Lubatud) { T = 1; return; }
        c.HandleCreated += (s, e) => Animatsioon.Kaivita(c, 260, t => { T = Animatsioon.Sujuv(t); c.Invalidate(); }, null, viivitus);
    }

    public void Kata(Graphics g)
    {
        if (T >= 1) return;
        using var b = new SolidBrush(Color.FromArgb((int)(255 * (1 - T)), c.Parent?.BackColor ?? Teema.Taust));
        g.FillRectangle(b, c.ClientRectangle);
    }
}

// Sujuv sees/väljas olek (hiir kohal, valitud): V liigub 0 ↔ 1 umbes 120 ms jooksul, element joonistub ise uuesti
public sealed class SujuvOlek
{
    private readonly Control c;
    private readonly int ms;
    private bool siht;
    private System.Windows.Forms.Timer? taimer;
    public double V { get; private set; }

    public SujuvOlek(Control c, int ms = 120) { this.c = c; this.ms = ms; }

    public bool Siht
    {
        get => siht;
        set
        {
            if (value == siht) return;
            siht = value;
            double algus = V, lopp = value ? 1 : 0;
            taimer?.Stop();
            taimer = Animatsioon.Kaivita(c, ms, t => { V = algus + (lopp - algus) * Animatsioon.Sujuv(t); c.Invalidate(); });
        }
    }
}
