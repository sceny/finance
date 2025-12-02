using System.IO.Pipelines;

namespace Sceny.Finance.IO;

/// <summary>
/// Defines the contract for source readers that extract financial data from various sources.
/// Uses IAsyncEnumerable for reactive, streaming consumption.
/// </summary>
/// <typeparam name="TAccountProperties">The account properties type for this reader</typeparam>
/// <typeparam name="TTransactionProperties">The transaction properties type for this reader</typeparam>
public interface ISourceReader<TAccountProperties, TTransactionProperties>
    where TAccountProperties : struct, IProperties
    where TTransactionProperties : struct, IProperties
{
    /// <summary>
    /// Streams accounts as they are discovered from the source.
    /// </summary>
    /// <param name="reader">The pipeline reader providing the source data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of discovered accounts</returns>
    IAsyncEnumerable<Account<TAccountProperties>> GetAccountsAsync(PipeReader reader, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams transactions for a specific account.
    /// </summary>
    /// <param name="account">The account to get transactions for</param>
    /// <param name="reader">The pipeline reader providing the source data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of transactions for the account</returns>
    IAsyncEnumerable<Transaction<TTransactionProperties>> GetTransactionsAsync(
        Account<TAccountProperties> account,
        PipeReader reader,
        CancellationToken cancellationToken = default);
}

