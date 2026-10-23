# Промпт для Claude Code: новые функции AKT pood

Скопируй всё ниже линии в Claude Code (в корне репозитория `IKTpv25_1_Toooted`).

---

Ты работаешь в репозитории WinForms-приложения **AKT pood** (`IKTpv25_1_Toooted/`, .NET 8, `net8.0-windows`, EF Core 8 + SQLite, PdfSharpCore, BCrypt). Код и имена — на эстонском, комментарии короткие, на эстонском, в том же стиле, что уже есть. Нужно реализовать новые функции и единую навигацию «Назад» по готовому дизайну.

## 0. Дизайн (обязательно открой)

Холст: https://claude.ai/artifact/E11vFPRmD1yhr3B8H8yKVW — страница **«Новые функции»** (доски N1–N12, телефон T1–T11), плюс общий стиль на страницах «Компьютер» и «Телефон» (доска «4.3 Система» — отступы 4/8/12/16/24/32/48, радиусы 8/18/24/32/пилюля, кривая анимации `cubic-bezier(.2,.8,.2,1)`, длительности). Если холст недоступен, ориентируйся на описания ниже — они самодостаточны.

Цвета бери **только** из `Teema` (`Teema.Aktsent` = красный Rimi, `Teema.Teine` = сливовый, `Teema.Edu`, `Teema.Hoiatus`, `Teema.Viga`, `Teema.Taust`, `Teema.Pind`, `Teema.Paneel2`, `Teema.Tekst`, `Teema.Tekst2`, `Teema.TekstNork`), чтобы работали светлая/тёмная тема и цвет магазина. Элементы — через `UiStiil` (`LooNupp`, `LooSuurNupp`, `LooSisend`, `LooKaardiPaneel`, `LooKaart`), `RoundedPanel`, `RoundedButton` (`Peamine`, `Ohtlik`, `Radius`), `Ymardus.Tee(...)` для скруглений, `Ikoonid` для иконок (никаких эмодзи в новом UI). Каждая новая форма в `OnLoad` вызывает `Teema.Rakenda(this)` (или наследуется от `KujundusVorm`).

## 1. Правила проекта (не нарушать)

- **Не редактировать** `Form1.Designer.cs` — всё новое создаётся в коде (как сейчас делают `Kassa.cs`, `Form1Rollid.cs`).
- Все строки интерфейса — на эстонском через `Keel.T("...")` / `Keel.F("... {0}", x)`. Для **каждой** новой строки добавь перевод RU и EN: сделай `Keel` `partial`, создай `KeelUued.cs` с методом `static partial void LisaUuedTolked()` (или обычным `private static void`), вызови его в конце статического конструктора `Keel`, внутри — `L("eesti", "русский", "English");`.
- Деньги — `decimal`, формат через `Ostukorv.Eur(...)`.
- Новые таблицы/колонки — через **одну** EF-миграцию `Lisafunktsioonid` (папка `Migrations/`, namespace `IKTpv25_1_Toooted.Migrations`, обнови `ToodedContextModelSnapshot.cs`). Генерируй её инструментом (`dotnet ef migrations add Lisafunktsioonid` или `Add-Migration Lisafunktsioonid`), а не руками, если инструмент доступен. В `Program.cs` добавь проверки `_ = db.<НоваяТаблица>.AsNoTracking().FirstOrDefault();` как для остальных таблиц.
- Анимации в WinForms: создай `Animatsioon.cs` — помощник на `System.Windows.Forms.Timer` (15 мс) + `Stopwatch`, с easing «ease-out cubic» (`1 - Math.Pow(1 - t, 3)`) и пружиной для «поп» (`back-out`). API: `Animatsioon.Kaivita(Control omanik, int ms, Action<double> samm, Action? valmis = null)`. Если `!SystemInformation.UIEffectsEnabled` — сразу вызвать `samm(1)` и `valmis` (уважение к «уменьшить движение»). Примеры уже есть в `KliendikaartVorm` (таймер + `Stopwatch`) — используй тот же подход.

## 2. Навигация «Назад» (доска N1)

Цель: пользователь всегда может вернуться назад одной кнопкой.

