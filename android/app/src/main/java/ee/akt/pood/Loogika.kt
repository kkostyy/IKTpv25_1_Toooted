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
        if (Raiskamine.aegub(t, aeg.toLocalDate()) != null) {
            val r = soodusHind(tava, Raiskamine.PROTSENT)
            if (r < hind) {
                hind = r
                soodus = "Ilma raiskamiseta −${Raiskamine.PROTSENT} %"
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

// "Sarnased ja soodsamad" (sama mis arvutis, SarnasedLoogika.cs): sama kategooria ja ühik,
// kilo-/liitrihind vähemalt 3 % madalam ja nimes vähemalt üks ühine sõna (ka liitsõna lõpp: toorjuust ~ juust)
object Sarnased {
    private val tyhisonad = setOf("eesti", "suur", "väike", "klassikaline", "maitseline")

    fun sonad(t: Toode): Set<String> = t.nimi.split(' ', ',', '-', '/', '(', ')')
        .map { o -> o.filter { it.isLetter() }.lowercase() }
        .filter { it.length >= 4 && it !in tyhisonad && !it.equals(t.kaubamark ?: "", true) }
        .toSet()

    fun sarnane(a: String, b: String) = a == b || (b.length >= 4 && a.endsWith(b)) || (a.length >= 4 && b.endsWith(a))

    fun leia(toode: Toode, koik: List<Toode>, max: Int = 3): List<Toode> {
        val minu = toode.uhikuHind ?: return emptyList()
        if (toode.uhik != "kg" && toode.uhik != "l") return emptyList()
        val minuSonad = sonad(toode)
        if (minuSonad.isEmpty()) return emptyList()
        return koik.asSequence()
            .filter { it.id != toode.id && it.laos && it.kategooriaId == toode.kategooriaId && it.uhik == toode.uhik && (it.uhikuHind ?: Int.MAX_VALUE) < minu * 0.97 }
            .map { t -> t to sonad(t).count { w -> minuSonad.any { sarnane(w, it) } } }
            .filter { it.second > 0 }
            .sortedWith(compareByDescending<Pair<Toode, Int>> { it.second }.thenBy { it.first.uhikuHind })
            .take(max).map { it.first }.toList()
    }

    fun odavam(alus: Toode, t: Toode): Int {
        val a = alus.uhikuHind ?: return 0
        val b = t.uhikuHind ?: return 0
        return if (a > 0) Math.round((1 - b.toDouble() / a) * 100).toInt() else 0
    }
}

// "Ilma raiskamiseta": kiiresti riknevad tooted, mille säilivusaeg lõpeb 0–2 päeva pärast, on −30 %.
// Telefonis partiisid pole – valik sõltub päevast ja toote id-st (iga päev erinev, kõigil sama).
object Raiskamine {
    const val PROTSENT = 30
    private val kategooriad = setOf(1, 2, 4, 9)

    // Mitme päeva pärast aegub (0 = täna) või null
    fun aegub(t: Toode, paev: LocalDate): Int? {
        if (t.kategooriaId !in kategooriad || !t.laos) return null
        val h = Math.floorMod(t.id * 31 + paev.dayOfYear * 17, 61)
        return if (h < 3) h else null
    }

    fun tekst(paevi: Int): String = when (paevi) { 0 -> Keel.t("aegub täna"); 1 -> Keel.t("aegub homme"); else -> Keel.f("aegub {0} päeva pärast", paevi) }
}

// Kulutused kuude ja kategooriate kaupa (sama mis arvutis, Kulutused.cs)
object Kulutused {
    data class Osa(val nimi: String, val summa: Int)

    fun kuu(ostud: List<Ost>, kategooria: (Int?) -> String, aasta: Int, kuu: Int): List<Osa> {
        val k = mutableMapOf<String, Int>()
        for (o in ostud) if (!o.tagastatud && o.aeg.year == aasta && o.aeg.monthValue == kuu)
            for (r in o.read) { val n = kategooria(r.toodeId); k[n] = (k[n] ?: 0) + r.summa }
        val koik = k.values.sum()
        if (koik <= 0) return emptyList()
        val jarjest = k.entries.sortedByDescending { it.value }
        val suured = jarjest.take(5).filter { it.value * 25 >= koik }
        val muu = koik - suured.sumOf { it.value }
        return suured.map { Osa(it.key, it.value) } + (if (muu > 0) listOf(Osa("Muu", muu)) else emptyList())
    }
}
