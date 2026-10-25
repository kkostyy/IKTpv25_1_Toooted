package ee.akt.pood

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
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.Slider
import androidx.compose.material3.SliderDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableFloatStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import java.time.LocalDate
import java.time.format.DateTimeFormatter

// ---------------------------------------------------------------
//  Haldus (TelAdmin): müük täna, vähe laos, toodete nimekiri, "+" uus toode
// ---------------------------------------------------------------
@Composable
fun AdminEkraan(olek: AppOlek) {
    var otsi by remember { mutableStateOf("") }
    val tana = remember { olek.pood.ostud(null).filter { !it.tagastatud && it.aeg.toLocalDate() == LocalDate.now() }.sumOf { it.summa } }
    val vahe = olek.tooted.count { it.kogus in 0..9 }
    val tooted = olek.tooted.filter { otsi.isBlank() || it.nimi.contains(otsi.trim(), true) }
    Box(Modifier.fillMaxSize()) {
        LazyColumn(Modifier.fillMaxSize().padding(horizontal = 16.dp), contentPadding = androidx.compose.foundation.layout.PaddingValues(top = 16.dp, bottom = 100.dp)) {
            item {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    RingNupp(Ik.Valja, { olek.logiValja() }, V.Pind, V.Tekst, 48.dp)
                    Spacer(Modifier.width(12.dp))
                    Column(Modifier.weight(1f)) {
                        T(Keel.t("Tooted"), 24.sp, V.Tekst, FontWeight.ExtraBold)
                        T(Keel.t("Haldus") + " · " + (olek.kasutaja?.nimi ?: ""), 12.sp, V.Nork)
                    }
                    RingNupp(Ik.Kott, { olek.vaheleht(Ekraan.Avaleht) }, V.Punane, Color.White, 44.dp)
                    Spacer(Modifier.width(8.dp))
                    RingNupp(Ik.Silt, { olek.ava(Ekraan.Kampaaniad) }, V.Tume, Color.White, 44.dp)
                }
                Vahe(14.dp)
                Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                    Column(Modifier.weight(1f).clip(RoundedCornerShape(22.dp)).background(V.Tume).padding(14.dp)) {
                        T(Keel.t("Müük täna"), 12.sp, Color(0xFFC9CCD1))
                        T(eur(tana), 22.sp, Color.White, FontWeight.ExtraBold)
                    }
                    Column(Modifier.weight(1f).clip(RoundedCornerShape(22.dp)).background(V.Kreem).padding(14.dp)) {
                        T("● " + Keel.t("Alla 10 tk"), 12.sp, V.Oranz, FontWeight.Bold)
                        T(Keel.f("{0} toodet", vahe), 22.sp, V.Oranz, FontWeight.ExtraBold)
                    }
                }
                Vahe(12.dp)
                Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    for ((tekst, siht) in listOf("Müügipaneel" to Ekraan.Myygipaneel, "Autotellimus" to Ekraan.Autotellimus, "Ostetakse koos" to Ekraan.Koos,
                        "Kliendid" to Ekraan.Kliendid, "Kampaaniad" to Ekraan.Kampaaniad, "Seaded" to Ekraan.Seaded))
                        Kiip(Keel.t(tekst), V.Pind, V.Tekst, onClick = { olek.ava(siht) })
                }
                Vahe(12.dp)
                Sisend(otsi, { otsi = it }, Keel.t("Otsi toodet"), taust = V.Pind, ees = { Ikoon(Ik.Otsi, V.Tekst2, 18.dp) })
                Vahe(10.dp)
                T(Keel.f("{0} toodet", tooted.size), 12.sp, V.Nork)
                Vahe(6.dp)
            }
            items(tooted.take(300), key = { it.id }) { t ->
                Row(
                    Modifier.fillMaxWidth().padding(bottom = 8.dp).clip(RoundedCornerShape(18.dp)).background(V.Pind)
                        .vajuta { olek.ava(Ekraan.ToodeMuuda(t.id)) }.padding(10.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Box(Modifier.size(48.dp).clip(RoundedCornerShape(12.dp)).background(V.Paneel), Alignment.Center) { ToodePilt(t.pilt, Modifier.size(40.dp), 20.dp) }
                    Spacer(Modifier.width(10.dp))
                    Column(Modifier.weight(1f)) {
                        T(t.nimi, 14.sp, V.Tekst, FontWeight.Bold, read = 1)
                        T(Keel.t(olek.kategooriad.firstOrNull { it.id == t.kategooriaId }?.nimi ?: ""), 11.sp, V.Nork, read = 1)
                    }
                    Spacer(Modifier.width(8.dp))
                    Column(horizontalAlignment = Alignment.End) {
                        T(eur(t.hind), 14.sp, V.Tekst, FontWeight.ExtraBold)
                        T(Keel.f("{0} tk", t.kogus), 11.sp, if (t.kogus < 10) V.Oranz else V.Nork, FontWeight.Bold)
                    }
                }
            }
        }
        RingNupp(Ik.Pluss, { olek.ava(Ekraan.ToodeMuuda(null)) }, V.Punane, Color.White, 60.dp, Modifier.align(Alignment.BottomEnd).padding(20.dp))
    }
}

