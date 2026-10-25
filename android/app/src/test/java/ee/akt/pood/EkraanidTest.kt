package ee.akt.pood

import app.cash.paparazzi.DeviceConfig
import app.cash.paparazzi.Paparazzi
import org.junit.Rule
import org.junit.Test
import java.time.LocalDate
import java.time.LocalDateTime

// Ekraanipildid kõigist telefonivaadetest (./gradlew recordPaparazziDebug) – võrdluseks lõuendi maketiga
class EkraanidTest {
    @get:Rule
    val paparazzi = Paparazzi(deviceConfig = DeviceConfig.PIXEL_5, theme = "android:Theme.Material.Light.NoActionBar")

    private val kell = LocalDateTime.of(2026, 10, 24, 18, 25)

    private fun olek(sisse: Boolean = true): AppOlek {
        val o = AppOlek(MaluPood(kell), { kell })
        if (sisse) o.logiSisse("mari", "mari123")
        return o
    }

    private fun pilt(olek: AppOlek) = paparazzi.snapshot { App(olek) }

    @Test fun sisse() = pilt(olek(sisse = false))
    @Test fun avaleht() = pilt(olek())
    @Test fun otsing() = pilt(olek().apply { ava(Ekraan.Otsing("kohvioad")) })
    @Test fun otsingTyhi() = pilt(olek().apply { ava(Ekraan.Otsing()) })
    @Test fun toode() = pilt(olek().apply { ava(Ekraan.ToodeLeht(1)) })
    @Test fun kategooria() = pilt(olek().apply { ava(Ekraan.Ruudustik("Piimatooted, munad, juust", 1)) })
    @Test fun korvTyhi() = pilt(olek().apply { vaheleht(Ekraan.Korv) })
    @Test fun korv() = pilt(olek().apply { lisaKorvi(toode(1)!!, 2); lisaKorvi(toode(4)!!); lisaKorvi(toode(6)!!); vaheleht(Ekraan.Korv) })
    @Test fun maksmine() = pilt(olek().apply { lisaKorvi(toode(1)!!, 2); lisaKorvi(toode(4)!!); vaheleht(Ekraan.Korv); ava(Ekraan.Maksmine) })
    @Test fun makseOnnestus() = pilt(olek().apply { lisaKorvi(toode(1)!!, 2); val o = vormista(true, "Tänav 1-2", "Kaart", false); vaheleht(Ekraan.MakseOnnestus(o.id)) })
    @Test fun kaart() = pilt(olek().apply { vaheleht(Ekraan.Kaart) })
    @Test fun preemiadTase() = pilt(olek().apply { ava(Ekraan.Preemiad(0)) })
    @Test fun preemiadValjakutsed() = pilt(olek().apply { ava(Ekraan.Preemiad(1)) })
    @Test fun preemiadKleepsud() = pilt(olek().apply { ava(Ekraan.Preemiad(2)) })
    @Test fun preemiadYllatus() = pilt(olek().apply { ava(Ekraan.Preemiad(3)) })
    @Test fun nimekiri() = pilt(olek().apply { vaheleht(Ekraan.Nimekiri) })
    @Test fun profiil() = pilt(olek().apply { vaheleht(Ekraan.Profiil) })
    @Test fun ajalugu() = pilt(olek().apply { lisaKorvi(toode(1)!!); vormista(false, null, "Kaart", false); ava(Ekraan.Ajalugu) })
    @Test fun dieet() = pilt(olek().apply { vahetaDieet(Dieet.Mark.Laktoosivaba); ava(Ekraan.Dieet) })
    @Test fun ohtu() = pilt(olek().apply { ava(Ekraan.Ohtu) })
    @Test fun toitevaartus() = pilt(olek().apply { lisaKorvi(toode(1)!!); lisaKorvi(toode(4)!!); ava(Ekraan.Toitevaartus) })
    @Test fun pere() = pilt(olek().apply { ava(Ekraan.Pere) })
    @Test fun kulutused() = pilt(olek().apply {
        lisaKorvi(toode(1)!!, 2); lisaKorvi(toode(4)!!); lisaKorvi(toode(6)!!); vormista(false, null, "Kaart", false); ava(Ekraan.Kulutused)
    })
    @Test fun pereKorv() = pilt(olek().apply {
        pood.seaSeade("pere", "AKT7Q2K")
        PereKorv.lisa(pood, PereKorv.loe(pood), 1, 2, "Jaan", aeg()); PereKorv.lisa(pood, PereKorv.loe(pood), 4, 1, "Mari", aeg())
        ava(Ekraan.PereKorv)
    })
    @Test fun raiskamine() = pilt(olek().apply { ava(Ekraan.Ruudustik("Ilma raiskamiseta", vaade = "raiskamine")) })
    @Test fun admin() = pilt(AppOlek(MaluPood(kell), { kell }).apply { logiSisse("admin", "admin123") })
    @Test fun toodeMuuda() = pilt(AppOlek(MaluPood(kell), { kell }).apply { logiSisse("admin", "admin123"); ava(Ekraan.ToodeMuuda(1)) })
    @Test fun kampaaniad() = pilt(AppOlek(MaluPood(kell), { kell }).apply { logiSisse("admin", "admin123"); ava(Ekraan.Kampaaniad) })
    @Test fun venekeelne() = pilt(olek().apply { seaKeel(Keel.Kood.RU) }).also { Keel.praegu = Keel.Kood.ET }
}

