package ee.akt.pood

import android.content.Context
import android.content.Intent
import androidx.compose.animation.animateContentSize
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import java.time.LocalDate
import java.time.LocalTime
import java.time.temporal.ChronoUnit

// ---------------------------------------------------------------
//  Kõik ülejäänud arvutiversiooni funktsioonid telefonis
// ---------------------------------------------------------------

// CSV jagamine (e-post, Drive, Messenger …) – Androidi jagamise aken
fun jagaCsv(context: Context, pealkiri: String, sisu: String) {
    val i = Intent(Intent.ACTION_SEND).apply {
        type = "text/csv"
        putExtra(Intent.EXTRA_SUBJECT, pealkiri)
        putExtra(Intent.EXTRA_TEXT, sisu)
    }
    context.startActivity(Intent.createChooser(i, pealkiri))
}

private fun csv(vararg v: Any?) = v.joinToString(";") { (it ?: "").toString().replace(";", ",").replace("\n", " ") }
private fun euroCsv(sendid: Int) = "%d,%02d".format(sendid / 100, Math.abs(sendid % 100))

// ---- Salvestatud ostukorvid (ostukorvi lehel) ----
@Composable
fun SalvestatudKorvid(olek: AppOlek) {
    var korvid by remember { mutableStateOf(olek.salvestatudKorvid()) }
    var nimi by remember { mutableStateOf("") }
    var lahti by remember { mutableStateOf(false) }
    Kaart(Modifier.fillMaxWidth().animateContentSize()) {
        Row(Modifier.fillMaxWidth().vajuta { lahti = !lahti }, verticalAlignment = Alignment.CenterVertically) {
            Ikoon(Ik.Tsekk, V.Ploom, 20.dp)
            Spacer(Modifier.width(10.dp))
            T(Keel.t("Salvestatud ostukorvid"), 15.sp, V.Tekst, FontWeight.Bold, modifier = Modifier.weight(1f))
            VaikeKiip(korvid.size.toString(), V.Roosa, V.Ploom)
            Spacer(Modifier.width(6.dp))
            Ikoon(if (lahti) Ik.Miinus else Ik.Pluss, V.Nork, 16.dp)
        }
        if (lahti) {
            if (olek.korv.isNotEmpty()) {
                Vahe(10.dp)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Sisend(nimi, { nimi = it }, Keel.t("Korvi nimi, nt Nädalavarud"), Modifier.weight(1f))
                    Spacer(Modifier.width(8.dp))
                    RingNupp(Ik.Linnuke, {
                        olek.salvestaKorv(nimi); nimi = ""; korvid = olek.salvestatudKorvid()
                        olek.teade = Keel.t("Ostukorv salvestatud")
                    }, V.Ploom, Color.White, 48.dp)
                }
            }
            if (korvid.isEmpty()) { Vahe(8.dp); T(Keel.t("Salvestatud korve pole. Täida korv ja salvesta see siin."), 13.sp, V.Nork) }
            for (k in korvid) {
                Vahe(8.dp)
                Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(16.dp)).background(V.Paneel).padding(10.dp), verticalAlignment = Alignment.CenterVertically) {
                    Column(Modifier.weight(1f)) {
                        T(k.nimi, 14.sp, V.Tekst, FontWeight.Bold, read = 1)
                        T(Keel.f("{0} toodet", k.read.size), 11.sp, V.Nork)
                    }
                    Kiip(Keel.t("Lisa korvi"), V.Tume, Color.White, onClick = {
                        val (n, puudu) = olek.taastaKorv(k)
                        olek.teade = Keel.f("Korvi lisatud {0} tk", n) + if (puudu.isNotEmpty()) " · " + Keel.f("puudu: {0}", puudu.take(2).joinToString(", ")) else ""
                    })
                    Spacer(Modifier.width(6.dp))
                    RingNupp(Ik.Prygi, { olek.kustutaKorv(k.nimi); korvid = olek.salvestatudKorvid() }, V.Pind, V.Tekst2, 36.dp)
                }
            }
        }
    }
}

