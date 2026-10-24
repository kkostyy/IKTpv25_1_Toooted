# tooted.jsonl (kraabi.py väljund) → IKTpv25_1_Toooted/Andmed/rimi_kataloog.json.gz
# Kategooriate Id 1–7 jäävad samaks (programmis on neile loogika: õhtune soodustus = 2, puuviljad = 3).
import json, gzip, sys, re, random, datetime

SISSE, VALJA = sys.argv[1], sys.argv[2]

KATEGOORIAD = [  # (programmi Id, nimi, Rimi kood)
    (1, "Piimatooted, munad, juust", "SH-11"),
    (2, "Leivad, saiad, kondiitritooted", "SH-6"),
    (3, "Puuviljad, köögiviljad", "SH-12"),
    (4, "Liha- ja kalatooted", "SH-8"),
    (5, "Joogid", "SH-3"),
    (6, "Maiustused ja snäkid", "SH-9"),
    (7, "Külmutatud toidukaubad", "SH-4"),
    (8, "Kauasäilivad toidukaubad", "SH-13"),
    (9, "Valmistoit", "SH-16"),
    (10, "Vegantooted", "SH-17"),
    (11, "Talu toidab", "SH-19"),
    (12, "Alkohol", "SH-1"),
    (13, "Lastekaubad", "SH-5"),
    (14, "Enesehooldustarbed", "SH-2"),
    (15, "Puhastusvahendid", "SH-14"),
    (16, "Kodu- ja vabaajakaubad", "SH-10"),
    (17, "Lemmikloomakaubad", "SH-7"),
]
KOOD = {k: i for i, _, k in KATEGOORIAD}
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
    kat = KOOD.get(t.get('kategooria'))
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
