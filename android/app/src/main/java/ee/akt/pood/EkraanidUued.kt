package ee.akt.pood

import androidx.compose.animation.animateContentSize
import androidx.compose.animation.core.Animatable
import androidx.compose.animation.core.tween
import androidx.compose.animation.core.LinearEasing
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import java.time.LocalDate
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
    val eelvaade = (LocalInspectionMode.current || Eelvaade.kohe)   // ekraanipildi testis kohe täis
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
        if (olek.pood.seade("pere_teated") != "0") olek.teade = Keel.f("{0} lisas pere korvi: {1}", "Kati", t.nimi)
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


// ---------------------------------------------------------------
//  Hooajaline riba avalehel (sama mis arvutis): Halloween, jõulud, talv, sõbrapäev.
//  Seadetes saab välja lülitada. Vajutus otsib teemakohaseid tooteid.
// ---------------------------------------------------------------
enum class Hooaeg(val tekst: String, val otsi: String, val taust: Long) {
    Halloween("Halloween 31.10 – kõrvitsad, kommid ja kostüümid", "kõrvits", 0xFF2A1840),
    Joulud("Häid pühi! Jõulukaubad →", "jõulu", 0xFF0F3D2A),
    Talv("Talvised soojad joogid →", "kakao", 0xFF1E3A5F),
    Sobrapaev("Sõbrapäeva kingitused →", "šokolaad", 0xFF9D174D);

    companion object {
        fun praegu(p: LocalDate): Hooaeg? = when {
            (p.monthValue == 10 && p.dayOfMonth >= 15) || (p.monthValue == 11 && p.dayOfMonth <= 2) -> Halloween
            p.monthValue == 12 || (p.monthValue == 1 && p.dayOfMonth <= 6) -> Joulud
            p.monthValue == 2 && p.dayOfMonth in 10..14 -> Sobrapaev
            p.monthValue <= 2 -> Talv
            else -> null
        }
    }
}

@Composable
fun HooajaRiba(olek: AppOlek) {
    if (olek.pood.seade("hooaeg") == "0") return
    val h = Hooaeg.praegu(olek.aeg().toLocalDate()) ?: return
    val liikumine = rememberInfiniteTransition(label = "hooaeg")
    val aeg by liikumine.animateFloat(0f, 1f, infiniteRepeatable(tween(6000, easing = LinearEasing)), label = "aeg")
    // Peidetud mäng: kõrvitsale vajutades see "puhkeb" kommideks; kõik 4 leitud → teade
    val leitud = remember { androidx.compose.runtime.mutableStateListOf<Int>() }
    Box(
        Modifier.fillMaxWidth().height(86.dp).clip(RoundedCornerShape(22.dp)).background(Color(h.taust))
            .pointerInput(h) {
                detectTapGestures { p ->
                    val i = if (h == Hooaeg.Halloween) (0 until 4).firstOrNull { i -> kotlin.math.abs(p.x - size.width * (0.52f + i * 0.13f)) < 34f } else null
                    if (i == null) olek.ava(Ekraan.Otsing(h.otsi))
                    else if (i !in leitud) {
                        leitud.add(i)
                        if (leitud.size == 4) olek.teade = Keel.t("Leidsid kõik kõrvitsad! Head Halloweeni!")
                    }
                }
            }
    ) {
        Canvas(Modifier.fillMaxSize()) {
            val w = size.width; val k = size.height
            val faas = aeg * 2 * Math.PI.toFloat()
            when (h) {
                Hooaeg.Halloween -> {
                    for (i in 0 until 4) {   // kõrvitsad hüplevad; leitud kõrvits = kommid ümber
                        val x = w * (0.52f + i * 0.13f); val y = k * 0.62f + kotlin.math.sin(faas + i) * 5f
                        val leitudSee = i in leitud
                        drawOval(Color(if (leitudSee) 0xFFFDBA74 else 0xFFEA580C), Offset(x - 18f, y - 14f), Size(36f, 28f))
                        drawRect(Color(0xFF3F6212), Offset(x - 2f, y - 22f), Size(4f, 9f))
                        if (leitudSee) for (j in 0 until 6) {
                            val nurk = j / 6f * 2 * Math.PI.toFloat() + faas
                            val r = 24f + kotlin.math.sin(faas * 2 + j) * 4f
                            drawCircle(Color(listOf(0xFFF97316, 0xFFA855F7, 0xFF22C55E, 0xFFFACC15)[j % 4]), 3.5f, Offset(x + kotlin.math.cos(nurk) * r, y + kotlin.math.sin(nurk) * r * 0.6f))
                        }
                    }
                    val bx = w * aeg; val by = k * 0.25f + kotlin.math.sin(faas * 3) * 4f   // nahkhiir lendab üle
                    drawOval(Color(0xFF0B0B0F), Offset(bx - 14f, by - 4f), Size(28f, 8f))
                }
                Hooaeg.Joulud, Hooaeg.Talv -> {
                    if (h == Hooaeg.Joulud) for (i in 0 until 14) {   // tuled vilguvad
                        val v = listOf(0xFFEF4444, 0xFFFACC15, 0xFF22C55E, 0xFF3B82F6)[i % 4]
                        val hele = 0.35f + 0.65f * ((kotlin.math.sin(faas * 2 + i) + 1) / 2)
                        drawCircle(Color(v).copy(alpha = hele), 6f, Offset(w * (i + 0.5f) / 14, 10f + (i % 2) * 6f))
                    }
                    for (i in 0 until 26) {   // lumi sajab
                        val y = ((aeg + i * 0.137f) % 1f) * k
                        drawCircle(Color.White.copy(alpha = 0.8f), 2f + i % 3, Offset((i * 97f) % w, y))
                    }
                }
                Hooaeg.Sobrapaev -> for (i in 0 until 10) {   // südamed tõusevad
                    val y = k - ((aeg + i * 0.1f) % 1f) * k
                    drawCircle(Color(0xFFF9A8D4).copy(alpha = 0.7f), 5f + i % 3 * 2, Offset(w * (0.45f + (i % 5) * 0.11f), y))
                }
            }
        }
        T(Keel.t(h.tekst), 15.sp, Color.White, FontWeight.ExtraBold, read = 2, modifier = Modifier.align(Alignment.CenterStart).padding(start = 16.dp).fillMaxWidth(0.55f))
    }
}
