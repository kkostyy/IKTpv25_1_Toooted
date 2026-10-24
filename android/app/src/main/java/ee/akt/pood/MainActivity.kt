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
        val olek = AppOlek(SqlPood(applicationContext))
        setContent {
            BackHandler { if (!olek.tagasi()) finish() }
            App(olek)
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
