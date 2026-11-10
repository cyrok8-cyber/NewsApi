namespace NewsApi.DTOs.Responses
{
    public class NewsResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        // Готовая подпись публикации: сначала наименование редакции, затем редактор.
        // Например: "Редакция «Городские вести», редактор: Журналистов Пётр Петрович".
        // У журналиста-одиночки - только ФИО
        public string PublishedBy { get; set; } = string.Empty;

        // Редакция, от имени которой опубликована новость (null - без редакции)
        public EditorialOfficeShortResponse? EditorialOffice { get; set; }

        public UserShortResponse InsertedBy { get; set; } = null!;
        public UserShortResponse? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}
