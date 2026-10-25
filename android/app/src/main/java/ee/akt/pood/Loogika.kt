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

// Autotellimus (sama mis arvutis, Autotellimus.cs): müügitempo viimase 28 päeva järgi → mitu päeva laoseis kestab → mida tellida
object Autotellimus {
    const val AJALUGU = 28
    const val VARU = 3

    data class Rida(val toode: Toode, val tarnija: String, val paevas: Double, val jatkub: Double, val soovitus: Int) {
        val ostuhind: Int get() = Math.round(toode.hind * 0.7).toInt()   // telefonis ostuhinda pole: hinnang 70 %
    }

    fun tarnija(t: Toode) = t.tootja?.takeIf { it.isNotBlank() } ?: t.kaubamark?.takeIf { it.isNotBlank() } ?: "Muu tarnija"

    fun arvuta(ostud: List<Ost>, tooted: List<Toode>, nyyd: LocalDateTime, kate: Int = 14): List<Rida> {
        val piir = nyyd.minusDays(AJALUGU.toLong())
        val myydud = ostud.filter { !it.tagastatud && it.aeg.isAfter(piir) }.flatMap { it.read }.filter { it.toodeId != null }
            .groupBy { it.toodeId!! }.mapValues { (_, r) -> r.sumOf { it.kogus } }
        return tooted.mapNotNull { t ->
            val tk = myydud[t.id] ?: return@mapNotNull null
            val paevas = tk.toDouble() / AJALUGU
            val laos = maxOf(0, t.kogus)
            val jatkub = laos / paevas
            if (jatkub >= kate) return@mapNotNull null
            val vaja = Math.ceil(paevas * (kate + VARU)).toInt() - laos
            if (vaja > 0) Rida(t, tarnija(t), paevas, jatkub, vaja) else null
        }.sortedWith(compareBy<Rida> { it.jatkub }.thenBy { it.tarnija })
    }
}

// "Ostetakse koos" (sama mis arvutis, Ostukorvianalyys.cs): A → B, kindlus = P(B|A), tõus = kindlus / P(B)
object Ostukorvianalyys {
    data class Reegel(val a: String, val b: String, val koos: Int, val osteA: Int, val kindlus: Double, val tous: Double, val toodeB: Int? = null)

    fun <T> reeglid(ostud: List<Set<T>>, nimi: (T) -> String, minKoos: Int = 3, minTous: Double = 1.1): List<Reegel> {
        val hulgad = ostud.filter { it.isNotEmpty() }
        val n = hulgad.size
        if (n == 0) return emptyList()
        val yks = HashMap<T, Int>()
        val paar = HashMap<Pair<T, T>, Int>()
        for (h in hulgad) {
            for (a in h) yks[a] = (yks[a] ?: 0) + 1
            for (a in h) for (b in h) if (a != b) paar[a to b] = (paar[a to b] ?: 0) + 1
        }
        return paar.mapNotNull { (ab, koos) ->
            if (koos < minKoos) return@mapNotNull null
            val kindlus = koos.toDouble() / yks[ab.first]!!
            val tous = kindlus / (yks[ab.second]!!.toDouble() / n)
            if (tous < minTous) null else Reegel(nimi(ab.first), nimi(ab.second), koos, yks[ab.first]!!, kindlus, tous, ab.second as? Int)
        }.sortedWith(compareByDescending<Reegel> { it.tous * Math.sqrt(it.koos.toDouble()) }.thenByDescending { it.kindlus })
    }

    // A→B ja B→A on sama paar: jääb tugevam suund
    fun paarid(r: List<Reegel>, max: Int = 25): List<Reegel> {
        val nahtud = HashSet<String>()
        return r.filter { nahtud.add(if (it.a < it.b) it.a + "|" + it.b else it.b + "|" + it.a) }.take(max)
    }
}

