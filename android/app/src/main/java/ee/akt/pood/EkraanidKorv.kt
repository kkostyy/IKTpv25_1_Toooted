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
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
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
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

// ---------------------------------------------------------------
//  Ostukorv (TelKorv, TelTyhi, TelUusPant)
// ---------------------------------------------------------------
@Composable
fun KorvEkraan(olek: AppOlek) {
    if (olek.korv.isEmpty()) {
        Column(Modifier.fillMaxSize().padding(16.dp).padding(bottom = 92.dp)) {
            Pais("Ostukorv", null) { VaikeKiip(Keel.f("{0} tk", 0), V.Pind, V.Tekst) }
            Vahe(14.dp)
            SalvestatudKorvid(olek)
            Vahe(12.dp)
            Kaart(Modifier.fillMaxWidth().weight(1f), raadius = 28.dp) {
                Column(Modifier.fillMaxSize(), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
                    Tyhi(Ik.Korv, Keel.t("Sinu ostukorv on tühi"), Keel.t("Lisa tooteid nupuga „+“ või vaata pakkumisi."))
                    Vahe(8.dp)
                    Row(
                        Modifier.height(48.dp).clip(RoundedCornerShape(24.dp)).background(V.Tume).vajuta { olek.vaheleht(Ekraan.Avaleht) }.padding(start = 18.dp, end = 6.dp),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        T(Keel.t("Vaata pakkumisi"), 15.sp, Color.White, FontWeight.ExtraBold)
                        Spacer(Modifier.width(12.dp))
                        Box(Modifier.size(36.dp).clip(CircleShape).background(Color.White), Alignment.Center) { Ikoon(Ik.Nool, V.Tume, 16.dp, 2.6f) }
                    }
                }
            }
        }
        return
    }
    val tarne = Reeglid.tarne(olek.kaubad, true)
    val kokku = olek.kaubad + olek.pant
    Box(Modifier.fillMaxSize()) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 250.dp)) {
            Pais("Ostukorv", { olek.vaheleht(Ekraan.Avaleht) }) {
                VaikeKiip(Keel.f("{0} tk", olek.korvisTk), V.Tume, Color.White)
                Spacer(Modifier.width(8.dp))
                RingNupp(Ik.Prygi, { olek.tyhjendaKorv(); olek.teade = Keel.t("Ostukorv tühjendatud") }, V.Pind, V.Tekst, 44.dp)
            }
            Vahe(12.dp)
            Kaart(sisu = 6.dp) {
                for (r in olek.korv.toList()) KorviRida(olek, r)
            }
            Vahe(12.dp)
            SalvestatudKorvid(olek)
            Vahe(12.dp)
            // Tasuta tarneni
            val puudu = Reeglid.TASUTA_TARNE_ALATES - olek.kaubad
            Kaart(sisu = 14.dp) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Ikoon(Ik.Auto, V.Roheline, 22.dp)
                    Spacer(Modifier.width(10.dp))
                    T(if (puudu > 0) Keel.f("Lisa veel {0} ja kojuvedu on tasuta", eur(puudu)) else Keel.t("Kojuvedu on tasuta!"), 13.sp, V.Tekst, FontWeight.Bold)
                }
                Vahe(8.dp)
                Riba(olek.kaubad / Reeglid.TASUTA_TARNE_ALATES.toFloat(), V.Roheline)
            }
            Vahe(10.dp)
            Kaart(Modifier.fillMaxWidth().vajuta { olek.ava(Ekraan.Toitevaartus) }, sisu = 14.dp) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Box(Modifier.size(8.dp).clip(CircleShape).background(V.Roheline))
                    Spacer(Modifier.width(10.dp))
                    T(Keel.t("Toiteväärtus"), 14.sp, V.Tekst, FontWeight.Bold, Modifier.weight(1f))
                    Ikoon(Ik.Parem, V.Nork, 16.dp)
                }
            }
        }
        // Tume kokkuvõte all
        Column(
            Modifier.align(Alignment.BottomCenter).padding(16.dp).fillMaxWidth().clip(RoundedCornerShape(28.dp)).background(V.Tume).padding(18.dp)
        ) {
            ReaSumma(Keel.t("Tooted"), eur(olek.kaubad), Color(0xFFC9CCD1))
            val soodus = olek.korv.sumOf { (it.tavaHind - it.hind) * it.kogus }
            if (soodus > 0) ReaSumma(Keel.t("Soodustus"), "−" + eur(soodus), Color(0xFFFF8A8A))
            if (olek.pant > 0) ReaSumma(Keel.f("Pakendi tagatisraha ({0} tk)", olek.korv.filter { it.toode.pant > 0 }.sumOf { it.kogus }), eur(olek.pant), Color(0xFF7FD9A2))
            ReaSumma(Keel.t("Kojuvedu"), if (tarne == 0) Keel.t("TASUTA") else eur(tarne), Color(0xFFC9CCD1))
            Vahe(6.dp)
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(Keel.t("Kokku"), 16.sp, Color.White, FontWeight.ExtraBold, Modifier.weight(1f))
                T(eur(kokku), 28.sp, Color.White, FontWeight.ExtraBold)
            }
            Vahe(12.dp)
            SuurNupp(Keel.f("Maksma · {0}", eur(kokku)), { olek.ava(Ekraan.Maksmine) })
        }
    }
}

