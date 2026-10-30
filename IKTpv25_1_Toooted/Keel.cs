using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IKTpv25_1_Toooted;

public enum KeeleKood { ET, RU, EN }

// Programmi keel: eesti (vaikimisi), vene, inglise.
// Kood on kirjutatud eesti keeles; Keel.T("eesti tekst") annab valitud keele teksti.
// Vormide sildid, nupud, menüüd ja tabelite päised tõlgitakse automaatselt (Teema.Rakenda kaudu).
public static partial class Keel
{
    private static KeeleKood praegu = KeeleKood.ET;
    [ThreadStatic] private static KeeleKood? ajutine;   // ainult selles lõimes (nt tšekk / e-kiri teises keeles)

    public static KeeleKood Praegu { get => ajutine ?? praegu; private set => praegu = value; }

    // Ajutiselt teine keel (tšekk ja e-kiri kliendi keeles, programmi keel ei muutu):
    //   using (Keel.Ajutiselt(KeeleKood.ET)) { ... }
    public static IDisposable Ajutiselt(KeeleKood k)
    {
        var vana = ajutine;
        ajutine = k;
        return new Taastaja(() => ajutine = vana);
    }

    private sealed class Taastaja : IDisposable
    {
        private Action? taasta;
        public Taastaja(Action taasta) => this.taasta = taasta;
        public void Dispose() { taasta?.Invoke(); taasta = null; }
    }

    public static string Nimi(KeeleKood k) => k switch { KeeleKood.RU => "Русский", KeeleKood.EN => "English", _ => "Eesti" };

    // ---- Tõlkimine ----
    public static string T(string et)
    {
        if (Praegu == KeeleKood.ET || string.IsNullOrEmpty(et)) return et;
        if (sonastik.TryGetValue(et, out var t)) return Vali(t, et);

        // SUURTÄHTEDES silt (nt kaardi pealkiri "OSTUKORV")
        if (et.Length > 1 && et == et.ToUpper() && sonastik.TryGetValue(et.ToLower(), out var t2))
            return Vali(t2, et).ToUpper();

        // Lõpus "..." või ":" (menüüd, sildid)
        foreach (string lopp in new[] { "...", "…", ":" })
            if (et.EndsWith(lopp) && sonastik.TryGetValue(et[..^lopp.Length], out var t3))
                return Vali(t3, et) + lopp;

        return et;
    }

    // Mall + väärtused: Keel.F("Laos {0} tk", 5)
    public static string F(string etMall, params object?[] vaartused) => string.Format(T(etMall), vaartused);

    private static string Vali((string ru, string en) t, string et) =>
        Praegu == KeeleKood.RU ? (t.ru.Length > 0 ? t.ru : et) : (t.en.Length > 0 ? t.en : et);

    // Pikem tekst (teated): terve tekst, muidu rida-realt, muidu tuntud algus ("Viga lisamisel:" + viga)
    public static string TolgiTekst(string tekst)
    {
        if (Praegu == KeeleKood.ET || string.IsNullOrEmpty(tekst)) return tekst;
        string kogu = T(tekst);
        if (!ReferenceEquals(kogu, tekst) && kogu != tekst) return kogu;

        var read = tekst.Split('\n');
        for (int i = 0; i < read.Length; i++)
        {
            string r = read[i];
            string tr = T(r.Trim());
            if (tr != r.Trim()) { read[i] = r.Replace(r.Trim(), tr); continue; }

            // tuntud algus: "Viga: ...", "Salvestamine ebaõnnestus: ..."
            foreach (var voti in algused)
                if (r.StartsWith(voti, StringComparison.Ordinal))
                {
                    read[i] = T(voti) + r[voti.Length..];
                    break;
                }
        }
        return string.Join("\n", read);
    }

    // ---- Vormide elementide tõlkimine (algne eestikeelne tekst jäetakse meelde) ----
    private class Algne { public string? Tekst, Viimane, Vihje, ViimaneVihje; }
    private static readonly ConditionalWeakTable<object, Algne> algsed = new();

    public static void TolgiElement(Control c)
    {
        if (c is TextBoxBase && c is not TextBox) return;
        var a = algsed.GetValue(c, k => new Algne());

        if (c is not TextBox && c is not ComboBox && c is not NumericUpDown)
        {
            // Kood muutis teksti vahepeal ise → see on uus algne tekst
            if (c.Text != a.Viimane) a.Tekst = c.Text;
            string alg = a.Tekst ?? "";
            string uus = alg.Contains('\n') ? TolgiTekst(alg) : T(alg);
            if (c.Text != uus) c.Text = uus;
            a.Viimane = uus;
        }
        if (c is TextBox t)
        {
            if (t.PlaceholderText != a.ViimaneVihje) a.Vihje = t.PlaceholderText;
            string uus = T(a.Vihje ?? "");
            if (t.PlaceholderText != uus) t.PlaceholderText = uus;
            a.ViimaneVihje = uus;
        }
    }

    public static void TolgiMenuu(ToolStripItem i)
    {
        if (i.Tag is KeeleKood) return;   // keelte nimed jäävad oma keelde
        var a = algsed.GetValue(i, k => new Algne());
        if (i.Text != a.Viimane) a.Tekst = i.Text;
        a.Viimane = T(a.Tekst ?? "");
        if (i.Text != a.Viimane) i.Text = a.Viimane;
        if (i is ToolStripMenuItem m)
            foreach (ToolStripItem alam in m.DropDownItems) TolgiMenuu(alam);
    }

    public static void TolgiTabel(DataGridView dgv)
    {
        foreach (DataGridViewColumn col in dgv.Columns)
        {
            var a = algsed.GetValue(col, k => new Algne());
            if (col.HeaderText != a.Viimane) a.Tekst = col.HeaderText;
            a.Viimane = T(a.Tekst ?? "");
            if (col.HeaderText != a.Viimane) col.HeaderText = a.Viimane;
        }
    }

    // ---- Keele vahetus ----
    public static event Action? Muutus;