// ---- Toote lehel: "Sageli ostetakse koos" (kõigi ostude põhjal) ----
@Composable
fun SageliKoos(olek: AppOlek, t: Toode) {
    val koos = remember(t.id) {
        olek.pood.ostud(null).filter { o -> !o.tagastatud && o.read.any { it.toodeId == t.id } }
            .flatMap { o -> o.read.mapNotNull { it.toodeId }.filter { it != t.id }.distinct() }
            .groupingBy { it }.eachCount().filter { it.value >= 2 }.entries.sortedByDescending { it.value }.take(4)
            .mapNotNull { olek.toode(it.key) }
    }
    if (koos.isEmpty()) return
    Vahe(18.dp)
    T(Keel.t("Sageli ostetakse koos"), 16.sp, V.Tekst, FontWeight.ExtraBold)
    Vahe(8.dp)
    Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
        for (s in koos) ToodeKaart(olek, s, Modifier.width(150.dp))
    }
}

// ---------------------------------------------------------------
//  Omanik: kassa – müük poes (otsing, korv, kliendikaart, makseviis)
// ---------------------------------------------------------------
@Composable
fun KassaEkraan(olek: AppOlek) {
    var otsi by remember { mutableStateOf("") }
    var kaart by remember { mutableStateOf("") }
    var makse by remember { mutableIntStateOf(0) }
    val read = remember { mutableStateListOf<Pair<Toode, Int>>() }
    val klient = remember(kaart) { if (kaart.length >= 6) olek.pood.kasutajad().firstOrNull { it.kaart == kaart.trim() } else null }
    val vasted = if (otsi.trim().length < 2) emptyList() else olek.tooted.asSequence()
        .filter { it.laos && (it.nimi.contains(otsi.trim(), true) || (it.kaubamark ?: "").contains(otsi.trim(), true)) }.take(6).toList()
    val summa = read.sumOf { (t, k) -> olek.hind(t).hind * k }
    val pant = read.sumOf { (t, k) -> t.pant * k }

    Box(Modifier.fillMaxSize()) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 170.dp)) {
            if (olek.kasutaja?.roll == Roll.Myyja)
                // müüjal on kassa avaleht: tagasi asemel ostude ajalugu ja väljalogimine
                Pais("Kassa", null, Keel.t("Müük poes") + " · " + (olek.kasutaja?.taisNimi ?: "")) {
                    RingNupp(Ik.Tsekk, { olek.ava(Ekraan.Ajalugu) }, V.Pind, V.Tekst, 44.dp)
                    Spacer(Modifier.width(8.dp))
                    RingNupp(Ik.Valja, { olek.logiValja() }, V.Pind, V.Tekst, 44.dp)
                }
            else Pais("Kassa", { olek.tagasi() }, Keel.t("Müük poes"))
            Vahe(10.dp)
            Sisend(otsi, { otsi = it }, Keel.t("Otsi toodet või kaubamärki"), taust = V.Pind, ees = { Ikoon(Ik.Otsi, V.Tekst2, 18.dp) })
            for (t in vasted) {
                Row(
                    Modifier.fillMaxWidth().padding(top = 6.dp).clip(RoundedCornerShape(16.dp)).background(V.Pind).vajuta {
                        val i = read.indexOfFirst { it.first.id == t.id }
                        if (i >= 0) { if (read[i].second < t.kogus) read[i] = t to read[i].second + 1 } else read.add(t to 1)
                        otsi = ""
                    }.padding(10.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    ToodePilt(t.pilt, Modifier.size(36.dp), 16.dp)
                    Spacer(Modifier.width(10.dp))
                    T(t.nimi, 13.sp, V.Tekst, FontWeight.SemiBold, read = 1, modifier = Modifier.weight(1f))
                    T(eur(olek.hind(t).hind), 13.sp, V.Tekst, FontWeight.ExtraBold)
                }
            }
            Vahe(12.dp)
            if (read.isEmpty()) Tyhi(Ik.Korv, Keel.t("Kassa on tühi"), Keel.t("Otsi toode ja vajuta sellele, et lisada."))
            for ((i, rida) in read.withIndex()) {
                val (t, k) = rida
                Row(
                    Modifier.fillMaxWidth().padding(vertical = 3.dp).clip(RoundedCornerShape(16.dp)).background(V.Pind).padding(10.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(Modifier.weight(1f)) {
                        T(t.nimi, 13.sp, V.Tekst, FontWeight.Bold, read = 1)
                        T(eur(olek.hind(t).hind) + " × " + k, 11.sp, V.Nork)
                    }
                    KogusValija(k, { if (k > 1) read[i] = t to k - 1 else read.removeAt(i) }, { if (k < t.kogus) read[i] = t to k + 1 })
                }
            }
            Vahe(12.dp)
            Sisend(kaart, { kaart = it.filter { c -> c.isDigit() }.take(6) }, Keel.t("Kliendikaardi number (valikuline)"), number = true, taust = V.Pind,
                ees = { Ikoon(Ik.Kaart, V.Tekst2, 18.dp) })
            if (klient != null) { Vahe(6.dp); VaikeKiip("✓ " + klient.taisNimi + " · " + Keel.f("boonus {0}", eur(klient.boonus)), V.RohelineHele, V.Roheline) }
            else if (kaart.length == 6) { Vahe(6.dp); VaikeKiip(Keel.t("Kaarti ei leitud"), V.Roosa, V.Punane) }
            Vahe(10.dp)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                listOf("Kaart", "Sularaha").forEachIndexed { j, m -> Kiip(Keel.t(m), V.Pind, V.Tekst, valitud = makse == j, onClick = { makse = j }) }
            }
        }
        Column(Modifier.align(Alignment.BottomCenter).fillMaxWidth().clip(RoundedCornerShape(topStart = 28.dp, topEnd = 28.dp)).background(V.Tume).padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(Keel.f("{0} tk", read.sumOf { it.second }) + if (pant > 0) " · " + Keel.f("pant {0}", eur(pant)) else "", 13.sp, Color(0xFFC9CDD2), modifier = Modifier.weight(1f))
                T(eur(summa + pant), 26.sp, Color.White, FontWeight.ExtraBold)
            }
            Vahe(10.dp)
            SuurNupp(Keel.f("Maksa {0}", eur(summa + pant)), {
                val ostuRead = read.map { (t, k) -> val h = olek.hind(t); OstuRida(t.id, t.nimi, k, h.hind, h.tavaHind, h.soodus) }
                val boonus = if (klient != null) Reeglid.boonus(summa, Reeglid.Tase.Pronks) else 0
                olek.pood.lisaOst(Ost(0, olek.aeg(), klient?.nimi ?: "kassa", summa, 0, 0, pant, summa + pant, boonus,
                    if (makse == 0) "Kaart" else "Sularaha", "Kassa", null, ostuRead))
                olek.varskendaTooted()
                olek.teade = Keel.f("Makstud {0}", eur(summa + pant)) + if (klient != null) " · " + Keel.f("boonus +{0}", eur(boonus)) else ""
                read.clear(); kaart = ""
            }, lubatud = read.isNotEmpty(), noolega = false)
        }
    }
}