**2.1 Магазин — одно окно.** В `PoodVorm` центральная область (`sisu`) становится контейнером страниц:
- Создай `Navigaator.cs`: класс со стеком `Stack<Leht>`, методы `Ava(Leht leht)`, `Tagasi()`, `Avalehele()`, событие `Muutus`. `Leht` — абстрактный `UserControl` (`Leht.cs`) со свойством `Pealkiri` (для «хлебных крошек») и виртуальными `Avatud()` / `Suletud()`.
- Переход вперёд: новая страница въезжает справа (смещение 70 px → 0 + прозрачность через перерисовку), назад — слева; 450 мс, `Animatsioon`.
- Над `sisu` — полоса навигации: круглая кнопка **«← Tagasi»** (тёмная пилюля; на главной — серая и `Enabled = false`), «хлебные крошки» из `Pealkiri` (каждая кликабельна → возврат к этому уровню), подсказка `Esc` / `Alt+←`.
- Назад работает от: кнопки, `Esc` (если фокус не в поле ввода с текстом), `Alt+Left`, `Backspace` вне полей ввода, боковой кнопки мыши `XButton1` (обработай в `PoodVorm` через `WndProc`/`MouseDown` дочерних контролов или `IMessageFilter` для `WM_XBUTTONDOWN`).
- Текущие виды (`Vaade.Avaleht`, `Kategooria`, `Kampaaniad`, `Odav`, `Lemmikud`, `Otsing`) и новые страницы (Toiteväärtus, Preemiad, Pere nimekiri) открываются через `Navigaator.Ava(...)`. Логику `NaitaAvaleht` / `NaitaRuudustik` перенеси в страницы `AvalehtLeht` и `RuudustikLeht` (или оберни: страница вызывает существующий код заполнения).

**2.2 Диалоги и админ-окна — кнопка закрывает окно.** Для всех модальных форм (`ToodeVorm`, `OstunimekiriVorm`, `MaksmineVorm`, `KliendidVorm`, `StatistikaVorm`, `SeadedVorm`, `KasutajadVorm`, `EmailSeadedVorm`, `OstudVorm` (история), `TooteInfoVorm`, `ParooliMuutmineVorm`, `RegistreerimineVorm`, новые `KampaaniadVorm` и `PereDialoog`):
- Добавь `UiStiil.LisaTagasiNupp(Form vorm, Control pais)` — круглая кнопка «←» 44 px в левом верхнем углу шапки, `vorm.Close()` (для диалогов `DialogResult.Cancel`).
- `Esc` и `XButton1` тоже закрывают (`KeyPreview = true`). Если есть несохранённые изменения — спросить через `Teade.Show(..., MessageBoxButtons.YesNo)`.
- В `KliendikaartVorm` уже есть закрытие по Esc — добавь только кнопку «←» к существующему «✕».

## 3. Управление акциями (доска N2, админ)

**Модель** `Kampaania.cs`: `Id`, `Nimi` (string), `ToodeId` (FK → `Toode`, `OnDelete(Cascade)`), `SoodusProtsent` (int, 1–90), `Algus` (DateTime), `Lopp` (DateTime), `Automaatne` (bool, по умолчанию true), `Rakendatud` (bool — скидка сейчас применена к товару), `TavaHind` (`decimal?`, `[Precision(10,2)]` — цена до акции). `DbSet<Kampaania> Kampaaniad`.

**Логика** `Kampaaniad.cs` (static):
- `Uuenda(ToodedContext db, DateTime nyyd)` — 1) для применённых, у которых `nyyd > Lopp` (или удаляемых): вернуть `toode.Hind = TavaHind`, `toode.VanaHind = null`, `Rakendatud = false`; 2) для неприменённых с `Automaatne && Algus <= nyyd <= Lopp`: `TavaHind = toode.VanaHind ?? toode.Hind`, `toode.VanaHind = TavaHind`, `toode.Hind = Math.Round(TavaHind * (100 - SoodusProtsent) / 100m, 2)`, `Rakendatud = true`; `SaveChanges()`.
- `Olek(k, nyyd)` → `Käib` / `Tulekul` / `Lõppenud`.
- Валидация: у одного товара не может быть двух акций, которые пересекаются по датам; `Lopp > Algus`.
- Вызывать `Uuenda` в `Program.cs` после `Migrate()`, при открытии магазина (`PoodVorm.LaadiTooted`) и после сохранения в админке.

