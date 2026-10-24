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
import java.time.LocalDate
import java.time.format.DateTimeFormatter

// ---------------------------------------------------------------
//  Kliendikaart (TelKaart): punane kaart, triipkood, boonus, preemiate nupp
// ---------------------------------------------------------------
@Composable
fun KaartEkraan(olek: AppOlek) {
    val k = olek.kasutaja ?: return
    val tase = Reeglid.taseSummast(olek.aastaSumma())
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 92.dp)) {
        Pais("Kliendikaart", null) { VaikeKiip(Keel.t(tase.tase.nimi), V.Roosa, V.Ploom) }
        Vahe(14.dp)
        Box(
            Modifier.fillMaxWidth().aspectRatio(1.6f).clip(RoundedCornerShape(26.dp))
                .background(Brush.linearGradient(listOf(V.Punane, Color(0xFF9E0012))))
        ) {
            Canvas(Modifier.fillMaxSize()) {
                drawCircle(Color.White.copy(alpha = 0.08f), size.width * 0.5f, Offset(size.width * 1.0f, size.height * 0.1f))
                drawCircle(Color.White.copy(alpha = 0.06f), size.width * 0.3f, Offset(size.width * 0.1f, size.height * 1.05f))
            }
            Column(Modifier.padding(20.dp).fillMaxSize()) {
                T("AKT   POOD   KLIENT", 11.sp, Color.White, FontWeight.ExtraBold)
                Vahe(4.dp)
                T(k.taisNimi.uppercase(), 13.sp, Color.White.copy(alpha = 0.85f), FontWeight.Bold)
                Spacer(Modifier.weight(1f))
                Row(verticalAlignment = Alignment.Bottom) {
                    T(k.kaart, 26.sp, Color.White, FontWeight.ExtraBold, Modifier.weight(1f))
                    Column(horizontalAlignment = Alignment.End) {
                        T(Keel.t("BOONUS"), 10.sp, Color.White.copy(alpha = 0.8f), FontWeight.ExtraBold)
                        T(eur(k.boonus), 26.sp, Color.White, FontWeight.ExtraBold)
                    }
                }
            }
        }
        Vahe(14.dp)
        // Triipkood kassas skannimiseks
        Kaart(sisu = 18.dp) {
            Canvas(Modifier.fillMaxWidth().height(70.dp)) {
                var x = 0f
                val kood = k.kaart.padStart(6, '0') + "4740"
                var i = 0
                while (x < size.width) {
                    val laius = (((kood[i % kood.length].code + i) % 3) + 1) * 2.4f
                    if (i % 2 == 0) drawRect(Color(0xFF1B1C1F), Offset(x, 0f), androidx.compose.ui.geometry.Size(laius, size.height))
                    x += laius + 1.6f
                    i++
                }
            }
            Vahe(8.dp)
            T(k.kaart.toCharArray().joinToString("  "), 15.sp, V.Tekst, FontWeight.Bold, Modifier.fillMaxWidth(), keskel = true)
        }
        Vahe(12.dp)
        Row(
            Modifier.fillMaxWidth().clip(RoundedCornerShape(30.dp)).background(V.Ploom).vajuta { olek.ava(Ekraan.Preemiad()) }.padding(10.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(Modifier.size(42.dp).clip(CircleShape).background(Color.White.copy(alpha = 0.18f)), Alignment.Center) { Ikoon(Ik.Taht, Color.White, 20.dp) }
            Spacer(Modifier.width(12.dp))
            Column(Modifier.weight(1f)) {
                T(Keel.t("Preemiad") + " · " + Keel.t(tase.tase.nimi), 15.sp, Color.White, FontWeight.ExtraBold)
                T(Keel.t("Tase, ülesanded, kleepsud, üllatus"), 12.sp, Color.White.copy(alpha = 0.8f))
            }
            Ikoon(Ik.Parem, Color.White, 18.dp)
        }
        Vahe(12.dp)
        Kaart(sisu = 14.dp) {
            ReaSumma(Keel.t("Ostud viimase 12 kuu jooksul"), eur(tase.summa))
            ReaSumma(Keel.t("Boonus igalt eurolt"), "${tase.tase.promill / 10.0} %".replace('.', ','))
            if (tase.jargmine != null) ReaSumma(Keel.f("{0} tasemeni", Keel.t(tase.jargmine.nimi)), eur(tase.jargmiseni), V.Ploom, true)
        }
    }
}

