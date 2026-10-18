namespace NewsApi.Entities
{
    // Роль пользователя как отдельная сущность (admin/reporter/subscriber)
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}