// ---------------------------------------------------------------
//  Toote lisamine / muutmine
// ---------------------------------------------------------------
@Composable
fun ToodeMuudaEkraan(olek: AppOlek, id: Int?) {
    val vana = id?.let { olek.toode(it) }
    var nimi by remember { mutableStateOf(vana?.nimi ?: "") }
    var hind by remember { mutableStateOf(vana?.let { "%.2f".format(it.hind / 100.0) } ?: "") }
    var kogus by remember { mutableStateOf(vana?.kogus?.toString() ?: "") }
    var kat by remember { mutableStateOf(vana?.kategooriaId ?: 1) }
    var pakend by remember { mutableStateOf(vana?.pakend ?: "") }
    var viga by remember { mutableStateOf<String?>(null) }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais(if (vana == null) "Uus toode" else "Muuda toodet", { olek.tagasi() })
        Vahe(12.dp)
        Kaart {
            if (vana != null) {
                Box(Modifier.fillMaxWidth().height(140.dp), Alignment.Center) { ToodePilt(vana.pilt, Modifier.size(130.dp), 50.dp) }
                Vahe(10.dp)
            }
            T(Keel.t("Nimetus"), 12.sp, V.Tekst2, FontWeight.Bold); Vahe(4.dp)
            Sisend(nimi, { nimi = it; viga = null }, Keel.t("nt Piim 2,5% 1l"))
            Vahe(10.dp)
            Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                Column(Modifier.weight(1f)) {
                    T(Keel.t("Hind (€)"), 12.sp, V.Tekst2, FontWeight.Bold); Vahe(4.dp)
                    Sisend(hind, { hind = it; viga = null }, "1,99", number = true)
                }
                Column(Modifier.weight(1f)) {
                    T(Keel.t("Laos (tk)"), 12.sp, V.Tekst2, FontWeight.Bold); Vahe(4.dp)
                    Sisend(kogus, { kogus = it.filter(Char::isDigit); viga = null }, "10", number = true)
                }
            }
            Vahe(10.dp)
            T(Keel.t("Pakend"), 12.sp, V.Tekst2, FontWeight.Bold); Vahe(4.dp)
            Sisend(pakend, { pakend = it }, "0.5 kg")
            Vahe(10.dp)
            T(Keel.t("Kategooria"), 12.sp, V.Tekst2, FontWeight.Bold); Vahe(6.dp)
            Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                for (k in olek.kategooriad) Kiip(Keel.t(lyhike(k.nimi)), V.Paneel, V.Tekst, valitud = kat == k.id, onClick = { kat = k.id })
            }
            if (viga != null) { Vahe(10.dp); T(viga!!, 13.sp, V.Punane, FontWeight.Bold) }
            Vahe(16.dp)
            SuurNupp(Keel.t("Salvesta"), {
                val h = hind.replace(',', '.').toDoubleOrNull()
                val k = kogus.toIntOrNull()
                when {
                    nimi.isBlank() -> viga = Keel.t("Sisesta nimetus.")
                    h == null || h <= 0 -> viga = Keel.t("Hind peab olema positiivne arv.")
                    k == null -> viga = Keel.t("Laoseis peab olema täisarv.")
                    else -> {
                        val alus = vana ?: Toode(0, "", 0, 0)
                        olek.pood.salvestaToode(alus.copy(nimi = nimi.trim(), hind = Math.round(h * 100).toInt(), kogus = k, kategooriaId = kat, pakend = pakend.ifBlank { null }))
                        olek.varskendaTooted()
                        olek.teade = Keel.t("Toode salvestatud")
                        olek.tagasi()
                    }
                }
            }, noolega = false)
            if (vana != null) {
                Vahe(10.dp)
                AaarisNupp(Keel.t("Kustuta toode"), {
                    olek.pood.kustutaToode(vana.id); olek.varskendaTooted(); olek.teade = Keel.t("Toode kustutatud"); olek.tagasi()
                }, Modifier.fillMaxWidth(), V.Punane)
            }
        }
        Vahe(20.dp)
    }
}

