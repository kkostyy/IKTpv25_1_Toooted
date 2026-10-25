package ee.akt.pood

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.horizontalScroll
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

// ---------------------------------------------------------------
//  Sisselogimine (TelSisse): tume ülaosa kaartidega, valge leht all
// ---------------------------------------------------------------
@Composable
fun SisseEkraan(olek: AppOlek) {
    var nimi by remember { mutableStateOf("") }
    var parool by remember { mutableStateOf("") }
    var naita by remember { mutableStateOf(false) }
    var viga by remember { mutableStateOf<String?>(null) }
    Column(Modifier.fillMaxSize().background(V.Tume)) {
        Box(Modifier.fillMaxWidth().weight(1f)) {
            Canvas(Modifier.fillMaxSize()) {
                drawCircle(Color(0xFF3A1A22), radius = size.width * 0.55f, center = Offset(size.width * 0.95f, size.height * 0.05f))
            }
            Row(Modifier.padding(20.dp), verticalAlignment = Alignment.CenterVertically) {
                Box(Modifier.size(40.dp).clip(CircleShape).background(V.Punane), Alignment.Center) { Ikoon(Ik.Kott, Color.White, 20.dp, 2.4f) }
                Spacer(Modifier.width(12.dp))
                T("AKT", 15.sp, Color.White, FontWeight.ExtraBold)
            }
            // Kaks kliendikaarti nurga all
            Box(Modifier.align(Alignment.Center).offset(30.dp, (-20).dp).rotate(-8f).size(190.dp, 120.dp).clip(RoundedCornerShape(18.dp))
                .background(Brush.linearGradient(listOf(V.Ploom, Color(0xFF6B1648)))))
            Box(Modifier.align(Alignment.Center).offset((-10).dp, 10.dp).rotate(-14f).size(190.dp, 120.dp).clip(RoundedCornerShape(18.dp))
                .background(Brush.linearGradient(listOf(V.Punane, Color(0xFFB00010)))).padding(14.dp)) {
                T("AKT  POOD  KLIENT", 8.sp, Color.White, FontWeight.ExtraBold)
                T("100001", 10.sp, Color.White, FontWeight.Bold, Modifier.align(Alignment.BottomStart))
                T("0,42 €", 13.sp, Color.White, FontWeight.ExtraBold, Modifier.align(Alignment.BottomEnd))
            }
            Column(Modifier.align(Alignment.BottomStart).padding(20.dp)) {
                T(Keel.t("Tere tulemast!"), 30.sp, Color.White, FontWeight.ExtraBold)
                Vahe(6.dp)
                T(Keel.t("Kliendikaardiga 1 € = 1 sent boonust."), 14.sp, Color(0xFFC9CCD1))
            }
        }
        Column(
            Modifier.fillMaxWidth().clip(RoundedCornerShape(topStart = 30.dp, topEnd = 30.dp)).background(V.Pind).padding(20.dp)
        ) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(Keel.t("Logi sisse"), 22.sp, V.Tekst, FontWeight.ExtraBold, Modifier.weight(1f))
                KeeleValik(olek)
            }
            Vahe(16.dp)
            Sisend(nimi, { nimi = it; viga = null }, Keel.t("Kasutajanimi"))
            Vahe(10.dp)
            Sisend(parool, { parool = it; viga = null }, Keel.t("Parool"), parool = !naita, taga = {
                Box(Modifier.size(32.dp).vajuta { naita = !naita }, Alignment.Center) { Ikoon(Ik.Silm, V.Tekst2, 18.dp) }
            })
            if (viga != null) { Vahe(8.dp); T(viga!!, 13.sp, V.Punane, FontWeight.SemiBold) }
            Vahe(16.dp)
            SuurNupp(Keel.t("Logi sisse"), {
                if (!olek.logiSisse(nimi, parool)) viga = Keel.t("Vale kasutajanimi või parool.")
            })
            Vahe(14.dp)
            Box(Modifier.fillMaxWidth().vajuta { olek.ava(Ekraan.Registreeru) }, Alignment.Center) {
                T(Keel.t("Pole kontot? Registreeru"), 14.sp, V.Ploom, FontWeight.Bold)
            }
            Vahe(6.dp)
            T(Keel.t("Proovi: mari / mari123 (klient) või admin / admin123 (omanik)"), 11.sp, V.Nork, modifier = Modifier.fillMaxWidth(), keskel = true)
        }
    }
}

