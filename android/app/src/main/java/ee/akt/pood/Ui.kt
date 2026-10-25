package ee.akt.pood

import androidx.compose.animation.core.Spring
import androidx.compose.animation.core.animateFloatAsState
import androidx.compose.animation.core.spring
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.interaction.collectIsPressedAsState
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxScope
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.scale
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.graphics.vector.addPathNodes
import androidx.compose.ui.graphics.vector.rememberVectorPainter
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.TextUnit
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.foundation.text.BasicText
import coil.compose.AsyncImage

// ---- Värvid (lõuendi "Moodne" palett) ----
// Teema: hele / tume / nagu süsteemis (Seadetes). Compose jälgib "tume" väärtust – värvid vahetuvad kõikjal.
object Teema {
    var tume by mutableStateOf(false)
    var valik by mutableStateOf(0)        // 0 = nagu süsteemis, 1 = hele, 2 = tume
    var salvesta: (Int) -> Unit = {}      // MainActivity salvestab valiku (SharedPreferences)
}

object V {
    private fun v(hele: Long, tume: Long) = Color(if (Teema.tume) tume else hele)
    val Taust get() = v(0xFFE9EDEF, 0xFF121316)
    val Pind get() = v(0xFFFFFFFF, 0xFF1E1F23)
    val Tekst get() = v(0xFF1B1C1F, 0xFFECEDEF)
    val Tekst2 get() = v(0xFF5C6168, 0xFFB4B9C0)
    val Nork get() = v(0xFF8A9099, 0xFF8E949C)
    val Joon get() = v(0xFFE2E6E8, 0xFF34363B)
    val Paneel get() = v(0xFFF1F3F4, 0xFF2A2B30)
    val Punane get() = v(0xFFE2061A, 0xFFE8202F)
    val Ploom get() = v(0xFFA0236E, 0xFFC0408A)
    val Roosa get() = v(0xFFF6E6EF, 0xFF3A2231)
    val Roheline get() = v(0xFF148246, 0xFF239A58)
    val RohelineHele get() = v(0xFFE6F4EC, 0xFF183A27)
    val Kreem get() = v(0xFFFFF2D6, 0xFF3A2F16)
    val Oranz get() = v(0xFFB26200, 0xFFF0A040)
    val Tume get() = v(0xFF1B1C1F, 0xFF2C2D32)
    val Tume2 get() = v(0xFF2A2B2F, 0xFF3A3B41)
}

// ---- Tekst ----
@Composable
fun T(
    tekst: String,
    suurus: TextUnit = 14.sp,
    varv: Color = V.Tekst,
    paks: FontWeight = FontWeight.Normal,
    modifier: Modifier = Modifier,
    read: Int = Int.MAX_VALUE,
    labi: Boolean = false,
    reavahe: TextUnit = TextUnit.Unspecified,
    keskel: Boolean = false,
) {
    BasicText(
        tekst, modifier,
        style = TextStyle(
            fontSize = suurus, color = varv, fontWeight = paks, lineHeight = reavahe,
            textDecoration = if (labi) TextDecoration.LineThrough else null,
            textAlign = if (keskel) androidx.compose.ui.text.style.TextAlign.Center else androidx.compose.ui.text.style.TextAlign.Start
        ),
        maxLines = read, overflow = TextOverflow.Ellipsis
    )
}