// ---------------------------------------------------------------
//  Preemiad: tase (TelUusTase), väljakutsed, kleepsud, üllatus
// ---------------------------------------------------------------
@Composable
fun PreemiadEkraan(olek: AppOlek, algus: Int) {
    var leht by remember { mutableIntStateOf(algus) }
    val tase = Reeglid.taseSummast(olek.aastaSumma())
    val ostud = remember { olek.pood.ostud(olek.kasutaja?.nimi).filter { !it.tagastatud } }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Preemiad", { olek.tagasi() })
        Vahe(10.dp)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            listOf("Tase", "Väljakutsed", "Kleepsud", "Üllatus").forEachIndexed { i, s -> Kiip(Keel.t(s), V.Pind, V.Tekst, valitud = leht == i, onClick = { leht = i }) }
        }
        Vahe(14.dp)
        when (leht) {
            0 -> {
                Column(Modifier.fillMaxWidth().clip(RoundedCornerShape(28.dp)).background(V.Tume).padding(24.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                    val osa = if (tase.jargmine == null) 1f else (tase.summa - tase.tase.alates).toFloat() / (tase.jargmine.alates - tase.tase.alates)
                    Ring(osa, Color.White, Modifier.size(150.dp), 12.dp, V.Tume2) {
                        Column(horizontalAlignment = Alignment.CenterHorizontally) {
                            Box(Modifier.size(48.dp).clip(CircleShape).background(V.Tume2), Alignment.Center) { Ikoon(Ik.Taht, Color.White, 22.dp) }
                            Vahe(6.dp)
                            T(Keel.t(tase.tase.nimi), 20.sp, Color.White, FontWeight.ExtraBold)
                        }
                    }
                    Vahe(16.dp)
                    T(
                        if (tase.jargmine != null) Keel.f("{0}-tasemeni veel {1} oste", Keel.t(tase.jargmine.nimi), eur(tase.jargmiseni)) else Keel.t("Sul on kõrgeim tase!"),
                        14.sp, Color.White, keskel = true
                    )
                    Vahe(10.dp)
                    VaikeKiip(Keel.f("Boonus {0} % igalt eurolt", "${tase.tase.promill / 10.0}".replace('.', ',')), V.Punane, Color.White)
                }
                Vahe(14.dp)
                for (t in Reeglid.Tase.entries) Kaart(Modifier.fillMaxWidth().padding(bottom = 8.dp), sisu = 14.dp) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        T(Keel.t(t.nimi), 15.sp, if (t == tase.tase) V.Ploom else V.Tekst, FontWeight.ExtraBold, Modifier.weight(1f))
                        T(Keel.f("alates {0}", eur(t.alates)), 12.sp, V.Tekst2)
                        Spacer(Modifier.width(10.dp))
                        T("${t.promill / 10.0} %".replace('.', ','), 15.sp, V.Tekst, FontWeight.ExtraBold)
                    }
                }
                T(Keel.t("Tase arvutatakse viimase 12 kuu ostudest."), 12.sp, V.Nork, Modifier.fillMaxWidth(), keskel = true)
            }
            1 -> {
                val nadal = ostud.filter { Reeglid.nadalaKood(it.aeg.toLocalDate()) == Reeglid.nadalaKood(LocalDate.now()) }
                val ylesanded = listOf(
                    Triple("Tee 3 ostu", nadal.size, 3),
                    Triple("Osta 5 erinevat puuvilja või köögivilja", nadal.flatMap { it.read }.mapNotNull { r -> r.toodeId?.let { olek.toode(it) } }.filter { it.kategooriaId == 3 }.distinctBy { it.id }.size, 5),
                    Triple("Kuluta nädalas 50 €", nadal.sumOf { it.summa } / 100, 50),
                )
                T(Keel.t("Nädala ülesanded"), 18.sp, V.Tekst, FontWeight.ExtraBold)
                Vahe(10.dp)
                for ((tekst, tehtud, vaja) in ylesanded) {
                    Kaart(Modifier.fillMaxWidth().padding(bottom = 8.dp), sisu = 14.dp) {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Column(Modifier.weight(1f)) {
                                T(Keel.t(tekst), 14.sp, V.Tekst, FontWeight.Bold)
                                Vahe(6.dp)
                                Riba(tehtud / vaja.toFloat(), if (tehtud >= vaja) V.Roheline else V.Ploom)
                            }
                            Spacer(Modifier.width(12.dp))
                            if (tehtud >= vaja) VaikeKiip(Keel.t("TEHTUD"), V.RohelineHele, V.Roheline) else T("${minOf(tehtud, vaja)}/$vaja", 14.sp, V.Tekst2, FontWeight.Bold)
                        }
                    }
                }
            }
            2 -> {
                val kleepsud = listOf(
                    Triple("Esimene ost", "Tegid esimese ostu", ostud.isNotEmpty()),
                    Triple("Püsiklient", "10 ostu", ostud.size >= 10),
                    Triple("Tervislik", "Ostsid puu- ja köögivilju", ostud.any { o -> o.read.any { r -> r.toodeId?.let { olek.toode(it) }?.kategooriaId == 3 } }),
                    Triple("Öökull", "Ostsid pärast 20:00", ostud.any { it.aeg.hour >= 20 }),
                    Triple("Hommikuinimene", "Ostsid enne 9:00", ostud.any { it.aeg.hour < 9 }),
                    Triple("Suur ost", "Ost üle 50 €", ostud.any { it.summa >= 5000 }),
                )
                val saadud = kleepsud.count { it.third }
                Row(
                    Modifier.fillMaxWidth().clip(RoundedCornerShape(24.dp)).background(V.Ploom).padding(16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Ikoon(Ik.Kingitus, Color.White, 24.dp)
                    Spacer(Modifier.width(12.dp))
                    Column(Modifier.weight(1f)) {
                        T(Keel.t("Kogu komplekt = kingitus 2,00 €"), 14.sp, Color.White, FontWeight.ExtraBold)
                        Vahe(6.dp)
                        Riba(saadud / kleepsud.size.toFloat(), Color.White, taust = Color.White.copy(alpha = 0.25f))
                    }
                    Spacer(Modifier.width(12.dp))
                    T("$saadud/${kleepsud.size}", 18.sp, Color.White, FontWeight.ExtraBold)
                }
                Vahe(12.dp)
                for (rida in kleepsud.chunked(2)) {
                    Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                        for ((nimi, kirjeldus, olemas) in rida) {
                            Column(
                                Modifier.weight(1f).padding(bottom = 10.dp).clip(RoundedCornerShape(22.dp)).background(if (olemas) V.Pind else V.Paneel).padding(14.dp),
                                horizontalAlignment = Alignment.CenterHorizontally
                            ) {
                                Box(Modifier.size(56.dp).rotate(if (olemas) -8f else 0f).clip(CircleShape).background(if (olemas) V.Punane else V.Joon), Alignment.Center) {
                                    Ikoon(if (olemas) Ik.Taht else Ik.Info, Color.White, 26.dp)
                                }
                                Vahe(8.dp)
                                T(Keel.t(nimi), 14.sp, if (olemas) V.Tekst else V.Nork, FontWeight.ExtraBold, keskel = true)
                                T(Keel.t(kirjeldus), 11.sp, V.Nork, keskel = true)
                            }
                        }
                    }
                }
            }
            else -> YllatusLeht(olek)
        }
        Vahe(40.dp)
    }
}