@Composable
fun KeeleValik(olek: AppOlek) {
    Row(Modifier.clip(RoundedCornerShape(18.dp)).background(V.Paneel).padding(3.dp)) {
        for (k in Keel.Kood.entries) {
            val v = Keel.praegu == k
            Box(
                Modifier.size(38.dp, 30.dp).clip(RoundedCornerShape(15.dp)).background(if (v) V.Tume else Color.Transparent).vajuta { olek.seaKeel(k) },
                Alignment.Center
            ) { T(k.name, 11.sp, if (v) Color.White else V.Tekst, FontWeight.ExtraBold) }
        }
    }
}

@Composable
fun RegistreeruEkraan(olek: AppOlek) {
    var nimi by remember { mutableStateOf("") }
    var taisNimi by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var parool by remember { mutableStateOf("") }
    var viga by remember { mutableStateOf<String?>(null) }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Registreeru", { olek.tagasi() })
        Vahe(16.dp)
        Kaart {
            T(Keel.t("Uus kliendikaart"), 18.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(4.dp)
            T(Keel.t("Kaardiga saad iga ostuga boonust ja ligipääsu preemiatele."), 13.sp, V.Tekst2)
            Vahe(16.dp)
            Sisend(taisNimi, { taisNimi = it }, Keel.t("Ees- ja perekonnanimi"))
            Vahe(10.dp)
            Sisend(nimi, { nimi = it; viga = null }, Keel.t("Kasutajanimi"))
            Vahe(10.dp)
            Sisend(email, { email = it }, Keel.t("E-post (tšekkide jaoks)"))
            Vahe(10.dp)
            Sisend(parool, { parool = it; viga = null }, Keel.t("Parool (vähemalt 6 märki)"), parool = true)
            if (viga != null) { Vahe(8.dp); T(viga!!, 13.sp, V.Punane, FontWeight.SemiBold) }
            Vahe(16.dp)
            SuurNupp(Keel.t("Loo konto"), {
                val v = olek.pood.registreeri(nimi, taisNimi, parool, email.ifBlank { null })
                if (v != null) viga = Keel.t(v)
                else if (olek.logiSisse(nimi, parool)) olek.teade = Keel.t("Konto loodud – tere tulemast!")
            })
        }
    }
}

