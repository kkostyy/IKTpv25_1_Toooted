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
        L("Muudatused on salvestamata. Kas sulgen ikkagi?", "Изменения не сохранены. Всё равно закрыть?", "Changes are not saved. Close anyway?");
    }
}
