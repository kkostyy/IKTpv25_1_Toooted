namespace IKTpv25_1_Toooted;

// Uute funktsioonide tõlked (eesti → vene, inglise)
public static partial class Keel
{
    private static void LisaUuedTolked()
    {
        // --- Navigeerimine ---
        L("Tagasi", "Назад", "Back"); L("Avaleht", "Главная", "Home");
        L("Otsing: „{0}“", "Поиск: «{0}»", "Search: “{0}”");
        // --- Kampaaniad ---
        L("Kampaaniad...", "Акции...", "Campaigns..."); L("Käib", "Идёт", "Running"); L("Tulekul", "Скоро", "Upcoming");
        L("Lõppenud", "Закончилась", "Ended"); L("Kõik", "Все", "All"); L("Uus kampaania", "Новая акция", "New campaign");
        L("Muuda kampaaniat", "Изменить акцию", "Edit campaign"); L("Uus", "Новая", "New"); L("Nimi", "Название", "Name");
        L("nt Nädala leivapakkumine", "напр. Хлеб недели", "e.g. Bread of the week"); L("Toode", "Товар", "Product");
        L("Soodustus", "Скидка", "Discount"); L("Algus", "Начало", "Start"); L("Lõpp", "Конец", "End");
        L("POES NÄEB VÄLJA", "В МАГАЗИНЕ", "IN THE SHOP"); L("Automaatne (hind muutub alguses ja lõpus ise)", "Автоматически (цена меняется сама в начале и в конце)", "Automatic (price changes by itself at start and end)");
        L("Salvesta kampaania", "Сохранить акцию", "Save campaign"); L("Lõpeta kohe", "Завершить сейчас", "End now"); L("Haldus", "Управление", "Management");
        L("Kampaania · toode", "Акция · товар", "Campaign · product"); L("Periood", "Период", "Period"); L("Uus / vana hind", "Новая / старая цена", "New / old price");
        L("Olek", "Статус", "Status"); L("Kõik kampaaniad", "Все акции", "All campaigns"); L("Ajatelg · 4 nädalat", "Шкала времени · 4 недели", "Timeline · 4 weeks");
        L("Nädal", "Неделя", "Week"); L("Täna", "Сегодня", "Today"); L("Kampaaniaid pole – lisa paremal uus.", "Акций нет – добавьте новую справа.", "No campaigns – add a new one on the right.");
        L("Vali toode.", "Выберите товар.", "Choose a product."); L("Kampaania", "Акция", "Campaign");
        L("Sisesta kampaania nimi.", "Введите название акции.", "Enter the campaign name."); L("Soodustus peab olema 1–90 %.", "Скидка должна быть 1–90 %.", "Discount must be 1–90 %.");
        L("Lõpp peab olema pärast algust.", "Конец должен быть после начала.", "End must be after start.");
        L("Sellel tootel on samal ajal juba kampaania.", "У этого товара в это время уже есть акция.", "This product already has a campaign at that time.");
        L("Kampaania „{0}“ salvestatud", "Акция «{0}» сохранена", "Campaign “{0}” saved"); L("Kampaania „{0}“ lõpetatud – hind on tagasi", "Акция «{0}» завершена – цена вернулась", "Campaign “{0}” ended – price restored");
        L("Kas kustutan kampaania?", "Удалить акцию?", "Delete the campaign?");
        // --- Dieet ja allergeenid ---
        L("Minu dieet", "Моя диета", "My diet"); L("Laktoosivaba", "Без лактозы", "Lactose-free"); L("Gluteenivaba", "Без глютена", "Gluten-free");
        L("Vegan", "Веган", "Vegan"); L("Pähklivaba", "Без орехов", "Nut-free"); L("Eesti toode", "Эстонский товар", "Estonian product");
        L("Range režiim", "Строгий режим", "Strict mode"); L("Peida sobimatud", "Скрыть неподходящие", "Hide unsuitable");
        L("Sulle sobib {0} / {1} toodet", "Вам подходит {0} / {1} товаров", "{0} / {1} products suit you");
        L("Koostis on teada {0} tootel", "Состав известен у {0} товаров", "Ingredients known for {0} products");
        L("Ei sobi", "Не подходит", "Not suitable"); L("Sisaldab: {0}", "Содержит: {0}", "Contains: {0}");
        L("Võib sisaldada: {0}", "Может содержать: {0}", "May contain: {0}"); L("Allergeenid", "Аллергены", "Allergens"); L("Märgised", "Отметки", "Labels");
        L("piim", "молоко", "milk"); L("nisu", "пшеница", "wheat"); L("rukis", "рожь", "rye"); L("oder", "ячмень", "barley"); L("kaer", "овёс", "oats");
        L("gluteen", "глютен", "gluten"); L("muna", "яйцо", "egg"); L("soja", "соя", "soy"); L("kala", "рыба", "fish"); L("pähklid", "орехи", "nuts");
        L("seller", "сельдерей", "celery"); L("sinep", "горчица", "mustard"); L("seesam", "кунжут", "sesame");
        // --- Toiteväärtus ---
        L("Toiteväärtus", "Пищевая ценность", "Nutrition"); L("Ostukorvi toiteväärtus", "Пищевая ценность корзины", "Basket nutrition");
        L("Kui palju energiat ja toitaineid on ostukorvis", "Сколько энергии и питательных веществ в корзине", "How much energy and nutrients are in the basket");
        L("päeva energiat ühele inimesele", "дней энергии для одного человека", "days of energy for one person"); L("kcal kokku", "ккал всего", "kcal total");
        L("andmed puuduvad", "нет данных", "no data"); L("Toiteväärtus puudub:", "Нет пищевой ценности:", "No nutrition data:");
        L("Ostukorv on tühi – lisa tooteid, et näha toiteväärtust.", "Корзина пуста – добавьте товары, чтобы увидеть пищевую ценность.", "The basket is empty – add products to see nutrition.");
        L("Energia", "Энергия", "Energy"); L("Rasvad", "Жиры", "Fat"); L("millest küllastunud", "из них насыщенные", "of which saturates");
        L("Süsivesikud", "Углеводы", "Carbohydrates"); L("millest suhkrud", "из них сахара", "of which sugars"); L("Valgud", "Белки", "Protein"); L("Sool", "Соль", "Salt");
        L("KUST KALORID TULEVAD", "ОТКУДА КАЛОРИИ", "WHERE CALORIES COME FROM"); L("Andmeid pole", "Нет данных", "No data"); L("OSTUKORV", "КОРЗИНА", "BASKET");
        // --- Pere ostunimekiri ---
        L("Pere ostunimekiri", "Семейный список покупок", "Family shopping list"); L("+ Kutsu / liitu", "+ Пригласить / вступить", "+ Invite / join");
        L("Ostetud {0} / {1} · kõik näevad sama nimekirja", "Куплено {0} / {1} · все видят один список", "Bought {0} / {1} · everyone sees the same list");
        L("Kopeeri kood", "Копировать код", "Copy code"); L("Pere kood kopeeritud", "Код семьи скопирован", "Family code copied"); L("PERE KOOD", "КОД СЕМЬИ", "FAMILY CODE");
        L("VIIMASED TEGEVUSED", "ПОСЛЕДНИЕ ДЕЙСТВИЯ", "RECENT ACTIVITY"); L("Tegevusi pole veel", "Действий пока нет", "No activity yet");
        L("lisas „{0}“", "добавил(а) «{0}»", "added “{0}”"); L("ostis „{0}“", "купил(а) «{0}»", "bought “{0}”"); L("ostis {0}", "купил(а) {0}", "bought by {0}");
        L("Lisas", "Добавил", "Added by"); L("Ostis", "Купил", "Bought by"); L("Liitu perega", "Вступить в семью", "Join a family"); L("Liitu", "Вступить", "Join");
        L("Sisesta pereliikmelt saadud 7-märgiline kood. Kõik pereliikmed näevad sama ostunimekirja (samas programmis).", "Введите 7-значный код от члена семьи. Все члены семьи видят один список покупок (в этой программе).", "Enter the 7-character code from a family member. Everyone in the family sees the same shopping list (in this app).");
        L("loo uus pere", "создать новую семью", "create a new family"); L("lahku perest", "выйти из семьи", "leave the family");
        L("Kas lahkud perest? Sinu ostunimekiri jääb alles.", "Выйти из семьи? Ваш список покупок сохранится.", "Leave the family? Your shopping list stays.");
        L("Pere", "Семья", "Family"); L("Liitusid perega – nimekiri on nüüd ühine", "Вы вступили в семью – список теперь общий", "You joined the family – the list is now shared");
        L("Uus pere loodud. Kood {0} on kopeeritud – saada see pereliikmetele.", "Семья создана. Код {0} скопирован – отправьте его членам семьи.", "New family created. Code {0} is copied – send it to family members.");
        L("Pere kood on {0} märki.", "Код семьи – {0} символов.", "The family code has {0} characters.");
        L("Sellise koodiga peret ei leitud.", "Семья с таким кодом не найдена.", "No family with this code was found.");
        // --- Preemiad ---
        L("Preemiad", "Награды", "Rewards"); L("Pronks", "Бронза", "Bronze"); L("Hõbe", "Серебро", "Silver"); L("Kuld", "Золото", "Gold");
        L("SINU TASE", "ВАШ УРОВЕНЬ", "YOUR LEVEL"); L("{0}-tasemeni veel {1} oste", "До уровня {0} ещё {1} покупок", "{1} more purchases to {0}");
        L("Kõrgeim tase – aitäh!", "Высший уровень – спасибо!", "Top level – thank you!"); L("alates {0}", "от {0}", "from {0}");
        L("Nädala väljakutsed", "Задания недели", "Weekly challenges"); L("Kleepsualbum  {0} / {1}", "Альбом наклеек  {0} / {1}", "Sticker album  {0} / {1}");
        L("Eesti nädal: osta 5 Eesti toodet", "Эстонская неделя: купите 5 эстонских товаров", "Estonian week: buy 5 Estonian products");
        L("Värske valik: 3 puu- või köögivilja", "Свежий выбор: 3 фрукта или овоща", "Fresh choice: 3 fruits or vegetables");
        L("Kolm korda: tee 3 ostu", "Три раза: сделайте 3 покупки", "Three times: make 3 purchases");
        L("Tehtud! ✓ · +{0} saadud", "Готово! ✓ · получено +{0}", "Done! ✓ · +{0} received");
        L("Esimene ost", "Первая покупка", "First purchase"); L("Tee esimene ost", "Сделайте первую покупку", "Make your first purchase");
        L("10 ostu", "10 покупок", "10 purchases"); L("Tee 10 ostu", "Сделайте 10 покупок", "Make 10 purchases");
        L("Eesti patrioot", "Эстонский патриот", "Estonian patriot"); L("Osta 50 Eesti toodet", "Купите 50 эстонских товаров", "Buy 50 Estonian products");
        L("Tervislik", "Здоровый", "Healthy"); L("Osta 20 puu- või köögivilja", "Купите 20 фруктов или овощей", "Buy 20 fruits or vegetables");
        L("Säästja", "Экономный", "Saver"); L("Säästa soodustustega 10 €", "Сэкономьте 10 € на скидках", "Save 10 € with discounts");
        L("Kogu ring", "Полный круг", "Full circle"); L("Osta igast kategooriast", "Купите из каждой категории", "Buy from every category");
        L("Hommikune", "Утренний", "Early bird"); L("Tee ost enne kella 9", "Сделайте покупку до 9 утра", "Shop before 9 am");
        L("Pere ostleja", "Семейный покупатель", "Family shopper"); L("Osta 10 rida pere nimekirjast", "Купите 10 строк из семейного списка", "Buy 10 rows from the family list");
        L("Kogu kogu! +{0}", "Вся коллекция! +{0}", "Full set! +{0}"); L("Kogu kogu! Kõik kleebised koos · +{0}", "Вся коллекция! Все наклейки собраны · +{0}", "Full set! All stickers collected · +{0}");
        L("Väljakutse tehtud: {0} · +{1}", "Задание выполнено: {0} · +{1}", "Challenge done: {0} · +{1}"); L("Uus kleeps: {0}", "Новая наклейка: {0}", "New sticker: {0}");
        L("Uus tase: {0} · {1} %", "Новый уровень: {0} · {1} %", "New level: {0} · {1} %"); L("PÄEVA ÜLLATUS", "СЮРПРИЗ ДНЯ", "SURPRISE OF THE DAY");
        L("Kord päevas – juhuslik soodustus ühele tootele", "Раз в день – случайная скидка на один товар", "Once a day – a random discount on one product");
        L("Ava kingitus", "Открыть подарок", "Open the gift"); L("Avatud ✓", "Открыто ✓", "Opened ✓"); L("ainult täna", "только сегодня", "today only");
        L("Täna pole üllatuseks sobivaid tooteid", "Сегодня нет подходящих товаров для сюрприза", "No products available for a surprise today");
        L("Päeva üllatus −{0} %", "Сюрприз дня −{0} %", "Surprise of the day −{0} %"); L("Preemiad · {0} {1} %", "Награды · {0} {1} %", "Rewards · {0} {1} %");
        L("Preemiate jaoks on vaja kliendikaarti. Ava „Kliendikaart“ ja loo see.", "Для наград нужна карта клиента. Откройте «Карта клиента» и создайте её.", "Rewards need a customer card. Open “Customer card” and create one.");
        // --- Demoandmed ---
        L("Demoandmed", "Демо-данные", "Demo data"); L("Demoandmed...", "Демо-данные...", "Demo data..."); L("toodet", "товаров", "products"); L("ostu", "покупок", "purchases");
        L("Üks nupp lisab 50 toodet, 3 klienti ja 20 ostu viimase 30 päeva jooksul, et programm ei näeks esitlusel tühi välja. Demoandmed on märgitud ja neid saab hiljem korraga eemaldada.", "Одна кнопка добавляет 50 товаров, 3 клиентов и 20 покупок за последние 30 дней, чтобы программа не выглядела пустой на защите. Демо-данные помечены, их можно удалить разом.", "One button adds 50 products, 3 customers and 20 purchases from the last 30 days so the app does not look empty in a demo. Demo data is marked and can be removed at once.");
        L("Lisa demoandmed", "Добавить демо-данные", "Add demo data"); L("Eemalda demoandmed", "Удалить демо-данные", "Remove demo data");
        L("Loon demoandmeid...", "Создаю демо-данные...", "Creating demo data..."); L("Demoandmed lisatud", "Демо-данные добавлены", "Demo data added"); L("Demoandmed eemaldatud", "Демо-данные удалены", "Demo data removed");
        L("Demoandmed on juba olemas.", "Демо-данные уже есть.", "Demo data already exists."); L("Demotoode esitluseks.", "Демо-товар для презентации.", "Demo product for presentation.");
        L("Lisati {0} toodet, {1} klienti ja {2} ostu.", "Добавлено товаров: {0}, клиентов: {1}, покупок: {2}.", "Added {0} products, {1} customers and {2} purchases.");
        L("Eemaldati {0} toodet, {1} klienti ja {2} ostu.", "Удалено товаров: {0}, клиентов: {1}, покупок: {2}.", "Removed {0} products, {1} customers and {2} purchases.");
        L("Kas eemaldan kõik demoandmed (tooted, kliendid, ostud)?", "Удалить все демо-данные (товары, клиентов, покупки)?", "Remove all demo data (products, customers, purchases)?");
        // --- Pant ---
        L("Pakendi tagatisraha", "Залог за тару", "Bottle deposit"); L("Pakendi tagatisraha {0}", "Залог за тару {0}", "Bottle deposit {0}");
        L("Pakendi tagatisraha ({0} tk) {1}", "Залог за тару ({0} шт) {1}", "Bottle deposit ({0} pcs) {1}"); L("{0} tk · {1}", "{0} шт · {1}", "{0} pcs · {1}");
        L("Pakendi tagatisraha kokku ({0} tk)", "Залог за тару итого ({0} шт)", "Bottle deposit total ({0} pcs)"); L("Soodustus", "Скидка", "Discount");
        L("Teenitud boonust ({0} %)", "Начислено бонусов ({0} %)", "Bonus earned ({0} %)");
        L("Muudatused on salvestamata. Kas sulgen ikkagi?", "Изменения не сохранены. Всё равно закрыть?", "Changes are not saved. Close anyway?");
    }
}
