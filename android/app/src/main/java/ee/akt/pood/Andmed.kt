package ee.akt.pood

import android.content.ContentValues
import android.content.Context
import android.database.Cursor
import android.database.sqlite.SQLiteDatabase
import android.database.sqlite.SQLiteOpenHelper
import org.json.JSONObject
import java.security.MessageDigest
import java.security.SecureRandom
import java.time.LocalDate
import java.time.LocalDateTime

// Andmete liides: päris rakendus kasutab telefoni SQLite andmebaasi (SqlPood),
// ekraanipiltide testid mälus olevat poodi (vt test/ MaluPood).
interface Pood {
    fun kategooriad(): List<Kategooria>
    fun tooted(): List<Toode>
    fun salvestaToode(t: Toode): Toode
    fun kustutaToode(id: Int)

    fun logiSisse(nimi: String, parool: String): Kasutaja?
    fun registreeri(nimi: String, taisNimi: String, parool: String, email: String?): String?
    fun kasutaja(nimi: String): Kasutaja?
    fun muudaBoonust(nimi: String, muutus: Int)
    fun kasutajad(): List<Kasutaja>
    fun muudaProfiili(nimi: String, taisNimi: String, email: String?)
    fun muudaParooli(nimi: String, vana: String, uus: String): String?   // veateade või null

    fun ostud(kasutaja: String?): List<Ost>
    fun lisaOst(ost: Ost): Ost
    fun tagasta(ostId: Int)

    fun nimekiri(): List<NimekirjaRida>
    fun lisaNimekirja(tekst: String, lisaja: String?)
    fun margiNimekirjas(id: Int, ostetud: Boolean)
    fun kustutaNimekirjast(id: Int)

    fun lemmikud(): Set<Int>
    fun vahetaLemmik(id: Int)

    fun kampaaniad(): List<Kampaania>
    fun lisaKampaania(k: Kampaania)

    fun partiid(): List<Partii>
    fun lisaPartii(toodeId: Int, kogus: Int, aegub: LocalDate)
    fun kannaMaha(partiiId: Int)
    fun lisaKategooria(nimi: String): Int
    fun kustutaKategooria(id: Int): String?          // veateade või null
    fun muudaRolli(nimi: String, roll: Roll)
    fun kustutaKasutaja(nimi: String)

    fun seade(voti: String): String?
    fun seaSeade(voti: String, vaartus: String?)
}

object Parool {
    fun rasi(parool: String, sool: String = uusSool()): String {
        val d = MessageDigest.getInstance("SHA-256").digest((sool + parool).toByteArray())
        return sool + ":" + d.joinToString("") { "%02x".format(it) }
    }

    fun kontrolli(parool: String, rasi: String): Boolean {
        val sool = rasi.substringBefore(':', "")
        return sool.isNotEmpty() && rasi(parool, sool) == rasi
    }

    private fun uusSool(): String = ByteArray(8).also { SecureRandom().nextBytes(it) }.joinToString("") { "%02x".format(it) }
}

class SqlPood(private val context: Context) : SQLiteOpenHelper(context, "akt.db", null, 2), Pood {

