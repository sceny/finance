using System;
using System.Collections.Generic;

namespace WebApp.Model
{
    public static class MockData
    {
        private static readonly List<Account> _accounts = new List<Account>
        {
            new Account { Id = 1, Name = "RBC 3132", InstitutionId = 1, Type = "Any" },
            new Account { Id = 2, Name = "RBC 5445", InstitutionId = 1, Type = "Any" },
            new Account { Id = 3, Name = "CapitalOne xxx9999", InstitutionId = 2, Type = "Any" },
        };


        private static readonly List<Institution> _institutions = new List<Institution>
        {
            new Institution { Id = 1, Name = "RBC", Slug = "rbc" },
            new Institution { Id = 2, Name = "CapitalOne", Slug = "CapitalOne" }
        };


        internal static void AddAccount(Account account)
        {
            account.Id = _accounts.Count + 1;
            _accounts.Add(account);
        }
        internal static bool TryUpdateAccount(int id, Account account)
        {
            var index = _accounts.FindIndex(0, _accounts.Count, a => a.Id == id);
            if (index == -1)
            {
                return false;
            }
            _accounts[index] = account;
            return true;
        }
        public static IEnumerable<Account> GetAccounts() => _accounts;
        public static IEnumerable<Institution> GetInstitutions() => _institutions;
    }
}