// ---------------------------------------------------------------
//  Kampaaniad (TelUusKampaaniad): nimekiri + uus kampaania liuguriga
// ---------------------------------------------------------------
@Composable
fun KampaaniadEkraan(olek: AppOlek) {
    var kampaaniad by remember { mutableStateOf(olek.pood.kampaaniad()) }
    var valitud by remember { mutableStateOf<Toode?>(null) }
    var protsent by remember { mutableFloatStateOf(20f) }
    var otsi by remember { mutableStateOf("") }
    val f = DateTimeFormatter.ofPattern("dd.MM")
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Kampaaniad", { olek.tagasi() })
        Vahe(12.dp)
        Kaart {
            T(Keel.t("Uus kampaania"), 18.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(10.dp)
            val t = valitud
            if (t == null) {
                Sisend(otsi, { otsi = it }, Keel.t("Otsi toodet"), ees = { Ikoon(Ik.Otsi, V.Tekst2, 18.dp) })
                if (otsi.length >= 2) for (x in olek.tooted.filter { it.nimi.contains(otsi, true) }.take(6))
                    Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(12.dp)).vajuta { valitud = x }.padding(10.dp)) {
                        T(x.nimi, 14.sp, V.Tekst, FontWeight.SemiBold, Modifier.weight(1f), read = 1); T(eur(x.hind), 14.sp, V.Tekst2)
                    }
            } else {
                Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(16.dp)).background(V.Paneel).padding(10.dp), verticalAlignment = Alignment.CenterVertically) {
                    ToodePilt(t.pilt, Modifier.size(36.dp), 18.dp)
                    Spacer(Modifier.width(10.dp))
                    T(t.nimi, 14.sp, V.Tekst, FontWeight.Bold, Modifier.weight(1f), read = 2)
                    Box(Modifier.size(30.dp).vajuta { valitud = null }, Alignment.Center) { Ikoon(Ik.Sulge, V.Nork, 16.dp) }
                }
                Vahe(14.dp)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(Keel.t("Soodustus"), 13.sp, V.Tekst2, FontWeight.Bold, Modifier.weight(1f))
                    T("−${protsent.toInt()}%", 30.sp, V.Punane, FontWeight.ExtraBold)
                }
                Slider(protsent, { protsent = it }, valueRange = 5f..70f, colors = SliderDefaults.colors(thumbColor = V.Punane, activeTrackColor = V.Punane))
                val tava = Reeglid.tavaHind(t)
                Column(Modifier.fillMaxWidth().clip(RoundedCornerShape(18.dp)).background(V.Tume).padding(14.dp)) {
                    T(LocalDate.now().format(f) + " – " + LocalDate.now().plusDays(7).format(f), 12.sp, Color(0xFFC9CCD1))
                    Row(verticalAlignment = Alignment.Bottom) {
                        T(eur(Reeglid.soodusHind(tava, protsent.toInt())), 28.sp, Color.White, FontWeight.ExtraBold)
                        Spacer(Modifier.width(8.dp))
                        T(eur(tava), 13.sp, Color(0xFF9CA1A8), labi = true)
                    }
                }
                Vahe(12.dp)
                SuurNupp(Keel.t("Salvesta kampaania"), {
                    olek.pood.lisaKampaania(Kampaania(0, t.id, protsent.toInt(), LocalDate.now(), LocalDate.now().plusDays(7)))
                    olek.varskendaTooted()
                    kampaaniad = olek.pood.kampaaniad()
                    valitud = null; otsi = ""
                    olek.teade = Keel.t("Kampaania salvestatud")
                }, noolega = false)
            }
        }
        Vahe(14.dp)
        T(Keel.t("Kampaaniad"), 16.sp, V.Tekst, FontWeight.ExtraBold)
        Vahe(8.dp)
        if (kampaaniad.isEmpty()) T(Keel.t("Kampaaniaid veel pole."), 13.sp, V.Nork)
        for (k in kampaaniad) {
            val t = olek.toode(k.toodeId)
            val kaib = !LocalDate.now().isBefore(k.algus) && !LocalDate.now().isAfter(k.lopp)
            Row(Modifier.fillMaxWidth().padding(bottom = 8.dp).clip(RoundedCornerShape(18.dp)).background(V.Pind).padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                Box(Modifier.size(44.dp).clip(CircleShape).background(if (kaib) V.Punane else V.Joon), Alignment.Center) { T("−${k.protsent}%", 11.sp, Color.White, FontWeight.ExtraBold) }
                Spacer(Modifier.width(10.dp))
                Column(Modifier.weight(1f)) {
                    T(t?.nimi ?: "#${k.toodeId}", 14.sp, V.Tekst, FontWeight.Bold, read = 1)
                    T(k.algus.format(f) + " – " + k.lopp.format(f), 12.sp, V.Nork)
                }
                VaikeKiip(Keel.t(if (kaib) "Käib" else if (LocalDate.now().isBefore(k.algus)) "Tulekul" else "Lõppenud"),
                    if (kaib) V.RohelineHele else V.Paneel, if (kaib) V.Roheline else V.Nork)
            }
        }
        Vahe(20.dp)
    }
}