**UI** `KampaaniadVorm` (меню «Haldus → Kampaaniad...» и пункт в `KulgMenuu`, только `AktiivneKasutaja.OnOmanik`):
- Шапка «Kampaaniad» + переключатель-пилюля `Kõik / Käib / Tulekul / Lõppenud` со счётчиками.
- **Ajatelg** — свой контрол `KampaaniaTelg` (OnPaint): 4 недели, полоски-пилюли акций (Käib = `Teema.Aktsent`, Tulekul = `Teema.Teine`, Lõppenud = серый), вертикальная красная линия «Täna» с пульсирующей точкой. Анимация: полоски вырастают слева направо при открытии (stagger 90 мс).
- Таблица: фото товара, название акции + товар, `−%`, период, новая/старая цена, чип статуса (у «Käib» мигающая точка).
- Правая панель «Uus kampaania»: Nimi, Toode (ComboBox с поиском), ползунок `TrackBar` 5–70 % (большая цифра `−30%`, «поп» при изменении), Algus/Lõpp (`DateTimePicker`), тёмный блок предпросмотра «Poes näeb välja: 1,25 € ~~1,79 €~~» — пересчёт вживую, переключатель «Automaatne», кнопки «Salvesta kampaania», «Lõpeta kohe» (ставит `Lopp = DateTime.Now` и вызывает `Uuenda`), «Kustuta».

## 4. Фильтр по диете и аллергенам (доска N3)

**Модель:** `Toode.Margised` (`string?`, через `;`: `laktoosivaba;gluteenivaba;vegan;pahklivaba`) — ручные отметки админа.

**Логика** `Dieet.cs`:
- `[Flags] enum DieediMark { Laktoosivaba = 1, Gluteenivaba = 2, Vegan = 4, Pahklivaba = 8, Eesti = 16 }`.
- `Analuusi(Toode t) → DieediInfo { HashSet<string> Sisaldab, HashSet<string> VoibSisaldada, bool KoostisTeada }`. Источники: 1) слова **ЗАГЛАВНЫМИ** в `Koostisosad` (правило ЕС: `PIIM`, `NISU`, `RUKIS`, `ODER`, `KAER`, `MUNA`, `SOJA`, `KALA`, `PÄHKEL`, `SARAPUU`, `MANDEL`, `MAAPÄHKEL`, `SELLER`, `SINEP`, `SEESAM`… — ищи и как часть слова: `täisPIIM`, `RUKKIjahu`, `NISUjahu`); 2) словарь ключевых слов в нижнем регистре (`piim`, `koor`, `juust`, `või`, `vadak`, `laktoos`, `nisu`, `rukis`, `gluteen`, `muna`, `liha`, `sea`, `veise`, `kana`, `peekon`, `vorst`, `kala`, `mesi`…); 3) «Võib sisaldada …» / «võib sisaldada … jääke» в `Lisainfo` → `VoibSisaldada`; 4) «laktoosivaba» в `Lisainfo`/`Kirjeldus`/названии → лактозы нет, даже если есть PIIM; 5) категория «Puuviljad, köögiviljad» → подходит под все диеты; 6) `Paritolumaa == "Eesti"` → Eesti; 7) `Margised` админа имеет приоритет.
- `Sobib(Toode, DieediMark valik, bool range)` — товар с неизвестным составом под диету **не** подходит; «võib sisaldada» исключается только в строгом режиме.
- `MinuDieet` — выбор пользователя в `dieet.json` (как `Lemmikud`: `Kaustad.Juur`, событие `Muutus`), поля `Valik` и `Range`.