// ---------------------------------------------------------------
//  Avaleht (TelAvaleht)
// ---------------------------------------------------------------
@Composable
fun AvalehtEkraan(olek: AppOlek) {
    val k = olek.kasutaja
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 92.dp)) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Box(Modifier.vajuta { olek.vaheleht(Ekraan.Profiil) }) { Initsiaalid(k?.initsiaalid ?: "?") }
            Spacer(Modifier.width(12.dp))
            Column(Modifier.weight(1f)) {
                T(Keel.f("Tere, {0}!", k?.eesnimi ?: ""), 17.sp, V.Tekst, FontWeight.ExtraBold)
                T(Keel.f("Boonus {0}", eur(k?.boonus ?: 0)), 12.sp, V.Tekst2)
            }
            RingNupp(Ik.Nimekiri, { olek.vaheleht(Ekraan.Nimekiri) }, V.Pind, V.Tekst, 44.dp)
        }
        Vahe(12.dp)
        Row(
            Modifier.fillMaxWidth().height(54.dp).clip(RoundedCornerShape(27.dp)).background(V.Pind).vajuta { olek.ava(Ekraan.Otsing()) }.padding(horizontal = 8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(Modifier.size(38.dp).clip(CircleShape).background(V.Punane), Alignment.Center) { Ikoon(Ik.Otsi, Color.White, 17.dp, 2.4f) }
            Spacer(Modifier.width(12.dp))
            T(Keel.t("Otsi toodet…"), 15.sp, V.Nork)
        }
        Vahe(12.dp)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            Kiip(
                (if (olek.dieet.isEmpty()) "" else "✓ ") + Keel.t("Minu dieet"), V.Roheline, Color.White,
                onClick = { olek.ava(Ekraan.Dieet) }
            )
            Kiip(Keel.t("Kõik"), V.Pind, V.Tekst, valitud = true, onClick = { olek.ava(Ekraan.Ruudustik("Kõik tooted", vaade = "koik")) })
            for (kat in olek.kategooriad)
                Kiip(Keel.t(lyhike(kat.nimi)), V.Pind, V.Tekst, onClick = { olek.ava(Ekraan.Ruudustik(kat.nimi, kat.id)) })
        }
        Vahe(12.dp)
        HooajaRiba(olek)
        Vahe(12.dp)
        val tooted = olek.tooted.filter { Dieet.sobib(it, olek.dieet) }
        tooted.filter { it.laos }.maxByOrNull { it.soodusProtsent ?: 0 }?.let { NadalaPakkumine(olek, it) }
        Vahe(12.dp)
        TarneKaart(olek)
        Vahe(12.dp)
        RaiskamiseRiba(olek)
        if (Reeglid.OHTU_SEES && (Reeglid.ohtuKaib(olek.aeg()) || olek.aeg().hour >= 17)) {
            Vahe(12.dp)
            OhtuRiba(olek)
        }
        Vahe(18.dp)
        Row(verticalAlignment = Alignment.CenterVertically) {
            T(Keel.t("Parimad pakkumised"), 18.sp, V.Tekst, FontWeight.ExtraBold, Modifier.weight(1f))
            T(Keel.t("Kõik"), 14.sp, V.Ploom, FontWeight.Bold, Modifier.vajuta { olek.ava(Ekraan.Ruudustik("Kampaaniad", vaade = "kampaaniad")) })
        }
        Vahe(10.dp)
        Ruudud(olek, tooted.filter { it.soodusProtsent != null && it.laos }.sortedByDescending { it.soodusProtsent }.take(6))
    }
}

fun lyhike(kategooria: String): String = when {
    kategooria.startsWith("Piim") -> "Piim, munad"
    kategooria.startsWith("Leiv") -> "Leib ja sai"
    kategooria.startsWith("Puuv") -> "Puu- ja köögiviljad"
    kategooria.startsWith("Liha") -> "Liha ja kala"
    kategooria.startsWith("Maius") -> "Maiustused"
    kategooria.startsWith("Külm") -> "Külmutatud"
    else -> kategooria
}

@Composable
private fun NadalaPakkumine(olek: AppOlek, t: Toode) {
    val h = olek.hind(t)
    Box(Modifier.fillMaxWidth().height(170.dp).clip(RoundedCornerShape(26.dp)).background(V.Tume).vajuta { olek.ava(Ekraan.ToodeLeht(t.id)) }) {
        Canvas(Modifier.fillMaxSize()) {
            drawCircle(Color(0xFF4A1C24), radius = size.height * 0.95f, center = Offset(size.width * 0.92f, size.height * 0.1f))
        }
        Column(Modifier.padding(18.dp).fillMaxWidth(0.58f).fillMaxHeight()) {
            Row(
                Modifier.clip(RoundedCornerShape(12.dp)).background(V.Tume2).padding(horizontal = 10.dp, vertical = 5.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Box(Modifier.size(6.dp).clip(CircleShape).background(V.Punane))
                Spacer(Modifier.width(6.dp))
                T(Keel.t("NÄDALA PAKKUMINE"), 10.sp, Color.White, FontWeight.ExtraBold)
            }
            Vahe(10.dp)
            T(t.nimi, 18.sp, Color.White, FontWeight.ExtraBold, read = 2, reavahe = 21.sp)
            Spacer(Modifier.weight(1f))
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(eur(h.hind), 26.sp, Color.White, FontWeight.ExtraBold)
                t.soodusProtsent?.let { Spacer(Modifier.width(8.dp)); VaikeKiip("−$it%", V.Punane, Color.White) }
            }
        }
        Box(Modifier.align(Alignment.CenterEnd).padding(end = 16.dp).size(124.dp).clip(CircleShape).background(V.Pind), Alignment.Center) {
            ToodePilt(t.pilt, Modifier.size(92.dp))
        }
        RingNupp(Ik.Pluss, { olek.teade = olek.lisaKorvi(t) ?: Keel.f("„{0}“ lisatud ostukorvi", t.nimi) },
            modifier = Modifier.align(Alignment.BottomEnd).padding(12.dp))
    }
}

