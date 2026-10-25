package ee.akt.pood

import androidx.compose.animation.animateContentSize
import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.tween
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.platform.LocalInspectionMode
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import kotlinx.coroutines.delay
import java.time.LocalDateTime

// ---------------------------------------------------------------
//  1.3: sarnased ja soodsamad, minu kulutused, pere korv
//  (lõuendi maketid TelV13Sarnased, TelV13Kulutused, TelV13PereKorv)
// ---------------------------------------------------------------

private val kuuNimed = listOf("Jaan.", "Veebr", "Märts", "Apr", "Mai", "Juuni", "Juuli", "Aug", "Sept", "Okt", "Nov", "Dets")
private val kategooriaVarvid = listOf(
    Color(0xFF2E6FD8), Color(0xFFC2410C), Color(0xFF15803D), Color(0xFFB45309), Color(0xFF7C3AED), Color(0xFF64748B)
)

// Toote lehel: odavamad sarnased tooted kilohinna järgi (horisontaalne rida)
@Composable
fun SarnasedJaSoodsamad(olek: AppOlek, t: Toode) {
    val sarnased = remember(t.id, olek.tooted) { Sarnased.leia(t, olek.tooted) }
    if (sarnased.isEmpty()) return
    Vahe(18.dp)
    T(Keel.f("Sarnased ja soodsamad €/{0} järgi", t.uhik ?: "kg"), 16.sp, V.Tekst, FontWeight.ExtraBold)
    Vahe(8.dp)
    Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
        for (s in sarnased) {
            val h = olek.hind(s)
            Column(
                Modifier.width(156.dp).clip(RoundedCornerShape(20.dp)).background(V.Paneel)
                    .vajuta { olek.ava(Ekraan.ToodeLeht(s.id)) }.padding(10.dp)
            ) {
                VaikeKiip(Keel.f("−{0} % €/{1}", Sarnased.odavam(t, s), s.uhik ?: ""), V.Roheline, Color.White)
                Vahe(6.dp)
                ToodePilt(s.pilt, Modifier.fillMaxWidth().height(70.dp))
                Vahe(6.dp)
                T(s.nimi, 13.sp, V.Tekst, FontWeight.Bold, read = 2, reavahe = 16.sp, modifier = Modifier.height(34.dp))
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Column(Modifier.weight(1f)) {
                        T(eur(h.hind), 16.sp, V.Tekst, FontWeight.ExtraBold)
                        s.uhikuHind?.let { T("${eur(it)}/${s.uhik}", 11.sp, V.Roheline, FontWeight.Bold) }
                    }
                    RingNupp(Ik.Pluss, { olek.teade = olek.lisaKorvi(s) ?: Keel.f("„{0}“ lisatud ostukorvi", s.nimi) }, suurus = 34.dp)
                }
            }
        }
    }
}

