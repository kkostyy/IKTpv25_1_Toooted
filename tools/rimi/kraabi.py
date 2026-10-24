# Rimi e-poe kataloogi allalaadija: sitemap → iga tootelehe andmed → JSONL (jätkab pooleli jäänud kohast)
import re, json, html, sys, time, random, threading, urllib.request
from concurrent.futures import ThreadPoolExecutor

ALUS = sys.argv[1]            # kaust, kus p1..p4.xml
VALJUND = sys.argv[2]         # tooted.jsonl
PIIR = int(sys.argv[3]) if len(sys.argv) > 3 else 0
TOOLISI = int(sys.argv[4]) if len(sys.argv) > 4 else 6

def lae(url, katseid=4):
    for k in range(katseid):
        try:
            req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0 (AKT oppeprojekt)', 'Accept-Language': 'et'})
            with urllib.request.urlopen(req, timeout=40) as r:
                return r.status, r.read().decode('utf-8', 'replace')
        except urllib.error.HTTPError as e:
            if e.code in (404, 410): return e.code, ''
            time.sleep(2 ** k)
        except Exception:
            time.sleep(2 ** k)
    return 0, ''

def puhas(t):
    t = re.sub(r'<br\s*/?>', '\n', t)
    t = re.sub(r'</(p|li|div)>', '\n', t)
    t = re.sub(r'<[^>]+>', '', t)
    t = html.unescape(t)
    t = re.sub(r'[ \t\r\f\v]+', ' ', t)
    t = '\n'.join(x.strip() for x in t.split('\n') if x.strip())
    return t.strip()

def arv(s):
    s = s.replace('\xa0', '').replace(' ', '').replace('€', '').replace(',', '.')
    try: return float(s)
    except: return None

def parsi(sku, url, s):
    d = re.sub(r'\\u([0-9a-fA-F]{4})', lambda m: chr(int(m.group(1), 16)), s).replace('\\/', '/').replace('\\n', '\n').replace('\\"', '"')
    d = d.encode('utf-16', 'surrogatepass').decode('utf-16', 'replace')   # emoji paarid (\ud83d\ude00) kokku
    t = {'sku': int(sku), 'url': url}
    m = re.search(r'<h1 id="product_name" class="name">(.*?)</h1>', s, re.S)
    if not m: return None
    t['nimi'] = puhas(m.group(1))
    ld = re.search(r'application/ld\+json[^>]*>(.*?)</script>', s, re.S)
    hind = None; saadaval = True
    if ld:
        try:
            j = json.loads(ld.group(1))
            hind = float(j['offers']['price'])
            saadaval = 'InStock' in j['offers'].get('availability', 'InStock')
        except Exception: pass
    if hind is None:
        m = re.search(r'<div class="price">\s*<span>(\d+)</span>\s*<div>\s*<sup>(\d+)</sup>', s)
        if m: hind = float(m.group(1) + '.' + m.group(2))
    if hind is None: return None
    t['hind'] = hind
    t['saadaval'] = saadaval
    m = re.search(r'class="price__old-price">([^<]*)<', s)
    t['vanaHind'] = arv(m.group(1)) if m else None
    m = re.search(r'class="price-per">\s*([\d.,]+)\s*€/(\w+)', s)
    t['uhikuHind'] = arv(m.group(1)) if m else None
    t['uhik'] = m.group(2) if m else None
    m = re.search(r'"category":"(SH-[\d-]+)', d)
    kood = m.group(1) if m else ''
    t['kategooria'] = '-'.join(kood.split('-')[:2]) if kood else None
    t['alamkategooria'] = kood
    # dl: Päritoluriik, Kaubamärk, Tootja, Kogus
    for dt, dd in re.findall(r'<dt>(.*?)</dt>\s*<dd class="text">(.*?)</dd>', d, re.S):
        dt = puhas(dt); dd = puhas(dd)
        if dt.startswith('Päritolu'): t['maa'] = dd
        elif dt.startswith('Kaubamärk'): t['kaubamark'] = dd
        elif dt.startswith('Tootja'): t['tootja'] = dd
        elif dt == 'Kogus': t['kogus'] = dd
    # jaotised
    for pealk, sisu in re.findall(r'<h3 class="heading">(.*?)</h3>\s*(<div class="content">.*?</div>|<ul class="list">.*?</ul>)', d, re.S):
        p = puhas(pealk)
        if p == 'Toote andmed':
            k = '\n'.join(x for x in puhas(sisu).split('\n') if not x.startswith('Vaata ') and not x.endswith('siit>'))
            if k: t['kirjeldus'] = k
        elif p == 'Koostisosad': t['koostis'] = puhas(sisu)
        elif p == 'Lisainfo': t['lisainfo'] = puhas(sisu)
    # toiteväärtus
    tab = re.search(r'<div class="product__table">(.*?)</table>', d, re.S)
    if tab:
        read = []
        for klass, a, b in re.findall(r'<tr>\s*<td\s*(class="indent")?\s*>(.*?)</td>\s*<td>(.*?)</td>', tab.group(1), re.S):
            a = puhas(a); b = puhas(b)
            if a and b: read.append(('-' if klass else '') + a[0].upper() + a[1:] + ';' + b)
        if read: t['toitevaartus'] = '\n'.join(read)
    if re.search(r'tagatisraha|pandipakend', d, re.I): t['pant'] = True
    return t

def main():
    urlid = []
    for i in range(1, 5):
        try:
            urlid += re.findall(r'<loc>([^<]+/p/(\d+))</loc>', open(f'{ALUS}/p{i}.xml', encoding='utf-8').read())
        except FileNotFoundError: pass
    nahtud = set(); unikaalne = []
    for u, sku in urlid:
        if sku not in nahtud: nahtud.add(sku); unikaalne.append((u, sku))
    tehtud = set()
    try:
        for rida in open(VALJUND, encoding='utf-8'):
            try: tehtud.add(str(json.loads(rida)['sku']))
            except Exception: pass
    except FileNotFoundError: pass
    try:
        for rida in open(VALJUND + '.vead', encoding='utf-8'): tehtud.add(rida.split()[0])
    except FileNotFoundError: pass
    too = [(u, s) for u, s in unikaalne if s not in tehtud]
    if PIIR: too = too[:PIIR]
    print(f'kokku {len(unikaalne)}, tehtud {len(tehtud)}, teha {len(too)}', flush=True)
    lukk = threading.Lock(); loendur = [0]
    out = open(VALJUND, 'a', encoding='utf-8', errors='replace'); vead = open(VALJUND + '.vead', 'a', encoding='utf-8')
    algus = time.time()
    def tee(paar):
        u, sku = paar
        time.sleep(random.uniform(0.2, 0.6))   # viisakalt
        kood, s = lae(u)
        t = parsi(sku, u, s) if kood == 200 else None
        with lukk:
            if t: out.write(json.dumps(t, ensure_ascii=False) + '\n')
            else: vead.write(f'{sku} {kood}\n')
            loendur[0] += 1
            if loendur[0] % 200 == 0:
                out.flush(); vead.flush()
                kiirus = loendur[0] / (time.time() - algus)
                print(f'{loendur[0]}/{len(too)}  {kiirus:.1f}/s  jääb ~{(len(too)-loendur[0])/max(kiirus,0.01)/60:.0f} min', flush=True)
    with ThreadPoolExecutor(TOOLISI) as ex: list(ex.map(tee, too))
    out.close(); vead.close()
    print('valmis', flush=True)

main()