// Päeva üllatus: kingikarp avaneb, sees üks toode soodushinnaga (iga päev sama)
@Composable
private fun YllatusLeht(olek: AppOlek) {
    var avatud by remember { mutableStateOf(false) }
    val paev = LocalDate.now()
    val toode = remember(paev) {
        val sobivad = olek.tooted.filter { it.laos && it.hind in 100..800 }
        if (sobivad.isEmpty()) null else sobivad[Math.floorMod(paev.toEpochDay().toInt() * 31, sobivad.size)]
    }
    Column(
        Modifier.fillMaxWidth().clip(RoundedCornerShape(28.dp)).background(V.Ploom).padding(20.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        T(Keel.t("PÄEVA ÜLLATUS") + " · " + paev.format(DateTimeFormatter.ofPattern("dd.MM")), 11.sp, Color.White, FontWeight.ExtraBold)
        Vahe(18.dp)
        if (!avatud || toode == null) {
            Box(Modifier.size(160.dp).vajuta { avatud = true }, Alignment.Center) {
                Box(Modifier.size(140.dp, 110.dp).align(Alignment.BottomCenter).clip(RoundedCornerShape(14.dp)).background(V.Punane))
                Box(Modifier.size(160.dp, 34.dp).align(Alignment.Center).clip(RoundedCornerShape(10.dp)).background(Color(0xFFFF2B3D)))
                Box(Modifier.size(18.dp, 150.dp).align(Alignment.BottomCenter).background(Color.White))
            }
            Vahe(16.dp)
            T(Keel.t("Puuduta kingitust"), 18.sp, Color.White, FontWeight.ExtraBold)
            T(Keel.t("Iga päev uus toode −25 %"), 13.sp, Color.White.copy(alpha = 0.8f))
        } else {
            val hind = Reeglid.soodusHind(toode.hind, 25)
            Column(
                Modifier.fillMaxWidth().clip(RoundedCornerShape(22.dp)).background(Color.White).padding(16.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                VaikeKiip("−25%", V.Punane, Color.White)
                ToodePilt(toode.pilt, Modifier.size(110.dp), 40.dp)
                T(toode.nimi, 15.sp, V.Tekst, FontWeight.ExtraBold, keskel = true, read = 2)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(eur(hind), 24.sp, V.Punane, FontWeight.ExtraBold)
                    Spacer(Modifier.width(8.dp))
                    T(eur(toode.hind), 13.sp, V.Nork, labi = true)
                }
            }
            Vahe(14.dp)
            SuurNupp(Keel.t("Lisa korvi soodushinnaga"), {
                olek.teade = olek.lisaKorvi(toode)?.let { it } ?: run {
                    val i = olek.korv.indexOfFirst { it.toode.id == toode.id }
                    if (i >= 0) olek.korv[i] = olek.korv[i].copy(hind = hind, soodus = "Päeva üllatus −25 %")
                    Keel.f("„{0}“ lisatud ostukorvi", toode.nimi)
                }
            }, taust = Color.White, varv = V.Ploom)
        }
    }
}

// ---------------------------------------------------------------
//  Ostunimekiri (TelNimekiri)
// ---------------------------------------------------------------
@Composable
fun NimekiriEkraan(olek: AppOlek) {
    var read by remember { mutableStateOf(olek.pood.nimekiri()) }
    var uus by remember { mutableStateOf("") }
    fun varskenda() { read = olek.pood.nimekiri() }
    val ostmata = read.filter { !it.ostetud }
    Box(Modifier.fillMaxSize()) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 170.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Box(Modifier.size(48.dp).clip(CircleShape).background(V.Ploom), Alignment.Center) { Ikoon(Ik.Nimekiri, Color.White, 22.dp) }
                Spacer(Modifier.width(12.dp))
                Column(Modifier.weight(1f)) {
                    T(Keel.t("Ostunimekiri"), 22.sp, V.Tekst, FontWeight.ExtraBold)
                    T(Keel.f("Ostetud {0} / {1}", read.count { it.ostetud }, read.size), 12.sp, V.Nork)
                }
            }
            Vahe(10.dp)
            Riba(if (read.isEmpty()) 0f else read.count { it.ostetud } / read.size.toFloat(), V.Ploom)
            Vahe(12.dp)
            Row(
                Modifier.fillMaxWidth().clip(RoundedCornerShape(24.dp)).background(V.Pind).vajuta { olek.ava(Ekraan.Pere) }.padding(12.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Initsiaalid("MM", V.Ploom, 30.dp)
                Initsiaalid("JM", V.Punane, 30.dp)
                Spacer(Modifier.width(10.dp))
                T(Keel.t("Jaga perega"), 14.sp, V.Tekst, FontWeight.Bold, Modifier.weight(1f))
                Ikoon(Ik.Parem, V.Tekst, 16.dp)
            }
            Vahe(10.dp)
            Sisend(uus, { uus = it }, Keel.t("Mida osta?"), taust = V.Pind, taga = {
                RingNupp(Ik.Pluss, { if (uus.isNotBlank()) { olek.pood.lisaNimekirja(uus, olek.kasutaja?.eesnimi); uus = ""; varskenda() } }, V.Tume, Color.White, 38.dp)
            })
            Vahe(12.dp)
            Kaart(sisu = 6.dp) {
                if (read.isEmpty()) T(Keel.t("Nimekiri on tühi."), 14.sp, V.Nork, Modifier.padding(12.dp))
                for (r in read) {
                    Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(14.dp)).vajuta { olek.pood.margiNimekirjas(r.id, !r.ostetud); varskenda() }.padding(10.dp), verticalAlignment = Alignment.CenterVertically) {
                        Box(
                            Modifier.size(26.dp).clip(RoundedCornerShape(8.dp)).background(if (r.ostetud) V.Roheline else V.Paneel),
                            Alignment.Center
                        ) { if (r.ostetud) Ikoon(Ik.Linnuke, Color.White, 16.dp, 3f) }
                        Spacer(Modifier.width(12.dp))
                        Column(Modifier.weight(1f)) {
                            T(r.tekst, 15.sp, if (r.ostetud) V.Nork else V.Tekst, FontWeight.SemiBold, labi = r.ostetud)
                            r.lisaja?.let { T(Keel.f("lisas {0}", it), 11.sp, V.Nork) }
                        }
                        Box(Modifier.size(32.dp).vajuta { olek.pood.kustutaNimekirjast(r.id); varskenda() }, Alignment.Center) { Ikoon(Ik.Sulge, V.Nork, 16.dp) }
                    }
                }
            }
        }
        if (ostmata.isNotEmpty())
            SuurNupp(Keel.f("Lisa ostmata korvi · {0}", ostmata.size), {
                var lisatud = 0
                for (r in ostmata) olek.tooted.filter { it.laos && it.nimi.contains(r.tekst, true) }.minByOrNull { it.hind }?.let { if (olek.lisaKorvi(it) == null) lisatud++ }
                olek.teade = Keel.f("Ostukorvi lisati {0} toodet", lisatud)
            }, Modifier.align(Alignment.BottomCenter).padding(start = 16.dp, end = 16.dp, bottom = 96.dp), ikoon = Ik.Korv)
    }
}

