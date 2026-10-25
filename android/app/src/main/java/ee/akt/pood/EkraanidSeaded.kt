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
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalInspectionMode
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import java.time.LocalDate

// ---------------------------------------------------------------
//  Seaded: profiil, parool, välimus (teema, keel), teavitused, ostlemine, teave
// ---------------------------------------------------------------
@Composable
fun SeadedEkraan(olek: AppOlek) {
    val k = olek.kasutaja ?: return
    var nimi by remember { mutableStateOf(k.taisNimi) }
    var email by remember { mutableStateOf(k.email ?: "") }
    var aadress by remember { mutableStateOf(olek.pood.seade("aadress") ?: "") }
    var vana by remember { mutableStateOf("") }
    var uus by remember { mutableStateOf("") }
    var uus2 by remember { mutableStateOf("") }
    var hooaeg by remember { mutableStateOf(olek.pood.seade("hooaeg") != "0") }
    var pereTeated by remember { mutableStateOf(olek.pood.seade("pere_teated") != "0") }
    var kojuvedu by remember { mutableStateOf(olek.pood.seade("kojuvedu") != "0") }

    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 40.dp).animateContentSize()) {
        Pais("Seaded", { olek.tagasi() })
        Vahe(12.dp)

        Osa("Profiil") {
            Silt("Ees- ja perekonnanimi"); Sisend(nimi, { nimi = it }, "Mari Maasikas")
            Vahe(8.dp)
            Silt("E-post (sinna saadetakse tšekk)"); Sisend(email, { email = it }, "nimi@email.ee")
            Vahe(8.dp)
            Silt("Kojuveo aadress"); Sisend(aadress, { aadress = it }, Keel.t("Tänav 1-2, Tallinn, 10111"), ees = { Ikoon(Ik.Asukoht, V.Tekst2, 18.dp) })
            Vahe(12.dp)
            SuurNupp(Keel.t("Salvesta profiil"), {
                when {
                    nimi.trim().length < 2 -> olek.teade = Keel.t("Sisesta nimi.")
                    email.isNotBlank() && !Regex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$").matches(email.trim()) -> olek.teade = Keel.t("E-posti aadress pole korrektne.")
                    else -> {
                        olek.pood.muudaProfiili(k.nimi, nimi, email)
                        olek.pood.seaSeade("aadress", aadress.trim().ifEmpty { null })
                        olek.varskendaKasutaja()
                        olek.teade = Keel.t("Profiil salvestatud")
                    }
                }
            }, noolega = false)
        }

        Osa("Turvalisus") {
            Sisend(vana, { vana = it }, Keel.t("Praegune parool"), parool = true)
            Vahe(8.dp)
            Sisend(uus, { uus = it }, Keel.t("Uus parool (vähemalt 6 märki)"), parool = true)
            Vahe(8.dp)
            Sisend(uus2, { uus2 = it }, Keel.t("Korda uut parooli"), parool = true)
            Vahe(12.dp)
            SuurNupp(Keel.t("Muuda parooli"), {
                if (uus != uus2) olek.teade = Keel.t("Paroolid ei kattu.")
                else {
                    val viga = olek.pood.muudaParooli(k.nimi, vana, uus)
                    olek.teade = viga?.let { Keel.t(it) } ?: Keel.t("Parool muudetud")
                    if (viga == null) { vana = ""; uus = ""; uus2 = "" }
                }
            }, taust = V.Tume, noolega = false, lubatud = vana.isNotEmpty() && uus.isNotEmpty())
        }

        Osa("Välimus") {
            Silt("Teema")
            Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(20.dp)).background(V.Paneel).padding(4.dp)) {
                listOf("Nagu süsteemis", "Hele", "Tume").forEachIndexed { i, s ->
                    val v = Teema.valik == i
                    Box(
                        Modifier.weight(1f).height(36.dp).clip(RoundedCornerShape(18.dp)).background(if (v) V.Tume else Color.Transparent)
                            .vajuta { Teema.valik = i; Teema.salvesta(i) },
                        Alignment.Center
                    ) { T(Keel.t(s), 13.sp, if (v) Color.White else V.Tekst, FontWeight.Bold, read = 1) }
                }
            }
            Vahe(12.dp)
            Row(verticalAlignment = Alignment.CenterVertically) {
                Silt("Keel", Modifier.weight(1f))
                KeeleValik(olek)
            }
            Vahe(4.dp)
            Valik("Hooajaline kujundus", "Halloween, jõulud, sõbrapäev avalehel", hooaeg) { hooaeg = it; olek.pood.seaSeade("hooaeg", if (it) null else "0") }
        }

        Osa("Teavitused ja ostlemine") {
            Valik("Pere korvi teated", "Kui pereliige lisab pere korvi toote", pereTeated) { pereTeated = it; olek.pood.seaSeade("pere_teated", if (it) null else "0") }
            Valik("Kojuvedu vaikimisi", "Maksmisel on kohe valitud kojuvedu", kojuvedu) { kojuvedu = it; olek.pood.seaSeade("kojuvedu", if (it) null else "0") }
        }

        if (k.roll == Roll.Omanik) Osa("Haldus") {
            T(Keel.t("Loob 3 klienti ja umbes 60 ostu viimase 60 päeva jooksul – müügipaneel, autotellimus ja „Ostetakse koos“ ei ole siis tühjad."), 13.sp, V.Tekst2, reavahe = 18.sp)
            Vahe(10.dp)
            AaarisNupp(Keel.t("Loo demoandmed"), {
                val n = Demo.loo(olek.pood, olek.tooted, olek.aeg())
                olek.varskendaTooted()
                olek.teade = Keel.f("Lisatud {0} ostu", n)
            }, Modifier.fillMaxWidth())
        }

        Osa("Teave") {
            ReaSumma(Keel.t("Versioon"), "1.3")
            ReaSumma(Keel.t("Tooteid kataloogis"), olek.tooted.size.toString())
            ReaSumma(Keel.t("Kataloog"), olek.pood.seade("kataloog")?.removePrefix("rimi-")?.replace('T', ' ') ?: "–")
            ReaSumma(Keel.t("Kasutaja"), "${k.nimi} · ${Keel.t(if (k.roll == Roll.Omanik) "Omanik" else "Ostja")}")
        }
        AaarisNupp(Keel.t("Logi välja"), { olek.logiValja() }, Modifier.fillMaxWidth(), V.Punane)
    }
}