// Mälus olev pood testideks (SQLite-t Paparazzis pole)
class MaluPood(private val kell: LocalDateTime) : Pood {
    private val kat = listOf(
        Kategooria(1, "Piimatooted, munad, juust"), Kategooria(2, "Leivad, saiad, kondiitritooted"), Kategooria(3, "Puuviljad, köögiviljad"),
        Kategooria(4, "Liha- ja kalatooted"), Kategooria(5, "Joogid"), Kategooria(6, "Maiustused ja snäkid"), Kategooria(7, "Külmutatud toidukaubad")
    )
    private val tooted = mutableListOf(
        Toode(1, "Juust Atleet Eesti viilutatud Valio 500g", 40, 299, 619, null, 1, "Valio", "Valio Eesti AS", "0.5 kg", "kg", 0.5,
            "Eesti juustuklassika aastast 1984. Viilutatud juust taassuletavas pakendis – laktoosivaba, kaltsiumi- ja valgurikas.",
            "Pastöriseeritud PIIM, sool, piimhappebakterid, laap, värvaine annatto.", null, "Energia;1450 kJ / 346 kcal\nRasvad;26 g\n-millest küllastunud;17 g\nValgud;27 g\nSool;1,2 g", "laktoosivaba", 0, "Eesti"),
        Toode(2, "Õrrekanade munad M Kulli Farm 10tk", 60, 289, kategooriaId = 1, pakend = "10 tk", uhik = "tk", uhikuKogus = 10.0, koostis = "MUNAD"),
        Toode(3, "Kodujuust murakamoosiga Alma 200g", 35, 139, 179, kategooriaId = 1, kaubamark = "Alma", pakend = "0.2 kg", uhik = "kg", uhikuKogus = 0.2, koostis = "kohupiim, moos"),
        Toode(4, "Täispiim 3,8-4,2% Farmi 2L", 50, 219, kategooriaId = 1, kaubamark = "Farmi", pakend = "2 l", uhik = "l", uhikuKogus = 2.0, koostis = "PIIM", toitevaartus = "Energia;270 kJ / 64 kcal"),
        Toode(5, "Rukkileib Rehe Eesti Pagar 450g", 45, 169, kategooriaId = 2, kaubamark = "Eesti Pagar", pakend = "0.45 kg", uhik = "kg", uhikuKogus = 0.45, koostis = "RUKKIJAHU, vesi, sool"),
        Toode(6, "Coca-Cola 0,5l", 30, 149, kategooriaId = 5, kaubamark = "Coca-Cola", pakend = "0.5 l", uhik = "l", uhikuKogus = 0.5, pant = 10, koostis = "vesi, suhkur"),
        Toode(7, "Banaan Cavendish, kg", 80, 129, kategooriaId = 3, pakend = "1 kg", uhik = "kg", uhikuKogus = 1.0),
        Toode(8, "Šokolaad Kalev Mesikäpp 100g", 0, 179, 229, kategooriaId = 6, kaubamark = "Kalev", pakend = "0.1 kg", uhik = "kg", uhikuKogus = 0.1, koostis = "suhkur, kakaovõi, täisPIIMApulber"),
        Toode(9, "Sai Perenaise suur Eesti Pagar 500g", 30, 125, kategooriaId = 2, pakend = "0.5 kg", uhik = "kg", uhikuKogus = 0.5, koostis = "NISUJAHU, vesi"),
        Toode(10, "Kohv jahvatatud Löfbergs 450g", 31, 699, 899, kategooriaId = 5, pakend = "0.45 kg", uhik = "kg", uhikuKogus = 0.45),
        Toode(11, "Juust Gouda viilutatud Rimi 500g", 25, 249, kategooriaId = 1, kaubamark = "Rimi", pakend = "0.5 kg", uhik = "kg", uhikuKogus = 0.5),
    )
    private val kasutajad = mutableMapOf(
        "admin" to (Kasutaja("admin", "Admin Omanik", Roll.Omanik, "100000", 0) to "admin123"),
        "mari" to (Kasutaja("mari", "Mari Maasikas", Roll.Klient, "100001", 42, "mari@email.ee") to "mari123"),
    )
    private val ostud = mutableListOf<Ost>()
    private val nimekiri = mutableListOf(NimekirjaRida(1, "Piim", false, "Mari"), NimekirjaRida(2, "Leib", true, "Jaan"), NimekirjaRida(3, "Banaanid", false, "Mari"))
    private val lemmikud = mutableSetOf(1)
    private val kampaaniad = mutableListOf(Kampaania(1, 3, 22, LocalDate.of(2026, 10, 20), LocalDate.of(2026, 10, 27)))
    private val seaded = mutableMapOf<String, String>()