// ---- Ikoonid: SVG teed 24×24 vaateväljas (samad mis lõuendil) ----
object Ik {
    const val Kodu = "M3 11l9-8 9 8M5 9v12h14V9M10 21v-6h4v6"
    const val Nimekiri = "M9 6h11M9 12h11M9 18h11M4 6h.01M4 12h.01M4 18h.01"
    const val Korv = "M9 20a1 1 0 1 0 0 .01M18 20a1 1 0 1 0 0 .01M2 3h3l2.7 12.4a2 2 0 0 0 2 1.6h8.6a2 2 0 0 0 2-1.6L22 7H6"
    const val Kaart = "M2 5h20v14H2zM2 10h20M6 15h4"
    const val Profiil = "M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"
    const val Otsi = "M11 4a7 7 0 1 0 0 14 7 7 0 0 0 0-14zM20 20l-3.5-3.5"
    const val Pluss = "M12 5v14M5 12h14"
    const val Miinus = "M5 12h14"
    const val Tagasi = "M19 12H5M12 19l-7-7 7-7"
    const val Vasak = "M15 18l-6-6 6-6"
    const val Parem = "M9 18l6-6-6-6"
    const val Nool = "M5 12h14M12 5l7 7-7 7"
    const val Prygi = "M3 6h18M8 6V4h8v2M19 6l-1 14H6L5 6"
    const val Syda = "M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-8.6a5.5 5.5 0 0 0 0-7.8z"
    const val Taht = "M12 2l3.1 6.3 6.9 1-5 4.9 1.2 6.8L12 17.8 5.8 21l1.2-6.8-5-4.9 6.9-1z"
    const val Sulge = "M18 6L6 18M6 6l12 12"
    const val Linnuke = "M20 6L9 17l-5-5"
    const val Kaamera = "M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2zM12 17a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"
    const val Kingitus = "M20 12v10H4V12M2 7h20v5H2zM12 22V7M12 7H7.5a2.5 2.5 0 0 1 0-5C11 2 12 7 12 7zM12 7h4.5a2.5 2.5 0 0 0 0-5C13 2 12 7 12 7z"
    const val Auto = "M1 3h15v13H1zM16 8h4l3 3v5h-7zM5.5 21a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5zM18.5 21a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z"
    const val Asukoht = "M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0zM12 13a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"
    const val Silm = "M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8zM12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"
    const val Leht = "M11 20A7 7 0 0 1 9.8 6.1C15.5 5 17 4.5 19 2c1 2 2 4.2 2 8 0 5.5-4.8 10-10 10zM2 21c0-3 1.9-5.4 5.1-6"
    const val Kell = "M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20zM12 6v6l4 2"
    const val Valja = "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9"
    const val Kasutajad = "M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"
    const val Silt = "M20.6 13.4l-7.2 7.2a2 2 0 0 1-2.8 0L2 12V2h10l8.6 8.6a2 2 0 0 1 0 2.8zM7 7h.01"
    const val Graafik = "M3 3v18h18M7 16l4-4 4 4 5-6"
    const val Tsekk = "M4 2v20l2-1 2 1 2-1 2 1 2-1 2 1 2-1 2 1V2l-2 1-2-1-2 1-2-1-2 1-2-1-2 1zM8 8h8M8 12h8M8 16h5"
    const val Pliiats = "M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"
    const val Info = "M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20zM12 16v-4M12 8h.01"
    const val Hoiatus = "M10.3 3.9L1.8 18a2 2 0 0 0 1.7 3h16.9a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0zM12 9v4M12 17h.01"
    const val Kuu = "M21 12.8A9 9 0 1 1 11.2 3 7 7 0 0 0 21 12.8z"
    const val Pudel = "M10 2h4M10 2v4l-2 3v12a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V9l-2-3V2"
    const val Jaga = "M18 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM6 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM18 22a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM8.6 13.5l6.8 4M15.4 6.5l-6.8 4"
    const val Kopeeri = "M9 9h13v13H9zM5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"
    const val Kott = "M6 7h12l-1 14H7zM9 7a3 3 0 0 1 6 0"
    const val Ratas = "M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z"
    const val Kast = "M21 8l-9-5-9 5v8l9 5 9-5zM3 8l9 5 9-5M12 13v8"
}

