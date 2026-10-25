package ee.akt.pood

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.BackHandler
import androidx.activity.compose.setContent
import androidx.compose.animation.AnimatedContent
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.core.tween
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.slideInHorizontally
import androidx.compose.animation.slideInVertically
import androidx.compose.animation.slideOutHorizontally
import androidx.compose.animation.slideOutVertically
import androidx.compose.animation.togetherWith
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.safeDrawingPadding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.remember
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.mutableFloatStateOf
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.animation.core.animateFloatAsState
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import kotlin.concurrent.thread
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.foundation.layout.Arrangement
import kotlinx.coroutines.delay

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // Esimesel käivitamisel laaditakse ~18 000 Rimi toodet andmebaasi: taustalõimes, ekraanil sujuv edenemisriba
        val olekHoidja = mutableStateOf<AppOlek?>(null)
        val edenemine = mutableFloatStateOf(0f)
        val tehtud = mutableIntStateOf(0)
        Rimi.edenemine = { t, k -> runOnUiThread { tehtud.intValue = t; edenemine.floatValue = t.toFloat() / k } }
        thread(name = "andmebaas") {
            val pood = SqlPood(applicationContext)
            pood.readableDatabase   // käivitab onCreate (kataloogi laadimine) siin, mitte UI-lõimes
            val o = AppOlek(pood)
            runOnUiThread { olekHoidja.value = o }
        }
        val eelistused = getSharedPreferences("akt", MODE_PRIVATE)
        Teema.valik = eelistused.getInt("teema", 0)
        Teema.salvesta = { eelistused.edit().putInt("teema", it).apply() }
        setContent {
            val susteemTume = isSystemInDarkTheme()
            Teema.tume = when (Teema.valik) { 1 -> false; 2 -> true; else -> susteemTume }
            val vaade = LocalView.current
            SideEffect { WindowCompat.getInsetsController(window, vaade).isAppearanceLightStatusBars = !Teema.tume }
            val olek = olekHoidja.value
            AnimatedContent(olek, transitionSpec = { fadeIn(tween(400)) togetherWith fadeOut(tween(300)) }, label = "algus") { o ->
                if (o == null) Laadimine(edenemine.floatValue, tehtud.intValue)
                else {
                    BackHandler { if (!o.tagasi()) finish() }
                    App(o)
                }
            }
        }
    }
}

private val vahelehed = listOf(
    Triple("Avaleht", Ik.Kodu, Ekraan.Avaleht as Ekraan),
    Triple("Ostunimekiri", Ik.Nimekiri, Ekraan.Nimekiri),
    Triple("Ostukorv", Ik.Korv, Ekraan.Korv),
    Triple("Kliendikaart", Ik.Kaart, Ekraan.Kaart),
    Triple("Profiil", Ik.Profiil, Ekraan.Profiil),
)

// Kas alumine menüü on nähtav (põhilehtedel)
private fun menyyga(e: Ekraan) = e is Ekraan.Avaleht || e is Ekraan.Nimekiri || e is Ekraan.Kaart || e is Ekraan.Profiil ||
    e is Ekraan.Otsing || e is Ekraan.Ruudustik || (e is Ekraan.Korv)

@Composable
fun App(olek: AppOlek) {
    Box(Modifier.fillMaxSize().background(V.Taust).safeDrawingPadding()) {
        AnimatedContent(
            olek.ekraan,
            transitionSpec = {
                (slideInHorizontally(tween(360)) { it / 6 } + fadeIn(tween(360))) togetherWith fadeOut(tween(200))
            },
            label = "ekraan"
        ) { e ->
            Box(Modifier.fillMaxSize()) { Ekraanid(olek, e) }
        }
        val e = olek.ekraan
        if (menyyga(e) && !(e is Ekraan.Korv && olek.korv.isNotEmpty()))
            AlumineMenyy(olek, Modifier.align(Alignment.BottomCenter))
        Teade(olek, Modifier.align(Alignment.TopCenter))
    }
}