@Composable
private fun Osa(pealkiri: String, sisu: @Composable () -> Unit) {
    T(Keel.t(pealkiri).uppercase(), 11.sp, V.Ploom, FontWeight.ExtraBold, modifier = Modifier.padding(start = 6.dp, bottom = 6.dp))
    Kaart(Modifier.fillMaxWidth()) { sisu() }
    Vahe(16.dp)
}

@Composable
private fun Silt(tekst: String, modifier: Modifier = Modifier) {
    T(Keel.t(tekst), 12.sp, V.Tekst2, FontWeight.Bold, modifier = modifier.padding(start = 6.dp, bottom = 4.dp))
}

@Composable
private fun Valik(pealkiri: String, selgitus: String, sees: Boolean, muuda: (Boolean) -> Unit) {
    Row(Modifier.fillMaxWidth().clip(RoundedCornerShape(14.dp)).vajuta { muuda(!sees) }.padding(vertical = 10.dp, horizontal = 4.dp), verticalAlignment = Alignment.CenterVertically) {
        Column(Modifier.weight(1f)) {
            T(Keel.t(pealkiri), 15.sp, V.Tekst, FontWeight.SemiBold)
            T(Keel.t(selgitus), 12.sp, V.Nork)
        }
        Lyliti(sees)
    }
}

