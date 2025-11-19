using System.IO.Pipelines;

namespace Sceny.Finance.IO;

/// <summary>
/// Defines the contract for source writers that write financial data to various targets.
/// Uses IAsyncEnumerable for reactive, streaming writes.
/// </summary>
public interface ISourceWriter
{
    /// <summary>
    /// Writes accounts to the target as they are provided.
    /// </summary>
    /// <param name="writer">The pipeline writer providing the target destination</param>
    /// <param name="accounts">The accounts to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    Task WriteAccountsAsync(
        PipeWriter writer,
        IAsyncEnumerable<Account> accounts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes transactions for a specific account to the target.
    /// </summary>
    /// <param name="account">The account the transactions belong to</param>
    /// <param name="writer">The pipeline writer providing the target destination</param>
    /// <param name="transactions">The transactions to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    Task WriteTransactionsAsync(
        Account account,
        PipeWriter writer,
        IAsyncEnumerable<Transaction> transactions,
        CancellationToken cancellationToken = default);
}