@Composable
fun Ekraanid(olek: AppOlek, e: Ekraan) {
    when (e) {
        Ekraan.Sisse -> SisseEkraan(olek)
        Ekraan.Registreeru -> RegistreeruEkraan(olek)
        Ekraan.Avaleht -> AvalehtEkraan(olek)
        is Ekraan.Otsing -> OtsingEkraan(olek, e.algus)
        is Ekraan.Ruudustik -> RuudustikEkraan(olek, e)
        is Ekraan.ToodeLeht -> ToodeEkraan(olek, e.id)
        Ekraan.Korv -> KorvEkraan(olek)
        Ekraan.Maksmine -> MaksmineEkraan(olek)
        is Ekraan.MakseOnnestus -> MakseOnnestusEkraan(olek, e.ostId)
        Ekraan.Kaart -> KaartEkraan(olek)
        is Ekraan.Preemiad -> PreemiadEkraan(olek, e.vaheleht)
        Ekraan.Nimekiri -> NimekiriEkraan(olek)
        Ekraan.Profiil -> ProfiilEkraan(olek)
        Ekraan.Ajalugu -> AjaluguEkraan(olek)
        Ekraan.Admin -> AdminEkraan(olek)
        is Ekraan.ToodeMuuda -> ToodeMuudaEkraan(olek, e.id)
        Ekraan.Dieet -> DieetEkraan(olek)
        Ekraan.Kampaaniad -> KampaaniadEkraan(olek)
        Ekraan.Ohtu -> OhtuEkraan(olek)
        Ekraan.Toitevaartus -> ToitevaartusEkraan(olek)
        Ekraan.Pere -> PereEkraan(olek)
        Ekraan.Kulutused -> KulutusedEkraan(olek)
        Ekraan.PereKorv -> PereKorvEkraan(olek)
        Ekraan.Seaded -> SeadedEkraan(olek)
        Ekraan.Myygipaneel -> MyygipaneelEkraan(olek)
        Ekraan.Autotellimus -> AutotellimusEkraan(olek)
        Ekraan.Koos -> KoosEkraan(olek)
        Ekraan.Kliendid -> KliendidEkraan(olek)
    }
}

// Must hõljuv menüüriba: aktiivne vaheleht valges ringis, ostukorvil punane märk
@Composable
fun AlumineMenyy(olek: AppOlek, modifier: Modifier = Modifier) {
    Row(
        modifier.padding(16.dp).fillMaxWidth().height(68.dp).clip(RoundedCornerShape(34.dp)).background(V.Tume).padding(horizontal = 8.dp),
        horizontalArrangement = Arrangement.SpaceAround, verticalAlignment = Alignment.CenterVertically
    ) {
        val praegu = olek.pinu.first()
        for ((_, ikoon, siht) in vahelehed) {
            val aktiivne = praegu == siht
            Box(Modifier.size(52.dp)) {
                Box(
                    Modifier.size(52.dp).clip(CircleShape).background(if (aktiivne) Color.White else Color.Transparent).vajuta { olek.vaheleht(siht) },
                    Alignment.Center
                ) { Ikoon(ikoon, if (aktiivne) V.Tekst else Color(0xFFD5D8DD), 22.dp) }
                if (siht == Ekraan.Korv && olek.korvisTk > 0)
                    Box(
                        Modifier.align(Alignment.TopEnd).offset(2.dp, (-2).dp).size(20.dp).clip(CircleShape).background(V.Punane),
                        Alignment.Center
                    ) { T("${olek.korvisTk}", 10.sp, Color.White, FontWeight.ExtraBold) }
            }
        }
    }
}

// Lühike teade ülal (kaob 2,5 s pärast)
@Composable
fun Teade(olek: AppOlek, modifier: Modifier = Modifier) {
    val tekst = olek.teade
    LaunchedEffect(tekst) {
        if (tekst != null) { delay(2500); olek.teade = null }
    }
    AnimatedVisibility(tekst != null, modifier, enter = slideInVertically { -it } + fadeIn(), exit = slideOutVertically { -it } + fadeOut()) {
        Row(
            Modifier.padding(16.dp).fillMaxWidth().clip(RoundedCornerShape(20.dp)).background(V.Tume).padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(Modifier.size(28.dp).clip(CircleShape).background(V.Roheline), Alignment.Center) { Ikoon(Ik.Linnuke, Color.White, 16.dp, 3f) }
            androidx.compose.foundation.layout.Spacer(Modifier.size(12.dp))
            T(remember(tekst) { tekst ?: "" }, 14.sp, Color.White, FontWeight.SemiBold)
        }
    }
}

// Esimese käivitamise ekraan: tooted lisanduvad andmebaasi, riba liigub sujuvalt
@Composable
fun Laadimine(osa: Float, tehtud: Int) {
    val riba by animateFloatAsState(osa, tween(600), label = "riba")
    Box(Modifier.fillMaxSize().background(V.Tume).safeDrawingPadding().padding(28.dp)) {
        Column(Modifier.align(Alignment.Center).fillMaxWidth()) {
            Box(Modifier.size(64.dp).clip(RoundedCornerShape(20.dp)).background(V.Punane))
            Spacer(Modifier.height(24.dp))
            T("AKT pood", 28.sp, Color.White, FontWeight.Bold)
            Spacer(Modifier.height(8.dp))
            T(Keel.t("Laadin Rimi tooteid…"), 15.sp, Color(0xFFC9CDD2))
            Spacer(Modifier.height(20.dp))
            Box(Modifier.fillMaxWidth().height(8.dp).clip(RoundedCornerShape(4.dp)).background(V.Tume2)) {
                Box(Modifier.fillMaxWidth(riba.coerceIn(0.02f, 1f)).height(8.dp).clip(RoundedCornerShape(4.dp)).background(V.Roheline))
            }
            Spacer(Modifier.height(10.dp))
            T(if (tehtud > 0) Keel.f("{0} toodet", tehtud) else Keel.t("Ainult esimesel käivitamisel"), 12.sp, V.Nork)
        }
    }
}