    override fun onCreate(db: SQLiteDatabase) {
        db.execSQL("CREATE TABLE kategooriad(id INTEGER PRIMARY KEY, nimi TEXT)")
        db.execSQL(
            "CREATE TABLE tooted(id INTEGER PRIMARY KEY AUTOINCREMENT, nimi TEXT, kogus INTEGER, hind INTEGER, vanaHind INTEGER, pilt TEXT, kat INTEGER," +
                " kaubamark TEXT, tootja TEXT, pakend TEXT, uhik TEXT, uhikuKogus REAL, kirjeldus TEXT, koostis TEXT, lisainfo TEXT, toitevaartus TEXT," +
                " margised TEXT, pant INTEGER, maa TEXT)"
        )
        db.execSQL("CREATE TABLE kasutajad(nimi TEXT PRIMARY KEY, taisNimi TEXT, rasi TEXT, roll TEXT, kaart TEXT, boonus INTEGER, email TEXT)")
        db.execSQL(
            "CREATE TABLE ostud(id INTEGER PRIMARY KEY AUTOINCREMENT, aeg TEXT, kasutaja TEXT, vahesumma INTEGER, boonusKasutatud INTEGER, tarne INTEGER," +
                " pant INTEGER, summa INTEGER, boonusLisatud INTEGER, makseviis TEXT, tarneviis TEXT, aadress TEXT, tagastatud INTEGER DEFAULT 0)"
        )
        db.execSQL("CREATE TABLE ostuRead(ostId INTEGER, toodeId INTEGER, nimi TEXT, kogus INTEGER, hind INTEGER, tavaHind INTEGER, soodus TEXT)")
        db.execSQL("CREATE TABLE nimekiri(id INTEGER PRIMARY KEY AUTOINCREMENT, tekst TEXT, ostetud INTEGER, lisaja TEXT)")
        db.execSQL("CREATE TABLE lemmikud(toodeId INTEGER PRIMARY KEY)")
        db.execSQL("CREATE TABLE kampaaniad(id INTEGER PRIMARY KEY AUTOINCREMENT, toodeId INTEGER, protsent INTEGER, algus TEXT, lopp TEXT)")
        db.execSQL("CREATE TABLE seaded(voti TEXT PRIMARY KEY, vaartus TEXT)")
        looPartiid(db)
        algandmed(db)
    }

    override fun onUpgrade(db: SQLiteDatabase, vana: Int, uus: Int) {
        if (vana < 2) looPartiid(db)
    }

    private fun looPartiid(db: SQLiteDatabase) =
        db.execSQL("CREATE TABLE IF NOT EXISTS partiid(id INTEGER PRIMARY KEY AUTOINCREMENT, toodeId INTEGER, kogus INTEGER, aegub TEXT, lisatud TEXT)")

    // Uuem kataloog (rakenduse uuendus): tooted laaditakse uuesti, kasutajad ja ostud jäävad alles.
    // Vanad osturead jäävad tšekile nime ja hinnaga; lemmikud ja kampaaniad viitasid vanadele id-dele.
    override fun onOpen(db: SQLiteDatabase) {
        super.onOpen(db)
        if (db.isReadOnly) return
        val uus = Rimi.versioon(context) ?: return
        val praegu = db.rawQuery("SELECT vaartus FROM seaded WHERE voti='kataloog'", null).use { if (it.moveToFirst()) it.getString(0) else null }
        if (praegu == uus) return
        db.beginTransaction()
        try {
            db.execSQL("UPDATE ostuRead SET toodeId = NULL")
            db.execSQL("DELETE FROM lemmikud")
            db.execSQL("DELETE FROM kampaaniad")
            db.execSQL("DELETE FROM partiid")
            db.execSQL("DELETE FROM tooted")
            db.execSQL("DELETE FROM kategooriad")
            Rimi.laadi(context, db)
            db.setTransactionSuccessful()
        } finally {
            db.endTransaction()
        }
    }

    // Tooted ja kategooriad: Rimi e-poe kataloog (assets/rimi_kataloog.json.gz, sama mis arvutiversioonis),
    // kui seda pole, siis väike algandmestik (assets/tooted.json). Lisaks kaks kasutajat ja ostunimekiri.
    private fun algandmed(db: SQLiteDatabase) {
        if (context.assets.list("")?.contains("rimi_kataloog.json.gz") == true) Rimi.laadi(context, db) else vanadTooted(db)
        lisaKasutajad(db)
    }