// ---------------------------------------------------------------
//  Omanik: partiid ja säilivus – lisamine, aeguvad (−30 %), mahakandmine
// ---------------------------------------------------------------
@Composable
fun PartiidEkraan(olek: AppOlek) {
    var otsi by remember { mutableStateOf("") }
    var valitud by remember { mutableStateOf<Toode?>(null) }
    var kogus by remember { mutableIntStateOf(10) }
    var paevi by remember { mutableIntStateOf(5) }
    val tana = olek.aeg().toLocalDate()
    val vasted = if (valitud != null || otsi.trim().length < 2) emptyList() else olek.tooted.asSequence().filter { it.nimi.contains(otsi.trim(), true) }.take(5).toList()

    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 40.dp).animateContentSize()) {
        Pais("Partiid ja säilivus", { olek.tagasi() }, Keel.f("{0} partiid", olek.partiid.size))
        Vahe(10.dp)
        Kaart(Modifier.fillMaxWidth()) {
            T(Keel.t("Uus partii"), 15.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(8.dp)
            val v = valitud
            if (v == null) {
                Sisend(otsi, { otsi = it }, Keel.t("Otsi toodet"), ees = { Ikoon(Ik.Otsi, V.Tekst2, 18.dp) })
                for (t in vasted) T(t.nimi, 14.sp, V.Tekst, modifier = Modifier.fillMaxWidth().clip(RoundedCornerShape(12.dp)).vajuta { valitud = t; otsi = "" }.padding(10.dp), read = 1)
            } else {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(v.nimi, 14.sp, V.Tekst, FontWeight.Bold, read = 2, modifier = Modifier.weight(1f))
                    RingNupp(Ik.Sulge, { valitud = null }, V.Paneel, V.Tekst, 34.dp)
                }
                Vahe(10.dp)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(Keel.t("Kogus"), 13.sp, V.Tekst2, modifier = Modifier.weight(1f))
                    KogusValija(kogus, { if (kogus > 1) kogus -= if (kogus > 10) 5 else 1 }, { kogus += if (kogus >= 10) 5 else 1 })
                }
                Vahe(10.dp)
                T(Keel.t("Säilib kuni"), 13.sp, V.Tekst2)
                Vahe(6.dp)
                Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                    for (p in listOf(1, 2, 3, 5, 7, 14, 30)) Kiip(tana.plusDays(p.toLong()).let { "${it.dayOfMonth}.${it.monthValue}" }, V.Paneel, V.Tekst, valitud = paevi == p, onClick = { paevi = p })
                }
                Vahe(12.dp)
                SuurNupp(Keel.t("Lisa partii"), {
                    olek.pood.lisaPartii(v.id, kogus, tana.plusDays(paevi.toLong()))
                    olek.varskendaPartiid(); olek.varskendaTooted()
                    olek.teade = Keel.f("Partii lisatud: {0} · {1} tk", v.nimi, kogus)
                    valitud = null
                }, noolega = false)
            }
        }
        Vahe(14.dp)
        val aegunud = olek.partiid.filter { it.aegub.isBefore(tana) }
        if (aegunud.isNotEmpty()) {
            AaarisNupp(Keel.f("Kanna maha kõik aegunud ({0})", aegunud.size), {
                for (p in aegunud) olek.pood.kannaMaha(p.id)
                olek.varskendaPartiid(); olek.varskendaTooted()
            }, Modifier.fillMaxWidth(), V.Punane)
            Vahe(10.dp)
        }
        if (olek.partiid.isEmpty()) Tyhi(Ik.Kell, Keel.t("Partiisid pole"), Keel.t("Lisa partii koos säilivusajaga – 2 päeva enne aegumist läheb toode −30 % („Ilma raiskamiseta“)."))
        for (p in olek.partiid) {
            val t = olek.toode(p.toodeId)
            val jaanud = ChronoUnit.DAYS.between(tana, p.aegub)
            val (taust, varv, silt) = when {
                jaanud < 0 -> Triple(V.Roosa, V.Punane, Keel.t("aegunud"))
                jaanud <= Raiskamine.PAEVI_ENNE -> Triple(V.Kreem, V.Oranz, Raiskamine.tekst(jaanud.toInt()) + " · −30 %")
                else -> Triple(V.Pind, V.Tekst2, Keel.f("{0} päeva", jaanud))
            }
            Row(Modifier.fillMaxWidth().padding(vertical = 4.dp).clip(RoundedCornerShape(18.dp)).background(taust).padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                Column(Modifier.weight(1f)) {
                    T(t?.nimi ?: "#${p.toodeId}", 14.sp, V.Tekst, FontWeight.Bold, read = 1)
                    T(Keel.f("{0} tk", p.kogus) + " · " + "${p.aegub.dayOfMonth}.${p.aegub.monthValue}.${p.aegub.year}", 11.sp, V.Nork)
                    T(silt, 12.sp, varv, FontWeight.Bold)
                }
                Kiip(Keel.t("Kanna maha"), V.Paneel, V.Tekst, onClick = {
                    olek.pood.kannaMaha(p.id); olek.varskendaPartiid(); olek.varskendaTooted()
                    olek.teade = Keel.t("Partii maha kantud")
                })
            }
        }
    }
}