**UI в магазине:**
- Под навигацией — карточка «Minu dieet»: зелёный круг с листом, чипы-переключатели `Laktoosivaba / Gluteenivaba / Vegan / Pähklivaba / Eesti toode` (выбран = `Teema.Edu`, белая галочка «поп»), переключатель «Range režiim», счётчик «Sulle sobib **4** / 8 toodet» («поп» при изменении), «Koostis on teada N tootel».
- Фильтр применяется к полкам главной и ко всем сеткам. Неподходящие карточки плавно гаснут (прозрачность 0.38 + оттенки серого + scale 0.96, 450 мс) с чёрной меткой «Ei sobi» — или скрываются, если включено «Peida sobimatud».
- На `TooteKaart` — до 3 чипов: зелёный «✓ Laktoosivaba», красный «Sisaldab: piim», оранжевый «Võib sisaldada: nisu».
- В `ToodeVorm` — блок «Allergeenid», где аллергены в составе подсвечены красным фоном.
- В `TooteInfoVorm` (админ) — чекбоксы «Märgised».

## 5. Питательность корзины (доска N4)

**Логика** `Toitevaartus.cs`:
- `Parsi(string? tekst) → Dictionary<string, decimal>` из `Toode.Toitevaartus` (строки `nimetus;väärtus`, подстроки начинаются с `-`; энергия — число перед `kcal`; граммы — число перед `g`; запятая/точка).
- `KaalGrammides(Toode t)`: `Uhik` = `kg` или `l` → `UhikuKogus * 1000` (литры = граммы 1:1); иначе пробуй разобрать `Pakend` (`0.5 kg`, `2 l`, `450 g`); `tk` → `null` (нет веса).
- `Kokkuvote(IEnumerable<KorvRida> read, IDictionary<int, Toode> tooted) → KorviToitevaartus { kcal, rasvad, kyllastunud, susivesikud, suhkrud, valgud, sool, Dictionary<int, decimal> kcalToote, List<string> IlmaAndmeteta }`.
- Суточные нормы (RI): 2000 kcal, жиры 70, насыщенные 20, углеводы 260, сахар 90, белки 50, соль 6.

**UI** `ToitevaartusLeht` (страница навигатора; ссылка «Toiteväärtus» в подвале корзины `PoodVorm` рядом с «Salvesta ostukorv»):
- Слева — корзина со степперами (изменения сразу пересчитывают всё), под строкой «N kcal» или «andmed puuduvad» оранжевым, внизу — плашка-предупреждение со списком товаров без данных.
- В центре — кольцо (свой контрол, `DrawArc` толщиной 26): доли энергии жиры/углеводы/белки (9/4/4 ккал на г) цветами Aktsent / оранжевый / Edu; при открытии кольцо раскручивается за 1,2 с; в центре «5 152 kcal kokku». Справа крупно «**2,6** päeva energiat ühele inimesele», легенда с процентами.
- Полосы по каждому нутриенту: граммы и «×» от суточной нормы; если сахар/насыщенные/соль больше 1,5× — чип красный. Ширина полос анимируется при каждом изменении (700 мс).
- Справа тёмная панель «Kust kalorid tulevad»: составная полоса и список вклада товаров в процентах.

## 6. Общий список с семьёй (доска N5)

**Модель:** `Kasutaja.PereKood` (`string?`, индекс), `OstunimekirjaRida.OstisKasutajaId` (`int?` — кто отметил «куплено»).

**Логика** `Pere.cs`: `LooKood()` (7 символов, без похожих `0/O/1/I`, уникальный), `Liitu(string kood)`, `Lahku()`, `Liikmed(db)`, `NahtavadKasutajad(db)` → id всех с тем же `PereKood` (или только свой id). Все запросы в `Ostunimekiri` и `OstunimekiriVorm` (`Lisa`, `MargiOstetuks`, `OstmataArv`, `Laadi`, `Muuda`, `Kustuta…`) переводятся с `r.KasutajaId == AktiivneKasutaja.Id` на `ids.Contains(r.KasutajaId)`. При отметке «куплено» записывать `OstisKasutajaId`. Это одна локальная база — «семья» = аккаунты в этом же приложении; так и напиши в подсказке.

