using System.IO.Pipelines;

namespace Sceny.Finance.IO;

/// <summary>
/// Defines the contract for source writers that write financial data to various targets.
/// Uses IAsyncEnumerable for reactive, streaming writes.
/// </summary>
/// <typeparam name="TAccountProperties">The account properties type for this writer</typeparam>
/// <typeparam name="TTransactionProperties">The transaction properties type for this writer</typeparam>
public interface ISourceWriter<TAccountProperties, TTransactionProperties>
    where TAccountProperties : struct, IProperties
    where TTransactionProperties : struct, IProperties
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
        IAsyncEnumerable<Account<TAccountProperties>> accounts,
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
        Account<TAccountProperties> account,
        PipeWriter writer,
        IAsyncEnumerable<Transaction<TTransactionProperties>> transactions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a single account to the target. Used for manual iteration scenarios.
    /// </summary>
    /// <param name="writer">The pipeline writer providing the target destination</param>
    /// <param name="account">The account to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    Task WriteAccountAsync(
        PipeWriter writer,
        Account<TAccountProperties> account,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a single transaction to the target. Used for manual iteration scenarios.
    /// </summary>
    /// <param name="writer">The pipeline writer providing the target destination</param>
    /// <param name="transaction">The transaction to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    Task WriteTransactionAsync(
        PipeWriter writer,
        Transaction<TTransactionProperties> transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the header/footer of the format. Called before any data is written for header, after all data for footer.
    /// </summary>
    /// <param name="writer">The pipeline writer providing the target destination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    Task WriteHeaderAsync(
        PipeWriter writer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the footer/closing of the format. Called after all data is written.
    /// </summary>
    /// <param name="writer">The pipeline writer providing the target destination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    Task WriteFooterAsync(
        PipeWriter writer,
        CancellationToken cancellationToken = default);
}