    override fun kategooriad() = kat
    override fun tooted() = tooted.sortedBy { it.nimi }
    override fun salvestaToode(t: Toode): Toode { val u = if (t.id == 0) t.copy(id = tooted.maxOf { it.id } + 1) else t; tooted.removeAll { it.id == u.id }; tooted.add(u); return u }
    override fun kustutaToode(id: Int) { tooted.removeAll { it.id == id } }
    override fun logiSisse(nimi: String, parool: String) = kasutajad[nimi]?.takeIf { it.second == parool }?.first
    override fun registreeri(nimi: String, taisNimi: String, parool: String, email: String?): String? { kasutajad[nimi] = Kasutaja(nimi, taisNimi, Roll.Klient, "100002", 0, email) to parool; return null }
    override fun kasutaja(nimi: String) = kasutajad[nimi]?.first
    override fun muudaBoonust(nimi: String, muutus: Int) {}
    override fun ostud(kasutaja: String?) = ostud.filter { kasutaja == null || it.kasutaja == kasutaja }.sortedByDescending { it.aeg }
    override fun lisaOst(ost: Ost): Ost { val o = ost.copy(id = ostud.size + 1); ostud.add(o); return o }
    override fun tagasta(ostId: Int) {}
    override fun nimekiri() = nimekiri.sortedBy { it.ostetud }
    override fun lisaNimekirja(tekst: String, lisaja: String?) { nimekiri.add(NimekirjaRida(nimekiri.size + 1, tekst, false, lisaja)) }
    override fun margiNimekirjas(id: Int, ostetud: Boolean) {}
    override fun kustutaNimekirjast(id: Int) {}
    override fun lemmikud() = lemmikud.toSet()
    override fun vahetaLemmik(id: Int) { if (!lemmikud.remove(id)) lemmikud.add(id) }
    override fun kampaaniad() = kampaaniad.toList()
    override fun lisaKampaania(k: Kampaania) { kampaaniad.add(k) }
    override fun seade(voti: String) = seaded[voti]
    override fun seaSeade(voti: String, vaartus: String?) { if (vaartus == null) seaded.remove(voti) else seaded[voti] = vaartus }
}
