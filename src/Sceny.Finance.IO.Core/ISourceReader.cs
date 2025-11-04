using System.IO.Pipelines;

namespace Sceny.Finance.IO;

/// <summary>
/// Defines the contract for source readers that extract financial data from various sources.
/// Uses IAsyncEnumerable for reactive, streaming consumption.
/// </summary>
public interface ISourceReader
{
    /// <summary>
    /// Streams accounts as they are discovered from the source.
    /// </summary>
    /// <param name="reader">The pipeline reader providing the source data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of discovered accounts</returns>
    IAsyncEnumerable<Account> GetAccountsAsync(PipeReader reader, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams transactions for a specific account.
    /// </summary>
    /// <param name="account">The account to get transactions for</param>
    /// <param name="reader">The pipeline reader providing the source data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of transactions for the account</returns>
    IAsyncEnumerable<Transaction> GetTransactionsAsync(
        Account account,
        PipeReader reader,
        CancellationToken cancellationToken = default);
}