// ---------------------------------------------------------------
//  Profiil ja seaded (TelSeaded)
// ---------------------------------------------------------------
@Composable
fun ProfiilEkraan(olek: AppOlek) {
    val k = olek.kasutaja ?: return
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 92.dp)) {
        Pais("Profiil", null)
        Vahe(12.dp)
        Row(
            Modifier.fillMaxWidth().clip(RoundedCornerShape(26.dp)).background(V.Tume).padding(18.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Initsiaalid(k.initsiaalid, V.Ploom, 52.dp)
            Spacer(Modifier.width(14.dp))
            Column(Modifier.weight(1f)) {
                T(k.taisNimi, 17.sp, Color.White, FontWeight.ExtraBold)
                T(Keel.t(if (k.roll == Roll.Omanik) "Omanik" else "Ostja") + " · " + Keel.f("kaart {0}", k.kaart), 12.sp, Color(0xFFC9CCD1))
            }
            Column(horizontalAlignment = Alignment.End) {
                T(Keel.t("Boonus"), 11.sp, Color(0xFFC9CCD1))
                T(eur(k.boonus), 18.sp, Color.White, FontWeight.ExtraBold)
            }
        }
        Vahe(16.dp)
        T(Keel.t("Keel"), 13.sp, V.Tekst2, FontWeight.Bold)
        Vahe(6.dp)
        KeeleValik(olek)
        Vahe(16.dp)
        Kaart(sisu = 6.dp) {
            val punktid = buildList {
                add(Triple(Ik.Tsekk, "Minu ostud", { olek.ava(Ekraan.Ajalugu) }))
                add(Triple(Ik.Leht, "Minu dieet", { olek.ava(Ekraan.Dieet) }))
                add(Triple(Ik.Syda, "Minu lemmikud", { olek.ava(Ekraan.Ruudustik("Minu lemmikud", vaade = "lemmikud")) }))
                add(Triple(Ik.Kasutajad, "Pere", { olek.ava(Ekraan.Pere) }))
                add(Triple(Ik.Taht, "Preemiad", { olek.ava(Ekraan.Preemiad()) }))
                add(Triple(Ik.Kuu, "Õhtune soodustus", { olek.ava(Ekraan.Ohtu) }))
                if (k.roll == Roll.Omanik) add(Triple(Ik.Pliiats, "Halda tooteid", { olek.ava(Ekraan.Admin) }))
            }
            for ((ikoon, tekst, tegevus) in punktid) {
                Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(14.dp)).vajuta(tegevus).padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                    Box(Modifier.size(36.dp).clip(RoundedCornerShape(12.dp)).background(V.Paneel), Alignment.Center) { Ikoon(ikoon, V.Tekst, 18.dp) }
                    Spacer(Modifier.width(12.dp))
                    T(Keel.t(tekst), 15.sp, V.Tekst, FontWeight.SemiBold, Modifier.weight(1f))
                    Ikoon(Ik.Parem, V.Nork, 16.dp)
                }
            }
        }
        Vahe(14.dp)
        AaarisNupp(Keel.t("Logi välja"), { olek.logiValja() }, Modifier.fillMaxWidth(), V.Punane)
    }
}

