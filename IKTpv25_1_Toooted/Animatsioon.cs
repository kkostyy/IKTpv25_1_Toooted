using System.Diagnostics;

namespace IKTpv25_1_Toooted;

// Lihtne animatsioon: taimer (15 ms) + Stopwatch, samm(t) saab väärtuse 0 → 1.
// Kui Windowsis on efektid välja lülitatud ("vähenda liikumist"), hüpatakse kohe lõppu.
public static class Animatsioon
{
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

        var taimer = new System.Windows.Forms.Timer { Interval = 15 };
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
        var taimer = new System.Windows.Forms.Timer { Interval = intervall };
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
