package ee.akt.pood

import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalClipboardManager
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.google.zxing.BarcodeFormat
import com.google.zxing.EncodeHintType
import com.google.zxing.qrcode.QRCodeWriter
import com.google.zxing.qrcode.decoder.ErrorCorrectionLevel

// Pangaülekanne (nagu arvutiversioonis Pangamakse.cs): raha otse poe kontole.
// Ostja näeb saajat, IBAN-i, summat ja selgitust + EPC QR-koodi; tellimus "Ootab makset",
// kuni omanik märgib selle makstuks (seade "makstud:<ostId>").
object Pangamakse {
    const val VIIS = "Pangaülekanne"
    private const val SAAJA = "KONSTANTIN GAPONENKO"
    private const val IBAN = "EE502200221075082971"

    fun saaja(pood: Pood) = pood.seade("pank_saaja") ?: SAAJA
    fun iban(pood: Pood) = (pood.seade("pank_iban") ?: IBAN).replace(" ", "")
    fun loetav(iban: String) = iban.chunked(4).joinToString(" ")
    fun selgitus(ostId: Int) = "AKT tellimus $ostId"

    fun ootab(pood: Pood, o: Ost) = o.makseviis == VIIS && !o.tagastatud && pood.seade("makstud:${o.id}") != "1"
    fun margiMakstuks(pood: Pood, ostId: Int) = pood.seaSeade("makstud:$ostId", "1")

    // EPC QR (SEPA ülekanne): pangaäpp täidab kõik väljad ise
    fun epc(pood: Pood, summa: Int, selgitus: String): String =
        listOf("BCD", "002", "1", "SCT", "", saaja(pood), iban(pood), "EUR%d.%02d".format(summa / 100, summa % 100), "", "", selgitus).joinToString("\n")

    fun qr(tekst: String): Array<BooleanArray>? = runCatching {
        val m = QRCodeWriter().encode(tekst, BarcodeFormat.QR_CODE, 0, 0,
            mapOf(EncodeHintType.ERROR_CORRECTION to ErrorCorrectionLevel.M, EncodeHintType.MARGIN to 1, EncodeHintType.CHARACTER_SET to "UTF-8"))
        Array(m.height) { y -> BooleanArray(m.width) { x -> m.get(x, y) } }
    }.getOrNull()
}

// QR + rekvisiidid kopeerimisnuppudega (tumedal taustal)
@Composable
fun PangaRekvisiidid(olek: AppOlek, ost: Ost) {
    val saaja = Pangamakse.saaja(olek.pood)
    val iban = Pangamakse.iban(olek.pood)
    val selgitus = Pangamakse.selgitus(ost.id)
    val maatriks = remember(ost.id) { Pangamakse.qr(Pangamakse.epc(olek.pood, ost.summa, selgitus)) }
    val louend = LocalClipboardManager.current
    var kopeeritud by remember { mutableStateOf<String?>(null) }

    Column(Modifier.fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally) {
        if (maatriks != null)
            Box(Modifier.size(210.dp).clip(RoundedCornerShape(18.dp)).background(Color.White).padding(12.dp)) {
                Canvas(Modifier.size(186.dp)) {
                    val n = maatriks.size
                    val k = size.width / n
                    for (y in 0 until n) for (x in 0 until n)
                        if (maatriks[y][x]) drawRect(Color.Black, Offset(x * k, y * k), Size(k + 0.5f, k + 0.5f))
                }
            }
        Vahe(8.dp)
        T(Keel.t("Skaneeri pangaäpis või maksa internetipangas:"), 12.sp, Color(0xFFC9CCD1), keskel = true)
        Vahe(10.dp)
        for ((silt, naita, kopeeri) in listOf(
            Triple(Keel.t("Saaja"), saaja, saaja),
            Triple("IBAN", Pangamakse.loetav(iban), iban),
            Triple(Keel.t("Summa"), eur(ost.summa), "%d.%02d".format(ost.summa / 100, ost.summa % 100)),
            Triple(Keel.t("Selgitus"), selgitus, selgitus),
        )) {
            Row(
                Modifier.fillMaxWidth().padding(bottom = 6.dp).clip(RoundedCornerShape(14.dp)).background(V.Tume2).padding(horizontal = 14.dp, vertical = 8.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Column(Modifier.weight(1f)) {
                    T(silt, 11.sp, Color(0xFFC9CCD1))
                    T(naita, 15.sp, Color.White, FontWeight.Bold, read = 1)
                }
                Spacer(Modifier.width(8.dp))
                val ok = kopeeritud == silt
                Box(
                    Modifier.clip(RoundedCornerShape(14.dp)).background(if (ok) V.Roheline else Color(0xFF4A4C52))
                        .vajuta { louend.setText(AnnotatedString(kopeeri)); kopeeritud = silt }.padding(horizontal = 12.dp, vertical = 7.dp)
                ) { T(Keel.t(if (ok) "Kopeeritud" else "Kopeeri"), 12.sp, Color.White, FontWeight.Bold) }
            }
        }
        Vahe(4.dp)
        T(Keel.t("Kaup saadetakse / antakse kätte pärast makse laekumist."), 11.sp, Color(0xFFC9CCD1), keskel = true)
    }
}