    private fun vanadTooted(db: SQLiteDatabase) {
        val json = JSONObject(context.assets.open("tooted.json").bufferedReader().use { it.readText() })
        val kat = json.getJSONArray("kategooriad")
        for (i in 0 until kat.length()) {
            val k = kat.getJSONObject(i)
            db.insert("kategooriad", null, ContentValues().apply { put("id", k.getInt("id")); put("nimi", k.getString("nimi")) })
        }
        val tooted = json.getJSONArray("tooted")
        for (i in 0 until tooted.length()) {
            val o = tooted.getJSONObject(i)
            fun s(v: String) = if (o.isNull(v)) null else o.getString(v)
            fun n(v: String) = if (o.isNull(v)) null else o.getInt(v)
            val t = Toode(
                id = o.getInt("id"), nimi = o.getString("nimi"), kogus = o.getInt("kogus"), hind = o.getInt("hind"), vanaHind = n("vanaHind"),
                pilt = s("pilt"), kategooriaId = o.getInt("kat"), kaubamark = s("kaubamark"), tootja = s("tootja"), pakend = s("pakend"),
                uhik = s("uhik"), uhikuKogus = if (o.isNull("uhikuKogus")) null else o.getDouble("uhikuKogus"), kirjeldus = s("kirjeldus"),
                koostis = s("koostis"), lisainfo = s("lisainfo"), toitevaartus = s("toitevaartus"), margised = s("margised"), pant = n("pant") ?: 0,
                paritolumaa = s("maa")
            )
            db.insert("tooted", null, vaartused(t, kaasaId = true))
        }
    }

    private fun lisaKasutajad(db: SQLiteDatabase) {
        db.insert("kasutajad", null, ContentValues().apply {
            put("nimi", "admin"); put("taisNimi", "Admin Omanik"); put("rasi", Parool.rasi("admin123")); put("roll", Roll.Omanik.name)
            put("kaart", "100000"); put("boonus", 0)
        })
        db.insert("kasutajad", null, ContentValues().apply {
            put("nimi", "mari"); put("taisNimi", "Mari Maasikas"); put("rasi", Parool.rasi("mari123")); put("roll", Roll.Klient.name)
            put("kaart", "100001"); put("boonus", 42); put("email", "mari@email.ee")
        })
        for ((tekst, ostetud) in listOf("Piim" to false, "Leib" to true, "Banaanid" to false, "Kohv" to false))
            db.insert("nimekiri", null, ContentValues().apply { put("tekst", tekst); put("ostetud", if (ostetud) 1 else 0); put("lisaja", "Mari") })
    }

    private fun vaartused(t: Toode, kaasaId: Boolean) = ContentValues().apply {
        if (kaasaId) put("id", t.id)
        put("nimi", t.nimi); put("kogus", t.kogus); put("hind", t.hind); put("vanaHind", t.vanaHind); put("pilt", t.pilt)
        put("kat", t.kategooriaId); put("kaubamark", t.kaubamark); put("tootja", t.tootja); put("pakend", t.pakend); put("uhik", t.uhik)
        put("uhikuKogus", t.uhikuKogus); put("kirjeldus", t.kirjeldus); put("koostis", t.koostis); put("lisainfo", t.lisainfo)
        put("toitevaartus", t.toitevaartus); put("margised", t.margised); put("pant", t.pant); put("maa", t.paritolumaa)
    }

    private fun Cursor.s(v: String): String? = getColumnIndex(v).let { if (isNull(it)) null else getString(it) }
    private fun Cursor.i(v: String): Int? = getColumnIndex(v).let { if (isNull(it)) null else getInt(it) }
    private fun Cursor.d(v: String): Double? = getColumnIndex(v).let { if (isNull(it)) null else getDouble(it) }

    private fun <T> paring(sql: String, vararg arg: String, loe: (Cursor) -> T): List<T> =
        readableDatabase.rawQuery(sql, arg).use { c -> buildList { while (c.moveToNext()) add(loe(c)) } }

    override fun kategooriad() = paring("SELECT * FROM kategooriad ORDER BY id") { Kategooria(it.i("id")!!, it.s("nimi")!!) }

    override fun tooted() = paring("SELECT * FROM tooted ORDER BY nimi") { c ->
        Toode(
            id = c.i("id")!!, nimi = c.s("nimi")!!, kogus = c.i("kogus") ?: 0, hind = c.i("hind") ?: 0, vanaHind = c.i("vanaHind"), pilt = c.s("pilt"),
            kategooriaId = c.i("kat") ?: 1, kaubamark = c.s("kaubamark"), tootja = c.s("tootja"), pakend = c.s("pakend"), uhik = c.s("uhik"),
            uhikuKogus = c.d("uhikuKogus"), kirjeldus = c.s("kirjeldus"), koostis = c.s("koostis"), lisainfo = c.s("lisainfo"),
            toitevaartus = c.s("toitevaartus"), margised = c.s("margised"), pant = c.i("pant") ?: 0, paritolumaa = c.s("maa")
        )
    }