// ---------------------------------------------------------------
//  Minu ostud (TelAjalugu): viimane tšekk paberil, varasemad loendis
// ---------------------------------------------------------------
@Composable
fun AjaluguEkraan(olek: AppOlek) {
    val ostud = remember { olek.pood.ostud(if (olek.kasutaja?.roll == Roll.Omanik) null else olek.kasutaja?.nimi) }
    var valitud by remember { mutableStateOf(ostud.firstOrNull()) }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Minu ostud", { olek.tagasi() })
        Vahe(12.dp)
        if (ostud.isEmpty()) { Tyhi(Ik.Tsekk, Keel.t("Oste veel pole"), Keel.t("Siia ilmuvad sinu tšekid.")); return@Column }
        valitud?.let { o ->
            Column(Modifier.fillMaxWidth().clip(RoundedCornerShape(26.dp)).background(V.Tume).padding(14.dp)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Box(Modifier.size(40.dp).clip(RoundedCornerShape(12.dp)).background(V.Ploom), Alignment.Center) { Ikoon(Ik.Tsekk, Color.White, 18.dp) }
                    Spacer(Modifier.width(10.dp))
                    Column(Modifier.weight(1f)) {
                        T(Keel.f("Ost nr {0}", o.id), 15.sp, Color.White, FontWeight.ExtraBold)
                        T(o.aeg.format(DateTimeFormatter.ofPattern("dd.MM.yyyy HH:mm")) + " · " + Keel.t(if (o.tarneviis == "Kojuvedu") "Kojuvedu" else "Poest kaasa"), 12.sp, Color(0xFFC9CCD1))
                    }
                    VaikeKiip(Keel.t(if (o.tagastatud) "Tagastatud" else "Makstud"), if (o.tagastatud) V.Punane else Color(0xFF1E4D33), if (o.tagastatud) Color.White else Color(0xFF7FD9A2))
                }
                Vahe(12.dp)
                Column(Modifier.fillMaxWidth().clip(RoundedCornerShape(16.dp)).background(Color.White).padding(16.dp)) {
                    T("AKT", 18.sp, V.Tekst, FontWeight.ExtraBold, Modifier.fillMaxWidth(), keskel = true)
                    Vahe(8.dp)
                    for (r in o.read) ReaSumma("${r.kogus} × ${r.nimi}", eur(r.summa), V.Tekst, suurus = 12.sp)
                    if (o.tarne > 0) ReaSumma(Keel.t("Kohaletoimetamine"), eur(o.tarne), suurus = 12.sp)
                    if (o.boonusKasutatud > 0) ReaSumma(Keel.t("Boonus"), "−" + eur(o.boonusKasutatud), V.Roheline, suurus = 12.sp)
                    o.aadress?.let { T(it, 11.sp, V.Ploom, FontWeight.Bold) }
                    Vahe(6.dp)
                    ReaSumma(Keel.t("KOKKU"), eur(o.summa), V.Tekst, true, 15.sp)
                    if (o.boonusLisatud > 0) ReaSumma(Keel.t("Teenitud boonust"), eur(o.boonusLisatud), V.Tekst2, suurus = 12.sp)
                }
                Vahe(12.dp)
                Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                    SuurNupp(Keel.t("Osta uuesti"), {
                        val (tk, puudu) = olek.ostaUuesti(o)
                        olek.teade = Keel.f("Ostukorvi lisatud {0} tk", tk) + if (puudu.isNotEmpty()) " · " + Keel.f("puudu {0}", puudu.size) else ""
                    }, Modifier.weight(1f), taust = Color.White, varv = V.Tekst, noolega = false)
                    if (olek.kasutaja?.roll == Roll.Omanik && !o.tagastatud)
                        AaarisNupp(Keel.t("Tagasta"), { olek.pood.tagasta(o.id); olek.varskendaTooted(); olek.teade = Keel.t("Ost tagastatud") }, Modifier.weight(1f), Color.White)
                }
            }
        }
        Vahe(16.dp)
        T(Keel.t("Varasemad"), 16.sp, V.Tekst, FontWeight.ExtraBold)
        Vahe(8.dp)
        Kaart(sisu = 6.dp) {
            for (o in ostud.take(40)) {
                Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(14.dp)).background(if (o == valitud) V.Paneel else Color.Transparent).vajuta { valitud = o }.padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                    Column(Modifier.weight(1f)) {
                        T(Keel.f("Ost nr {0}", o.id), 14.sp, V.Tekst, FontWeight.Bold)
                        T(o.aeg.format(DateTimeFormatter.ofPattern("dd.MM.yyyy HH:mm")), 12.sp, V.Nork)
                    }
                    T(eur(o.summa), 15.sp, V.Tekst, FontWeight.ExtraBold)
                }
            }
        }
    }
}

