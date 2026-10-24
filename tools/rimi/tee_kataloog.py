# tooted.jsonl (kraabi.py väljund) → IKTpv25_1_Toooted/Andmed/rimi_kataloog.json.gz
# Kategooriate Id 1–7 jäävad samaks (programmis on neile loogika: õhtune soodustus = 2, puuviljad = 3).
import json, gzip, sys, re, random, datetime

SISSE, VALJA = sys.argv[1], sys.argv[2]

KATEGOORIAD = [  # (programmi Id, nimi, Rimi URL-i esimene osa /tooted/<osa>/…)
    # NB! Rimi koodid (SH-x) ei sobi: joogid on kolme koodi all, puhastusvahendid kodukaupade all
    (1, "Piimatooted, munad, juust", "piimatooted-munad-juust"),
    (2, "Leivad, saiad, kondiitritooted", "leivad-saiad-kondiitritooted"),
    (3, "Puuviljad, köögiviljad", "puuviljad-koogiviljad-lilled"),
    (4, "Liha- ja kalatooted", "liha--ja-kalatooted"),
    (5, "Joogid", "joogid"),
    (6, "Maiustused ja snäkid", "maiustused-ja-snakid"),
    (7, "Külmutatud toidukaubad", "kulmutatud-toidukaubad"),
    (8, "Kauasäilivad toidukaubad", "kauasailivad-toidukaubad"),
    (9, "Valmistoit", "valmistoit"),
    (10, "Vegantooted", "vegantooted"),
    (11, "Talu toidab", "talu-toidab"),
    (12, "Alkohol", "alkohol"),
    (13, "Lastekaubad", "lastekaubad"),
    (14, "Enesehooldustarbed", "enesehooldustarbed"),
    (15, "Puhastusvahendid", "puhastusvahendid"),
    (16, "Kodu- ja vabaajakaubad", "kodu--ja-vabaajakaubad"),
    (17, "Lemmikloomakaubad", "lemmikloomakaubad"),
]
KOOD = {k: i for i, _, k in KATEGOORIAD}
def kategooria(t):
    m = re.search(r'/tooted/([^/]+)/', t.get('url') or '')
    return KOOD.get(m.group(1)) if m else None
PILT = "https://rimibaltic-res.cloudinary.com/image/upload/b_white,c_limit,f_jpg,h_400,q_85,w_400/d_ecommerce:backend-fallback.png/MAT_{}_PCE_EE"

def kogus(t):
    # "0.185 kg" / "1 l" / "10 tk" → (ühik, kogus)
    m = re.match(r'\s*([\d.,]+)\s*(kg|g|l|ml|tk)\b', t.get('kogus') or '')
    if m:
        v = float(m.group(1).replace(',', '.')); u = m.group(2)
        if u == 'g': v, u = v / 1000, 'kg'
        if u == 'ml': v, u = v / 1000, 'l'
        if v > 0: return u, round(v, 3)
    if t.get('uhikuHind') and t.get('uhik') in ('kg', 'l') and t['uhikuHind'] > 0:
        return t['uhik'], round(t['hind'] / t['uhikuHind'], 3)
    return 'tk', 1.0

tooted = []
for rida in open(SISSE, encoding='utf-8'):
    t = json.loads(rida)
    kat = kategooria(t)
    if not kat or not t.get('nimi') or not t.get('hind'): continue   # teenused jms
    juhus = random.Random(t['sku'])
    uhik, uk = kogus(t)
    vana = t.get('vanaHind')
    pant = 0.10 if t.get('pant') or (kat in (5, 12) and re.search(r'\d\s*(l|ml|cl)\b', t['nimi'], re.I) and re.search(r'pudel|purk|PET|vesi|limonaad|õlu|siider|karastus|mahl|jook|cola', t['nimi'], re.I)) else 0
    tooted.append({
        'n': t['nimi'], 'h': round(t['hind'], 2), 'v': round(vana, 2) if vana and vana > t['hind'] else None,
        'k': kat, 'p': PILT.format(t['sku']),
        'b': t.get('kaubamark'), 't': t.get('tootja'), 'm': t.get('maa'),
        'pk': t.get('kogus'), 'u': uhik, 'uk': uk,
        'ki': t.get('kirjeldus'), 'ko': t.get('koostis'), 'li': t.get('lisainfo'), 'tv': t.get('toitevaartus'),
        'pa': pant,
        'l': juhus.randint(4, 80) if t.get('saadaval', True) else 0,
        'oh': round(t['hind'] * juhus.uniform(0.55, 0.8), 2),
    })
tooted.sort(key=lambda x: (x['k'], x['n']))
data = {'versioon': 'rimi-' + datetime.datetime.now().strftime('%Y-%m-%dT%H:%M'),
        'kategooriad': [{'id': i, 'nimi': n} for i, n, _ in KATEGOORIAD],
        'tooted': tooted}
with gzip.open(VALJA, 'wt', encoding='utf-8', compresslevel=9) as f:
    json.dump(data, f, ensure_ascii=False, separators=(',', ':'))
from collections import Counter
print(len(tooted), 'toodet', dict(sorted(Counter(t['k'] for t in tooted).items())))