@Composable
fun Ikoon(tee: String, varv: Color = V.Tekst, suurus: Dp = 20.dp, paksus: Float = 2f, modifier: Modifier = Modifier) {
    val vektor = remember(tee, paksus) {
        ImageVector.Builder(defaultWidth = 24.dp, defaultHeight = 24.dp, viewportWidth = 24f, viewportHeight = 24f)
            .addPath(
                pathData = addPathNodes(tee), fill = null, stroke = SolidColor(Color.Black), strokeLineWidth = paksus,
                strokeLineCap = StrokeCap.Round, strokeLineJoin = StrokeJoin.Round
            ).build()
    }
    Image(rememberVectorPainter(vektor), null, modifier.size(suurus), colorFilter = ColorFilter.tint(varv))
}

// ---- Vajutus: kerge kokkutõmbumine (lõuendi .press) ----
@Composable
fun Modifier.vajuta(onClick: () -> Unit): Modifier {
    val allikas = remember { MutableInteractionSource() }
    val all by allikas.collectIsPressedAsState()
    val s by animateFloatAsState(if (all) 0.95f else 1f, spring(dampingRatio = Spring.DampingRatioMediumBouncy), label = "vajutus")
    return this.scale(s).clickable(allikas, null, onClick = onClick)
}

@Composable
fun Kaart(
    modifier: Modifier = Modifier,
    taust: Color = V.Pind,
    raadius: Dp = 22.dp,
    sisu: Dp = 16.dp,
    onClick: (() -> Unit)? = null,
    content: @Composable ColumnScope.() -> Unit,
) {
    var m = modifier.clip(RoundedCornerShape(raadius)).background(taust)
    if (onClick != null) m = m.clickable(onClick = onClick)
    Column(m.padding(sisu), content = content)
}