    override fun salvestaToode(t: Toode): Toode {
        val db = writableDatabase
        return if (t.id == 0) t.copy(id = db.insert("tooted", null, vaartused(t, kaasaId = false)).toInt())
        else { db.update("tooted", vaartused(t, kaasaId = false), "id=?", arrayOf(t.id.toString())); t }
    }

    override fun kustutaToode(id: Int) { writableDatabase.delete("tooted", "id=?", arrayOf(id.toString())) }

    private fun loeKasutaja(c: Cursor) = Kasutaja(
        c.s("nimi")!!, c.s("taisNimi") ?: c.s("nimi")!!, runCatching { Roll.valueOf(c.s("roll")!!) }.getOrDefault(Roll.Klient),
        c.s("kaart") ?: "", c.i("boonus") ?: 0, c.s("email")
    )

    override fun kasutaja(nimi: String) = paring("SELECT * FROM kasutajad WHERE nimi=?", nimi.lowercase()) { loeKasutaja(it) }.firstOrNull()

    override fun logiSisse(nimi: String, parool: String): Kasutaja? {
        val rasi = paring("SELECT rasi FROM kasutajad WHERE nimi=?", nimi.trim().lowercase()) { it.s("rasi") }.firstOrNull() ?: return null
        return if (Parool.kontrolli(parool, rasi)) kasutaja(nimi.trim()) else null
    }

    override fun registreeri(nimi: String, taisNimi: String, parool: String, email: String?): String? {
        val n = nimi.trim().lowercase()
        if (n.length < 3) return "Kasutajanimi peab olema vähemalt 3 tähte."
        if (parool.length < 6) return "Parool peab olema vähemalt 6 märki."
        if (kasutaja(n) != null) return "Selline kasutaja on juba olemas."
        val kaart = (paring("SELECT MAX(CAST(kaart AS INTEGER)) m FROM kasutajad") { it.i("m") }.firstOrNull() ?: 100000) + 1
        writableDatabase.insert("kasutajad", null, ContentValues().apply {
            put("nimi", n); put("taisNimi", taisNimi.trim().ifEmpty { nimi.trim() }); put("rasi", Parool.rasi(parool)); put("roll", Roll.Klient.name)
            put("kaart", kaart.toString()); put("boonus", 0); put("email", email)
        })
        return null
    }

    override fun partiid() = paring("SELECT * FROM partiid ORDER BY aegub") { c ->
        Partii(c.i("id")!!, c.i("toodeId")!!, c.i("kogus") ?: 0, LocalDate.parse(c.s("aegub")), LocalDate.parse(c.s("lisatud")))
    }

    // Uus partii suurendab laoseisu
    override fun lisaPartii(toodeId: Int, kogus: Int, aegub: LocalDate) {
        val db = writableDatabase
        db.insert("partiid", null, ContentValues().apply { put("toodeId", toodeId); put("kogus", kogus); put("aegub", aegub.toString()); put("lisatud", LocalDate.now().toString()) })
        db.execSQL("UPDATE tooted SET kogus = kogus + ? WHERE id=?", arrayOf<Any>(kogus, toodeId))
    }

    // Mahakandmine: partii kaob ja laoseis väheneb
    override fun kannaMaha(partiiId: Int) {
        val p = partiid().firstOrNull { it.id == partiiId } ?: return
        val db = writableDatabase
        db.delete("partiid", "id=?", arrayOf(partiiId.toString()))
        db.execSQL("UPDATE tooted SET kogus = MAX(0, kogus - ?) WHERE id=?", arrayOf<Any>(p.kogus, p.toodeId))
    }

    override fun lisaKategooria(nimi: String): Int =
        writableDatabase.insert("kategooriad", null, ContentValues().apply { put("nimi", nimi.trim()) }).toInt()

    override fun kustutaKategooria(id: Int): String? {
        val arv = paring("SELECT COUNT(*) n FROM tooted WHERE kat=?", id.toString()) { it.i("n") }.firstOrNull() ?: 0
        if (arv > 0) return "Kategoorias on tooteid – kustuta või liiguta need enne."
        writableDatabase.delete("kategooriad", "id=?", arrayOf(id.toString()))
        return null
    }