@Composable
private fun TarneKaart(olek: AppOlek) {
    val puudu = Reeglid.TASUTA_TARNE_ALATES - olek.kaubad
    Kaart(sisu = 14.dp) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            Ring(olek.kaubad / Reeglid.TASUTA_TARNE_ALATES.toFloat(), V.Roheline, Modifier.size(52.dp)) {
                T("${minOf(100, olek.kaubad * 100 / Reeglid.TASUTA_TARNE_ALATES)}%", 11.sp, V.Tekst, FontWeight.ExtraBold)
            }
            Spacer(Modifier.width(14.dp))
            Column {
                T(if (puudu > 0) Keel.f("Veel {0} tasuta tarneni", eur(puudu)) else Keel.t("Tarne on tasuta!"), 15.sp, V.Tekst, FontWeight.ExtraBold)
                T(Keel.f("Tasuta alates {0}", eur(Reeglid.TASUTA_TARNE_ALATES)), 12.sp, V.Tekst2)
            }
        }
    }
}

// "Ilma raiskamiseta": aeguvad tooted −30 %
@Composable
private fun RaiskamiseRiba(olek: AppOlek) {
    val arv = remember(olek.tooted) { olek.tooted.count { Raiskamine.aegub(it, olek.aeg().toLocalDate()) != null } }
    if (arv == 0) return
    Row(
        Modifier.fillMaxWidth().clip(RoundedCornerShape(22.dp)).background(V.Roheline)
            .vajuta { olek.ava(Ekraan.Ruudustik("Ilma raiskamiseta", vaade = "raiskamine")) }.padding(16.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Column(Modifier.weight(1f)) {
            T(Keel.t("ILMA RAISKAMISETA"), 10.sp, Color(0xFFD5EFDF), FontWeight.ExtraBold)
            T(Keel.f("{0} toodet −{1} %", arv, Raiskamine.PROTSENT), 17.sp, Color.White, FontWeight.ExtraBold)
            T(Keel.t("Säilivusaeg lõpeb peagi – päästa toit"), 12.sp, Color(0xFFD5EFDF))
        }
        Ikoon(Ik.Leht, Color.White, 34.dp, 2.2f)
    }
}

@Composable
private fun OhtuRiba(olek: AppOlek) {
    val kaib = Reeglid.ohtuKaib(olek.aeg())
    Row(
        Modifier.fillMaxWidth().clip(RoundedCornerShape(22.dp)).background(Color(0xFF3B2150)).vajuta { olek.ava(Ekraan.Ohtu) }.padding(16.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Column(Modifier.weight(1f)) {
            T(Keel.t(if (kaib) "KÄIB PRAEGU" else "TÄNA ÕHTUL"), 10.sp, Color(0xFFF5D76E), FontWeight.ExtraBold)
            T(Keel.t("Küpsetiste tund"), 17.sp, Color.White, FontWeight.ExtraBold)
            T(Keel.f("Pärast {0} kõik küpsetised −{1} %", Reeglid.OHTU_ALGUS.toString(), Reeglid.OHTU_PROTSENT), 12.sp, Color(0xFFD9CDE6))
        }
        Ikoon(Ik.Kuu, Color(0xFFF5D76E), 36.dp, 2.2f)
    }
}

// Kahe veeruga tootekaardid
@Composable
fun Ruudud(olek: AppOlek, tooted: List<Toode>) {
    Column(verticalArrangement = Arrangement.spacedBy(10.dp)) {
        for (rida in tooted.chunked(2)) {
            Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                for (t in rida) ToodeKaart(olek, t, Modifier.weight(1f))
                if (rida.size == 1) Spacer(Modifier.weight(1f))
            }
        }
    }
}

@Composable
fun ToodeKaart(olek: AppOlek, t: Toode, modifier: Modifier = Modifier) {
    val h = olek.hind(t)
    val sobib = Dieet.sobib(t, olek.dieet)
    Column(
        modifier.clip(RoundedCornerShape(22.dp)).background(V.Pind).vajuta { olek.ava(Ekraan.ToodeLeht(t.id)) }.padding(start = 10.dp, end = 10.dp, top = 8.dp, bottom = 10.dp)
    ) {
        Box(Modifier.fillMaxWidth().aspectRatio(1.15f)) {
            ToodePilt(t.pilt, Modifier.fillMaxSize().padding(6.dp))
            t.soodusProtsent?.let { VaikeKiip("−$it%", V.Punane, Color.White, Modifier.align(Alignment.TopStart)) }
            if (t.id in olek.lemmikud) Box(Modifier.align(Alignment.TopEnd)) { Ikoon(Ik.Syda, V.Ploom, 18.dp) }
            if (!sobib && olek.dieet.isNotEmpty()) VaikeKiip(Keel.t("Ei sobi"), V.Tume, Color.White, Modifier.align(Alignment.BottomStart))
            else Raiskamine.aegub(t, olek.aeg().toLocalDate())?.let { VaikeKiip(Raiskamine.tekst(it), V.Roheline, Color.White, Modifier.align(Alignment.BottomStart)) }
        }
        Vahe(6.dp)
        T(t.nimi, 13.sp, V.Tekst, FontWeight.Bold, read = 2, reavahe = 16.sp, modifier = Modifier.height(34.dp))
        Vahe(2.dp)
        T(t.uhikuHind?.let { "${eur(it)}/${t.uhik}" } ?: (t.pakend ?: ""), 11.sp, V.Nork, read = 1)
        Vahe(6.dp)
        Row(verticalAlignment = Alignment.CenterVertically) {
            Column(Modifier.weight(1f)) {
                T(eur(h.hind), 17.sp, if (h.hind < h.tavaHind) V.Punane else V.Tekst, FontWeight.ExtraBold)
                if (h.hind < h.tavaHind) T(eur(h.tavaHind), 11.sp, V.Nork, labi = true)
            }
            if (t.laos) RingNupp(Ik.Pluss, { olek.teade = olek.lisaKorvi(t) ?: Keel.f("„{0}“ lisatud ostukorvi", t.nimi) }, suurus = 36.dp)
            else VaikeKiip(Keel.t("Otsas"), V.Paneel, V.Nork)
        }
    }
}

// ---------------------------------------------------------------
//  Otsing (TelOtsing)
// ---------------------------------------------------------------
@Composable
fun OtsingEkraan(olek: AppOlek, algus: String) {
    var otsi by remember { mutableStateOf(algus) }
    val s = otsi.trim()
    val vasted = if (s.isEmpty()) emptyList() else olek.tooted.filter {
        it.nimi.contains(s, true) || (it.kaubamark ?: "").contains(s, true)
    }.sortedWith(compareByDescending<Toode> { it.nimi.startsWith(s, true) }.thenByDescending { it.laos }.thenBy { it.nimi })
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 92.dp)) {
        Row(verticalAlignment = Alignment.CenterVertically) {
            RingNupp(Ik.Vasak, { olek.tagasi() }, V.Pind, V.Tekst, 48.dp)
            Spacer(Modifier.width(10.dp))
            Box(Modifier.weight(1f).clip(RoundedCornerShape(27.dp)).background(V.Ploom).padding(2.dp)) {
                Sisend(otsi, { otsi = it }, Keel.t("Otsi toodet…"), taust = V.Pind, taga = {
                    if (otsi.isNotEmpty()) RingNupp(Ik.Sulge, { otsi = "" }, V.Paneel, V.Tekst, 34.dp)
                })
            }
        }
        Vahe(14.dp)
        if (s.isEmpty()) {
            T(Keel.t("Vaata kategooriaid"), 16.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(10.dp)
            Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                for (kat in olek.kategooriad) Kiip(Keel.t(lyhike(kat.nimi)), V.Pind, V.Tekst, onClick = { olek.ava(Ekraan.Ruudustik(kat.nimi, kat.id)) })
            }
            Vahe(18.dp)
            T(Keel.t("Teised otsivad"), 16.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(10.dp)
            Kaart(sisu = 6.dp) { for (t in olek.tooted.filter { it.laos }.sortedByDescending { it.soodusProtsent ?: 0 }.take(5)) OtsinguRida(olek, t, "") }
        } else if (vasted.isEmpty()) {
            Kaart {
                Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally) {
                    Box(Modifier.size(90.dp).clip(CircleShape).background(V.Tume), Alignment.Center) { Ikoon(Ik.Otsi, Color.White, 38.dp, 2.2f) }
                    Vahe(14.dp)
                    T(Keel.f("„{0}“ – ei leidnud", s), 19.sp, V.Tekst, FontWeight.ExtraBold, keskel = true)
                    Vahe(6.dp)
                    T(Keel.t("Kontrolli kirjapilti või proovi lühemat sõna."), 13.sp, V.Tekst2, keskel = true)
                }
            }
        } else {
            T(Keel.f("{0} toodet", vasted.size), 13.sp, V.Tekst2, FontWeight.SemiBold)
            Vahe(8.dp)
            Kaart(sisu = 6.dp) { for (t in vasted.take(40)) OtsinguRida(olek, t, s) }
        }
    }
}