@Composable
private fun KorviRida(olek: AppOlek, r: KorvRida) {
    Row(Modifier.fillMaxWidth().padding(8.dp), verticalAlignment = Alignment.CenterVertically) {
        Box(Modifier.size(56.dp).clip(RoundedCornerShape(14.dp)).background(V.Paneel).vajuta { olek.ava(Ekraan.ToodeLeht(r.toode.id)) }, Alignment.Center) {
            ToodePilt(r.toode.pilt, Modifier.size(48.dp), 24.dp)
        }
        Spacer(Modifier.width(10.dp))
        Column(Modifier.weight(1f)) {
            T(r.toode.nimi, 13.sp, V.Tekst, FontWeight.Bold, read = 2, reavahe = 16.sp)
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(eur(r.hind), 13.sp, if (r.hind < r.tavaHind) V.Punane else V.Tekst2, FontWeight.ExtraBold)
                if (r.hind < r.tavaHind) { Spacer(Modifier.width(6.dp)); T(eur(r.tavaHind), 11.sp, V.Nork, labi = true) }
            }
            if (r.toode.pant > 0) T("● " + Keel.f("Pakendi tagatisraha {0}", eur(r.toode.pant)), 11.sp, V.Roheline, FontWeight.SemiBold)
            r.soodus?.let { T("● " + Keel.t(it), 11.sp, V.Punane, FontWeight.SemiBold) }
        }
        Spacer(Modifier.width(6.dp))
        Column(horizontalAlignment = Alignment.End) {
            KogusValija(r.kogus, { olek.vahenda(r.toode.id) }, { olek.teade = olek.lisaKorvi(r.toode) })
            Vahe(4.dp)
            T(eur(r.summa), 13.sp, V.Tekst, FontWeight.ExtraBold)
        }
    }
}