**UI** (`OstunimekiriVorm` → вид «Pere ostunimekiri», если пользователь в семье):
- Шапка: «Pere ostunimekiri», «Ostetud 2 / 6 · kõik näevad sama nimekirja», ряд аватаров-кругов с инициалами (появляются «поп» по очереди; зелёная точка — тот, кто сейчас вошёл в приложение), кнопка «+ Kutsu / liitu».
- Строки: круглый чекбокс (галочка «поп», зачёркивание), название, количество, аватар автора, «ostis Jaan» зелёным.
- Правая тёмная панель: «PERE KOOD» — 7 плиток-символов (первые 3 красные), выпадают по очереди; «Kopeeri kood» (`Clipboard.SetText`); «Viimased tegevused» — последние добавления и покупки с аватарами, новые въезжают справа.
- `PereDialoog`: затемнение + карточка снизу вверх, 7 ячеек ввода кода (автопереход к следующей, вставка из буфера), кнопки «Tagasi» / «Liitu», ссылки «loo uus pere» / «lahku perest».
- Простое «живое» обновление: `Timer` раз в 5 с перечитывает список, если окно открыто.

## 7. Игровые элементы клиентской карты (доска N6)

**Модель** `Saavutus.cs`: `Id`, `KlientId` (FK → `Klient`, Cascade), `Kood` (string), `Aeg` (DateTime), `Boonus` (`decimal`, Precision 10,2). Уникальный индекс (`KlientId`, `Kood`).

**Логика** `Preemiad.cs`:
- **Уровни** по сумме покупок клиента за последние 12 месяцев (`Ost.Summa - TarneHind`, без `Tagastatud`): Pronks 0 € → 1 %, Hõbe ≥ 100 € → 1,5 %, Kuld ≥ 300 € → 2 %. `Tase(db, klientId)` → (nimi, protsent, summa, järgmiseni). Поменяй `Tarne.Boonus(decimal summa)` на `Tarne.Boonus(decimal summa, decimal protsent)` и в `MaksmineVorm` передавай процент уровня клиента (вместо `Form1.BoonusProtsent`). Подпись на чеке «1 € = 1 sent» сделай зависимой от процента.
- **Задания недели** (ISO-неделя `ISOWeek.GetWeekOfYear`), считаются по покупкам этой недели: «Eesti nädal» — 5 эстонских товаров → +0,50 €; «Värske valik» — 3 товара из «Puuviljad, köögiviljad» → +0,30 €; «Kolm korda» — 3 покупки → +0,50 €. Награда начисляется один раз: `Saavutus` с `Kood = "VK:{id}:{aasta}-W{nädal}"`, бонус прибавляется к `Klient.Boonus`.
- **Наклейки** (постоянные): «Esimene ost», «10 ostu», «Eesti patrioot» (50 эстонских товаров), «Tervislik» (20 фруктов/овощей), «Säästja» (сэкономлено 10 € на скидках: `Σ (VanaHind − Hind)` по строкам, если есть данные, иначе по `Soodustus`), «Kogu ring» (купил из каждой категории), «Hommikune» (покупка до 9:00), «Pere ostleja» (купил 10 строк из семейного списка). Новая наклейка записывается как `Kood = "KL:{id}"`.
- **Сюрприз дня**: раз в календарный день `AvaYllatus(db, klientId)` → случайно 0,05 / 0,10 / 0,15 / 0,20 / 0,25 €, `Kood = "YL:{yyyy-MM-dd}"`.
- `KontrolliPealeOstu(db, ost)` — вызывается в `Form1.Vormista()` после успешной покупки; возвращает список новых наград → `Teade.Toast(...)` для каждой.

**UI** `PreemiadLeht` (из `KliendikaartVorm` — кнопка «Preemiad»; пункт «Preemiad» в `KulgMenuu`):
- Слева тёмная колонка «SINU TASE»: кольцо прогресса до следующего уровня (заполняется 1,6 с), медаль уровня с бегущим бликом, «Kuld-tasemeni veel 114 € oste», лестница Pronks / Hõbe / Kuld с процентами (текущий уровень — белая плашка).
- «Nädala väljakutsed»: 3 карточки с иконкой, текстом, полосой прогресса (заполняется с задержкой по очереди), чип награды; выполненное — зелёная карточка «Tehtud! ✓ · +0,30 € saadud».
- «Kleepsualbum»: сетка 4×2 — полученные — цветные круги с белой рамкой, тенью и бликом, появляются «поп» (вращение −30° → 0); закрытые — серые с пунктирной рамкой; наклон и увеличение при наведении; счётчик «5 / 8».
- Справа сливовая панель «PÄEVA ÜLLATUS»: подарочная коробка (рисуется в OnPaint) покачивается каждые 2,4 с; кнопка «Ava kingitus» → крышка улетает вверх с поворотом, вылетает 10 конфетти, всплывает белая карточка «+0,15 € boonust kaardile»; кнопка становится «Avatud ✓» (неактивна до завтра).
- На самой карте в `KliendikaartVorm` показать уровень (маленькая медаль + «Hõbe · 1,5 %»).