@Composable
private fun OtsinguRida(olek: AppOlek, t: Toode, otsi: String) {
    val h = olek.hind(t)
    Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(16.dp)).vajuta { olek.ava(Ekraan.ToodeLeht(t.id)) }.padding(8.dp), verticalAlignment = Alignment.CenterVertically) {
        Box(Modifier.size(52.dp).clip(RoundedCornerShape(14.dp)).background(V.Paneel), Alignment.Center) { ToodePilt(t.pilt, Modifier.size(44.dp), 22.dp) }
        Spacer(Modifier.width(12.dp))
        Column(Modifier.weight(1f)) {
            T(t.nimi, 14.sp, V.Tekst, FontWeight.Bold, read = 1)
            T(
                listOfNotNull(t.kaubamark, t.uhikuHind?.let { "${eur(it)}/${t.uhik}" }).joinToString(" · ").ifEmpty { t.pakend ?: "" },
                12.sp, if (t.laos) V.Nork else V.Punane, read = 1
            )
        }
        Spacer(Modifier.width(8.dp))
        T(eur(h.hind), 15.sp, if (h.hind < h.tavaHind) V.Punane else V.Tekst, FontWeight.ExtraBold)
        Spacer(Modifier.width(8.dp))
        if (t.laos) RingNupp(Ik.Pluss, { olek.teade = olek.lisaKorvi(t) ?: Keel.f("„{0}“ lisatud ostukorvi", t.nimi) }, suurus = 34.dp)
    }
}