    override fun muudaRolli(nimi: String, roll: Roll) {
        writableDatabase.update("kasutajad", ContentValues().apply { put("roll", roll.name) }, "nimi=?", arrayOf(nimi))
    }

    override fun kustutaKasutaja(nimi: String) { writableDatabase.delete("kasutajad", "nimi=?", arrayOf(nimi)) }

    override fun kasutajad() = paring("SELECT * FROM kasutajad ORDER BY nimi") { loeKasutaja(it) }

    override fun muudaProfiili(nimi: String, taisNimi: String, email: String?) {
        writableDatabase.update("kasutajad", ContentValues().apply { put("taisNimi", taisNimi.trim()); put("email", email?.trim()?.ifEmpty { null }) }, "nimi=?", arrayOf(nimi))
    }

    override fun muudaParooli(nimi: String, vana: String, uus: String): String? {
        val rasi = paring("SELECT rasi FROM kasutajad WHERE nimi=?", nimi) { it.s("rasi") }.firstOrNull() ?: return "Kasutajat ei leitud."
        if (!Parool.kontrolli(vana, rasi)) return "Praegune parool on vale."
        if (uus.length < 6) return "Parool peab olema vähemalt 6 märki."
        writableDatabase.update("kasutajad", ContentValues().apply { put("rasi", Parool.rasi(uus)) }, "nimi=?", arrayOf(nimi))
        return null
    }

    override fun muudaBoonust(nimi: String, muutus: Int) {
        writableDatabase.execSQL("UPDATE kasutajad SET boonus = MAX(0, boonus + ?) WHERE nimi=?", arrayOf<Any>(muutus, nimi))
    }

    override fun ostud(kasutaja: String?): List<Ost> {
        val read = paring("SELECT * FROM ostuRead") { c ->
            c.i("ostId")!! to OstuRida(c.i("toodeId"), c.s("nimi") ?: "", c.i("kogus") ?: 0, c.i("hind") ?: 0, c.i("tavaHind"), c.s("soodus"))
        }.groupBy({ it.first }, { it.second })
        val sql = if (kasutaja == null) "SELECT * FROM ostud ORDER BY aeg DESC" else "SELECT * FROM ostud WHERE kasutaja=? ORDER BY aeg DESC"
        val arg: Array<String> = if (kasutaja == null) emptyArray() else arrayOf(kasutaja)
        return paring(sql, *arg) { c ->
            val id = c.i("id")!!
            Ost(
                id, LocalDateTime.parse(c.s("aeg")), c.s("kasutaja") ?: "", c.i("vahesumma") ?: 0, c.i("boonusKasutatud") ?: 0, c.i("tarne") ?: 0,
                c.i("pant") ?: 0, c.i("summa") ?: 0, c.i("boonusLisatud") ?: 0, c.s("makseviis") ?: "", c.s("tarneviis") ?: "", c.s("aadress"),
                read[id] ?: emptyList(), (c.i("tagastatud") ?: 0) == 1
            )
        }
    }

    override fun lisaOst(ost: Ost): Ost {
        val db = writableDatabase
        db.beginTransaction()
        try {
            val id = db.insert("ostud", null, ContentValues().apply {
                put("aeg", ost.aeg.toString()); put("kasutaja", ost.kasutaja); put("vahesumma", ost.vahesumma); put("boonusKasutatud", ost.boonusKasutatud)
                put("tarne", ost.tarne); put("pant", ost.pant); put("summa", ost.summa); put("boonusLisatud", ost.boonusLisatud)
                put("makseviis", ost.makseviis); put("tarneviis", ost.tarneviis); put("aadress", ost.aadress)
            }).toInt()
            for (r in ost.read) {
                db.insert("ostuRead", null, ContentValues().apply {
                    put("ostId", id); put("toodeId", r.toodeId); put("nimi", r.nimi); put("kogus", r.kogus); put("hind", r.hind)
                    put("tavaHind", r.tavaHind); put("soodus", r.soodus)
                })
                if (r.toodeId != null) db.execSQL("UPDATE tooted SET kogus = MAX(0, kogus - ?) WHERE id=?", arrayOf<Any>(r.kogus, r.toodeId))
            }
            db.execSQL(
                "UPDATE kasutajad SET boonus = MAX(0, boonus - ? + ?) WHERE nimi=?",
                arrayOf<Any>(ost.boonusKasutatud, ost.boonusLisatud, ost.kasutaja)
            )
            db.setTransactionSuccessful()
            return ost.copy(id = id)
        } finally {
            db.endTransaction()
        }
    }

