package ee.akt.pood

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import java.time.LocalDateTime

// Ekraanid (lõuendi telefonimaketid)
sealed class Ekraan {
    data object Sisse : Ekraan()
    data object Registreeru : Ekraan()
    data object Avaleht : Ekraan()
    data class Otsing(val algus: String = "") : Ekraan()
    data class Ruudustik(val pealkiri: String, val kategooriaId: Int? = null, val vaade: String = "kategooria") : Ekraan()
    data class ToodeLeht(val id: Int) : Ekraan()
    data object Korv : Ekraan()
    data object Maksmine : Ekraan()
    data class MakseOnnestus(val ostId: Int) : Ekraan()
    data object Kaart : Ekraan()
    data class Preemiad(val vaheleht: Int = 0) : Ekraan()
    data object Nimekiri : Ekraan()
    data object Profiil : Ekraan()
    data object Ajalugu : Ekraan()
    data object Admin : Ekraan()
    data class ToodeMuuda(val id: Int?) : Ekraan()
    data object Dieet : Ekraan()
    data object Kampaaniad : Ekraan()
    data object Ohtu : Ekraan()
    data object Toitevaartus : Ekraan()
    data object Pere : Ekraan()
    data object Kulutused : Ekraan()
    data object PereKorv : Ekraan()
    data object Seaded : Ekraan()
    data object Myygipaneel : Ekraan()
    data object Autotellimus : Ekraan()
    data object Koos : Ekraan()
    data object Kliendid : Ekraan()
    data object Kassa : Ekraan()
    data object Partiid : Ekraan()
}

// Rakenduse olek: kasutaja, ostukorv, navigeerimine. Compose jälgib kõiki välju.
class AppOlek(val pood: Pood, private val kell: () -> LocalDateTime = { LocalDateTime.now() }) {
    var kasutaja by mutableStateOf<Kasutaja?>(null)
        private set
    var tooted by mutableStateOf(pood.tooted())
        private set
    val kategooriad: List<Kategooria> = pood.kategooriad()
    var lemmikud by mutableStateOf(pood.lemmikud())
        private set
    var dieet by mutableStateOf(
        (pood.seade("dieet") ?: "").split(';').mapNotNull { n -> Dieet.Mark.entries.firstOrNull { it.name == n } }.toSet()
    )
        private set
    var teade by mutableStateOf<String?>(null)

    val korv = mutableStateListOf<KorvRida>()
    val pinu = mutableStateListOf<Ekraan>(Ekraan.Sisse)
    val ekraan: Ekraan get() = pinu.last()

    init {
        Keel.praegu = runCatching { Keel.Kood.valueOf(pood.seade("keel") ?: "ET") }.getOrDefault(Keel.Kood.ET)
    }

    // ---- Õhtune soodustus (omaniku seaded) ----
    fun laadiOhtu() {
        Reeglid.OHTU_SEES = pood.seade("ohtu_sees") != "0"
        Reeglid.OHTU_ALGUS = runCatching { java.time.LocalTime.parse(pood.seade("ohtu_algus")) }.getOrDefault(java.time.LocalTime.of(20, 0))
        Reeglid.OHTU_PROTSENT = pood.seade("ohtu_protsent")?.toIntOrNull() ?: 50
    }

    fun salvestaOhtu(sees: Boolean, algus: java.time.LocalTime, protsent: Int) {
        pood.seaSeade("ohtu_sees", if (sees) null else "0")
        pood.seaSeade("ohtu_algus", algus.toString())
        pood.seaSeade("ohtu_protsent", protsent.toString())
        laadiOhtu()
        hinnadMuutusid++
    }

    var hinnadMuutusid by mutableStateOf(0)   // muutub → hinnad arvutatakse uuesti

    // ---- Partiid: varaseim säilivusaeg toote kohta (ilma raiskamiseta −30 %) ----
    var partiid by mutableStateOf(listOf<Partii>())
        private set

    fun varskendaPartiid() {
        partiid = pood.partiid()
        Raiskamine.partiid = partiid.groupBy { it.toodeId }.mapValues { (_, p) -> p.minOf { it.aegub } }
        hinnadMuutusid++
    }

    // ---- Salvestatud ostukorvid (seades "korvid:<kasutaja>": nimi|id:kogus,id:kogus) ----
    data class SalvestatudKorv(val nimi: String, val read: List<Pair<Int, Int>>)

    private val korvideVoti get() = "korvid:" + (kasutaja?.nimi ?: "")