// Must "← Tagasi" pill + pealkiri (kõigi alamlehtede päis)
@Composable
fun Pais(pealkiri: String, tagasi: (() -> Unit)?, alapealkiri: String? = null, parem: @Composable RowScope.() -> Unit = {}) {
    Row(Modifier.fillMaxWidth().height(52.dp), verticalAlignment = Alignment.CenterVertically) {
        if (tagasi != null) {
            Row(
                Modifier.height(48.dp).clip(RoundedCornerShape(24.dp)).background(V.Tume).vajuta(tagasi).padding(horizontal = 16.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Ikoon(Ik.Tagasi, Color.White, 18.dp, 2.4f)
                Spacer(Modifier.width(8.dp))
                T(Keel.t("Tagasi"), 15.sp, Color.White, FontWeight.Bold)
            }
            Spacer(Modifier.width(12.dp))
        }
        Column(Modifier.weight(1f)) {
            T(Keel.t(pealkiri), 22.sp, V.Tekst, FontWeight.ExtraBold, read = 1)
            if (alapealkiri != null) T(alapealkiri, 12.sp, V.Nork, read = 1)
        }
        parem()
    }
}

@Composable
fun SuurNupp(
    tekst: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    taust: Color = V.Punane,
    varv: Color = Color.White,
    noolega: Boolean = true,
    lubatud: Boolean = true,
    ikoon: String = Ik.Nool,
) {
    Row(
        modifier.fillMaxWidth().height(56.dp).clip(RoundedCornerShape(28.dp))
            .background(if (lubatud) taust else V.Joon)
            .then(if (lubatud) Modifier.vajuta(onClick) else Modifier)
            .padding(start = 22.dp, end = 8.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        T(tekst, 16.sp, if (lubatud) varv else V.Nork, FontWeight.ExtraBold, Modifier.weight(1f), read = 1)
        if (noolega && lubatud)
            Box(Modifier.size(40.dp).clip(CircleShape).background(if (taust == V.Pind) V.Tume else Color.White), Alignment.Center) {
                Ikoon(ikoon, if (taust == V.Pind) Color.White else taust, 18.dp, 2.6f)
            }
    }
}

@Composable
fun AaarisNupp(tekst: String, onClick: () -> Unit, modifier: Modifier = Modifier, varv: Color = V.Tekst, taust: Color = Color.Transparent) {
    Box(
        modifier.height(52.dp).clip(RoundedCornerShape(26.dp)).background(taust).border(1.5.dp, varv.copy(alpha = 0.25f), RoundedCornerShape(26.dp))
            .vajuta(onClick).padding(horizontal = 18.dp),
        Alignment.Center
    ) { T(tekst, 15.sp, varv, FontWeight.Bold) }
}

@Composable
fun Kiip(tekst: String, taust: Color, varv: Color, modifier: Modifier = Modifier, valitud: Boolean = false, onClick: (() -> Unit)? = null) {
    Box(
        modifier.height(36.dp).clip(RoundedCornerShape(18.dp)).background(if (valitud) V.Ploom else taust)
            .then(if (onClick != null) Modifier.vajuta(onClick) else Modifier).padding(horizontal = 14.dp),
        Alignment.Center
    ) { T(tekst, 13.sp, if (valitud) Color.White else varv, FontWeight.Bold, read = 1) }
}

@Composable
fun VaikeKiip(tekst: String, taust: Color, varv: Color, modifier: Modifier = Modifier) {
    Box(modifier.clip(RoundedCornerShape(10.dp)).background(taust).padding(horizontal = 8.dp, vertical = 3.dp)) {
        T(tekst, 11.sp, varv, FontWeight.ExtraBold, read = 1)
    }
}

@Composable
fun RingNupp(tee: String, onClick: () -> Unit, taust: Color = V.Punane, varv: Color = Color.White, suurus: Dp = 40.dp, modifier: Modifier = Modifier) {
    Box(modifier.size(suurus).clip(CircleShape).background(taust).vajuta(onClick), Alignment.Center) {
        Ikoon(tee, varv, suurus * 0.45f, 2.6f)
    }
}

// Toote pilt: veebipilt (internet) või rakendusse kaasa pandud pilt; puudumisel kotiikoon
@Composable
fun ToodePilt(pilt: String?, modifier: Modifier = Modifier, ikooniSuurus: Dp = 32.dp) {
    Box(modifier, Alignment.Center) {
        Ikoon(Ik.Kott, V.Joon, ikooniSuurus, 1.8f)
        if (!pilt.isNullOrBlank()) {
            val url = if (pilt.startsWith("http")) pilt else "file:///android_asset/pildid/$pilt"
            AsyncImage(url, null, Modifier.fillMaxSize(), contentScale = ContentScale.Fit)
        }
    }
}


@Composable
fun KogusValija(kogus: Int, miinus: () -> Unit, pluss: () -> Unit, modifier: Modifier = Modifier, tume: Boolean = false) {
    Row(
        modifier.height(40.dp).clip(RoundedCornerShape(20.dp)).background(if (tume) V.Tume2 else V.Paneel),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Box(Modifier.size(40.dp).vajuta(miinus), Alignment.Center) { Ikoon(Ik.Miinus, if (tume) Color.White else V.Tekst, 16.dp, 2.6f) }
        T("$kogus", 15.sp, if (tume) Color.White else V.Tekst, FontWeight.ExtraBold, Modifier.width(26.dp), keskel = true)
        Box(Modifier.size(40.dp).vajuta(pluss), Alignment.Center) { Ikoon(Ik.Pluss, if (tume) Color.White else V.Tekst, 16.dp, 2.6f) }
    }
}

// Valge ümar sisestuslahter (lõuendi .sisend)
@Composable
fun Sisend(
    vaartus: String,
    muutus: (String) -> Unit,
    vihje: String,
    modifier: Modifier = Modifier,
    parool: Boolean = false,
    number: Boolean = false,
    taust: Color = V.Paneel,
    ees: (@Composable () -> Unit)? = null,
    taga: (@Composable () -> Unit)? = null,
) {
    Row(
        modifier.fillMaxWidth().height(54.dp).clip(RoundedCornerShape(27.dp)).background(taust).padding(horizontal = 18.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        if (ees != null) { ees(); Spacer(Modifier.width(12.dp)) }
        Box(Modifier.weight(1f)) {
            if (vaartus.isEmpty()) T(vihje, 15.sp, V.Nork, read = 1)
            BasicTextField(
                vaartus, muutus, Modifier.fillMaxWidth(), singleLine = true,
                textStyle = TextStyle(fontSize = 15.sp, color = V.Tekst),
                visualTransformation = if (parool) PasswordVisualTransformation() else VisualTransformation.None,
                keyboardOptions = KeyboardOptions(keyboardType = if (parool) KeyboardType.Password else if (number) KeyboardType.Decimal else KeyboardType.Text),
                cursorBrush = SolidColor(V.Ploom)
            )
        }
        if (taga != null) taga()
    }
}

// Ringikujuline edenemine (nt tasuta tarneni 34 %)
@Composable
fun Ring(osa: Float, varv: Color, modifier: Modifier = Modifier, paksus: Dp = 7.dp, taust: Color = Color(0xFFE8EDEF), content: @Composable BoxScope.() -> Unit = {}) {
    val a by animateFloatAsState(osa.coerceIn(0f, 1f), androidx.compose.animation.core.tween(1400), label = "ring")
    Box(modifier, Alignment.Center) {
        androidx.compose.foundation.Canvas(Modifier.fillMaxSize()) {
            val p = paksus.toPx()
            val st = androidx.compose.ui.graphics.drawscope.Stroke(p, cap = StrokeCap.Round)
            val r = androidx.compose.ui.geometry.Size(size.width - p, size.height - p)
            val o = androidx.compose.ui.geometry.Offset(p / 2, p / 2)
            drawArc(taust, 0f, 360f, false, o, r, style = st)
            if (a > 0f) drawArc(varv, -90f, 360f * a, false, o, r, style = st)
        }
        content()
    }
}

// Tulbaga edenemisriba
@Composable
fun Riba(osa: Float, varv: Color, modifier: Modifier = Modifier, taust: Color = V.Joon, korgus: Dp = 8.dp) {
    val a by animateFloatAsState(osa.coerceIn(0f, 1f), androidx.compose.animation.core.tween(900), label = "riba")
    Box(modifier.fillMaxWidth().height(korgus).clip(RoundedCornerShape(korgus / 2)).background(taust)) {
        Box(Modifier.fillMaxWidth(a).height(korgus).clip(RoundedCornerShape(korgus / 2)).background(varv))
    }
}

@Composable
fun Vahe(k: Dp) = Spacer(Modifier.height(k))

@Composable
fun ReaSumma(silt: String, summa: String, varv: Color = V.Tekst2, paks: Boolean = false, suurus: TextUnit = 14.sp) {
    Row(Modifier.fillMaxWidth().padding(vertical = 3.dp)) {
        T(silt, suurus, varv, if (paks) FontWeight.ExtraBold else FontWeight.Normal, Modifier.weight(1f))
        T(summa, suurus, varv, if (paks) FontWeight.ExtraBold else FontWeight.SemiBold)
    }
}

@Composable
fun Initsiaalid(tekst: String, taust: Color = V.Ploom, suurus: Dp = 44.dp) {
    Box(Modifier.size(suurus).clip(CircleShape).background(taust), Alignment.Center) {
        T(tekst, (suurus.value * 0.34f).sp, Color.White, FontWeight.ExtraBold)
    }
}

@Composable
fun Hinnasilt(hind: Int, varv: Color = V.Tekst, suurus: TextUnit = 22.sp) {
    T(eur(hind), suurus, varv, FontWeight.ExtraBold)
}

@Composable
fun Tyhi(ikoon: String, pealkiri: String, tekst: String, modifier: Modifier = Modifier) {
    Column(modifier.fillMaxWidth().padding(24.dp), horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.Center) {
        Box(Modifier.size(100.dp).clip(CircleShape).background(V.Roosa), Alignment.Center) { Ikoon(ikoon, V.Ploom, 44.dp, 2.2f) }
        Vahe(18.dp)
        T(pealkiri, 19.sp, V.Tekst, FontWeight.ExtraBold, keskel = true)
        Vahe(6.dp)
        T(tekst, 14.sp, V.Tekst2, keskel = true)
    }
}