// ---------------------------------------------------------------
//  Vormista ost (TelMaksmine): kättesaamine + kojuveo aeg, kliendikaart, makse
// ---------------------------------------------------------------
@Composable
fun MaksmineEkraan(olek: AppOlek) {
    var kojuvedu by remember { mutableStateOf(olek.pood.seade("kojuvedu") != "0") }
    var aadress by remember { mutableStateOf(olek.pood.seade("aadress") ?: "") }
    var boonus by remember { mutableStateOf(false) }
    var makse by remember { mutableStateOf(Pangamakse.VIIS) }   // päris makse: ülekanne poe kontole
    var aken by remember { mutableStateOf(Tarneaknad.aknad(olek.aeg()).firstOrNull()) }
    var paev by remember { mutableStateOf(aken?.toLocalDate()) }
    var viga by remember { mutableStateOf<String?>(null) }
    var synd by remember { mutableStateOf(olek.pood.seade("synd:" + (olek.kasutaja?.nimi ?: "")) ?: "") }
    var kinnitan by remember { mutableStateOf(false) }
    // vanusepiiranguga tooted korvis (alkohol 18+, energiajoogid 16+)
    val piiratud = olek.korv.mapNotNull { r -> olek.vanusePiir(r.toode)?.let { r.toode.nimi to it } }
    val piir = piiratud.maxOfOrNull { it.second }
    val k = olek.kasutaja ?: return
    val tarne = Reeglid.tarne(olek.kaubad, kojuvedu)
    val kasutatud = if (boonus) minOf(k.boonus, olek.kaubad) else 0
    val kokku = olek.kaubad - kasutatud + tarne + olek.pant

    Box(Modifier.fillMaxSize()) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 300.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                RingNupp(Ik.Vasak, { olek.tagasi() }, V.Pind, V.Tekst, 48.dp)
                Spacer(Modifier.width(12.dp))
                T(Keel.t("Vormista ost"), 24.sp, V.Tekst, FontWeight.ExtraBold, Modifier.weight(1f))
                VaikeKiip("3 / 3", V.Pind, V.Tekst)
            }
            Vahe(16.dp)
            T(Keel.t("Kättesaamine"), 15.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(8.dp)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                Kiip(Keel.t("Kojuvedu"), V.Pind, V.Tekst, valitud = kojuvedu, onClick = { kojuvedu = true })
                Kiip(Keel.t("Poest kaasa"), V.Pind, V.Tekst, valitud = !kojuvedu, onClick = { kojuvedu = false })
            }
            if (kojuvedu) {
                Vahe(10.dp)
                Sisend(aadress, { aadress = it; viga = null }, Keel.t("Tänav 1-2, Tallinn, 10111"), taust = V.Pind, ees = { Ikoon(Ik.Asukoht, V.Tekst2, 18.dp) })
                Vahe(12.dp)
                T(Keel.t("Kojuveo aeg"), 13.sp, V.Tekst2, FontWeight.Bold)
                Vahe(6.dp)
                val koik = Tarneaknad.aknad(olek.aeg())
                Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    for (p in koik.map { it.toLocalDate() }.distinct())
                        Kiip(Tarneaknad.paevaNimi(p, olek.aeg().toLocalDate()) + " " + p.format(DateTimeFormatter.ofPattern("dd.MM")), V.Pind, V.Tekst,
                            valitud = p == paev, onClick = { paev = p; aken = koik.firstOrNull { it.toLocalDate() == p } })
                }
                Vahe(8.dp)
                Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    for (a in koik.filter { it.toLocalDate() == paev })
                        Kiip("${a.hour}–${a.hour + Tarneaknad.TUNDE}", V.Pind, V.Tekst, valitud = a == aken, onClick = { aken = a })
                }
            }
            Vahe(16.dp)
            Kaart(sisu = 14.dp) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Ikoon(Ik.Kaart, V.Tekst2, 20.dp)
                    Spacer(Modifier.width(10.dp))
                    T(k.kaart, 15.sp, V.Tekst, FontWeight.Bold, Modifier.weight(1f))
                    VaikeKiip(k.taisNimi, V.Roheline, Color.White)
                }
            }
            if (k.boonus > 0) {
                Vahe(10.dp)
                Kaart(Modifier.fillMaxWidth().vajuta { boonus = !boonus }, taust = V.Roosa, sisu = 14.dp) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Column(Modifier.weight(1f)) {
                            T(Keel.t("Kasuta boonust"), 14.sp, V.Tekst, FontWeight.ExtraBold)
                            T(Keel.f("Kaardil {0}", eur(k.boonus)), 12.sp, V.Tekst2)
                        }
                        Lyliti(boonus)
                    }
                }
            }
            Vahe(16.dp)
            if (piir != null) {
                Kaart(Modifier.fillMaxWidth(), taust = V.Kreem, sisu = 14.dp) {
                    T(Keel.f("Vanuse kontroll · {0}+", piir), 15.sp, V.Tekst, FontWeight.ExtraBold)
                    Vahe(4.dp)
                    T(piiratud.take(4).joinToString("\n") { "• ${it.first} (${it.second}+)" }, 12.sp, V.Tekst2)
                    Vahe(10.dp)
                    Sisend(synd, { synd = it.filter { c -> c.isDigit() || c == '.' }.take(10); viga = null }, Keel.t("Sünnikuupäev pp.kk.aaaa"), number = true, taust = V.Pind)
                    Vahe(10.dp)
                    Row(Modifier.fillMaxWidth().vajuta { kinnitan = !kinnitan }, verticalAlignment = Alignment.CenterVertically) {
                        T(Keel.t("Kinnitan, et andmed on õiged. Kauba üleandmisel kontrollitakse isikut tõendavat dokumenti."), 12.sp, V.Tekst, modifier = Modifier.weight(1f))
                        Spacer(Modifier.width(8.dp))
                        Lyliti(kinnitan)
                    }
                }
                Vahe(16.dp)
            }
            T(Keel.t("Makse"), 15.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(8.dp)
            Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                for (m in listOf(Pangamakse.VIIS, "Kaart", "Sularaha"))
                    Kiip(Keel.t(when (m) { "Kaart" -> "Pangakaart"; "Sularaha" -> "Sularaha kullerile"; else -> m }), V.Pind, V.Tekst, valitud = makse == m, onClick = { makse = m })
            }
            if (makse == Pangamakse.VIIS) {
                Vahe(6.dp)
                T(Keel.f("Ülekanne kontole {0} – rekvisiidid ja QR-kood pärast tellimist", Pangamakse.loetav(Pangamakse.iban(olek.pood))), 12.sp, V.Tekst2)
            }
            if (viga != null) { Vahe(10.dp); T(viga!!, 13.sp, V.Punane, FontWeight.Bold) }
        }
        Column(
            Modifier.align(Alignment.BottomCenter).fillMaxWidth().clip(RoundedCornerShape(topStart = 30.dp, topEnd = 30.dp)).background(V.Tume).padding(20.dp)
        ) {
            Box(Modifier.align(Alignment.CenterHorizontally).size(40.dp, 4.dp).clip(RoundedCornerShape(2.dp)).background(Color(0xFF4A4C52)))
            Vahe(12.dp)
            ReaSumma(Keel.f("{0} toodet", olek.korvisTk), eur(olek.kaubad), Color(0xFFC9CCD1))
            ReaSumma(Keel.t("Tarne"), if (tarne == 0) Keel.t("TASUTA") else eur(tarne), Color(0xFFC9CCD1))
            if (olek.pant > 0) ReaSumma(Keel.t("Pakendi tagatisraha"), eur(olek.pant), Color(0xFF7FD9A2))
            if (kasutatud > 0) ReaSumma(Keel.t("Boonus"), "−" + eur(kasutatud), Color(0xFF7FD9A2))
            Vahe(6.dp)
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(Keel.t("Kokku"), 17.sp, Color.White, FontWeight.ExtraBold, Modifier.weight(1f))
                T(eur(kokku), 30.sp, Color.White, FontWeight.ExtraBold)
            }
            Vahe(12.dp)
            SuurNupp(Keel.f(if (makse == Pangamakse.VIIS) "Telli · {0}" else "Maksa {0}", eur(kokku)), {
                when {
                    kojuvedu && aadress.trim().length < 5 -> viga = Keel.t("Sisesta kojuveo aadress.")
                    kojuvedu && aken == null -> viga = Keel.t("Vali kojuveo päev ja kellaaeg.")
                    piir != null && Vanus.vanus(synd, olek.aeg().toLocalDate()) == null -> viga = Keel.t("Sisesta sünnikuupäev kujul pp.kk.aaaa.")
                    piir != null && Vanus.vanus(synd, olek.aeg().toLocalDate())!! < piir -> viga = Keel.f("Alla {0}-aastasele ei müüda: {1}. Eemalda need tooted ostukorvist.", piir,
                        piiratud.filter { it.second > Vanus.vanus(synd, olek.aeg().toLocalDate())!! }.joinToString(", ") { it.first })
                    piir != null && !kinnitan -> viga = Keel.t("Kinnita vanuse andmed.")
                    else -> {
                        if (piir != null) olek.pood.seaSeade("synd:" + k.nimi, synd.trim())   // järgmisel korral täidetud
                        val ost = olek.vormista(kojuvedu, aadress.trim().let { a -> if (aken != null) "$a · ${Tarneaknad.tekst(aken!!, olek.aeg().toLocalDate())}" else a }, makse, boonus)
                        olek.asenda(Ekraan.MakseOnnestus(ost.id))
                    }
                }
            })
        }
    }
}