    fun salvestatudKorvid(): List<SalvestatudKorv> = (pood.seade(korvideVoti) ?: "").lines().filter { '|' in it }.map { r ->
        val (n, sisu) = r.split('|', limit = 2)
        SalvestatudKorv(n, sisu.split(',').mapNotNull { p -> p.split(':').takeIf { it.size == 2 }?.let { (a, b) -> a.toIntOrNull()?.let { x -> b.toIntOrNull()?.let { y -> x to y } } } })
    }

    fun salvestaKorv(nimi: String) {
        val puhas = nimi.replace("|", " ").replace("\n", " ").trim().ifEmpty { Keel.f("Korv {0}", aeg().toLocalDate()) }
        val uus = salvestatudKorvid().filter { it.nimi != puhas } + SalvestatudKorv(puhas, korv.map { it.toode.id to it.kogus })
        pood.seaSeade(korvideVoti, uus.joinToString("\n") { k -> k.nimi + "|" + k.read.joinToString(",") { "${it.first}:${it.second}" } })
    }

    fun kustutaKorv(nimi: String) {
        val uus = salvestatudKorvid().filter { it.nimi != nimi }
        pood.seaSeade(korvideVoti, uus.joinToString("\n") { k -> k.nimi + "|" + k.read.joinToString(",") { "${it.first}:${it.second}" } }.ifEmpty { null })
    }

    // Tagastab: mitu tk lisati ja mis puudu
    fun taastaKorv(k: SalvestatudKorv): Pair<Int, List<String>> {
        var lisatud = 0
        val puudu = mutableListOf<String>()
        for ((id, kogus) in k.read) {
            val t = toode(id) ?: continue
            val vaba = minOf(kogus, t.kogus - korvis(id))
            if (vaba <= 0 || lisaKorvi(t, vaba) != null) puudu += t.nimi else lisatud += vaba
        }
        return lisatud to puudu
    }

    // ---- Lemmik on taas laos: kontrollitakse sisselogimisel ----
    private fun kontrolliLemmikuid() {
        val voti = "lemmikud_otsas:" + (kasutaja?.nimi ?: return)
        val enne = (pood.seade(voti) ?: "").split(',').mapNotNull { it.toIntOrNull() }.toSet()
        val lemmikTooted = tooted.filter { it.id in lemmikud }
        val tagasi = lemmikTooted.filter { it.id in enne && it.laos }
        pood.seaSeade(voti, lemmikTooted.filter { !it.laos }.joinToString(",") { it.id.toString() }.ifEmpty { null })
        if (tagasi.isNotEmpty())
            teade = Keel.t("Lemmik on taas laos:") + " " + tagasi.take(3).joinToString(", ") { it.nimi } + (if (tagasi.size > 3) " " + Keel.f("ja veel {0}", tagasi.size - 3) else "")
    }

    fun aeg(): LocalDateTime = kell()

    // ---- Navigeerimine ----
    fun ava(e: Ekraan) { if (pinu.last() != e) pinu.add(e) }

    fun asenda(e: Ekraan) { pinu[pinu.lastIndex] = e }

    // Alumise menüü vahelehed alustavad uue pinu
    fun vaheleht(e: Ekraan) { pinu.clear(); pinu.add(e) }

    fun tagasi(): Boolean {
        if (pinu.size <= 1) return false
        pinu.removeAt(pinu.lastIndex)
        return true
    }

    // ---- Kasutaja ----
    fun logiSisse(nimi: String, parool: String): Boolean {
        val k = pood.logiSisse(nimi, parool) ?: return false
        kasutaja = k
        vaheleht(if (k.roll == Roll.Omanik) Ekraan.Admin else Ekraan.Avaleht)
        kontrolliLemmikuid()
        return true
    }

    fun logiValja() {
        kasutaja = null
        korv.clear()
        vaheleht(Ekraan.Sisse)
    }

    fun varskendaKasutaja() { kasutaja = kasutaja?.let { pood.kasutaja(it.nimi) } }

    fun seaKeel(k: Keel.Kood) {
        Keel.praegu = k
        pood.seaSeade("keel", k.name)
    }

    // ---- Tooted ----
    fun varskendaTooted() { tooted = pood.tooted() }

    // id → toode (HashMap): varem otsiti 9000 toote seast järjest igal korral (ostukorv, partiid, pere korv …)
    private var indeks: Map<Int, Toode> = emptyMap()
    private var indeksiAllikas: List<Toode>? = null

    fun toode(id: Int): Toode? {
        val praegu = tooted
        if (indeksiAllikas !== praegu) { indeks = praegu.associateBy { it.id }; indeksiAllikas = praegu }
        return indeks[id]
    }