// ---------------------------------------------------------------
//  Minu dieet (TelUusDieet)
// ---------------------------------------------------------------
@Composable
fun DieetEkraan(olek: AppOlek) {
    val sobivad = olek.tooted.filter { Dieet.sobib(it, olek.dieet) }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Minu dieet", { olek.tagasi() })
        Vahe(12.dp)
        Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            for (m in Dieet.Mark.entries) Kiip((if (m in olek.dieet) "✓ " else "") + Keel.t(m.nimi), V.Pind, V.Tekst, valitud = m in olek.dieet, onClick = { olek.vahetaDieet(m) })
        }
        Vahe(14.dp)
        Row(verticalAlignment = Alignment.Bottom) {
            T(Keel.t("Sulle sobib"), 16.sp, V.Tekst, FontWeight.ExtraBold, Modifier.weight(1f))
            T("${sobivad.size}", 28.sp, V.Roheline, FontWeight.ExtraBold)
            T(" / ${olek.tooted.size}", 14.sp, V.Nork)
        }
        Vahe(10.dp)
        Ruudud(olek, sobivad.filter { it.laos }.take(20))
        Vahe(20.dp)
    }
}

// ---------------------------------------------------------------
//  Õhtune soodustus (TelUusOhtu): kell, küpsetised −50 %
// ---------------------------------------------------------------
@Composable
fun OhtuEkraan(olek: AppOlek) {
    val nyyd = olek.aeg()
    val kaib = Reeglid.ohtuKaib(nyyd)
    val kupsetised = olek.tooted.filter { it.kategooriaId == Reeglid.KUPSETISTE_KATEGOORIA && it.laos }.take(20)
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Küpsetised", { olek.tagasi() })
        Vahe(12.dp)
        Column(Modifier.fillMaxWidth().clip(RoundedCornerShape(26.dp)).background(Color(0xFF3B2150)).padding(20.dp)) {
            VaikeKiip(Keel.t(if (kaib) "KÄIB PRAEGU" else "TÄNA ÕHTUL"), Color(0xFF5A3A70), Color(0xFFF5D76E))
            Vahe(8.dp)
            T(Keel.t("Pärast 20:00 kõik küpsetised −50 %"), 22.sp, Color.White, FontWeight.ExtraBold)
            Vahe(8.dp)
            T(nyyd.format(DateTimeFormatter.ofPattern("HH:mm")), 40.sp, Color.White, FontWeight.ExtraBold)
            val minutid = (20 * 60) - (nyyd.hour * 60 + nyyd.minute)
            Vahe(8.dp)
            Riba(if (kaib) 1f else 1f - minutid.coerceIn(0, 600) / 600f, Color(0xFFF5D76E), taust = Color(0xFF5A3A70))
            if (!kaib) { Vahe(6.dp); T(Keel.f("Algab {0} h {1} min pärast", minutid / 60, minutid % 60), 12.sp, Color(0xFFD9CDE6)) }
        }
        Vahe(14.dp)
        Ruudud(olek, kupsetised)
        Vahe(10.dp)
        T(Keel.t("Ei liitu teiste soodustustega – kehtib suurim."), 11.sp, V.Nork, Modifier.fillMaxWidth(), keskel = true)
        Vahe(20.dp)
    }
}

