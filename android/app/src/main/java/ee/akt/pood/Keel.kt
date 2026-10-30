package ee.akt.pood

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue

// Tõlked: võti on eestikeelne tekst (nagu arvutiversioonis Keel.T)
object Keel {
    enum class Kood { ET, RU, EN }

    var praegu by mutableStateOf(Kood.ET)

    fun t(s: String): String = when (praegu) {
        Kood.ET -> s
        Kood.RU -> tolked[s]?.first ?: s
        Kood.EN -> tolked[s]?.second ?: s
    }

    fun f(s: String, vararg a: Any?): String {
        var r = t(s)
        a.forEachIndexed { i, v -> r = r.replace("{$i}", v.toString()) }
        return r
    }

    // Tõlked failist resources/tolked.json:  "eestikeelne tekst": { "ru": "…", "en": "…" }
    // Uue tõlke lisamiseks lisa rida JSON-faili (koodi pole vaja muuta).
    private val tolked = HashMap<String, Pair<String, String>>()

    init {
        runCatching {
            val tekst = Keel::class.java.getResourceAsStream("/tolked.json")?.use { it.readBytes().toString(Charsets.UTF_8) }
            if (tekst != null)
                for ((et, v) in Json(tekst).objekt()) {
                    val kirje = v as? Map<*, *> ?: continue
                    tolked[et] = (kirje["ru"] as? String ?: "") to (kirje["en"] as? String ?: "")
                }
        }
    }

    // Väike JSON-lugeja (objektid, tekstid, true/false/null, arvud) – ei sõltu Androidi teekidest
    private class Json(private val s: String) {
        private var i = 0
        private fun tyhik() { while (i < s.length && s[i].isWhitespace()) i++ }
        private fun ootan(c: Char) { tyhik(); require(s[i] == c) { "JSON: oodati '$c' kohal $i" }; i++ }

        fun vaartus(): Any? {
            tyhik()
            return when (s[i]) {
                '{' -> objekt()
                '"' -> tekst()
                't' -> { i += 4; true }
                'f' -> { i += 5; false }
                'n' -> { i += 4; null }
                else -> { val a = i; while (i < s.length && s[i] !in ",}] \n\r\t") i++; s.substring(a, i).toDoubleOrNull() }
            }
        }

        fun objekt(): Map<String, Any?> {
            val m = LinkedHashMap<String, Any?>()
            ootan('{'); tyhik()
            if (s[i] == '}') { i++; return m }
            while (true) {
                tyhik(); val k = tekst(); ootan(':'); m[k] = vaartus(); tyhik()
                if (s[i] == ',') { i++; continue }
                ootan('}'); return m
            }
        }

        private fun tekst(): String {
            ootan('"')
            val b = StringBuilder()
            while (s[i] != '"') {
                val c = s[i++]
                if (c != '\\') { b.append(c); continue }
                when (val e = s[i++]) {
                    'n' -> b.append('\n'); 't' -> b.append('\t'); 'r' -> b.append('\r'); 'b' -> b.append('\b'); 'f' -> b.append('\u000C')
                    'u' -> { b.append(s.substring(i, i + 4).toInt(16).toChar()); i += 4 }
                    else -> b.append(e)
                }
            }
            i++
            return b.toString()
        }
    }
}
