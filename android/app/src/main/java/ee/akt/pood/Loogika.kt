package ee.akt.pood

import java.time.LocalDate
import java.time.LocalDateTime
import java.time.LocalTime
import java.time.temporal.IsoFields

// Hinnad ja poe reeglid (samad mis arvutiversioonis)
object Reeglid {
    const val TARNE = 399                  // kojuvedu
    const val TASUTA_TARNE_ALATES = 2500   // tasuta tarne alates 25 €
    const val KUPSETISTE_KATEGOORIA = 2    // õhtune soodustus: leivad, saiad, kondiitritooted
    val OHTU_ALGUS: LocalTime = LocalTime.of(20, 0)
    const val OHTU_PROTSENT = 50

    data class Hind(val hind: Int, val tavaHind: Int, val soodus: String?)

    fun tavaHind(t: Toode): Int = t.vanaHind?.takeIf { it > t.hind } ?: t.hind

    fun soodusHind(tava: Int, protsent: Int): Int = Math.round(tava * (100 - protsent) / 100.0).toInt()

    fun ohtuKaib(aeg: LocalDateTime): Boolean = !aeg.toLocalTime().isBefore(OHTU_ALGUS)

    // Lõpphind: kampaania (Toode.hind) või õhtune -50 % küpsetistele – kehtib suurem soodustus, need ei liitu
    fun lopphind(t: Toode, aeg: LocalDateTime = LocalDateTime.now()): Hind {
        val tava = tavaHind(t)
        var hind = t.hind
        var soodus: String? = if (t.hind < tava) "Kampaania −${t.soodusProtsent}%" else null
        if (t.kategooriaId == KUPSETISTE_KATEGOORIA && ohtuKaib(aeg)) {
            val ohtu = soodusHind(tava, OHTU_PROTSENT)
            if (ohtu < hind) {
                hind = ohtu
                soodus = "Õhtune soodustus −$OHTU_PROTSENT %"
            }
        }
        return Hind(hind, tava, soodus)
    }

    fun tarne(kaubad: Int, kojuvedu: Boolean): Int = if (!kojuvedu || kaubad >= TASUTA_TARNE_ALATES) 0 else TARNE

    // ---- Kliendikaardi tasemed: viimase 12 kuu ostude summa ----
    enum class Tase(val nimi: String, val alates: Int, val promill: Int) {
        Pronks("Pronks", 0, 10), Hobe("Hõbe", 10000, 15), Kuld("Kuld", 30000, 20)
    }

    data class TaseInfo(val tase: Tase, val summa: Int, val jargmine: Tase?, val jargmiseni: Int)

    fun taseSummast(summa: Int): TaseInfo {
        val k = Tase.entries
        var i = k.size - 1
        while (i > 0 && summa < k[i].alates) i--
        val j = k.getOrNull(i + 1)
        return TaseInfo(k[i], summa, j, if (j != null) j.alates - summa else 0)
    }

    // Boonus: 1 / 1,5 / 2 % ostusummast (ilma tarne ja pandita)
    fun boonus(kaubad: Int, tase: Tase): Int = kaubad * tase.promill / 1000

    fun nadalaKood(p: LocalDate): String = "%d-W%02d".format(p.get(IsoFields.WEEK_BASED_YEAR), p.get(IsoFields.WEEK_OF_WEEK_BASED_YEAR))
}

// Dieet: admini märgised ja koostise allergeenid (SUURTÄHED)
object Dieet {
    enum class Mark(val nimi: String) { Laktoosivaba("Laktoosivaba"), Gluteenivaba("Gluteenivaba"), Vegan("Vegan"), Pahklivaba("Pähklivaba") }

    private val ruhmad = listOf(
        "piim" to listOf("piim", "koor", "juust", "vadak", "laktoos", "jogurt", "kohupiim"),
        "gluteen" to listOf("nisu", "rukis", "rukki", "oder", "odra", "kaer", "gluteen", "spelt"),
        "muna" to listOf("muna"),
        "pähklid" to listOf("pähk", "sarapuu", "mandel", "mandli", "kašu"),
        "kala" to listOf("kala"),
        "liha" to listOf("liha", "veise", "sealiha", "peekon", "vorst", "sink"),
        "soja" to listOf("soja"),
    )

    fun allergeenid(t: Toode): Set<String> {
        val k = (t.koostis ?: "").lowercase()
        return ruhmad.filter { (_, tyved) -> tyved.any { k.contains(it) } }.map { it.first }.toSet()
    }

    fun margid(t: Toode): Set<Mark> = (t.margised ?: "").split(';').mapNotNull { m ->
        when (m.trim().lowercase()) {
            "laktoosivaba" -> Mark.Laktoosivaba
            "gluteenivaba" -> Mark.Gluteenivaba
            "vegan" -> Mark.Vegan
            "pahklivaba", "pähklivaba" -> Mark.Pahklivaba
            else -> null
        }
    }.toSet()

    // Tundmatu koostisega toode ei sobi (ohutum), puu- ja köögiviljad sobivad alati
    fun sobib(t: Toode, valik: Set<Mark>): Boolean {
        if (valik.isEmpty()) return true
        val m = margid(t)
        val teada = !t.koostis.isNullOrBlank() || t.kategooriaId == 3
        val a = allergeenid(t)
        val laktoosivaba = (t.nimi + " " + (t.kirjeldus ?: "")).lowercase().contains("laktoosivaba")
        return valik.all { v ->
            v in m || (teada && when (v) {
                Mark.Laktoosivaba -> "piim" !in a || laktoosivaba
                Mark.Gluteenivaba -> "gluteen" !in a
                Mark.Vegan -> a.none { it in setOf("piim", "muna", "kala", "liha") }
                Mark.Pahklivaba -> "pähklid" !in a
            })
        }
    }
}

// Toiteväärtus: rida "nimetus;väärtus", nt "Energia;1450 kJ / 346 kcal"
object Toitevaartus {
    data class Rida(val nimi: String, val vaartus: String, val alam: Boolean)

    fun read(t: Toode): List<Rida> = (t.toitevaartus ?: "").lines().filter { it.contains(';') }.map {
        val alam = it.startsWith("-")
        val (n, v) = it.removePrefix("-").split(';', limit = 2)
        Rida(n.trim(), v.trim(), alam)
    }

    // kcal 100 g kohta
    fun kcal100(t: Toode): Double? = read(t).firstOrNull { it.nimi.lowercase().startsWith("energia") }?.let { r ->
        Regex("([0-9]+(?:[.,][0-9]+)?)\\s*kcal").find(r.vaartus)?.groupValues?.get(1)?.replace(',', '.')?.toDoubleOrNull()
    }

    // Pakendi kaal grammides / ml
    fun grammid(t: Toode): Double? = t.uhikuKogus?.takeIf { t.uhik == "kg" || t.uhik == "l" }?.let { it * 1000 }

    fun kcal(t: Toode): Double? {
        val k = kcal100(t) ?: return null
        val g = grammid(t) ?: return null
        return k * g / 100
    }
}
