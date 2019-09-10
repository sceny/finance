using System.Collections.Generic;

namespace WebApp.Model
{
    public static class MockData
    {
        private static readonly List<Account> _accounts = new List<Account>
        {
            new Account { Id = 1, Name = "RBC 3132", Slug = "rbc-3132", InstitutionId = 1, Category = "Any" },
            new Account { Id = 2, Name = "RBC 5445", Slug = "rbc-5445", InstitutionId = 1, Category = "Any" },
            new Account { Id = 3, Name = "CapitalOne xxx9999", Slug = "capitalone-xxx9999", InstitutionId = 2, Category = "Any" },
        };
        private static readonly List<Institution> _institutions = new List<Institution>
        {
            new Institution { Id = 1, Name = "RBC", Slug = "rbc" },
            new Institution { Id = 2, Name = "CapitalOne", Slug = "CapitalOne" }
        };

        public static IEnumerable<Account> GetAccounts() => _accounts;
        public static IEnumerable<Institution> GetInstitutions() => _institutions;
    }
}
