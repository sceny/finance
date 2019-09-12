using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Model
{
    public class Account
    {
        private string name;

        public int Id { get; set; }
        [ReadOnly(true)]
        public string Slug { get; private set; }
        [Required]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                Slug = name?.Replace(" ", "-").ToLowerInvariant();
            }
        }
        [Required]
        public string Type { get; set; }

        [Required]
        public int InstitutionId { get; set; }
    }
}