// ---------------------------------------------------------------
//  Toiteväärtus (TelUusToitevaartus): korvi energia ja toitained
// ---------------------------------------------------------------
@Composable
fun ToitevaartusEkraan(olek: AppOlek) {
    val kcal = olek.korv.sumOf { r -> (Toitevaartus.kcal(r.toode) ?: 0.0) * r.kogus }
    val paevi = kcal / 2000.0
    val ilma = olek.korv.filter { Toitevaartus.kcal(it.toode) == null }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Toiteväärtus", { olek.tagasi() })
        Vahe(12.dp)
        Kaart(sisu = 18.dp) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Ring((paevi / 7).toFloat(), V.Roheline, Modifier.size(120.dp), 10.dp) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        T("%.0f".format(kcal), 22.sp, V.Tekst, FontWeight.ExtraBold)
                        T("kcal", 11.sp, V.Nork)
                    }
                }
                Spacer(Modifier.width(18.dp))
                Column {
                    T("%.1f".format(paevi).replace('.', ','), 34.sp, V.Ploom, FontWeight.ExtraBold)
                    T(Keel.t("päeva energiat ühele inimesele"), 13.sp, V.Tekst2)
                    T(Keel.t("(2000 kcal päevas)"), 11.sp, V.Nork)
                }
            }
        }
        Vahe(12.dp)
        Kaart(sisu = 6.dp) {
            if (olek.korv.isEmpty()) T(Keel.t("Ostukorv on tühi."), 14.sp, V.Nork, Modifier.padding(12.dp))
            for (r in olek.korv) {
                val k = Toitevaartus.kcal(r.toode)
                Row(Modifier.fillMaxWidth().padding(10.dp), verticalAlignment = Alignment.CenterVertically) {
                    Box(Modifier.size(40.dp).clip(RoundedCornerShape(10.dp)).background(V.Paneel), Alignment.Center) { ToodePilt(r.toode.pilt, Modifier.size(34.dp), 18.dp) }
                    Spacer(Modifier.width(10.dp))
                    Column(Modifier.weight(1f)) {
                        T(r.toode.nimi, 13.sp, V.Tekst, FontWeight.Bold, read = 1)
                        T(if (k != null) "%.0f kcal".format(k * r.kogus) else Keel.t("andmed puuduvad"), 11.sp, V.Nork)
                    }
                    KogusValija(r.kogus, { olek.vahenda(r.toode.id) }, { olek.teade = olek.lisaKorvi(r.toode) })
                }
            }
        }
        if (ilma.isNotEmpty()) {
            Vahe(10.dp)
            Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(16.dp)).background(V.Kreem).padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                Ikoon(Ik.Hoiatus, V.Oranz, 18.dp)
                Spacer(Modifier.width(8.dp))
                T(ilma.joinToString(", ") { it.toode.nimi.substringBefore(' ') } + " – " + Keel.t("kaal puudub, ei ole arvestatud"), 12.sp, V.Oranz, read = 2)
            }
        }
        Vahe(20.dp)
    }
}