// ---------------------------------------------------------------
//  Müügipaneeli lisad: 7 päeva prognoos ja soojuskaart (nädalapäev × tund)
// ---------------------------------------------------------------
object Prognoos {
    // Iga päev = sama nädalapäeva kaalutud keskmine viimasest 4 nädalast (4,3,2,1) × trend (viimased 14 p / eelnevad 14 p, 0,8…1,25)
    fun arvuta(ajalugu: List<Int>, paevi: Int = 7): List<Int> {
        val n = ajalugu.size
        if (n == 0) return List(paevi) { 0 }
        val viimased = ajalugu.drop(maxOf(0, n - 14)).sum().toDouble()
        val eelnevad = ajalugu.drop(maxOf(0, n - 28)).take(maxOf(0, minOf(14, n - 14))).sum().toDouble()
        val trend = if (eelnevad > 0) (viimased / eelnevad).coerceIn(0.8, 1.25) else 1.0
        return (0 until paevi).map { d ->
            var summa = 0.0; var kaal = 0.0
            for (nadal in 1..4) {
                var i = n + d - 7 * nadal
                while (i >= n) i -= 7
                if (i < 0) continue
                val w = (5 - nadal).toDouble()
                summa += ajalugu[i] * w; kaal += w
            }
            if (kaal > 0) Math.round(summa / kaal * trend).toInt() else 0
        }
    }
}