// ---------------------------------------------------------------
//  Minu kulutused: sõõrikdiagramm (kaared kasvavad sujuvalt), kuu valik, legend
// ---------------------------------------------------------------
@Composable
fun KulutusedEkraan(olek: AppOlek) {
    val nyyd = olek.aeg()
    val kuud = remember { (5 downTo 0).map { nyyd.minusMonths(it.toLong()) } }
    var valitud by remember { mutableIntStateOf(5) }
    val ostud = remember { olek.kasutaja?.let { olek.pood.ostud(it.nimi) } ?: emptyList() }
    val katNimi = remember { olek.kategooriad.associate { it.id to it.nimi } }
    val tootelt = remember(olek.tooted) { olek.tooted.associate { it.id to it.kategooriaId } }
    val nimi: (Int?) -> String = { id -> tootelt[id]?.let { katNimi[it] }?.let { lyhike(it) } ?: "Muu" }
    val summad = kuud.map { k -> Kulutused.kuu(ostud, nimi, k.year, k.monthValue) }
    val osad = summad[valitud]
    val kokku = osad.sumOf { it.summa }
    val eelvaade = LocalInspectionMode.current   // ekraanipildi testis kohe täis
    val kasv = remember { Animatable(if (eelvaade) 1f else 0f) }
    LaunchedEffect(valitud) { if (!eelvaade) { kasv.snapTo(0f); kasv.animateTo(1f, tween(700)) } }

    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 24.dp)) {
        Pais("Minu kulutused", { olek.tagasi() }, Keel.t("Viimased 6 kuud"))
        Vahe(10.dp)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            kuud.forEachIndexed { i, k -> Kiip(Keel.t(kuuNimed[k.monthValue - 1]), V.Pind, V.Tekst, valitud = i == valitud, onClick = { valitud = i }) }
        }
        Vahe(12.dp)
        Kaart(Modifier.fillMaxWidth()) {
            Box(Modifier.fillMaxWidth().height(230.dp), Alignment.Center) {
                Canvas(Modifier.size(210.dp)) {
                    val paksus = 40.dp.toPx()
                    val r = Size(size.width - paksus, size.height - paksus)
                    val tl = Offset(paksus / 2, paksus / 2)
                    drawArc(Color(0xFFF1F3F4), 0f, 360f, false, tl, r, style = Stroke(paksus))
                    var algus = -90f
                    osad.forEachIndexed { i, o ->
                        val kaar = 360f * o.summa / kokku * kasv.value
                        drawArc(kategooriaVarvid[i % kategooriaVarvid.size], algus, (kaar - 1.5f).coerceAtLeast(0f), false, tl, r, style = Stroke(paksus))
                        algus += kaar
                    }
                }
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    T(Keel.t(kuuNimed[kuud[valitud].monthValue - 1]), 13.sp, V.Tekst2)
                    T(eur(kokku), 26.sp, V.Tekst, FontWeight.ExtraBold)
                }
            }
            if (valitud > 0) {
                val vahe = kokku - summad[valitud - 1].sumOf { it.summa }
                T(Keel.f("{0} võrreldes eelmise kuuga", (if (vahe >= 0) "+" else "") + eur(vahe)), 12.sp, V.Tekst2, modifier = Modifier.fillMaxWidth(), keskel = true)
            }
        }
        Vahe(10.dp)
        if (osad.isEmpty()) Tyhi(Ik.Graafik, Keel.t("Selles kuus oste pole"), Keel.t("Siia ilmub, kuhu su raha läheb."))
        osad.forEachIndexed { i, o ->
            Row(
                Modifier.fillMaxWidth().padding(vertical = 3.dp).clip(RoundedCornerShape(14.dp)).background(V.Pind).padding(horizontal = 12.dp, vertical = 10.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Box(Modifier.size(12.dp).clip(RoundedCornerShape(4.dp)).background(kategooriaVarvid[i % kategooriaVarvid.size]))
                Spacer(Modifier.width(10.dp))
                T(Keel.t(o.nimi), 14.sp, V.Tekst, modifier = Modifier.weight(1f), read = 1)
                T("${o.summa * 100 / kokku} %", 12.sp, V.Tekst2)
                Spacer(Modifier.width(10.dp))
                T(eur(o.summa), 14.sp, V.Tekst, FontWeight.Bold)
            }
        }
    }
}

// ---------------------------------------------------------------
//  Pere korv: pereliikmete lisatud tooted ühes korvis; "Võta kõik minu korvi"
//  Hoitakse seades "perekorv" ridadena toodeId;kogus;lisaja;aeg
// ---------------------------------------------------------------
data class PereKorviRida(val toodeId: Int, val kogus: Int, val lisaja: String, val aeg: LocalDateTime)

object PereKorv {
    fun loe(pood: Pood): List<PereKorviRida> = (pood.seade("perekorv") ?: "").lines().mapNotNull { r ->
        val o = r.split(';')
        if (o.size < 4) null else runCatching { PereKorviRida(o[0].toInt(), o[1].toInt(), o[2], LocalDateTime.parse(o[3])) }.getOrNull()
    }

    fun salvesta(pood: Pood, read: List<PereKorviRida>) =
        pood.seaSeade("perekorv", read.filter { it.kogus > 0 }.joinToString("\n") { "${it.toodeId};${it.kogus};${it.lisaja};${it.aeg}" }.ifEmpty { null })

    fun lisa(pood: Pood, read: List<PereKorviRida>, toodeId: Int, kogus: Int, lisaja: String, aeg: LocalDateTime): List<PereKorviRida> {
        val i = read.indexOfFirst { it.toodeId == toodeId }
        val uus = if (i >= 0) read.toMutableList().also { it[i] = it[i].copy(kogus = it[i].kogus + kogus, lisaja = lisaja, aeg = aeg) }
        else listOf(PereKorviRida(toodeId, kogus, lisaja, aeg)) + read
        salvesta(pood, uus)
        return uus
    }
}