@Composable
fun Lyliti(sees: Boolean) {
    Box(Modifier.size(48.dp, 28.dp).clip(RoundedCornerShape(14.dp)).background(if (sees) V.Roheline else V.Joon).padding(3.dp), if (sees) Alignment.CenterEnd else Alignment.CenterStart) {
        Box(Modifier.size(22.dp).clip(CircleShape).background(Color.White))
    }
}

// Kojuveo ajaaknad (samad reeglid mis arvutiversioonis): 2 h aknad 10–22, 7 päeva, vähemalt 2 h ette
object Tarneaknad {
    const val TUNDE = 2
    private val algused = listOf(10, 12, 14, 16, 18, 20)

    fun aknad(nyyd: LocalDateTime, paevi: Int = 7): List<LocalDateTime> =
        (0 until paevi).flatMap { p -> algused.map { nyyd.toLocalDate().plusDays(p.toLong()).atTime(it, 0) } }
            .filter { java.time.Duration.between(nyyd, it).toMinutes() >= 120 }

    fun paevaNimi(p: java.time.LocalDate, tana: java.time.LocalDate): String = when (p) {
        tana -> Keel.t("Täna")
        tana.plusDays(1) -> Keel.t("Homme")
        else -> Keel.t(listOf("Esmaspäev", "Teisipäev", "Kolmapäev", "Neljapäev", "Reede", "Laupäev", "Pühapäev")[p.dayOfWeek.value - 1]).take(2)
    }

