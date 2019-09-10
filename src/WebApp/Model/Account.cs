namespace WebApp.Model
{
    public class Account
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int InstitutionId { get; set; }
    }
}
