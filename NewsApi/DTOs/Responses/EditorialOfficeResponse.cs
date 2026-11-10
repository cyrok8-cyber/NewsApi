namespace NewsApi.DTOs.Responses
{
    // Полная информация о редакции вместе со списком её журналистов
    public class EditorialOfficeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserShortResponse> Members { get; set; } = new();
    }
}
