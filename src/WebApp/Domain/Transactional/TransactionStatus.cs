using System.ComponentModel.DataAnnotations;

namespace WebApp.Domain.Transactional
{
    public class TransactionStatus
    {
        public TransactionStatusId Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; }
    }

    public enum TransactionStatusId
    {
        Void = 1,
        Cleared = 2
    }
}