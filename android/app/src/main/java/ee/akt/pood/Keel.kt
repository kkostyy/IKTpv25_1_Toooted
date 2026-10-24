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

    private val tolked = HashMap<String, Pair<String, String>>()
    private fun L(et: String, ru: String, en: String) { tolked[et] = ru to en }

    init { Tolked.lisa(::L) }
}