// ---------------------------------------------------------------
//  Omanik: müügipaneel (14 päeva tulbad, numbrid, enim müüdud, lõpevad peagi)
// ---------------------------------------------------------------
@Composable
fun MyygipaneelEkraan(olek: AppOlek) {
    val ostud = remember { olek.pood.ostud(null).filter { !it.tagastatud } }
    val nyyd = olek.aeg()
    val paevad = remember { Myyk.paevad(ostud, nyyd) }
    val tana = paevad.last().second
    val kuu = ostud.filter { it.aeg.isAfter(nyyd.minusDays(30)) }
    val eelvaade = LocalInspectionMode.current
    val kasv = remember { Animatable(if (eelvaade) 1f else 0f) }
    LaunchedEffect(Unit) { if (!eelvaade) kasv.animateTo(1f, tween(800)) }
    val lopevad = remember { Autotellimus.arvuta(ostud, olek.tooted, nyyd, 7).take(5) }

    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 40.dp)) {
        Pais("Müügipaneel", { olek.tagasi() }, Keel.f("{0} ostu kokku", ostud.size))
        Vahe(12.dp)
        Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            NumbriKaart(Keel.t("Müük täna"), eur(tana), V.Tume, Color.White, Modifier.weight(1f))
            NumbriKaart(Keel.t("30 päeva"), eur(kuu.sumOf { it.summa }), V.Pind, V.Tekst, Modifier.weight(1f))
        }
        Vahe(10.dp)
        Row(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            NumbriKaart(Keel.t("Keskmine ost"), eur(if (kuu.isEmpty()) 0 else kuu.sumOf { it.summa } / kuu.size), V.Pind, V.Tekst, Modifier.weight(1f))
            NumbriKaart(Keel.t("Ostjaid"), kuu.map { it.kasutaja }.distinct().size.toString(), V.Pind, V.Tekst, Modifier.weight(1f))
        }
        Vahe(14.dp)
        Kaart(Modifier.fillMaxWidth()) {
            T(Keel.t("Müük viimase 14 päeva jooksul"), 15.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(10.dp)
            val max = maxOf(1, paevad.maxOf { it.second })
            val tulp = V.Ploom; val tanaV = V.Punane; val joon = V.Joon
            Canvas(Modifier.fillMaxWidth().height(150.dp)) {
                val w = size.width / paevad.size
                drawLine(joon, Offset(0f, size.height), Offset(size.width, size.height), 2f)
                paevad.forEachIndexed { i, (_, s) ->
                    val h = (size.height - 4) * s / max * kasv.value
                    drawRoundRect(if (i == paevad.lastIndex) tanaV else tulp, Offset(i * w + w * 0.18f, size.height - h), Size(w * 0.64f, h), CornerRadius(8f, 8f))
                }
            }
            Row(Modifier.fillMaxWidth()) {
                paevad.forEachIndexed { i, (p, _) ->
                    T(if (i % 2 == 1) "" else "${p.dayOfMonth}.${p.monthValue}", 9.sp, V.Nork, modifier = Modifier.weight(1f), keskel = true)
                }
            }
        }
        Vahe(14.dp)
        Kaart(Modifier.fillMaxWidth()) {
            T(Keel.t("Enim müüdud"), 15.sp, V.Tekst, FontWeight.ExtraBold)
            Vahe(6.dp)
            val top = remember { Myyk.top(ostud) }
            if (top.isEmpty()) T(Keel.t("Oste veel pole. Seadetes saab luua demoandmed."), 13.sp, V.Nork)
            val maxTk = maxOf(1, top.maxOfOrNull { it.second } ?: 1)
            for ((n, tk, s) in top) {
                Row(Modifier.padding(vertical = 5.dp), verticalAlignment = Alignment.CenterVertically) {
                    Column(Modifier.weight(1f)) {
                        T(n, 13.sp, V.Tekst, FontWeight.SemiBold, read = 1)
                        Box(Modifier.padding(top = 4.dp).fillMaxWidth(tk.toFloat() / maxTk * kasv.value).height(5.dp).clip(RoundedCornerShape(3.dp)).background(V.Ploom))
                    }
                    Spacer(Modifier.width(10.dp))
                    Column(horizontalAlignment = Alignment.End) {
                        T(Keel.f("{0} tk", tk), 13.sp, V.Tekst, FontWeight.Bold)
                        T(eur(s), 11.sp, V.Nork)
                    }
                }
            }
        }
        if (lopevad.isNotEmpty()) {
            Vahe(14.dp)
            Kaart(Modifier.fillMaxWidth(), taust = V.Kreem) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    T(Keel.t("Lõpevad peagi"), 15.sp, V.Oranz, FontWeight.ExtraBold, modifier = Modifier.weight(1f))
                    T(Keel.t("Autotellimus →"), 13.sp, V.Ploom, FontWeight.Bold, Modifier.vajuta { olek.ava(Ekraan.Autotellimus) })
                }
                for (r in lopevad) ReaSumma(r.toode.nimi, Keel.f("{0} p", "%.1f".format(r.jatkub)), suurus = 13.sp)
            }
        }
    }
}

@Composable
private fun NumbriKaart(silt: String, vaartus: String, taust: Color, varv: Color, modifier: Modifier) {
    Column(modifier.clip(RoundedCornerShape(22.dp)).background(taust).padding(14.dp)) {
        T(silt, 12.sp, varv.copy(alpha = 0.7f))
        T(vaartus, 20.sp, varv, FontWeight.ExtraBold, read = 1)
    }
}