    public static void Sea(KeeleKood k)
    {
        if (Praegu == k) return;
        Praegu = k;
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(Fail, JsonSerializer.Serialize(k.ToString()));
        }
        catch { }
        Muutus?.Invoke();
        Teema.TeavitaMuutusest();   // kõik avatud vormid värvivad ja tõlgivad end uuesti
    }

    private static string Fail => Path.Combine(Kaustad.Juur, "keel.json");

    public static void Lae()
    {
        try
        {
            if (File.Exists(Fail) && Enum.TryParse(JsonSerializer.Deserialize<string>(File.ReadAllText(Fail)), out KeeleKood k))
                Praegu = k;
        }
        catch { }
    }

    // ---- Sõnastik: eesti → (vene, inglise) ----
    private static readonly Dictionary<string, (string ru, string en)> sonastik = new(StringComparer.Ordinal);
    private static readonly List<string> algused = new();

    private static void L(string et, string ru, string en)
    {
        sonastik[et] = (ru, en);
        string vaike = et.ToLower();
        if (!sonastik.ContainsKey(vaike)) sonastik[vaike] = (ru.ToLower(), en.ToLower());
    }

    // Teadete algused, mille järel tuleb muutuv tekst (nt veateade)
    private static void A(string et, string ru, string en)
    {
        L(et, ru, en);
        algused.Add(et);
    }

    static Keel()
    {
        // --- Üldine ---
        L("Jah", "Да", "Yes"); L("Ei", "Нет", "No"); L("OK", "OK", "OK"); L("Tühista", "Отмена", "Cancel");
        L("Teade", "Сообщение", "Message"); L("Viga", "Ошибка", "Error"); L("Hoiatus", "Внимание", "Warning");
        L("Kinnitus", "Подтверждение", "Confirmation"); L("Sulge", "Закрыть", "Close"); L("Salvesta", "Сохранить", "Save");
        L("Lisa", "Добавить", "Add"); L("Uuenda", "Обновить", "Update"); L("Kustuta", "Удалить", "Delete");
        L("Värskenda", "Обновить", "Refresh"); L("Eksport", "Экспорт", "Export"); L("Tühjenda väljad", "Очистить поля", "Clear fields");
        L("Puhasta", "Очистить", "Clear"); L("Tundmatu viga", "Неизвестная ошибка", "Unknown error");
        L("Programmi viga", "Ошибка программы", "Program error"); L("täna", "сегодня", "today");
        L("Eesti", "Эстония", "Estonia");
        L("Gmail ei luba saata tavalise parooliga. Vaja on RAKENDUSE parooli:", "Gmail не позволяет отправлять с обычным паролем. Нужен ПАРОЛЬ ПРИЛОЖЕНИЯ:", "Gmail does not allow sending with a normal password. You need an APP password:");
        L("1) Google'i kontol sisse lülitada kaheastmeline kinnitamine;", "1) включите двухэтапную проверку в аккаунте Google;", "1) turn on 2-step verification in your Google account;");
        L("2) myaccount.google.com → Turvalisus → Rakenduse paroolid;", "2) myaccount.google.com → Безопасность → Пароли приложений;", "2) myaccount.google.com → Security → App passwords;");
        L("3) loo uus ja kopeeri 16-märgiline parool siia.", "3) создайте новый и скопируйте сюда 16-значный пароль.", "3) create a new one and copy the 16-character password here.");
        L("Parool hoitakse krüpteeritult failis AppData\\Local\\ToodedApp\\email.json,", "Пароль хранится в зашифрованном виде в файле AppData\\Local\\ToodedApp\\email.json,", "The password is stored encrypted in AppData\\Local\\ToodedApp\\email.json,");
        L("mitte koodis – GitHubi see ei satu.", "а не в коде – на GitHub он не попадёт.", "not in the code – it won't end up on GitHub.");
        L("PDF-tšekk on manuses.", "PDF-чек во вложении.", "The PDF receipt is attached.");
        L("Täname ostu eest ({0}).", "Спасибо за покупку ({0}).", "Thank you for your purchase ({0}).");
        L("Tere{0}!", "Здравствуйте{0}!", "Hello{0}!"); L("Kokku: {0}", "Итого: {0}", "Total: {0}");
        L("Boonuse soodustus: -{0}", "Скидка бонусом: -{0}", "Bonus discount: -{0}"); L("Boonust lisandus: {0}", "Начислено бонусов: {0}", "Bonus added: {0}");
        L("Hind (€)", "Цена (€)", "Price (€)"); L("Summa (€)", "Сумма (€)", "Amount (€)"); L("Laos kokku (€)", "Всего на складе (€)", "Stock value (€)");
        L("Ost nr", "Покупка №", "Purchase no."); L("jah", "да", "yes"); L("ei", "нет", "no");
        L("Id", "Id", "Id");
        L("Русский", "Русский", "Русский"); L("English", "English", "English");

        // --- Peavorm (Designer) ---
        L("Minu Epood", "Мой интернет-магазин", "My e-shop"); L("Tooted", "Товары", "Products"); L("Kassa", "Касса", "Checkout");
        L("Toode", "Товар", "Product"); L("Kategooria", "Категория", "Category"); L("Pilt", "Фото", "Picture");
        L("Ost", "Покупка", "Purchase"); L("Arve", "Счёт", "Receipt"); L("Pood", "Магазин", "Shop"); L("Kokku", "Итого", "Total");
        L("Lisa kategooria", "Добавить категорию", "Add category"); L("Kustuta kategooria", "Удалить категорию", "Delete category");
        L("Otsi fail", "Выбрать файл", "Choose file"); L("Valin", "Выбрать", "Select"); L("Ostan", "Купить", "Buy");
        L("Maksta", "Оплатить", "Pay"); L("Saada arve", "Отправить счёт", "Send receipt");
        L("Toodenimetus", "Название товара", "Product name"); L("Toote andmed", "Данные товара", "Product details");
        L("Kogus", "Количество", "Quantity"); L("Hind", "Цена", "Price"); L("Tooteinfo", "Информация о товаре", "Product info");
        L("Tooteinfo salvestatud", "Информация о товаре сохранена", "Product info saved");
        L("Kassa ja ladu", "Касса и склад", "Checkout & stock"); L("Ava e-pood  ›", "Открыть магазин  ›", "Open e-shop  ›");
        L("Leitud: {0} / {1}", "Найдено: {0} / {1}", "Found: {0} / {1}");
        L("Kõik kategooriad", "Все категории", "All categories"); L("Otsi toodet nime järgi", "Искать товар по названию", "Search product by name");
        L("Tühjenda otsing", "Очистить поиск", "Clear search");
        L("Sisse logitud: {0}   |   Roll: {1}", "Вход выполнен: {0}   |   Роль: {1}", "Logged in: {0}   |   Role: {1}");
        L("Tere, {0}!   ·   {1}   ·   {2}", "Здравствуйте, {0}!   ·   {1}   ·   {2}", "Hello, {0}!   ·   {1}   ·   {2}");
        L("Omanik", "Владелец", "Owner"); L("Müüja", "Продавец", "Seller");

        // --- Menüüd ---
        L("Kasutaja", "Пользователь", "User"); L("Vaheta kasutajat", "Сменить пользователя", "Switch user");
        L("Muuda parooli", "Сменить пароль", "Change password"); L("Registreeri kasutaja", "Зарегистрировать пользователя", "Register user");
        L("Välju", "Выход", "Exit"); L("Haldus", "Управление", "Admin"); L("Ostude ajalugu", "История покупок", "Purchase history");
        L("Kliendid", "Клиенты", "Customers"); L("Statistika", "Статистика", "Statistics");
        L("Ekspordi tooted (CSV)", "Экспорт товаров (CSV)", "Export products (CSV)"); L("E-posti seaded", "Настройки e-mail", "E-mail settings");
        L("Vaade", "Вид", "View"); L("Hele teema", "Светлая тема", "Light theme"); L("Tume teema", "Тёмная тема", "Dark theme");
        L("Poe värv", "Цвет магазина", "Shop colour"); L("Punane", "Красный", "Red"); L("Roheline", "Зелёный", "Green");
        L("Sinine", "Синий", "Blue"); L("Oranž", "Оранжевый", "Orange"); L("Lilla", "Фиолетовый", "Purple");
        L("E-poe disain (Rimi)", "Дизайн интернет-магазина (Rimi)", "E-shop design (Rimi)"); L("Klassikaline disain", "Классический дизайн", "Classic design");
        L("Keel", "Язык", "Language");
        L("Disain: e-pood (Rimi stiil)", "Дизайн: интернет-магазин (Rimi)", "Design: e-shop (Rimi style)");
        L("Disain: klassikaline", "Дизайн: классический", "Design: classic");

        // --- Toodete haldus ---
        L("Tooteinfo…", "Информация о товаре…", "Product info…");
        L("Sisesta toote nimetus!", "Введите название товара!", "Enter the product name!");
        L("Kogus peab olema täisarv (0 või rohkem)!", "Количество должно быть целым числом (0 или больше)!", "Quantity must be a whole number (0 or more)!");
        L("Hind peab olema arv (0 või rohkem)!", "Цена должна быть числом (0 или больше)!", "Price must be a number (0 or more)!");
        L("Vali kategooria (või lisa uus)!", "Выберите категорию (или добавьте новую)!", "Choose a category (or add a new one)!");
        L("Vali kategooria!", "Выберите категорию!", "Choose a category!"); L("Vali tabelist toode!", "Выберите товар в таблице!", "Select a product in the table!");
        L("Toode „{0}“ lisatud", "Товар «{0}» добавлен", "Product “{0}” added"); L("Toode uuendatud", "Товар обновлён", "Product updated");
        L("Toode kustutatud", "Товар удалён", "Product deleted"); L("Kas kustutan valitud toote?", "Удалить выбранный товар?", "Delete the selected product?");
        L("Toodet ei leitud (võib-olla juba kustutatud).", "Товар не найден (возможно, уже удалён).", "Product not found (maybe already deleted).");
        L("Sisesta uue kategooria nimi:", "Введите название новой категории:", "Enter the new category name:");
        L("Selline kategooria on juba olemas.", "Такая категория уже есть.", "This category already exists.");
        L("Kas kustutan kategooria „{0}“?", "Удалить категорию «{0}»?", "Delete category “{0}”?");
        L("Seda kategooriat ei saa kustutada, sest selles on tooteid.", "Эту категорию нельзя удалить: в ней есть товары.", "This category can't be deleted because it has products.");
        L("Kustuta või liiguta tooted enne teise kategooriasse.", "Сначала удалите товары или перенесите их в другую категорию.", "Delete the products or move them to another category first.");
        A("Viga lisamisel:", "Ошибка при добавлении:", "Error while adding:"); A("Viga uuendamisel:", "Ошибка при обновлении:", "Error while updating:");
        A("Viga kustutamisel:", "Ошибка при удалении:", "Error while deleting:"); A("Viga kategooria lisamisel:", "Ошибка при добавлении категории:", "Error adding category:");
        A("Viga kategooria kustutamisel:", "Ошибка при удалении категории:", "Error deleting category:");
        A("Andmete laadimine ebaõnnestus:", "Не удалось загрузить данные:", "Loading data failed:"); A("Viga:", "Ошибка:", "Error:");

        // --- Tooteinfo vorm / toote leht ---
        L("Kaubamärk", "Бренд", "Brand"); L("Tootja", "Производитель", "Manufacturer"); L("Päritolumaa", "Страна происхождения", "Country of origin");
        L("Päritolu: {0}", "Происхождение: {0}", "Origin: {0}");
        L("Kogus pakendis (nt 0.5 kg, 2 l, 10 tk)", "Количество в упаковке (напр. 0.5 kg, 2 l, 10 tk)", "Amount in package (e.g. 0.5 kg, 2 l, 10 pcs)");
        L("Ühik (ühikuhinna jaoks)", "Единица (для цены за единицу)", "Unit (for unit price)"); L("Ühikuid pakendis (nt 0.5)", "Единиц в упаковке (напр. 0.5)", "Units in package (e.g. 0.5)");
        L("Vana hind € (soodustus, tühi = pole)", "Старая цена € (скидка, пусто = нет)", "Old price € (discount, empty = none)");
        L("Toote kirjeldus", "Описание товара", "Product description"); L("Koostisosad", "Состав", "Ingredients");
        L("Lisainfo", "Дополнительно", "Additional info"); L("Lisainfo (iga info eraldi real)", "Дополнительно (каждое на новой строке)", "Additional info (one per line)");
        L("Toitumisalane teave", "Пищевая ценность", "Nutrition information");
        L("Toitumisalane teave – iga rida „nimetus;väärtus“, alamrida algab „-“-ga (nt „-millest suhkrud;2.6 g“)",
          "Пищевая ценность – строка «название;значение», подстрока начинается с «-» (напр. «-millest suhkrud;2.6 g»)",
          "Nutrition – each line “name;value”, sub-line starts with “-” (e.g. “-millest suhkrud;2.6 g”)");
        L("Ühikuid pakendis peab olema arv (nt 0.5).", "Единиц в упаковке должно быть числом (напр. 0.5).", "Units in package must be a number (e.g. 0.5).");
        L("Vana hind peab olema arv (nt 6.19) või tühi.", "Старая цена должна быть числом (напр. 6.19) или пустой.", "Old price must be a number (e.g. 6.19) or empty.");
        A("Salvestamine ebaõnnestus:", "Не удалось сохранить:", "Saving failed:");
        L("Kogus 100 g/ml kohta", "Количество на 100 г/мл", "Amount per 100 g/ml"); L("Toiteväärtus", "Пищевая ценность", "Nutritional value");
        L("Soovitatud tooted", "Рекомендуемые товары", "Recommended products"); L("Soovitusi pole.", "Рекомендаций нет.", "No recommendations.");
        L("Selle toote kohta pole veel andmeid lisatud.", "Данные об этом товаре ещё не добавлены.", "No details have been added for this product yet.");
        L("Veel tooteid kaubamärgilt", "Ещё товары бренда", "More products from");
        L("✓ Ostukorvis: {0} tk", "✓ В корзине: {0} шт", "✓ In basket: {0} pcs");
        L("energiasisaldus", "энергетическая ценность", "energy"); L("rasvad", "жиры", "fat");
        L("millest küllastunud rasvhapped", "из них насыщенные жирные кислоты", "of which saturates"); L("süsivesikud", "углеводы", "carbohydrate");
        L("millest suhkrud", "из них сахара", "of which sugars"); L("valgud", "белки", "protein"); L("sool", "соль", "salt");

        // --- Kategooriad (andmed) ---
        L("Piimatooted, munad, juust", "Молочные продукты, яйца, сыр", "Dairy, eggs, cheese");
        L("Leivad, saiad, kondiitritooted", "Хлеб, булки, выпечка", "Bread, buns, pastries");
        L("Puuviljad, köögiviljad", "Фрукты, овощи", "Fruit, vegetables");
        L("Liha- ja kalatooted", "Мясо и рыба", "Meat and fish"); L("Joogid", "Напитки", "Drinks");
        L("Maiustused ja snäkid", "Сладости и снеки", "Sweets and snacks"); L("Külmutatud toidukaubad", "Замороженные продукты", "Frozen food");

        // --- Pood (e-pood) ---
        L("Pood – {0}", "Магазин – {0}", "Shop – {0}"); L("{0} – e-pood", "{0} – интернет-магазин", "{0} – e-shop");
        L("Tere, {0}!     Kliendikaardiga iga ost {1}% boonust", "Здравствуйте, {0}!     С картой клиента {1}% бонусов с каждой покупки", "Hello, {0}!     {1}% bonus on every purchase with a loyalty card");
        L("Viimane tšekk", "Последний чек", "Last receipt"); L("Sulge pood", "Закрыть магазин", "Close shop");
        L("Avaleht", "Главная", "Home"); L("Otsi toodet", "Искать товар", "Search products");
        L("Sisse logitud: {0} ({1})", "Вход выполнен: {0} ({1})", "Logged in: {0} ({1})"); L("Maksma", "К оплате", "Checkout");
        L("Kampaaniad", "Акции", "Offers"); L("Tõesti odav!", "Очень дёшево!", "Really cheap!"); L("Minu lemmikud", "Мои избранные", "My favourites");
        L("Sinu ostukorv", "Ваша корзина", "Your basket"); L("Salvestatud ostukorvid", "Сохранённые корзины", "Saved baskets");
        L("Pakkimise tasu", "Плата за упаковку", "Packaging fee"); L("Salvesta ostukorv", "Сохранить корзину", "Save basket");
        L("Tühjenda ostukorv", "Очистить корзину", "Empty basket"); L("Kas tühjendan ostukorvi?", "Очистить корзину?", "Empty the basket?");
        L("Ostukorv", "Корзина", "Basket"); L("Ostukorvi nimi:", "Название корзины:", "Basket name:");
        L("Ostukorv „{0}“ salvestatud", "Корзина «{0}» сохранена", "Basket “{0}” saved");
        L("Salvestatud ostukorve pole", "Сохранённых корзин нет", "No saved baskets"); L("Salvestatud ostukorv kustutatud", "Сохранённая корзина удалена", "Saved basket deleted");
        L("„{0}“: lisati {1} toodet", "«{0}»: добавлено товаров: {1}", "“{0}”: {1} products added"); L(", {0} pole laos piisavalt", ", {0} нет в нужном количестве", ", {0} not enough in stock");
        L("Parimad pakkumised", "Лучшие предложения", "Best offers"); L("Näita kõiki", "Показать все", "Show all");
        L("Poes pole veel tooteid. Lisa neid vahelehel „Tooted“.", "В магазине пока нет товаров. Добавьте их на вкладке «Товары».", "The shop has no products yet. Add them on the “Products” tab.");
        L("Otsingu tulemused: „{0}“", "Результаты поиска: «{0}»", "Search results: “{0}”"); L("{0} toodet", "{0} товаров", "{0} products");
        L("Järjesta: nimi A–Z", "Сортировка: название А–Я", "Sort: name A–Z"); L("Hind: odavam enne", "Цена: сначала дешёвые", "Price: low to high");
        L("Hind: kallim enne", "Цена: сначала дорогие", "Price: high to low"); L("Suurim soodustus", "Наибольшая скидка", "Biggest discount");
        L("Lemmikuid pole veel. Märgi toode tärniga ☆, et see siia lisada.", "Избранных пока нет. Отметьте товар звёздочкой ☆, чтобы добавить его сюда.", "No favourites yet. Mark a product with ☆ to add it here.");
        L("Praegu kampaaniatooteid pole.", "Сейчас акционных товаров нет.", "No offers right now.");
        L("Midagi ei leitud. Proovi teist otsingusõna.", "Ничего не найдено. Попробуйте другое слово.", "Nothing found. Try another search word.");
        L("Selles kategoorias pole veel tooteid.", "В этой категории пока нет товаров.", "No products in this category yet.");
        L("Selles kategoorias pole tooteid.", "В этой категории нет товаров.", "No products in this category.");
        L("„{0}“ on otsas.", "«{0}» закончился.", "“{0}” is out of stock."); L("{0} lisati korvi", "{0} добавлен в корзину", "{0} added to basket");
        L("Lisatud lemmikutesse ★", "Добавлено в избранное ★", "Added to favourites ★"); L("Eemaldatud lemmikutest", "Удалено из избранного", "Removed from favourites");
        L("Sinu ostukorv on tühi", "Ваша корзина пуста", "Your basket is empty"); L("Sinu ostukorv on tühi.", "Ваша корзина пуста.", "Your basket is empty.");
        L("Lisa tooteid nupuga „Lisa ostukorvi“", "Добавляйте товары кнопкой «В корзину»", "Add products with “Add to basket”");
        L("Lisa ostukorvi", "В корзину", "Add to basket"); L("Pole laos", "Нет в наличии", "Out of stock"); L("Otsas", "Нет", "Sold out");
        L("Vähe: {0} tk", "Мало: {0} шт", "Low: {0} pcs"); L("Vähe: {0}", "Мало: {0}", "Low: {0}"); L("{0} tk", "{0} шт", "{0} pcs");
        L("Laos {0} tk", "На складе {0} шт", "{0} pcs in stock"); L("Korvis: {0}", "В корзине: {0}", "In basket: {0}");
        L("Korv on tühi", "Корзина пуста", "Basket is empty"); L("{0} toodet, {1} rida", "{0} товаров, {1} строк", "{0} items, {1} lines");
        L("Kõik", "Все", "All"); L("Kõik tooted", "Все товары", "All products"); L("Tühjenda korv", "Очистить корзину", "Empty basket");
        L("Viimane tšekk e-postile", "Последний чек на e-mail", "Last receipt by e-mail"); L("Vormista ost  →", "Оформить покупку  →", "Check out  →");
        L("Otsi toodet...  (Ctrl+F)", "Искать товар...  (Ctrl+F)", "Search products...  (Ctrl+F)");
        L("„{0}“ – midagi ei leitud.", "«{0}» – ничего не найдено.", "“{0}” – nothing found.");

        // --- Ostukorv (Kassa vahelehel) ---
        L("Toodet „{0}“ pole rohkem laos (laos: {1}, korvis: {2}).", "Товара «{0}» больше нет на складе (на складе: {1}, в корзине: {2}).", "No more “{0}” in stock (in stock: {1}, in basket: {2}).");
        L("Ostukorv on tühi. Lisa tooteid nupuga \"Valin\" või \"Pood\".", "Корзина пуста. Добавьте товары кнопкой «Выбрать» или «Магазин».", "The basket is empty. Add products with “Select” or “Shop”.");

        // --- Maksmine ---
        L("Vormista ost", "Оформление покупки", "Checkout"); L("Klient ja tšekk", "Клиент и чек", "Customer and receipt");
        L("Kliendikaardi number (valikuline)", "Номер карты клиента (необязательно)", "Loyalty card number (optional)");
        L("E-post, kuhu PDF-tšekk saata (tühi = ei saadeta)", "E-mail для PDF-чека (пусто = не отправлять)", "E-mail for the PDF receipt (empty = don't send)");
        L("Makse", "Оплата", "Payment"); L("Sularaha", "Наличные", "Cash"); L("Pangakaart", "Банковская карта", "Bank card");
        L("Saadud summa (€)", "Получено (€)", "Amount received (€)"); L("Täpne", "Без сдачи", "Exact"); L("Maksa  ✓", "Оплатить  ✓", "Pay  ✓");
        L("Maksta", "Оплатить", "Pay"); L("Kaarti ei leitud", "Карта не найдена", "Card not found");
        L("✓ {0}   ·   boonus {1}", "✓ {0}   ·   бонус {1}", "✓ {0}   ·   bonus {1}");
        L("Kasuta boonust allahindluseks (−{0})", "Использовать бонус как скидку (−{0})", "Use bonus as discount (−{0})");
        L("Vahesumma: {0}", "Промежуточная сумма: {0}", "Subtotal: {0}"); L("boonus −{0}", "бонус −{0}", "bonus −{0}");
        L("Makse pangakaardiga", "Оплата банковской картой", "Paid by bank card"); L("Sisesta saadud summa", "Введите полученную сумму", "Enter the amount received");
        L("Puudu: {0}", "Не хватает: {0}", "Missing: {0}"); L("Tagasiraha: {0}", "Сдача: {0}", "Change: {0}");
        L("Ostukorv on tühi.", "Корзина пуста.", "The basket is empty."); L("Kliendikaart", "Карта клиента", "Loyalty card");
        L("Sellise numbriga kliendikaarti ei leitud.", "Карта клиента с таким номером не найдена.", "No loyalty card with this number.");
        L("Tühjenda väli, kui klient kaarti ei kasuta.", "Очистите поле, если клиент без карты.", "Clear the field if the customer has no card.");
        L("E-posti aadress pole korrektne.", "Неверный адрес e-mail.", "The e-mail address is not valid."); L("E-post", "E-mail", "E-mail");
        L("Saadud summa peab olema vähemalt {0}.", "Полученная сумма должна быть не меньше {0}.", "The amount received must be at least {0}.");
        L("Laoseis", "Остаток", "Stock"); L("Vähenda kogust korvis.", "Уменьшите количество в корзине.", "Reduce the quantity in the basket.");
        L("Toodet „{0}“ pole piisavalt laos (laos: {1}).", "Товара «{0}» недостаточно на складе (на складе: {1}).", "Not enough “{0}” in stock (in stock: {1}).");
        A("Ost on salvestatud, kuid PDF-tšeki loomine ebaõnnestus:", "Покупка сохранена, но создать PDF-чек не удалось:", "The purchase was saved, but creating the PDF receipt failed:");
        A("Viga ostu salvestamisel:", "Ошибка при сохранении покупки:", "Error saving the purchase:");
        L("Ost nr {0} makstud: {1}", "Покупка № {0} оплачена: {1}", "Purchase no. {0} paid: {1}");
        L("Boonust lisandus {0}", "Начислено бонусов {0}", "Bonus added {0}"); L("Väike laoseis:", "Малый остаток:", "Low stock:");
        L("Saadan tšeki aadressile {0}...", "Отправляю чек на {0}...", "Sending receipt to {0}...");
        A("Tšeki saatmine e-postile ebaõnnestus:", "Не удалось отправить чек на e-mail:", "Sending the receipt by e-mail failed:");
        L("Proovi uuesti nupuga \"Saada arve\" või kontrolli Haldus → E-posti seaded.", "Попробуйте снова кнопкой «Отправить счёт» или проверьте Управление → Настройки e-mail.", "Try again with “Send receipt” or check Admin → E-mail settings.");
        L("Selles seansis pole veel ostu tehtud.", "В этом сеансе ещё не было покупок.", "No purchase has been made in this session yet.");
        L("Vanemaid tšekke leiad menüüst Haldus → Ostude ajalugu.", "Старые чеки — в меню Управление → История покупок.", "Older receipts are in Admin → Purchase history.");
        A("PDF-i loomine ebaõnnestus:", "Не удалось создать PDF:", "Creating the PDF failed:");
        L("Ost nr {0} ({1}).", "Покупка № {0} ({1}).", "Purchase no. {0} ({1}).");
        L("Kliendi e-post (tühi = ava PDF):", "E-mail клиента (пусто = открыть PDF):", "Customer e-mail (empty = open PDF):");
        L("Tšekk saadetud aadressile {0}", "Чек отправлен на {0}", "Receipt sent to {0}");
        L("E-posti seaded puuduvad, seepärast avati e-posti programm.", "Настройки e-mail не заданы, поэтому открыта почтовая программа.", "E-mail settings are missing, so the mail program was opened.");
        L("Lisa PDF manusena (fail on kaustas valitud):", "Приложите PDF к письму (файл выделен в папке):", "Attach the PDF (the file is selected in the folder):");
        L("Automaatseks saatmiseks täida menüüs Haldus → E-posti seaded.", "Для автоматической отправки заполните Управление → Настройки e-mail.", "For automatic sending fill in Admin → E-mail settings.");

        // --- Sisselogimine / registreerimine ---
        L("Sisselogimine – {0}", "Вход – {0}", "Sign in – {0}"); L("Tere tulemast!", "Добро пожаловать!", "Welcome!");
        L("Logi sisse, et kassat kasutada", "Войдите, чтобы пользоваться кассой", "Sign in to use the checkout");
        L("Kasutajanimi", "Имя пользователя", "Username"); L("Parool", "Пароль", "Password"); L("Näita parooli", "Показать пароль", "Show password");
        L("Logi sisse", "Войти", "Sign in"); L("Pole kontot? Registreeri müüjaks", "Нет аккаунта? Зарегистрируйтесь продавцом", "No account? Register as a seller");
        L("Vaikimisi: admin / admin123  ·  myyja / myyja123", "По умолчанию: admin / admin123  ·  myyja / myyja123", "Default: admin / admin123  ·  myyja / myyja123");
        L("☀ Hele teema", "☀ Светлая тема", "☀ Light theme"); L("☾ Tume teema", "☾ Тёмная тема", "☾ Dark theme");
        L("Sisesta kasutajanimi.", "Введите имя пользователя.", "Enter the username."); L("Vale kasutajanimi või parool.", "Неверное имя пользователя или пароль.", "Wrong username or password.");
        L("Kassa ja e-pood", "Касса и интернет-магазин", "Checkout and e-shop");
        L("Tooted  ·  Kassa  ·  Kliendid  ·  Statistika", "Товары  ·  Касса  ·  Клиенты  ·  Статистика", "Products  ·  Checkout  ·  Customers  ·  Statistics");
        L("Registreerimine", "Регистрация", "Registration"); L("Uus kasutaja", "Новый пользователь", "New user");
        L("Parool (vähemalt 4 märki)", "Пароль (минимум 4 символа)", "Password (at least 4 characters)"); L("Korda parooli", "Повторите пароль", "Repeat password");
        L("Roll", "Роль", "Role"); L("Registreeri", "Зарегистрировать", "Register");
        L("Kasutajanimi peab olema vähemalt 3 märki.", "Имя пользователя — минимум 3 символа.", "Username must be at least 3 characters.");
        L("Parool peab olema vähemalt 4 märki.", "Пароль — минимум 4 символа.", "Password must be at least 4 characters.");
        L("Paroolid ei kattu.", "Пароли не совпадают.", "Passwords don't match."); L("Selline kasutajanimi on juba olemas.", "Такое имя пользователя уже есть.", "This username already exists.");
        L("Kasutaja „{0}“ ({1}) on loodud", "Пользователь «{0}» ({1}) создан", "User “{0}” ({1}) created");
        L("Praegune parool", "Текущий пароль", "Current password"); L("Uus parool (vähemalt 4 märki)", "Новый пароль (минимум 4 символа)", "New password (at least 4 characters)");
        L("Korda uut parooli", "Повторите новый пароль", "Repeat new password"); L("Parool muudetud", "Пароль изменён", "Password changed");
        L("Uus parool peab olema vähemalt 4 märki.", "Новый пароль — минимум 4 символа.", "New password must be at least 4 characters.");
        L("Uued paroolid ei kattu.", "Новые пароли не совпадают.", "New passwords don't match."); L("Praegune parool on vale.", "Текущий пароль неверный.", "The current password is wrong.");
        L("Muuda parooli – {0}", "Сменить пароль – {0}", "Change password – {0}");

        // --- Kliendid ---
        L("Kliendid (administraatori paneel)", "Клиенты (панель администратора)", "Customers (admin panel)");
        L("Otsi nime, kaardi või e-posti järgi", "Искать по имени, карте или e-mail", "Search by name, card or e-mail");
        L("Kõik kliendid", "Все клиенты", "All customers"); L("Kliendi andmed", "Данные клиента", "Customer details"); L("Nimi", "Имя", "Name");
        L("E-post (PDF-tšekk saadetakse siia)", "E-mail (сюда отправляется PDF-чек)", "E-mail (PDF receipt is sent here)"); L("Boonus (€)", "Бонус (€)", "Bonus (€)");
        L("Uus kaardinumber", "Новый номер карты", "New card number"); L("Boonus", "Бонус", "Bonus"); L("Oste", "Покупок", "Purchases");
        L("Kaardinumber", "Номер карты", "Card number"); L("Email", "E-mail", "E-mail");
        L("Iga ostuga lisandub kliendile boonust {0} % ostusummast. Boonust saab kassas kasutada allahindluseks.", "С каждой покупкой клиент получает {0} % суммы бонусами. Бонусы можно использовать на кассе как скидку.", "Each purchase gives the customer {0} % of the amount as bonus. The bonus can be used as a discount at checkout.");
        L("Vali tabelist klient!", "Выберите клиента в таблице!", "Select a customer in the table!"); L("Sisesta kliendi nimi!", "Введите имя клиента!", "Enter the customer's name!");
        L("Sisesta kaardinumber (või vajuta \"Uus kaardinumber\")!", "Введите номер карты (или нажмите «Новый номер карты»)!", "Enter a card number (or press “New card number”)!");
        L("Boonus peab olema arv (0 või rohkem)!", "Бонус должен быть числом (0 или больше)!", "Bonus must be a number (0 or more)!");
        L("E-posti aadress pole korrektne!", "Неверный адрес e-mail!", "The e-mail address is not valid!");
        L("Selle kaardinumbriga klient on juba olemas.", "Клиент с таким номером карты уже есть.", "A customer with this card number already exists.");
        L("Selle kaardinumbriga on juba teine klient.", "Этот номер карты уже у другого клиента.", "Another customer already has this card number.");
        L("Klient „{0}“ lisatud", "Клиент «{0}» добавлен", "Customer “{0}” added"); L("Kliendi andmed uuendatud", "Данные клиента обновлены", "Customer updated");
        L("Klient kustutatud", "Клиент удалён", "Customer deleted");
        L("Kas kustutan valitud kliendi? (tema ostud jäävad alles)", "Удалить выбранного клиента? (его покупки сохранятся)", "Delete the selected customer? (their purchases stay)");

        // --- Statistika ---
        L("Müügistatistika", "Статистика продаж", "Sales statistics"); L("Müük täna", "Продажи сегодня", "Sales today"); L("Müük kokku", "Продажи всего", "Total sales");
        L("Oste kokku", "Всего покупок", "Total purchases"); L("Kliente", "Клиентов", "Customers");
        L("Müük viimase 14 päeva jooksul (€)", "Продажи за последние 14 дней (€)", "Sales in the last 14 days (€)");
        L("Väike laoseis (alla {0} tk) – telli juurde", "Малый остаток (меньше {0} шт) – закажите", "Low stock (under {0} pcs) – reorder");
        L("Enim müüdud tooted", "Самые продаваемые товары", "Best-selling products"); L("Ekspordi müük (CSV)", "Экспорт продаж (CSV)", "Export sales (CSV)");
        L("Müüdud", "Продано", "Sold"); L("Summa", "Сумма", "Amount");

        // --- Ostude ajalugu ---
        L("Näita ka tagastatud", "Показать и возвращённые", "Show returned too"); L("Ostu read", "Строки покупки", "Purchase lines");
        L("Tšekk (PDF)", "Чек (PDF)", "Receipt (PDF)"); L("Tagasta ost", "Вернуть покупку", "Return purchase");
        L("Nr", "№", "No."); L("Aeg", "Время", "Time"); L("Klient", "Клиент", "Customer"); L("Soodustus", "Скидка", "Discount"); L("Staatus", "Статус", "Status");
        L("Makstud", "Оплачено", "Paid"); L("Maksmata", "Не оплачено", "Unpaid"); L("Tagastatud", "Возвращено", "Returned");
        L("Vali ost.", "Выберите покупку.", "Select a purchase."); L("See ost on juba tagastatud.", "Эта покупка уже возвращена.", "This purchase has already been returned.");
        L("Tagastus", "Возврат", "Return"); L("Kas tagastan ostu nr {0} ({1})?", "Вернуть покупку № {0} ({1})?", "Return purchase no. {0} ({1})?");
        L("Kaup läheb tagasi lattu.", "Товар вернётся на склад.", "The goods go back to stock.");
        L("Ost nr {0} tagastatud. Kaup on tagasi laos.", "Покупка № {0} возвращена. Товар снова на складе.", "Purchase no. {0} returned. The goods are back in stock.");
        L("Tagasta kliendile {0}.", "Верните клиенту {0}.", "Refund the customer {0}.");
        A("Tšeki loomine ebaõnnestus:", "Не удалось создать чек:", "Creating the receipt failed:"); A("Tagastamine ebaõnnestus:", "Не удалось оформить возврат:", "Return failed:");
        L("Fail salvestatud:", "Файл сохранён:", "File saved:"); L("Kas avan?", "Открыть?", "Open it?");

        // --- E-posti seaded ---
        L("Saatja Gmail", "Gmail отправителя", "Sender Gmail"); L("Rakenduse parool (16 märki)", "Пароль приложения (16 символов)", "App password (16 characters)");
        L("✓ Seaded on salvestatud.", "✓ Настройки сохранены.", "✓ Settings saved."); L("Seaded puuduvad – tšekke ei saadeta automaatselt.", "Настроек нет – чеки не отправляются автоматически.", "No settings – receipts are not sent automatically.");
        L("Testkiri (kontrolli, kas saatmine töötab)", "Тестовое письмо (проверка отправки)", "Test e-mail (check that sending works)"); L("Saada testkiri", "Отправить тест", "Send test e-mail");
        L("Sisesta korrektne Gmaili aadress.", "Введите правильный адрес Gmail.", "Enter a valid Gmail address."); L("Sisesta rakenduse parool.", "Введите пароль приложения.", "Enter the app password.");
        L("Sisesta korrektne testkirja aadress.", "Введите правильный адрес для теста.", "Enter a valid test address.");
        L("Testkiri saadetud aadressile {0}", "Тестовое письмо отправлено на {0}", "Test e-mail sent to {0}"); A("Saatmine ebaõnnestus:", "Не удалось отправить:", "Sending failed:");

        // --- Käivitamine ---
        A("Andmebaasiga ühendamine ebaõnnestus:", "Не удалось подключиться к базе данных:", "Connecting to the database failed:");
        L("Andmebaas puudub", "Нет базы данных", "No database"); L("Migratsioone pole veel loodud.", "Миграции ещё не созданы.", "No migrations have been created yet.");

        // --- Ostjad, seaded, ostunimekiri, kasutajad ---
        L("Ostja", "Покупатель", "Customer"); L("E-post (valikuline)", "E-mail (необязательно)", "E-mail (optional)");
        L("Laos", "На складе", "In stock"); L("Lisatud", "Добавлено", "Added"); L("Kliendi nimi:", "Имя клиента:", "Customer name:");
        L("Kasutajad", "Пользователи", "Users"); L("Kõik kasutajad", "Все пользователи", "All users"); L("Valitud kasutaja", "Выбранный пользователь", "Selected user");
        L("Pole kontot? Registreeru (kliendikaart kohe)", "Нет аккаунта? Зарегистрируйтесь (карта клиента сразу)", "No account? Sign up (customer card right away)");
        L("Ees- ja perekonnanimi", "Имя и фамилия", "First and last name"); L("E-post (sinna tuleb PDF-tšekk)", "E-mail (сюда придёт PDF-чек)", "E-mail (the PDF receipt is sent here)");
        L("Loo kliendikaart", "Создать карту клиента", "Create customer card");
        L("Ostja saab kliendikaardi kohe. Igast ostust lisandub boonust.", "Покупатель сразу получает карту клиента. С каждой покупки начисляются бонусы.", "A customer gets a card right away. Every purchase adds bonus.");
        L("Sisesta e-post – sinna saadetakse tšekid.", "Введите e-mail – туда отправляются чеки.", "Enter an e-mail – receipts are sent there.");
        L("Kasutaja „{0}“ on loodud.", "Пользователь «{0}» создан.", "User “{0}” has been created."); L("Sinu kliendikaardi number: {0}", "Номер вашей карты клиента: {0}", "Your customer card number: {0}");
        L("Maksmisel täidetakse kaart ja e-post ise.", "При оплате карта и e-mail подставляются сами.", "Card and e-mail are filled in automatically at checkout.");
        L("Kliendikaart: {0}", "Карта клиента: {0}", "Customer card: {0}"); L("tühi = antakse ise", "пусто = выдаётся автоматически", "empty = assigned automatically");
        L("nimi@gmail.com", "имя@gmail.com", "name@gmail.com"); L("klient@email.ee", "клиент@email.ee", "customer@email.ee"); L("nt 100001", "напр. 100001", "e.g. 100001");
        L("+ Uus klient (kaart kohe)", "+ Новый клиент (карта сразу)", "+ New customer (card right away)"); L("Uus klient", "Новый клиент", "New customer");
        L("Kliendi e-post (tšeki jaoks, võib jätta tühjaks):", "E-mail клиента (для чека, можно оставить пустым):", "Customer e-mail (for the receipt, may be empty):");
        L("Ostukorv on tühi. Lisa tooteid nupuga \"Valin\" või \"Pood\".", "Корзина пуста. Добавьте товары кнопкой «Valin» или «Pood».", "The basket is empty. Add products with “Valin” or “Pood”.");
        L("Ostunimekirjas märgiti ostetuks: {0}", "В списке покупок отмечено купленным: {0}", "Marked as bought in the shopping list: {0}");
        L("Seaded", "Настройки", "Settings"); L("⚙  Seaded", "⚙  Настройки", "⚙  Settings"); L("Seaded – {0}", "Настройки – {0}", "Settings – {0}");
        L("Seaded salvestatud", "Настройки сохранены", "Settings saved"); L("Klõps: seaded", "Клик: настройки", "Click: settings");
        L("PROFIIL", "ПРОФИЛЬ", "PROFILE"); L("TURVALISUS", "БЕЗОПАСНОСТЬ", "SECURITY"); L("VÄLIMUS", "ВНЕШНИЙ ВИД", "APPEARANCE");
        L("Teema", "Тема", "Theme"); L("Disain (Ctrl+Shift+D)", "Дизайн (Ctrl+Shift+D)", "Design (Ctrl+Shift+D)");
        L("E-posti seaded (saatja)", "Настройки e-mail (отправитель)", "E-mail settings (sender)"); L("puudub", "нет", "none");
        L("Teema, värv ja keel muutuvad kohe. Disain vahetub pärast akna sulgemist.", "Тема, цвет и язык меняются сразу. Дизайн сменится после закрытия окна.", "Theme, colour and language change at once. The design switches after closing the window.");
        L("{0}   ·   boonus {1}", "{0}   ·   бонус {1}", "{0}   ·   bonus {1}"); L("Kasutajat ei leitud.", "Пользователь не найден.", "User not found.");
        L("Otsi nime, rolli või e-posti järgi", "Искать по имени, роли или e-mail", "Search by name, role or e-mail");
        L("Uus parool", "Новый пароль", "New password"); L("Uus parool kasutajale „{0}“ (vähemalt 4 märki):", "Новый пароль для «{0}» (минимум 4 символа):", "New password for “{0}” (at least 4 characters):");
        L("Vali tabelist kasutaja.", "Выберите пользователя в таблице.", "Select a user in the table."); L("Valitud: {0}", "Выбран: {0}", "Selected: {0}");
        L("Vähemalt üks kasutaja peab olema omanik.", "Хотя бы один пользователь должен быть владельцем.", "At least one user must be an owner.");
        L("Iseennast ei saa kustutada.", "Нельзя удалить самого себя.", "You cannot delete yourself.");
        L("Kas kustutan kasutaja „{0}“? (kliendikaart ja ostud jäävad alles)", "Удалить пользователя «{0}»? (карта клиента и покупки сохранятся)", "Delete user “{0}”? (customer card and purchases stay)");
        L("Kasutaja kustutatud", "Пользователь удалён", "User deleted"); L("Kasutaja „{0}“ salvestatud", "Пользователь «{0}» сохранён", "User “{0}” saved");
        L("Ostunimekiri", "Список покупок", "Shopping list"); L("☑  Ostunimekiri", "☑  Список покупок", "☑  Shopping list");
        L("Mis on ostetud, mis veel mitte", "Что куплено, а что ещё нет", "What is bought and what is not yet");
        L("Mida osta? (nt Piim või toote nimi)", "Что купить? (напр. молоко или название товара)", "What to buy? (e.g. milk or a product name)");
        L("Ostetud", "Куплено", "Bought"); L("Ostmata", "Не куплено", "Not bought");
        L("Nimekiri  (klõps ruudul või tühik = ostetud / ostmata)", "Список  (клик по квадратику или пробел = куплено / не куплено)", "List  (click the box or press Space = bought / not bought)");
        L("Lisa ostmata korvi", "Некупленное в корзину", "Add unbought to basket"); L("Märgi valitud ostetuks", "Отметить купленным", "Mark selected as bought");
        L("Kustuta valitud", "Удалить выбранные", "Delete selected"); L("Kustuta ostetud", "Удалить купленные", "Delete bought");
        L("Nimekiri on tühi – lisa ülal, mida on vaja osta.", "Список пуст – добавьте вверху, что нужно купить.", "The list is empty – add above what you need to buy.");
        L("Ostetud {0} / {1}   ·   ostmata {2}", "Куплено {0} / {1}   ·   осталось {2}", "Bought {0} / {1}   ·   left {2}");
        L("Vali nimekirjast rida.", "Выберите строку в списке.", "Select a row in the list."); L("Ostetud ridu pole", "Купленных строк нет", "No bought rows");
        L("Nimekirjas pole ostmata tooteid, mis on poes olemas.", "В списке нет некупленных товаров, которые есть в магазине.", "The list has no unbought products that the shop has.");
        L("Vali toode nimekirja lisamisel soovitustest.", "При добавлении выбирайте товар из подсказок.", "When adding, pick the product from the suggestions.");
        L("Korvi lisati {0} toodet", "В корзину добавлено товаров: {0}", "{0} products added to the basket");
        L("☐  Lisa ostunimekirja", "☐  В список покупок", "☐  Add to shopping list"); L("„{0}“ lisati ostunimekirja", "«{0}» добавлен в список покупок", "“{0}” added to the shopping list");
        L("e-pood", "интернет-магазин", "online shop");

        // --- Boonus, Wallet-kaart, tarne, külgmenüü, kassatšekk, e-kiri ---
        L("Ava e-pood", "Открыть интернет-магазин", "Open online shop"); L("Ava kaart  ›", "Открыть карту  ›", "Open card  ›");
        L("Kojuveo aadress", "Адрес доставки", "Delivery address"); L("Soovin ostutšekke e-postile", "Хочу получать чеки на e-mail", "Send me receipts by e-mail");
        L("Tänav 1-2, Tallinn, 10111", "Улица 1-2, Таллинн, 10111", "Street 1-2, Tallinn, 10111"); L("Ümarad nurgad ja külgmenüü", "Закруглённые углы и боковое меню", "Rounded corners and side menu");
        L("1 € = 1 sent boonust · kasuta maksmisel", "1 € = 1 цент бонусов · используйте при оплате", "1 € = 1 cent bonus · use it at checkout");
        L("Klõpsa kaardil – vöötkood kassas skannimiseks", "Нажмите на карту – штрихкод для кассы", "Tap the card – barcode for the till");
        L("Näita kassas · kaart {0} · boonus {1}", "Покажите на кассе · карта {0} · бонус {1}", "Show at the till · card {0} · bonus {1}");
        L("Ostusid veel pole – iga 1 € annab 1 sendi boonust.", "Покупок ещё нет – каждый 1 € даёт 1 цент бонусов.", "No purchases yet – every 1 € gives 1 cent bonus.");
        L("Sul pole veel kliendikaarti. Kas loon selle kohe?", "У вас ещё нет карты клиента. Создать сейчас?", "You don't have a customer card yet. Create it now?");
        L("Viimased ostud", "Последние покупки", "Recent purchases"); L("KLIENDIKAART", "КАРТА КЛИЕНТА", "CUSTOMER CARD");
        L("Kojuvedu {0} (tasuta alates {1})", "Доставка {0} (бесплатно от {1})", "Home delivery {0} (free from {1})");
        L("Poest kaasa (tasuta)", "Самовывоз (бесплатно)", "Pick up in store (free)"); L("Sisesta kojuveo aadress.", "Введите адрес доставки.", "Enter the delivery address.");
        L("TASUTA", "БЕСПЛАТНО", "FREE"); L("Tarne: {0}", "Доставка: {0}", "Delivery: {0}"); L("veel {0} → tasuta", "ещё {0} → бесплатно", "{0} more → free");
        L("✓ Tarne tasuta", "✓ Доставка бесплатно", "✓ Free delivery"); L("Kättesaamine", "Получение", "Delivery");
        L("Lisa veel {0} ja kojuvedu on tasuta", "Добавьте ещё {0} – и доставка бесплатна", "Add {0} more for free delivery");
        L("✓ Tasuta kojuvedu (ostukorv üle {0})", "✓ Бесплатная доставка (корзина от {0})", "✓ Free delivery (basket over {0})");
        L("Tere, {0}!     Kliendikaardiga 1 € = 1 sent boonust  ·  tasuta kojuvedu alates {1}", "Привет, {0}!     С картой клиента 1 € = 1 цент бонусов  ·  бесплатная доставка от {1}", "Hi, {0}!     With the customer card 1 € = 1 cent bonus  ·  free delivery from {1}");
        L("Kohaletoimetamine", "Доставка", "Delivery"); L("Kohaletoimetamine: {0}", "Доставка: {0}", "Delivery: {0}"); L("Kojuvedu", "Доставка на дом", "Home delivery");
        L("Poest kaasa", "Самовывоз", "Store pickup");
        L("AKT e-pood ja kassa · õppeprojekt", "AKT интернет-магазин и касса · учебный проект", "AKT online shop & till · school project");
        L("Digitaalne tšekk", "Цифровой чек", "Digital receipt"); L("KLIENT: {0}", "КЛИЕНТ: {0}", "CUSTOMER: {0}");
        L("Tasuta (ostukorv üle {0} EUR)", "Бесплатно (корзина от {0} EUR)", "Free (basket over {0} EUR)");
        L("SINU SOODUSTUSED:", "ВАШИ СКИДКИ:", "YOUR DISCOUNTS:"); L("Boonus kliendikaardilt", "Бонус с карты клиента", "Customer card bonus");
        L("Oled säästnud", "Вы сэкономили", "You saved"); L("KAARDIMAKSE", "ОПЛАТА КАРТОЙ", "CARD PAYMENT"); L("SULARAHA", "НАЛИЧНЫЕ", "CASH");
        L("MÜÜK", "ПРОДАЖА", "SALE"); L("AEG", "ВРЕМЯ", "TIME"); L("SUMMA:", "СУММА:", "AMOUNT:"); L("MAKSE KINNITATUD", "ОПЛАТА ПОДТВЕРЖДЕНА", "PAYMENT APPROVED");
        L("Teenitud boonust (1 € = 1 sent)", "Начислено бонусов (1 € = 1 цент)", "Bonus earned (1 € = 1 cent)"); L("Sinu boonuse saldo", "Ваш бонусный баланс", "Your bonus balance");
        L("KOKKU", "ИТОГО", "TOTAL"); L("KM %", "НСО %", "VAT %"); L("KM-ga", "С НСО", "Incl. VAT"); L("KM-ta", "Без НСО", "Excl. VAT"); L("KM", "НСО", "VAT");
        L("KASSA: {0}", "КАССА: {0}", "TILL: {0}"); L("ARVE NR: {0}", "ЧЕК №: {0}", "RECEIPT NO: {0}"); L("AEG: {0}", "ВРЕМЯ: {0}", "TIME: {0}");
        L("KUUPÄEV: {0}", "ДАТА: {0}", "DATE: {0}"); L("KOJUVEO AADRESS:", "АДРЕС ДОСТАВКИ:", "DELIVERY ADDRESS:"); L("Säilitage tšekk", "Сохраните чек", "Keep your receipt");
        L("{0} x {1}", "{0} x {1}", "{0} x {1}"); L("Tšekk nr {0}", "Чек № {0}", "Receipt no. {0}");
        L("Sinu ostutšekk", "Ваш чек", "Your receipt"); L("Sinu ostutšekk nr {0} – {1}", "Ваш чек № {0} – {1}", "Your receipt no. {0} – {1}");
        L("Lugupeetud klient,", "Уважаемый клиент,", "Dear customer,"); L("Lugupeetud {0},", "Уважаемый(ая) {0},", "Dear {0},");
        L("Soovisid saada {0} ostutšeki e-postiga. Saadame tšeki kirja manusena (PDF).", "Вы выбрали получать чеки {0} по e-mail. Чек приложен к письму (PDF).", "You chose to get {0} receipts by e-mail. The receipt is attached (PDF).");
        L("Ost nr {0} · {1}", "Покупка № {0} · {1}", "Purchase no. {0} · {1}"); L("Ostu nr", "№ покупки", "Purchase no."); L("Kuupäev", "Дата", "Date");
        L("Tooteid", "Товаров", "Items"); L("Boonusega makstud", "Оплачено бонусом", "Paid with bonus"); L("Teenitud boonust", "Начислено бонусов", "Bonus earned");
        L("Kui sa ei soovi enam ostutšekke e-postiga saada, lülita {0} programmi Seadetes välja valik „Soovin ostutšekke e-postile“.", "Если вы больше не хотите получать чеки по e-mail, отключите в настройках программы {0} опцию «Хочу получать чеки на e-mail».", "If you no longer want receipts by e-mail, turn off “Send me receipts by e-mail” in the {0} app settings.");
        L("Küsimuste korral kirjuta meile: {0}", "Если есть вопросы, напишите нам: {0}", "If you have questions, write to us: {0}");
        L("Soovime sulle meeldivaid oste!", "Желаем приятных покупок!", "Happy shopping!"); L("Sinu {0}", "Ваш {0}", "Your {0}");
        L("Saatsime selle teate aadressile {0}. Kui soovid tšekke tulevikus teisele aadressile, muuda e-posti {1} Seadetes. Said selle kirja, sest andsid selleks nõusoleku.", "Мы отправили это письмо на адрес {0}. Если хотите получать чеки на другой адрес, измените e-mail в настройках {1}. Вы получили это письмо, потому что дали на это согласие.", "We sent this message to {0}. To get receipts at another address, change your e-mail in the {1} settings. You received this e-mail because you agreed to it.");

        // --- PDF-tšekk ---
        L("ARVE nr {0}", "ЧЕК № {0}", "RECEIPT No. {0}"); L("Kuupäev: {0}", "Дата: {0}", "Date: {0}"); L("Müüja: {0}", "Продавец: {0}", "Seller: {0}");
        L("Klient: {0} (kaart {1})", "Клиент: {0} (карта {1})", "Customer: {0} (card {1})"); L("Vahesumma:", "Промежуточная сумма:", "Subtotal:");
        L("Boonuse soodustus:", "Скидка бонусом:", "Bonus discount:"); L("KOKKU:", "ИТОГО:", "TOTAL:"); L("Makstud:", "Оплачено:", "Paid:");
        L("Tagasiraha:", "Сдача:", "Change:"); L("Boonust lisandus:", "Начислено бонусов:", "Bonus added:");
        L("Staatus: MAKSTUD", "Статус: ОПЛАЧЕНО", "Status: PAID"); L("Staatus: MAKSMATA", "Статус: НЕ ОПЛАЧЕНО", "Status: UNPAID");
        L("Aitäh ostu eest!", "Спасибо за покупку!", "Thank you for shopping!"); L("Tšekk nr {0} – {1}", "Чек № {0} – {1}", "Receipt no. {0} – {1}");

        LisaUuedTolked();   // uute funktsioonide tõlked (KeelUued.cs)
    }
}