@Composable
fun PrognoosiKaart(olek: AppOlek, ostud: List<Ost>) {
    val tana = olek.aeg().toLocalDate()
    val ajalugu = remember { (28 downTo 1).map { d -> val p = tana.minusDays(d.toLong()); ostud.filter { it.aeg.toLocalDate() == p }.sumOf { it.summa - it.tarne } } }
    val prognoos = remember { Prognoos.arvuta(ajalugu) }
    val nadal = prognoos.sum()
    val eelmine = ajalugu.takeLast(7).sum()
    Kaart(Modifier.fillMaxWidth()) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Column(Modifier.weight(1f)) {
                T(Keel.t("Prognoos: järgmised 7 päeva"), 15.sp, V.Tekst, FontWeight.ExtraBold)
                T(Keel.t("Sama nädalapäeva keskmine 4 nädalast × trend"), 11.sp, V.Nork)
            }
            Column(horizontalAlignment = Alignment.End) {
                T(eur(nadal), 20.sp, V.Tekst, FontWeight.ExtraBold)
                if (eelmine > 0) {
                    val m = Math.round((nadal - eelmine) * 100.0 / eelmine)
                    T((if (m >= 0) "+" else "") + m + " %", 12.sp, if (m >= 0) V.Roheline else V.Punane, FontWeight.Bold)
                }
            }
        }
        Vahe(10.dp)
        val koik = ajalugu.takeLast(14) + prognoos
        val max = maxOf(1, koik.maxOrNull() ?: 1)
        val tulp = V.Ploom; val tulp2 = V.Ploom.copy(alpha = 0.35f)
        Canvas(Modifier.fillMaxWidth().height(110.dp)) {
            val w = size.width / koik.size
            koik.forEachIndexed { i, s ->
                val h = (size.height - 2) * s / max
                drawRoundRect(if (i < 14) tulp else tulp2, Offset(i * w + w * 0.2f, size.height - h), Size(w * 0.6f, h), CornerRadius(6f, 6f))
            }
            drawLine(tulp, Offset(14 * w, 0f), Offset(14 * w, size.height), 2f)
        }
        Row(Modifier.fillMaxWidth()) {
            T(Keel.t("14 päeva tegelik"), 10.sp, V.Nork, modifier = Modifier.weight(2f))
            T(Keel.t("prognoos"), 10.sp, V.Ploom, FontWeight.Bold, modifier = Modifier.weight(1f))
        }
    }
}