## 8. Карта клиента по уровням и доработки наград (доски N6, N9, T6–T9)

- **Дизайн карты зависит от уровня** (`KliendikaartVorm` и мини-карта в `PreemiadLeht`), рисуется в `OnPaint`:
  - **Pronks** — матовая медь (градиент `#8A4B2A → #C98552`), без эффектов;
  - **Hõbe** — шлифованное серебро (`#8E959C → #E4E8EB`, тонкие горизонтальные штрихи) + блик, пробегающий по карте каждые 4 с;
  - **Kuld** — золото (`#A77A1E → #F2D27A`) + голографическая полоса (радужный градиент, сдвигается при движении мыши над картой) + 6–8 мерцающих искр.
  - Повышение уровня: после покупки, если уровень вырос, — полноэкранный тост «Uus tase: Kuld · 2 %» и анимация смены карты (переворот 600 мс через сжатие по X).
  - Пользовательское фото на карте (уже есть) сохраняется на любом уровне; рамка и медаль — по уровню.
- **Наклейки — подарок за полный набор**: когда собраны все 8, один раз начисляется **+2,00 €** (`Kood = "KL:KOIK"`), в альбоме — золотая плашка «Kogu kogu! +2,00 €» с конфетти.
- **Сюрприз дня = скидка на один товар** (заменяет денежный бонус из п. 7): `AvaYllatus` выбирает случайный товар в наличии (`Laoseis > 0`, исключая алкоголь/табак, если есть такие категории) и скидку 10 / 15 / 20 / 25 %; сохраняется `Saavutus` с `Kood = "YL:{yyyy-MM-dd}:{toodeId}:{protsent}"`. Скидка действует **только сегодня и только для этого клиента** — учитывается в `Hinnad.Lopphind` (см. п. 13). В UI после открытия коробки: карточка товара с картинкой, «−25 % Kodujuust Alma · täna», кнопка «Lisa korvi».
- В телефонной версии (T6–T9) те же 4 экрана переключаются вкладками Tase / Ülesanded / Kleepsud / Üllatus; кнопка «Tagasi» ведёт на карту.

## 9. Демо-данные одной кнопкой (доска N7)

`DemoAndmed.cs`, кнопка в админ-панели «Demo-andmed» (только роль Admin):
- `Lisa(db)` — создаёт **50 товаров** (по 7–8 в каждой из 7 категорий, реалистичные эстонские названия, цены 0,49–12,99 €, `Laoseis` 0–40, часть с `VanaHind`, `Paritolu = "Eesti"` примерно у половины, `Margised` для фильтра диеты, `Toitevaartus` для части) и **20 заказов** за последние 30 дней (2–6 строк, разное время суток и дни недели — чтобы тепловая карта была заполнена; 2 заказа `Tagastatud`), плюс 3 демо-клиента.
- Все созданные записи помечены (`Toode.Demo`, `Ost.Demo`, `Klient.Demo` — `bool`, по умолчанию `false`).
- `Eemalda(db)` — удаляет только `Demo == true` (с подтверждением). Повторное «Lisa» не создаёт дублей (сначала проверка).
- UI: карточка с двумя счётчиками (анимация счёта 0 → 50 / 0 → 20), полоса прогресса во время генерации, тост «Demo-andmed lisatud».

## 10. Залоговая тара — pant (доска N8, T10)

