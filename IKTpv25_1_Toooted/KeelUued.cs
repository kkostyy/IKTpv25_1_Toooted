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
        L("Muudatused on salvestamata. Kas sulgen ikkagi?", "Изменения не сохранены. Всё равно закрыть?", "Changes are not saved. Close anyway?");
    }
}