@Composable
fun SoojuskaardiKaart(ostud: List<Ost>) {
    val algus = 8; val lopp = 21
    val kaart = remember {
        Array(7) { IntArray(lopp - algus + 1) }.also { k ->
            for (o in ostud) {
                val p = o.aeg.dayOfWeek.value - 1
                val t = o.aeg.hour.coerceIn(algus, lopp) - algus
                k[p][t]++
            }
        }
    }
    val max = maxOf(1, kaart.maxOf { r -> r.maxOrNull() ?: 0 })
    val paevad = listOf("E", "T", "K", "N", "R", "L", "P")
    val pohi = V.Ploom; val tyhi = V.Paneel
    Kaart(Modifier.fillMaxWidth()) {
        T(Keel.t("Millal ostetakse"), 15.sp, V.Tekst, FontWeight.ExtraBold)
        T(Keel.t("Nädalapäev × kellaaeg, tumedam = rohkem oste"), 11.sp, V.Nork)
        Vahe(10.dp)
        for (p in 0 until 7) Row(verticalAlignment = Alignment.CenterVertically) {
            T(Keel.t(paevad[p]), 11.sp, V.Tekst2, FontWeight.Bold, modifier = Modifier.width(18.dp))
            for (t in kaart[p].indices) {
                val osa = kaart[p][t].toFloat() / max
                Box(Modifier.weight(1f).aspectRatio(1f).padding(1.5.dp).clip(RoundedCornerShape(4.dp)).background(if (osa == 0f) tyhi else pohi.copy(alpha = 0.18f + 0.82f * osa)))
            }
        }
        Row(Modifier.fillMaxWidth().padding(start = 18.dp)) {
            for (t in algus..lopp) T(if (t % 4 == 0) "$t" else "", 9.sp, V.Nork, modifier = Modifier.weight(1f))
        }
    }
}

// ---------------------------------------------------------------
//  Seaded (omanik): õhtune soodustus ja kategooriad
// ---------------------------------------------------------------
@Composable
fun OhtuSeaded(olek: AppOlek) {
    var sees by remember { mutableStateOf(Reeglid.OHTU_SEES) }
    var algus by remember { mutableStateOf(Reeglid.OHTU_ALGUS) }
    var protsent by remember { mutableIntStateOf(Reeglid.OHTU_PROTSENT) }
    fun salvesta() = olek.salvestaOhtu(sees, algus, protsent)
    Row(Modifier.fillMaxWidth().vajuta { sees = !sees; salvesta() }.padding(vertical = 6.dp), verticalAlignment = Alignment.CenterVertically) {
        Column(Modifier.weight(1f)) {
            T(Keel.t("Õhtune soodustus"), 15.sp, V.Tekst, FontWeight.SemiBold)
            T(Keel.t("Leivad, saiad ja küpsetised õhtul odavamalt"), 12.sp, V.Nork)
        }
        Lyliti(sees)
    }
    if (sees) {
        Vahe(6.dp)
        T(Keel.t("Algab"), 12.sp, V.Tekst2, FontWeight.Bold)
        Row(Modifier.horizontalScroll(rememberScrollState()).padding(vertical = 4.dp), horizontalArrangement = Arrangement.spacedBy(6.dp)) {
            for (h in listOf(17, 18, 19, 20, 21)) Kiip("$h:00", V.Paneel, V.Tekst, valitud = algus.hour == h, onClick = { algus = LocalTime.of(h, 0); salvesta() })
        }
        T(Keel.t("Soodustus"), 12.sp, V.Tekst2, FontWeight.Bold)
        Row(Modifier.horizontalScroll(rememberScrollState()).padding(vertical = 4.dp), horizontalArrangement = Arrangement.spacedBy(6.dp)) {
            for (p in listOf(20, 30, 40, 50, 60, 70)) Kiip("−$p %", V.Paneel, V.Tekst, valitud = protsent == p, onClick = { protsent = p; salvesta() })
        }
    }
}