    fun hind(t: Toode): Reeglid.Hind { hinnadMuutusid; return Reeglid.lopphind(t, aeg()) }

    fun vahetaLemmik(id: Int) {
        pood.vahetaLemmik(id)
        lemmikud = pood.lemmikud()
        val t = toode(id)
        if (t != null && id in lemmikud && !t.laos) {
            val voti = "lemmikud_otsas:" + (kasutaja?.nimi ?: "")
            pood.seaSeade(voti, ((pood.seade(voti) ?: "").split(',').filter { it.isNotBlank() } + id.toString()).distinct().joinToString(","))
            teade = Keel.t("Lisatud lemmikutesse – anname teada, kui toode on taas laos")
        }
    }

    fun vahetaDieet(m: Dieet.Mark) {
        dieet = if (m in dieet) dieet - m else dieet + m
        pood.seaSeade("dieet", dieet.joinToString(";") { it.name })
    }

    // ---- Ostukorv ----
    val korvisTk: Int get() = korv.sumOf { it.kogus }
    val kaubad: Int get() = korv.sumOf { it.summa }
    val pant: Int get() = korv.sumOf { it.pantKokku }

    fun korvis(id: Int): Int = korv.firstOrNull { it.toode.id == id }?.kogus ?: 0

    // Tagastab veateate või null
    fun lisaKorvi(t: Toode, kogus: Int = 1): String? {
        val praegu = korvis(t.id)
        if (!t.laos) return Keel.f("„{0}“ on otsas.", t.nimi)
        if (praegu + kogus > t.kogus) return Keel.f("Rohkem pole laos (laos: {0}).", t.kogus)
        val h = hind(t)
        val i = korv.indexOfFirst { it.toode.id == t.id }
        if (i >= 0) korv[i] = korv[i].copy(kogus = praegu + kogus, hind = h.hind, tavaHind = h.tavaHind, soodus = h.soodus)
        else korv.add(KorvRida(t, kogus, h.hind, h.tavaHind, h.soodus))
        return null
    }

    fun vahenda(id: Int) {
        val i = korv.indexOfFirst { it.toode.id == id }
        if (i < 0) return
        if (korv[i].kogus > 1) korv[i] = korv[i].copy(kogus = korv[i].kogus - 1) else korv.removeAt(i)
    }

    fun tyhjendaKorv() = korv.clear()

    // Ost: salvestab, vähendab laoseisu, arvestab boonuse; tagastab ostu
    fun vormista(kojuvedu: Boolean, aadress: String?, makseviis: String, kasutaBoonust: Boolean): Ost {
        val k = kasutaja ?: error("Pole sisse logitud")
        val tarne = Reeglid.tarne(kaubad, kojuvedu)
        val boonusKasutatud = if (kasutaBoonust) minOf(k.boonus, kaubad) else 0
        val tase = Reeglid.taseSummast(aastaSumma()).tase
        val lisatud = Reeglid.boonus(kaubad - boonusKasutatud, tase)
        val ost = pood.lisaOst(
            Ost(
                0, aeg(), k.nimi, kaubad, boonusKasutatud, tarne, pant, kaubad - boonusKasutatud + tarne + pant, lisatud,
                makseviis, if (kojuvedu) "Kojuvedu" else "Kaasa", if (kojuvedu) aadress else null,
                korv.map { OstuRida(it.toode.id, it.toode.nimi, it.kogus, it.hind, it.tavaHind, it.soodus) }
            )
        )
        korv.clear()
        varskendaTooted()
        varskendaKasutaja()
        return ost
    }

    fun aastaSumma(): Int {
        val k = kasutaja ?: return 0
        val piir = aeg().minusYears(1)
        return pood.ostud(k.nimi).filter { !it.tagastatud && it.aeg.isAfter(piir) }.sumOf { it.summa - it.tarne - it.pant }
    }

    // Kõik varasema ostu tooted uuesti korvi (nii palju kui laos on)
    fun ostaUuesti(ost: Ost): Pair<Int, List<String>> {
        var lisatud = 0
        val puudu = mutableListOf<String>()
        for (r in ost.read) {
            val t = r.toodeId?.let { toode(it) }
            if (t == null) { puudu += r.nimi; continue }
            val vaba = t.kogus - korvis(t.id)
            val kogus = minOf(r.kogus, vaba)
            if (kogus <= 0 || lisaKorvi(t, kogus) != null) { puudu += r.nimi; continue }
            lisatud += kogus
        }
        return lisatud to puudu
    }

    // viimasena: kõik väljad on selleks ajaks loodud
    init {
        laadiOhtu()
        varskendaPartiid()
    }
}