@Composable
fun PereKorvEkraan(olek: AppOlek) {
    val kood = olek.pood.seade("pere")
    var read by remember { mutableStateOf(PereKorv.loe(olek.pood)) }
    val mina = olek.kasutaja?.eesnimi ?: "Mina"
    // Pereliige lisab toote (näidis, kui pere on loodud): ilmub sujuvalt nimekirja algusse koos teatega
    LaunchedEffect(kood) {
        if (kood == null) return@LaunchedEffect
        delay(5000)
        val t = olek.tooted.filter { it.laos && it.kategooriaId in setOf(1, 2, 3) }.randomOrNull() ?: return@LaunchedEffect
        read = PereKorv.lisa(olek.pood, read, t.id, 1, "Kati", olek.aeg())
        olek.teade = Keel.f("{0} lisas pere korvi: {1}", "Kati", t.nimi)
    }
    Box(Modifier.fillMaxSize()) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 150.dp).animateContentSize()) {
            Pais("Pere korv", { olek.tagasi() }, kood?.let { Keel.f("Pere kood {0}", it) })
            Vahe(12.dp)
            if (kood == null) {
                Tyhi(Ik.Kasutajad, Keel.t("Sa pole veel peres"), Keel.t("Loo pere või liitu koodiga – siis näevad kõik sama korvi."))
                Vahe(12.dp)
                SuurNupp(Keel.t("Pere seaded"), { olek.ava(Ekraan.Pere) })
                return@Column
            }
            AaarisNupp(Keel.t("Jaga minu korvi perega"), {
                var r = read
                for (k in olek.korv) r = PereKorv.lisa(olek.pood, r, k.toode.id, k.kogus, mina, olek.aeg())
                read = r
                olek.teade = if (olek.korv.isEmpty()) Keel.t("Sinu ostukorv on tühi.") else Keel.t("Korv jagatud perega")
            }, Modifier.fillMaxWidth())
            Vahe(12.dp)
            if (read.isEmpty()) Tyhi(Ik.Korv, Keel.t("Pere korv on tühi"), Keel.t("Lisa tooteid toote lehelt: „+ Pere korvi“."))
            for (r in read) {
                val t = olek.toode(r.toodeId) ?: continue
                Row(
                    Modifier.fillMaxWidth().padding(vertical = 4.dp).clip(RoundedCornerShape(18.dp))
                        .background(if (r.lisaja != mina && r.aeg.isAfter(olek.aeg().minusMinutes(2))) V.Kreem else V.Pind).padding(10.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Initsiaalid(r.lisaja.take(1).uppercase(), if (r.lisaja == mina) V.Ploom else V.Oranz, 34.dp)
                    Spacer(Modifier.width(10.dp))
                    Column(Modifier.weight(1f)) {
                        T(t.nimi, 14.sp, V.Tekst, FontWeight.Bold, read = 1)
                        T(Keel.f("lisas {0}", r.lisaja), 11.sp, V.Nork)
                    }
                    KogusValija(r.kogus, {
                        read = read.map { if (it.toodeId == r.toodeId) it.copy(kogus = it.kogus - 1) else it }.filter { it.kogus > 0 }
                        PereKorv.salvesta(olek.pood, read)
                    }, {
                        read = read.map { if (it.toodeId == r.toodeId) it.copy(kogus = it.kogus + 1) else it }
                        PereKorv.salvesta(olek.pood, read)
                    })
                }
            }
        }
        if (kood != null && read.isNotEmpty()) Column(
            Modifier.align(Alignment.BottomCenter).fillMaxWidth().clip(RoundedCornerShape(topStart = 28.dp, topEnd = 28.dp)).background(V.Tume).padding(16.dp)
        ) {
            val summa = read.sumOf { r -> olek.toode(r.toodeId)?.let { olek.hind(it).hind * r.kogus } ?: 0 }
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(Keel.t("Pere korv kokku"), 14.sp, Color(0xFFC9CDD2), modifier = Modifier.weight(1f))
                T(eur(summa), 24.sp, Color.White, FontWeight.ExtraBold)
            }
            Vahe(10.dp)
            SuurNupp(Keel.t("Võta kõik minu korvi"), {
                var puudu = 0
                for (r in read) {
                    val t = olek.toode(r.toodeId)
                    if (t == null || olek.lisaKorvi(t, minOf(r.kogus, t.kogus - olek.korvis(t.id)).coerceAtLeast(1)) != null) puudu++
                }
                read = emptyList(); PereKorv.salvesta(olek.pood, read)
                olek.teade = if (puudu == 0) Keel.t("Kõik tooted on sinu korvis") else Keel.f("{0} toodet polnud laos", puudu)
                olek.ava(Ekraan.Korv)
            })
        }
    }
}