    fun tekst(a: LocalDateTime, tana: java.time.LocalDate) =
        "${paevaNimi(a.toLocalDate(), tana)} ${a.format(DateTimeFormatter.ofPattern("dd.MM"))} ${a.hour}–${a.hour + TUNDE}"
}

// ---------------------------------------------------------------
//  Makse õnnestus (TelTeated)
// ---------------------------------------------------------------
@Composable
fun MakseOnnestusEkraan(olek: AppOlek, ostId: Int) {
    val ost = remember(ostId) { olek.pood.ostud(olek.kasutaja?.nimi).firstOrNull { it.id == ostId } }
    if (ost != null && Pangamakse.ootab(olek.pood, ost)) { PangaOotabEkraan(olek, ost); return }
    Column(Modifier.fillMaxSize().background(V.Tume).padding(20.dp), horizontalAlignment = Alignment.CenterHorizontally) {
        Row(
            Modifier.fillMaxWidth().clip(RoundedCornerShape(22.dp)).background(Color.White).padding(14.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(Modifier.size(36.dp).clip(RoundedCornerShape(10.dp)).background(V.Roosa), Alignment.Center) { Ikoon(Ik.Tsekk, V.Ploom, 18.dp) }
            Spacer(Modifier.width(12.dp))
            Column {
                T(Keel.t("Tšekk salvestatud"), 14.sp, V.Tekst, FontWeight.ExtraBold)
                T(olek.kasutaja?.email ?: Keel.t("Ostude ajalugu"), 12.sp, V.Tekst2)
            }
        }
        Spacer(Modifier.weight(1f))
        Box(Modifier.size(110.dp).clip(CircleShape).background(V.Roheline), Alignment.Center) { Ikoon(Ik.Linnuke, Color.White, 54.dp, 3f) }
        Vahe(26.dp)
        T(Keel.t("Makse õnnestus!"), 26.sp, Color.White, FontWeight.ExtraBold)
        Vahe(10.dp)
        T(eur(ost?.summa ?: 0), 44.sp, Color.White, FontWeight.ExtraBold)
        Vahe(14.dp)
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            VaikeKiip(Keel.t(if (ost?.tarneviis == "Kojuvedu") "Kojuvedu" else "Poest kaasa"), V.Tume2, Color.White)
            if ((ost?.boonusLisatud ?: 0) > 0) VaikeKiip("+" + eur(ost!!.boonusLisatud) + " " + Keel.t("boonust"), Color(0xFF1E4D33), Color(0xFF7FD9A2))
        }
        Spacer(Modifier.weight(1f))
        SuurNupp(Keel.t("Vaata tšekki"), { olek.vaheleht(Ekraan.Profiil); olek.ava(Ekraan.Ajalugu) }, taust = Color.White, varv = V.Tekst, noolega = false)
        Vahe(10.dp)
        AaarisNupp(Keel.t("Tagasi poodi"), { olek.vaheleht(Ekraan.Avaleht) }, Modifier.fillMaxWidth(), Color.White)
    }
}

// Pangaülekanne: tellimus vormistatud, ootab makset – QR-kood ja rekvisiidid
@Composable
fun PangaOotabEkraan(olek: AppOlek, ost: Ost) {
    Column(Modifier.fillMaxSize().background(V.Tume).verticalScroll(rememberScrollState()).padding(20.dp), horizontalAlignment = Alignment.CenterHorizontally) {
        T(Keel.t("Tellimus on vormistatud"), 24.sp, Color.White, FontWeight.ExtraBold, keskel = true)
        Vahe(8.dp)
        VaikeKiip(Keel.f("Ootab makset · tellimus nr {0}", ost.id), Color(0xFF4D3C14), Color(0xFFF5D76E))
        Vahe(14.dp)
        PangaRekvisiidid(olek, ost)
        Vahe(16.dp)
        SuurNupp(Keel.t("Minu ostud"), { olek.vaheleht(Ekraan.Profiil); olek.ava(Ekraan.Ajalugu) }, taust = Color.White, varv = V.Tekst, noolega = false)
        Vahe(10.dp)
        AaarisNupp(Keel.t("Tagasi poodi"), { olek.vaheleht(Ekraan.Avaleht) }, Modifier.fillMaxWidth(), Color.White)
    }
}
