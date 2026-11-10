namespace NewsApi.DTOs.Responses
{
    // Короткая информация о редакции - подставляется внутрь новостей и пользователей
    public class EditorialOfficeShortResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
