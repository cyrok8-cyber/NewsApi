namespace NewsApi.DTOs
{
    // Короткая информация об авторе - подставляется внутрь новостей и комментариев,
    // чтобы фронтенду не приходилось делать отдельный запрос за автором
    public class UserShortDto
    {
        public int Id { get; set; }
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
    }
}