// ---------------------------------------------------------------
//  Omanik: autotellimus – mida tellida, et kaup jätkuks valitud päevadeks
// ---------------------------------------------------------------
@Composable
fun AutotellimusEkraan(olek: AppOlek) {
    var kate by remember { mutableIntStateOf(14) }
    val ostud = remember { olek.pood.ostud(null) }
    val read = remember(kate) { Autotellimus.arvuta(ostud, olek.tooted, olek.aeg(), kate) }
    var muudetud by remember { mutableStateOf(mapOf<Int, Int>()) }
    fun telli(r: Autotellimus.Rida) = muudetud[r.toode.id] ?: r.soovitus
    val summa = read.sumOf { telli(it) * it.ostuhind }

    Box(Modifier.fillMaxSize()) {
        Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 130.dp)) {
            Pais("Autotellimus", { olek.tagasi() }, Keel.t("Müük viimased 28 päeva + 3 päeva varu"))
            Vahe(10.dp)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                for (p in listOf(7, 14, 30)) Kiip(Keel.f("{0} päeva", p), V.Pind, V.Tekst, valitud = kate == p, onClick = { kate = p; muudetud = emptyMap() })
            }
            Vahe(12.dp)
            if (read.isEmpty()) Tyhi(Ik.Kast, Keel.t("Midagi pole vaja tellida"), Keel.t("Kõiki müüdud tooteid jätkub valitud perioodiks. Seadetes saab luua demoandmed."))
            for (r in read) {
                val t = telli(r)
                Row(
                    Modifier.fillMaxWidth().padding(vertical = 4.dp).clip(RoundedCornerShape(18.dp)).background(V.Pind).padding(12.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(Modifier.weight(1f)) {
                        T(r.toode.nimi, 14.sp, V.Tekst, FontWeight.Bold, read = 1)
                        T(r.tarnija, 11.sp, V.Nork, read = 1)
                        Vahe(4.dp)
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            val kriitiline = r.jatkub < 4
                            Box(Modifier.width(70.dp).height(6.dp).clip(RoundedCornerShape(3.dp)).background(V.Paneel)) {
                                Box(Modifier.fillMaxWidth((r.jatkub / kate).toFloat().coerceIn(0.04f, 1f)).height(6.dp).clip(RoundedCornerShape(3.dp)).background(if (kriitiline) V.Punane else V.Roheline))
                            }
                            Spacer(Modifier.width(6.dp))
                            T(Keel.f("laos {0} tk · jätkub {1} p", r.toode.kogus, "%.1f".format(r.jatkub)), 11.sp, if (kriitiline) V.Punane else V.Tekst2, FontWeight.Bold)
                        }
                    }
                    KogusValija(t, { muudetud = muudetud + (r.toode.id to maxOf(0, t - 1)) }, { muudetud = muudetud + (r.toode.id to t + 1) })
                }
            }
        }
        if (read.isNotEmpty()) Column(
            Modifier.align(Alignment.BottomCenter).fillMaxWidth().clip(RoundedCornerShape(topStart = 28.dp, topEnd = 28.dp)).background(V.Tume).padding(16.dp)
        ) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                T(Keel.f("{0} toodet · ostuhinnaga", read.count { telli(it) > 0 }), 13.sp, Color(0xFFC9CDD2), modifier = Modifier.weight(1f))
                T(eur(summa), 22.sp, Color.White, FontWeight.ExtraBold)
            }
            Vahe(10.dp)
            SuurNupp(Keel.t("Kinnita tellimus"), {
                olek.teade = Keel.f("Tellimus saadetud: {0} toodet, {1}", read.count { telli(it) > 0 }, eur(summa))
            }, noolega = false)
        }
    }
}