- `Toode.Pant` (`decimal`, Precision 10,2, по умолчанию 0). Для напитков в бутылках/банках — **0,10 €** (в демо-данных и для существующих напитков из категории 5 «Joogid» — миграцией/начальными данными).
- Карточка товара: под ценой строка «+ Pakendi tagatisraha 0,10 €» (серым, иконка бутылки).
- Корзина и `MaksmineVorm`: отдельная строка «Pakendi tagatisraha (N tk) X €», входит в «Kokku».
- Чек (PDF, PdfSharpCore) и e-mail: после строк товара — отдельная строка залога; внизу итог по залогу.
- `OstuRida.Pant` сохраняет залог на момент покупки. **Бонус на залог не начисляется** и скидки на него не действуют; бесплатная доставка (`Tarne.TasutaAlates`) считается без залога.

## 11. Панель продаж (доска N10, админ)

`MyygipaneelLeht` / расширение `StatistikaVorm`:
- **Тепловая карта «час × день недели»**: 7 строк (E–P) × часы 8–22; значение — сумма продаж (без `Tagastatud`) за выбранный период (7 / 30 / 90 дней). Цвет ячейки — от `Teema.Paneel2` до `Teema.Aktsent` по квантилям; при наведении — подсказка «Neljapäev 18:00 · 12 ostu · 184,30 €». Ячейки появляются волной (задержка по диагонали).
- **Самые прибыльные категории**: горизонтальные столбцы, сортировка по марже. Нужна закупочная цена: `Toode.Ostuhind` (`decimal?`); маржа строки = `(Hind − Ostuhind) × Kogus`, если `Ostuhind` нет — считать 30 % от цены и пометить «hinnanguline». Столбцы вырастают слева направо.
- Сверху 4 KPI-плитки: выручка, количество покупок, средний чек, маржа — с изменением к прошлому периоду (▲/▼).
- Всё считается в `Myygianalyys.cs` (чистые методы, удобно проверять).

## 12. Уценка по сроку годности (доска N11)

- Модель `Partii`: `Id`, `ToodeId` (FK, Cascade), `Kogus` (int), `Aegub` (DateTime, дата), `Lisatud` (DateTime). `Toode.Laoseis` = сумма `Kogus` партий (если у товара есть партии; иначе как раньше).
- В `ToodeVorm` вкладка «Partiid»: список партий с датой и остатком, «Lisa partii».
- `Aegumine.Uuenda(db)` (при запуске и раз в час таймером): партия, у которой до `Aegub` **≤ 2 дня**, получает **−30 %**; товар показывает бейдж «−30 % · aegub 25.10» и цену по самой ранней партии. Просроченные партии (`Aegub < täna`) списываются: `Kogus = 0`, запись в журнал «Maha kantud».
- Продажа — **FEFO**: при покупке сначала уменьшаются партии с самой ранней датой.
- Админ-экран: таймлайн партий по дням, цвета зелёный / жёлтый (≤ 2 дня) / красный (просрочено), кнопка «Kanna maha».

## 13. Вечерняя выпечка −50 % (доски N12, T11)

- Правило: **каждый день с 20:00 до закрытия (00:00) все товары категории 2 «Leivad, saiad, kondiitritooted» в 2 раза дешевле.**
- Настройки (`Seaded`, админ): `OhtuAlgus` (по умолчанию 20:00), `OhtuProtsent` (50), `OhtuKategooriaId` (2), переключатель «Sees».
- Создай `Hinnad.cs` — **единое место расчёта цены**: `Lopphind(Toode t, Klient? k, DateTime aeg)` возвращает (hind, allikas). Учитывает: акцию (`Kampaania`), уценку по сроку (п. 12), вечернюю выпечку, сюрприз дня (п. 8). **Скидки не суммируются — берётся самая большая.** Округление `Math.Round(x, 2, MidpointRounding.AwayFromZero)` (Rehe 1,69 → 0,85). Все места, где сейчас берётся `Toode.Hind` для продажи (карточки, корзина, `Kassa`, `MaksmineVorm`, чек), переведи на `Hinnad.Lopphind`.
- Цена фиксируется **в момент оформления** покупки (если корзину собрали в 19:55, а оплатили в 20:01 — цена уже −50 %); в корзине при наступлении 20:00 цены обновляются сами (таймер раз в минуту).
- UI: на главной и в категории — тёмно-сливовый баннер с луной «Õhtune küpsetiste tund»; до 20:00 — обратный отсчёт «Algab 12 min pärast», после — «Kehtib kuni 00:00». Бейдж «−50%» прилетает «штампом», новая цена падает сверху, старая зачёркнута. На чеке — строка «Õhtune soodustus −50 %».