// Müügipaneel: müük päevade kaupa, enim müüdud tooted
object Myyk {
    fun paevad(ostud: List<Ost>, nyyd: LocalDateTime, paevi: Int = 14): List<Pair<LocalDate, Int>> {
        val tana = nyyd.toLocalDate()
        val k = ostud.filter { !it.tagastatud }.groupBy { it.aeg.toLocalDate() }.mapValues { (_, o) -> o.sumOf { it.summa } }
        return (paevi - 1 downTo 0).map { tana.minusDays(it.toLong()) }.map { it to (k[it] ?: 0) }
    }

    fun top(ostud: List<Ost>, max: Int = 8): List<Triple<String, Int, Int>> =
        ostud.filter { !it.tagastatud }.flatMap { it.read }.groupBy { it.nimi }
            .map { (n, r) -> Triple(n, r.sumOf { it.kogus }, r.sumOf { it.summa }) }
            .sortedByDescending { it.second }.take(max)
}

// Demoandmed (omanikule, et müügipaneel ja analüüs ei oleks tühjad): 3 klienti ja ~60 ostu viimase 60 päeva jooksul.
// Mõned paarid ostetakse sageli koos (õlu + krõpsud, pasta + kaste, kohv + koor, leib + või, banaan + jogurt).
object Demo {
    val kliendid = listOf(Triple("jaan", "Jaan Tamm", "jaan@demo.ee"), Triple("liis", "Liis Kask", "liis@demo.ee"), Triple("kati", "Kati Maasikas", "kati@demo.ee"))

    fun loo(pood: Pood, tooted: List<Toode>, nyyd: LocalDateTime): Int {
        for ((n, t, e) in kliendid) if (pood.kasutaja(n) == null) pood.registreeri(n, t, "demo123", e)
        val juhus = java.util.Random(42)
        val toit = tooted.filter { it.laos && it.kategooriaId in 1..9 }
        if (toit.isEmpty()) return 0
        fun leia(vararg sonad: String) = toit.firstOrNull { t -> sonad.any { t.nimi.contains(it, true) } }
        val paarid = listOf(leia("õlu") to leia("krõps"), leia("pasta", "makaron") to leia("tomatikaste", "pastakaste"),
            leia("kohv") to leia("koor"), leia("rukkileib", "leib") to leia("või "), leia("banaan") to leia("jogurt"))
            .mapNotNull { (a, b) -> if (a != null && b != null) a to b else null }
        val populaarsed = (0 until 40).map { toit[juhus.nextInt(toit.size)] }.distinctBy { it.id }
        val ostjad = listOf("mari") + kliendid.map { it.first }
        var n = 0
        for (i in 0 until 60) {
            val aeg = nyyd.minusDays(juhus.nextInt(60).toLong()).withHour(9 + juhus.nextInt(12)).withMinute(juhus.nextInt(60))
            val korv = LinkedHashMap<Toode, Int>()
            repeat(2 + juhus.nextInt(6)) { val t = populaarsed[juhus.nextInt(populaarsed.size)]; korv[t] = (korv[t] ?: 0) + 1 + juhus.nextInt(2) }
            for ((a, b) in paarid) if (juhus.nextInt(100) < 40) { korv[a] = 1; korv[b] = 1 }
            val read = korv.map { (t, k) -> OstuRida(t.id, t.nimi, k, t.hind, Reeglid.tavaHind(t)) }
            val kaubad = read.sumOf { it.summa }
            val kodu = juhus.nextBoolean()
            val tarne = Reeglid.tarne(kaubad, kodu)
            pood.lisaOst(Ost(0, aeg, ostjad[juhus.nextInt(ostjad.size)], kaubad, 0, tarne, 0, kaubad + tarne, kaubad / 100,
                if (juhus.nextBoolean()) "Kaart" else "Sularaha", if (kodu) "Kojuvedu" else "Kaasa", if (kodu) "Tartu mnt 87, Tallinn" else null, read))
            n++
        }
        // osa populaarseid on peaaegu otsas – autotellimusel on siis, mida soovitada
        for (t in populaarsed.take(12)) pood.salvestaToode(t.copy(kogus = 1 + juhus.nextInt(5)))
        return n
    }
}