// ---------------------------------------------------------------
//  Omanik: "Ostetakse koos" – paarid, mida ostetakse sageli koos; kampaania ühe vajutusega
// ---------------------------------------------------------------
@Composable
fun KoosEkraan(olek: AppOlek) {
    var kategooriad by remember { mutableStateOf(false) }
    val ostud = remember { olek.pood.ostud(null).filter { !it.tagastatud } }
    val nimed = remember(olek.tooted) { olek.tooted.associate { it.id to it.nimi } }
    val katNimi = remember { olek.kategooriad.associate { it.id to lyhike(it.nimi) } }
    val kat = remember(olek.tooted) { olek.tooted.associate { it.id to it.kategooriaId } }
    val reeglid = remember(kategooriad) {
        Ostukorvianalyys.paarid(
            if (kategooriad) Ostukorvianalyys.reeglid(ostud.map { o -> o.read.mapNotNull { r -> r.toodeId?.let { kat[it] }?.let { katNimi[it] } }.toSet() }, { it })
            else Ostukorvianalyys.reeglid(ostud.map { o -> o.read.mapNotNull { it.toodeId }.toSet() }, { nimed[it] ?: "#$it" })
        )
    }
    var loodud by remember { mutableStateOf(setOf<Int>()) }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 40.dp)) {
        Pais("Ostetakse koos", { olek.tagasi() }, Keel.f("{0} ostu põhjal", ostud.size))
        Vahe(10.dp)
        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
            Kiip(Keel.t("Tooted"), V.Pind, V.Tekst, valitud = !kategooriad, onClick = { kategooriad = false })
            Kiip(Keel.t("Kategooriad"), V.Pind, V.Tekst, valitud = kategooriad, onClick = { kategooriad = true })
        }
        Vahe(12.dp)
        if (reeglid.isEmpty()) Tyhi(Ik.Graafik, Keel.t("Seoseid pole veel"), Keel.t("Paar peab esinema vähemalt 3 ostus. Seadetes saab luua demoandmed."))
        for (r in reeglid) {
            Kaart(Modifier.fillMaxWidth().padding(vertical = 5.dp)) {
                Row(verticalAlignment = Alignment.CenterVertically) {
                    Box(Modifier.size(52.dp).clip(RoundedCornerShape(26.dp)).background(V.Roosa), Alignment.Center) {
                        T("×" + "%.1f".format(r.tous), 14.sp, V.Ploom, FontWeight.ExtraBold)
                    }
                    Spacer(Modifier.width(12.dp))
                    Column(Modifier.weight(1f)) {
                        T(r.a, 14.sp, V.Tekst, FontWeight.Bold, read = 2)
                        T("→ " + r.b, 14.sp, V.Ploom, FontWeight.Bold, read = 2)
                        T(Keel.f("{0} % ostis ka · koos {1} korda", Math.round(r.kindlus * 100), r.koos), 11.sp, V.Nork)
                    }
                }
                val b = r.toodeB
                if (!kategooriad && b != null) {
                    Vahe(10.dp)
                    if (b in loodud) VaikeKiip("✓ " + Keel.t("Kampaania 7 päeva"), V.RohelineHele, V.Roheline)
                    else AaarisNupp(Keel.t("Loo kampaania −10 %"), {
                        olek.pood.lisaKampaania(Kampaania(0, b, 10, LocalDate.now(), LocalDate.now().plusDays(7)))
                        olek.varskendaTooted()
                        loodud = loodud + b
                        olek.teade = Keel.f("Kampaania loodud: {0} −10 %", r.b)
                    }, Modifier.fillMaxWidth())
                }
            }
        }
    }
}

// ---------------------------------------------------------------
//  Omanik: kliendid (kaart, boonus, ostude arv ja summa)
// ---------------------------------------------------------------
@Composable
fun KliendidEkraan(olek: AppOlek) {
    var otsi by remember { mutableStateOf("") }
    val kasutajad = remember { olek.pood.kasutajad() }
    val ostud = remember { olek.pood.ostud(null).filter { !it.tagastatud }.groupBy { it.kasutaja } }
    val nahtavad = kasutajad.filter { otsi.isBlank() || it.taisNimi.contains(otsi, true) || it.nimi.contains(otsi, true) || it.kaart.contains(otsi) || (it.email ?: "").contains(otsi, true) }
    Column(Modifier.fillMaxSize().verticalScroll(rememberScrollState()).padding(16.dp).padding(bottom = 40.dp)) {
        Pais("Kliendid", { olek.tagasi() }, Keel.f("{0} kasutajat", kasutajad.size))
        Vahe(10.dp)
        Sisend(otsi, { otsi = it }, Keel.t("Otsi nime, kaardi või e-posti järgi"), taust = V.Pind, ees = { Ikoon(Ik.Otsi, V.Tekst2, 18.dp) })
        Vahe(10.dp)
        for (k in nahtavad) {
            val o = ostud[k.nimi] ?: emptyList()
            Row(
                Modifier.fillMaxWidth().padding(vertical = 4.dp).clip(RoundedCornerShape(18.dp)).background(V.Pind).padding(12.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Initsiaalid(k.initsiaalid, if (k.roll == Roll.Omanik) V.Tume else V.Ploom, 42.dp)
                Spacer(Modifier.width(12.dp))
                Column(Modifier.weight(1f)) {
                    T(k.taisNimi, 15.sp, V.Tekst, FontWeight.Bold, read = 1)
                    T(listOfNotNull(Keel.f("kaart {0}", k.kaart), k.email).joinToString(" · "), 11.sp, V.Nork, read = 1)
                }
                Column(horizontalAlignment = Alignment.End) {
                    T(eur(o.sumOf { it.summa }), 14.sp, V.Tekst, FontWeight.ExtraBold)
                    T(Keel.f("{0} ostu · boonus {1}", o.size, eur(k.boonus)), 11.sp, V.Nork)
                }
            }
        }
    }
}