// ---------------------------------------------------------------
//  Pere (TelUusPere): ühine ostunimekiri pere koodiga
// ---------------------------------------------------------------
@Composable
fun PereEkraan(olek: AppOlek) {
    var kood by remember { mutableStateOf(olek.pood.seade("pere") ?: "") }
    var sisend by remember { mutableStateOf("") }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp)) {
        Pais("Pere nimekiri", { olek.tagasi() })
        Vahe(12.dp)
        if (kood.isNotEmpty()) {
            Column(Modifier.fillMaxWidth().clip(RoundedCornerShape(24.dp)).background(V.Tume).padding(18.dp)) {
                T(Keel.t("Pere kood"), 12.sp, Color(0xFFC9CCD1))
                Vahe(6.dp)
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(kood.toCharArray().joinToString(" "), 26.sp, Color.White, FontWeight.ExtraBold, Modifier.weight(1f))
                    RingNupp(Ik.Kopeeri, { olek.teade = Keel.t("Kood kopeeritud") }, V.Tume2, Color.White, 44.dp)
                }
                Vahe(6.dp)
                T(Keel.t("Anna kood pereliikmele – ta näeb ja muudab sama ostunimekirja."), 12.sp, Color(0xFFC9CCD1))
            }
            Vahe(12.dp)
            AaarisNupp(Keel.t("Lahku perest"), { olek.pood.seaSeade("pere", null); kood = "" }, Modifier.fillMaxWidth(), V.Punane)
        } else {
            Kaart {
                T(Keel.t("Liitu perega"), 18.sp, V.Tekst, FontWeight.ExtraBold)
                Vahe(6.dp)
                T(Keel.t("Sisesta pereliikmelt saadud 7-märgiline kood."), 13.sp, V.Tekst2)
                Vahe(12.dp)
                Sisend(sisend, { sisend = it.uppercase().filter { c -> c.isLetterOrDigit() }.take(7) }, "ABC1234")
                Vahe(12.dp)
                SuurNupp(Keel.t("Liitu"), {
                    if (sisend.length == 7) { olek.pood.seaSeade("pere", sisend); kood = sisend; olek.teade = Keel.t("Liitusid perega") }
                    else olek.teade = Keel.t("Pere kood on 7 märki.")
                }, noolega = false)
                Vahe(10.dp)
                Box(Modifier.fillMaxWidth().vajuta {
                    val uus = (1..7).map { "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".random() }.joinToString("")
                    olek.pood.seaSeade("pere", uus); kood = uus
                }, Alignment.Center) { T(Keel.t("Loo uus pere"), 14.sp, V.Ploom, FontWeight.Bold) }
            }
        }
    }
}