    override fun tagasta(ostId: Int) {
        val ost = ostud(null).firstOrNull { it.id == ostId && !it.tagastatud } ?: return
        val db = writableDatabase
        db.beginTransaction()
        try {
            db.execSQL("UPDATE ostud SET tagastatud=1 WHERE id=?", arrayOf<Any>(ostId))
            for (r in ost.read) if (r.toodeId != null) db.execSQL("UPDATE tooted SET kogus = kogus + ? WHERE id=?", arrayOf<Any>(r.kogus, r.toodeId))
            db.execSQL("UPDATE kasutajad SET boonus = MAX(0, boonus + ? - ?) WHERE nimi=?", arrayOf<Any>(ost.boonusKasutatud, ost.boonusLisatud, ost.kasutaja))
            db.setTransactionSuccessful()
        } finally {
            db.endTransaction()
        }
    }

    override fun nimekiri() = paring("SELECT * FROM nimekiri ORDER BY ostetud, id") { c ->
        NimekirjaRida(c.i("id")!!, c.s("tekst") ?: "", (c.i("ostetud") ?: 0) == 1, c.s("lisaja"))
    }

    override fun lisaNimekirja(tekst: String, lisaja: String?) {
        writableDatabase.insert("nimekiri", null, ContentValues().apply { put("tekst", tekst.trim()); put("ostetud", 0); put("lisaja", lisaja) })
    }

    override fun margiNimekirjas(id: Int, ostetud: Boolean) {
        writableDatabase.execSQL("UPDATE nimekiri SET ostetud=? WHERE id=?", arrayOf<Any>(if (ostetud) 1 else 0, id))
    }

    override fun kustutaNimekirjast(id: Int) { writableDatabase.delete("nimekiri", "id=?", arrayOf(id.toString())) }

    override fun lemmikud() = paring("SELECT toodeId FROM lemmikud") { it.i("toodeId")!! }.toSet()

    override fun vahetaLemmik(id: Int) {
        if (writableDatabase.delete("lemmikud", "toodeId=?", arrayOf(id.toString())) == 0)
            writableDatabase.insert("lemmikud", null, ContentValues().apply { put("toodeId", id) })
    }

    override fun kampaaniad() = paring("SELECT * FROM kampaaniad ORDER BY algus DESC") { c ->
        Kampaania(c.i("id")!!, c.i("toodeId")!!, c.i("protsent")!!, LocalDate.parse(c.s("algus")), LocalDate.parse(c.s("lopp")))
    }

    // Kampaania: toote hind langeb kohe (vana hind jääb läbikriipsutatuks)
    override fun lisaKampaania(k: Kampaania) {
        val db = writableDatabase
        db.insert("kampaaniad", null, ContentValues().apply {
            put("toodeId", k.toodeId); put("protsent", k.protsent); put("algus", k.algus.toString()); put("lopp", k.lopp.toString())
        })
        val t = tooted().firstOrNull { it.id == k.toodeId } ?: return
        val tava = Reeglid.tavaHind(t)
        salvestaToode(t.copy(vanaHind = tava, hind = Reeglid.soodusHind(tava, k.protsent)))
    }

    override fun seade(voti: String) = paring("SELECT vaartus FROM seaded WHERE voti=?", voti) { it.s("vaartus") }.firstOrNull()

    override fun seaSeade(voti: String, vaartus: String?) {
        if (vaartus == null) writableDatabase.delete("seaded", "voti=?", arrayOf(voti))
        else writableDatabase.insertWithOnConflict("seaded", null, ContentValues().apply { put("voti", voti); put("vaartus", vaartus) }, SQLiteDatabase.CONFLICT_REPLACE)
    }
}