// ---------------------------------------------------------------
//  Kategooria / kampaaniad / lemmikud ruudustikuna
// ---------------------------------------------------------------
@Composable
fun RuudustikEkraan(olek: AppOlek, e: Ekraan.Ruudustik) {
    var jarjestus by remember { mutableIntStateOf(0) }
    var naita by remember { mutableIntStateOf(40) }   // suur kataloog: 40 kaupa
    val alus = when (e.vaade) {
        "kampaaniad" -> olek.tooted.filter { it.soodusProtsent != null }
        "lemmikud" -> olek.tooted.filter { it.id in olek.lemmikud }
        "koik" -> olek.tooted
        "raiskamine" -> olek.tooted.filter { Raiskamine.aegub(it, olek.aeg().toLocalDate()) != null }
        else -> olek.tooted.filter { it.kategooriaId == e.kategooriaId }
    }.filter { Dieet.sobib(it, olek.dieet) }
    val tooted = when (jarjestus) {
        1 -> alus.sortedBy { olek.hind(it).hind }
        2 -> alus.sortedWith(compareBy<Toode> { it.uhikuHind == null }.thenBy { it.uhik ?: "" }.thenBy { it.uhikuHind ?: it.hind })
        3 -> alus.sortedByDescending { it.soodusProtsent ?: 0 }
        else -> alus.sortedBy { it.nimi }
    }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 92.dp)) {
        Pais(lyhike(e.pealkiri), { olek.tagasi() }, Keel.f("{0} toodet", tooted.size))
        Vahe(10.dp)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            listOf("Nimi A–Z", "Odavam enne", "Kilohind", "Suurim soodustus").forEachIndexed { i, s ->
                Kiip(Keel.t(s), V.Pind, V.Tekst, valitud = jarjestus == i, onClick = { jarjestus = i })
            }
        }
        Vahe(12.dp)
        if (tooted.isEmpty()) Tyhi(Ik.Kott, Keel.t("Siin pole veel tooteid"), Keel.t(if (e.vaade == "lemmikud") "Märgi toode südamega, et see siia lisada." else "Proovi teist kategooriat või muuda dieeti."))
        else {
            Ruudud(olek, tooted.take(naita))
            if (tooted.size > naita) {
                Vahe(12.dp)
                AaarisNupp(Keel.f("Näita rohkem ({0} veel)", tooted.size - naita), { naita += 40 }, Modifier.fillMaxWidth())
            }
        }
    }
}

