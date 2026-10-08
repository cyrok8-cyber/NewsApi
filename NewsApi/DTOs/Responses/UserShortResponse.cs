namespace NewsApi.DTOs.Responses
{
    // Короткая информация об авторе - подставляется внутрь новостей и комментариев
    public class UserShortResponse
    {
        public int Id { get; set; }
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
    }
}