@Composable
fun KategooriadSeaded(olek: AppOlek) {
    var kategooriad by remember { mutableStateOf(olek.pood.kategooriad()) }
    var uus by remember { mutableStateOf("") }
    val arvud = remember(olek.tooted) { olek.tooted.groupingBy { it.kategooriaId }.eachCount() }
    T(Keel.t("Kategooriad"), 15.sp, V.Tekst, FontWeight.SemiBold)
    Vahe(6.dp)
    for (k in kategooriad) Row(Modifier.fillMaxWidth().padding(vertical = 3.dp), verticalAlignment = Alignment.CenterVertically) {
        T(Keel.t(k.nimi), 14.sp, V.Tekst, modifier = Modifier.weight(1f), read = 1)
        T(Keel.f("{0} toodet", arvud[k.id] ?: 0), 12.sp, V.Nork)
        Spacer(Modifier.width(8.dp))
        RingNupp(Ik.Prygi, {
            val viga = olek.pood.kustutaKategooria(k.id)
            olek.teade = viga?.let { Keel.t(it) } ?: Keel.t("Kategooria kustutatud")
            kategooriad = olek.pood.kategooriad()
        }, V.Paneel, V.Tekst2, 32.dp)
    }
    Vahe(8.dp)
    Row(verticalAlignment = Alignment.CenterVertically) {
        Sisend(uus, { uus = it }, Keel.t("Uue kategooria nimi"), Modifier.weight(1f))
        Spacer(Modifier.width(8.dp))
        RingNupp(Ik.Pluss, {
            if (uus.isNotBlank()) { olek.pood.lisaKategooria(uus); uus = ""; kategooriad = olek.pood.kategooriad(); olek.teade = Keel.t("Kategooria lisatud") }
        }, V.Ploom, Color.White, 48.dp)
    }
    T(Keel.t("Uus kategooria ilmub poodi pärast rakenduse taaskäivitust."), 11.sp, V.Nork)
}

// CSV-d jagamiseks
fun autotellimusCsv(read: List<Autotellimus.Rida>, telli: (Autotellimus.Rida) -> Int): String =
    (listOf(csv("Tarnija", "Toode", "Laos", "Jätkub päeva", "Telli", "Ostuhind", "Summa")) +
        read.filter { telli(it) > 0 }.map { csv(it.tarnija, it.toode.nimi, it.toode.kogus, "%.1f".format(it.jatkub), telli(it), euroCsv(it.ostuhind), euroCsv(it.ostuhind * telli(it))) })
        .joinToString("\n")

fun myykCsv(ostud: List<Ost>): String =
    (listOf(csv("Kuupäev", "Ostja", "Tooted", "Summa", "Makse", "Tarne")) +
        ostud.sortedBy { it.aeg }.map { csv(it.aeg.toString().replace('T', ' ').take(16), it.kasutaja, it.read.sumOf { r -> r.kogus }, euroCsv(it.summa), it.makseviis, it.tarneviis) })
        .joinToString("\n")