// ---------------------------------------------------------------
//  Toote leht (TelToode): tume ülaosa, ümar pilt, valge leht all
// ---------------------------------------------------------------
@Composable
fun ToodeEkraan(olek: AppOlek, id: Int) {
    val t = olek.toode(id) ?: run { Tyhi(Ik.Kott, Keel.t("Toodet ei leitud"), ""); return }
    val h = olek.hind(t)
    var kogus by remember { mutableIntStateOf(1) }
    val lemmik = t.id in olek.lemmikud
    Box(Modifier.fillMaxSize().background(V.Tume)) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState())) {
            Box(Modifier.fillMaxWidth().height(300.dp)) {
                Box(Modifier.align(Alignment.Center).size(230.dp).clip(CircleShape).background(V.Pind), Alignment.Center) {
                    ToodePilt(t.pilt, Modifier.size(170.dp), 60.dp)
                }
                RingNupp(Ik.Vasak, { olek.tagasi() }, V.Tume2, Color.White, 44.dp, Modifier.padding(16.dp))
                Row(Modifier.align(Alignment.TopEnd).padding(16.dp), verticalAlignment = Alignment.CenterVertically) {
                    t.soodusProtsent?.let { VaikeKiip("−$it%", V.Punane, Color.White); Spacer(Modifier.width(8.dp)) }
                    Box(Modifier.size(40.dp).clip(RoundedCornerShape(12.dp)).background(V.Pind).vajuta { olek.vahetaLemmik(t.id) }, Alignment.Center) {
                        Ikoon(Ik.Syda, if (lemmik) V.Ploom else V.Tekst, 20.dp, if (lemmik) 3.2f else 2f)
                    }
                }
            }
            Column(
                Modifier.fillMaxWidth().clip(RoundedCornerShape(topStart = 30.dp, topEnd = 30.dp)).background(V.Pind).padding(20.dp).padding(bottom = 90.dp)
            ) {
                t.kaubamark?.let { T(it, 12.sp, V.Ploom, FontWeight.ExtraBold) }
                T(t.nimi, 22.sp, V.Tekst, FontWeight.ExtraBold, reavahe = 27.sp)
                Vahe(10.dp)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(eur(h.hind), 30.sp, if (h.hind < h.tavaHind) V.Punane else V.Tekst, FontWeight.ExtraBold)
                    Spacer(Modifier.width(10.dp))
                    Column(Modifier.weight(1f)) {
                        if (h.hind < h.tavaHind) T(eur(h.tavaHind), 12.sp, V.Nork, labi = true)
                        t.uhikuHind?.let { T("${eur(it)}/${t.uhik}", 12.sp, V.Tekst2) }
                    }
                    VaikeKiip(
                        if (t.laos) "● " + Keel.t("Laos") else Keel.t("Otsas"),
                        if (t.laos) V.RohelineHele else V.Roosa, if (t.laos) V.Roheline else V.Punane
                    )
                }
                if (h.soodus != null) { Vahe(6.dp); T(Keel.t(h.soodus), 12.sp, V.Ploom, FontWeight.Bold) }
                val margid = Dieet.margid(t)
                if (margid.isNotEmpty()) {
                    Vahe(12.dp)
                    Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) { for (m in margid) VaikeKiip("✓ " + Keel.t(m.nimi), V.RohelineHele, V.Roheline) }
                }
                Raiskamine.aegub(t, olek.aeg().toLocalDate())?.let { Vahe(8.dp); VaikeKiip(Raiskamine.tekst(it), V.RohelineHele, V.Roheline) }
                if (olek.pood.seade("pere") != null) {
                    Vahe(10.dp)
                    T("+ " + Keel.t("Lisa pere korvi"), 14.sp, V.Ploom, FontWeight.Bold, Modifier.vajuta {
                        PereKorv.lisa(olek.pood, PereKorv.loe(olek.pood), t.id, kogus, olek.kasutaja?.eesnimi ?: "Mina", olek.aeg())
                        olek.teade = Keel.f("„{0}“ lisatud pere korvi", t.nimi)
                    })
                }
                SarnasedJaSoodsamad(olek, t)
                SageliKoos(olek, t)
                t.kirjeldus?.takeIf { it.isNotBlank() }?.let { Vahe(14.dp); T(it, 14.sp, V.Tekst2, reavahe = 20.sp) }
                t.koostis?.takeIf { it.isNotBlank() }?.let {
                    Vahe(14.dp)
                    T(Keel.t("Koostis"), 14.sp, V.Tekst, FontWeight.ExtraBold)
                    Vahe(4.dp)
                    T(it, 13.sp, V.Tekst2, reavahe = 19.sp)
                }
                val allergeenid = Dieet.allergeenid(t).filter { it != "liha" }
                if (allergeenid.isNotEmpty()) {
                    Vahe(10.dp)
                    VaikeKiip(Keel.t("Allergeen") + ": " + allergeenid.joinToString(", ") { Keel.t(it) }, V.Kreem, V.Oranz)
                }
                val tv = Toitevaartus.read(t)
                if (tv.isNotEmpty()) {
                    Vahe(16.dp)
                    T(Keel.t("Toitumisalane teave (100 g)"), 14.sp, V.Tekst, FontWeight.ExtraBold)
                    Vahe(6.dp)
                    for (r in tv) Row(Modifier.fillMaxWidth().padding(vertical = 3.dp, horizontal = if (r.alam) 12.dp else 0.dp)) {
                        T(Keel.t(r.nimi), 13.sp, if (r.alam) V.Nork else V.Tekst2, modifier = Modifier.weight(1f))
                        T(r.vaartus, 13.sp, V.Tekst, FontWeight.Bold)
                    }
                }
                val info = listOfNotNull(t.pakend?.let { Keel.t("Pakend") to it }, t.paritolumaa?.let { Keel.t("Päritolumaa") to it }, t.tootja?.let { Keel.t("Tootja") to it })
                if (info.isNotEmpty()) {
                    Vahe(14.dp)
                    for ((a, b) in info) ReaSumma(a, b, suurus = 13.sp)
                }
            }
        }
        if (t.laos) Row(
            Modifier.align(Alignment.BottomCenter).fillMaxWidth().background(V.Pind).padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            KogusValija(kogus, { if (kogus > 1) kogus-- }, { if (kogus < t.kogus - olek.korvis(t.id)) kogus++ }, Modifier.height(56.dp))
            Spacer(Modifier.width(10.dp))
            SuurNupp(Keel.f("Lisa · {0}", eur(h.hind * kogus)), {
                olek.teade = olek.lisaKorvi(t, kogus) ?: Keel.f("„{0}“ lisatud ostukorvi", t.nimi)
                kogus = 1
            }, Modifier.weight(1f), ikoon = Ik.Pluss)
        }
    }
}