## 14. Кнопка «Tagasi» на всех экранах

Помимо п. 2: проверь, что **каждый** экран и окно (в т.ч. новые: Kampaaniad, Dieet, Toiteväärtus, Pere, Preemiad, Demo-andmed, Pant-seaded, Müügipaneel, Partiid, Õhtune soodustus, а также Kliendikaart, Ostunimekiri, Maksmine, TooteInfo, Ajalugu, Seaded) имеет тёмную пилюлю «← Tagasi» слева в шапке (высота 48 px). Внутри магазина — `Navigaator.Tagasi()`, в отдельном окне — `Close()`. Старые «✕» в этих окнах убери (кроме `KliendikaartVorm`, где «✕» остаётся рядом).

## 15. Проверка и сдача

1. Сборка без ошибок и новых предупреждений. На Windows: `dotnet build`. Если работаешь в Linux, где нет Windows Desktop SDK, проверь компиляцию отдельным проектом, который включает все `*.cs` и ссылается на сборки из NuGet-пакета `Microsoft.WindowsDesktop.App.Ref` 8.0.x (`ref/net8.0/*.dll`), с глобальными `using System.Drawing; using System.Windows.Forms;` и заглушкой `ApplicationConfiguration.Initialize()`.
2. Миграция `Lisafunktsioonid` создана и применяется при запуске (`db.Database.Migrate()` в `Program.cs`); старая база пользователя обновляется без потери данных.
3. Ручной чек-лист (опиши результат в итоговом сообщении):
   - в магазине: открыть категорию → товар → «Tagasi» дважды возвращает на главную; `Esc`, `Alt+←` и боковая кнопка мыши работают; на главной кнопка неактивна;
   - диалоги закрываются «←» и `Esc`;
   - создать акцию на сегодня → цена в магазине изменилась, `VanaHind` зачёркнут; «Lõpeta kohe» → цена вернулась;
   - фильтр «Laktoosivaba + Gluteenivaba» оставляет Atleet и Munad (+ фрукты/овощи); выбор сохраняется после перезапуска;
   - Toiteväärtus для корзины Atleet ×1, Rehe ×2, Täispiim ×1 ≈ 5 152 kcal, 2,6 дня; «Munad» в списке без данных;
   - два пользователя в одной семье видят один список; «ostis …» показывает, кто купил;
   - после покупки начисляется бонус по проценту уровня; задание недели засчитывается один раз; сюрприз дня открывается один раз в день;
   - «Demo-andmed» добавляет 50 товаров и 20 заказов, повторно дублей нет, «Eemalda» удаляет только демо;
   - напиток в корзине → строка «Pakendi tagatisraha 0,10 €» в корзине и в PDF-чеке, бонус на неё не начислен;
   - тепловая карта и прибыльные категории заполнены на демо-данных;
   - партия с датой «послезавтра» даёт −30 %, просроченная списывается, продажа идёт с самой ранней партии;
   - в 20:00 (подмени время через `Func<DateTime> Kell` в `Hinnad`) Rehe стоит 0,85 €, акция −20 % на хлеб не суммируется с −50 %;
   - уровень Kuld меняет вид карты; 8/8 наклеек дают +2,00 € один раз; сюрприз дня даёт скидку на один товар только сегодня;
   - на каждом экране есть «Tagasi»;
   - переключение языка ET/RU/EN переводит все новые экраны; светлая/тёмная тема перекрашивает их.
4. Коммиты небольшие и по смыслу (навигация, акции, диета, питательность, семья, награды, демо, pant, панель продаж, партии, вечерняя скидка, переводы), сообщения на английском или эстонском. Не трогай `Form1.Designer.cs`. Push в текущую ветку.
