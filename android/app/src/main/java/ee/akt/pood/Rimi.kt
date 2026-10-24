package ee.akt.pood

import android.content.ContentValues
import android.content.Context
import android.database.sqlite.SQLiteDatabase
import android.util.JsonReader
import android.util.JsonToken
import java.io.InputStreamReader
import java.util.zip.GZIPInputStream

// Rimi kataloogi laadimine (sama fail mis arvutiversioonis, tools/rimi/tee_kataloog.py):
// {"versioon":…, "kategooriad":[{id,nimi}], "tooted":[{n,h,v,k,p,b,t,m,pk,u,uk,ki,ko,li,tv,pa,l,oh}]}
// Loetakse voona (JsonReader), et 18 000 toodet ei peaks korraga mälus olema.
object Rimi {
    fun laadi(context: Context, db: SQLiteDatabase) {
        JsonReader(InputStreamReader(GZIPInputStream(context.assets.open("rimi_kataloog.json.gz")), Charsets.UTF_8)).use { r ->
            r.beginObject()
            while (r.hasNext()) {
                when (r.nextName()) {
                    "kategooriad" -> {
                        r.beginArray()
                        while (r.hasNext()) {
                            var id = 0; var nimi = ""
                            r.beginObject()
                            while (r.hasNext()) when (r.nextName()) { "id" -> id = r.nextInt(); "nimi" -> nimi = r.nextString(); else -> r.skipValue() }
                            r.endObject()
                            db.insert("kategooriad", null, ContentValues().apply { put("id", id); put("nimi", nimi) })
                        }
                        r.endArray()
                    }
                    "tooted" -> {
                        r.beginArray()
                        var id = 1
                        while (r.hasNext()) {
                            val v = ContentValues()
                            v.put("id", id++)
                            r.beginObject()
                            while (r.hasNext()) {
                                val nimi = r.nextName()
                                if (r.peek() == JsonToken.NULL) { r.nextNull(); continue }
                                when (nimi) {
                                    "n" -> v.put("nimi", r.nextString())
                                    "h" -> v.put("hind", Math.round(r.nextDouble() * 100).toInt())
                                    "v" -> v.put("vanaHind", Math.round(r.nextDouble() * 100).toInt())
                                    "k" -> v.put("kat", r.nextInt())
                                    "p" -> v.put("pilt", r.nextString())
                                    "b" -> v.put("kaubamark", r.nextString())
                                    "t" -> v.put("tootja", r.nextString())
                                    "m" -> v.put("maa", r.nextString())
                                    "pk" -> v.put("pakend", r.nextString())
                                    "u" -> v.put("uhik", r.nextString())
                                    "uk" -> v.put("uhikuKogus", r.nextDouble())
                                    "ki" -> v.put("kirjeldus", r.nextString())
                                    "ko" -> v.put("koostis", r.nextString())
                                    "li" -> v.put("lisainfo", r.nextString())
                                    "tv" -> v.put("toitevaartus", r.nextString())
                                    "pa" -> v.put("pant", Math.round(r.nextDouble() * 100).toInt())
                                    "l" -> v.put("kogus", r.nextInt())
                                    else -> r.skipValue()
                                }
                            }
                            r.endObject()
                            db.insert("tooted", null, v)
                        }
                        r.endArray()
                    }
                    else -> r.skipValue()
                }
            }
            r.endObject()
        }
    }
}
