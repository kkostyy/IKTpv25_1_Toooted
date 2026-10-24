package ee.akt.pood

import java.time.LocalDateTime

// Kõik summad on sentides (Int), et ümardamisvigu ei tekiks

data class Kategooria(val id: Int, val nimi: String)

data class Toode(
    val id: Int,
    val nimi: String,
    val kogus: Int,
    val hind: Int,
    val vanaHind: Int? = null,
    val pilt: String? = null,
    val kategooriaId: Int = 1,
    val kaubamark: String? = null,
    val tootja: String? = null,
    val pakend: String? = null,
    val uhik: String? = null,
    val uhikuKogus: Double? = null,
    val kirjeldus: String? = null,
    val koostis: String? = null,
    val lisainfo: String? = null,
    val toitevaartus: String? = null,
    val margised: String? = null,
    val pant: Int = 0,
    val paritolumaa: String? = null,
) {
    // €/kg või €/l sentides
    val uhikuHind: Int? get() = uhikuKogus?.takeIf { it > 0 }?.let { Math.round(hind / it).toInt() }
    val soodusProtsent: Int? get() = vanaHind?.takeIf { it > hind }?.let { Math.round((1 - hind.toDouble() / it) * 100).toInt() }
    val laos: Boolean get() = kogus > 0
}

enum class Roll { Omanik, Klient }

data class Kasutaja(
    val nimi: String,
    val taisNimi: String,
    val roll: Roll,
    val kaart: String,
    val boonus: Int,
    val email: String? = null,
) {
    val initsiaalid: String
        get() = taisNimi.split(' ').filter { it.isNotBlank() }.take(2).joinToString("") { it.first().uppercase() }.ifEmpty { "?" }
    val eesnimi: String get() = taisNimi.substringBefore(' ')
}

data class KorvRida(val toode: Toode, val kogus: Int, val hind: Int, val tavaHind: Int, val soodus: String?) {
    val summa: Int get() = hind * kogus
    val pantKokku: Int get() = toode.pant * kogus
}

data class OstuRida(val toodeId: Int?, val nimi: String, val kogus: Int, val hind: Int, val tavaHind: Int? = null, val soodus: String? = null) {
    val summa: Int get() = hind * kogus
}

data class Ost(
    val id: Int,
    val aeg: LocalDateTime,
    val kasutaja: String,
    val vahesumma: Int,
    val boonusKasutatud: Int,
    val tarne: Int,
    val pant: Int,
    val summa: Int,
    val boonusLisatud: Int,
    val makseviis: String,
    val tarneviis: String,
    val aadress: String?,
    val read: List<OstuRida>,
    val tagastatud: Boolean = false,
)

data class NimekirjaRida(val id: Int, val tekst: String, val ostetud: Boolean, val lisaja: String? = null)

data class Kampaania(val id: Int, val toodeId: Int, val protsent: Int, val algus: java.time.LocalDate, val lopp: java.time.LocalDate)

fun eur(sendid: Int): String {
    val m = if (sendid < 0) "−" else ""
    val a = Math.abs(sendid)
    return "$m${a / 100},${"%02d".format(a % 100)} €"
}
