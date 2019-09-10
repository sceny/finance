using System.ComponentModel;

namespace WebApp.Model
{
    public class Account
    {
        private string name;

        public int Id { get; set; }
        [ReadOnly(true)]
        public string Slug { get; private set; }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                Slug = name?.Replace(" ", "-").ToLowerInvariant();
            }
        }
        public string Type { get; set; }
        public int InstitutionId { get; set; }
    }
}
