using System.IO.Pipelines;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO;

/// <summary>
/// Wraps a source and reader together to provide a configured reader for financial data.
/// This class combines the ISource (data source) with ISourceReader (format parser).
/// </summary>
public sealed class ConfiguredReader(ISource source, ISourceReader reader)
{
    private readonly ISource _source = source ?? throw new ArgumentNullException(nameof(source));
    private readonly ISourceReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    /// <summary>
    /// Streams accounts as they are discovered from the source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of discovered accounts</returns>
    public async IAsyncEnumerable<Account> GetAccountsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        PipeReader pipeReader;

        // Handle async sources (FileSource, StreamSource) properly
        if (_source is FileSource fileSource)
        {
            pipeReader = await fileSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (_source is StreamSource streamSource)
        {
            pipeReader = await streamSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            pipeReader = _source.GetPipeReader(cancellationToken);
        }

        await foreach (var account in _reader.GetAccountsAsync(pipeReader, cancellationToken).ConfigureAwait(false))
        {
            yield return account;
        }
    }

    /// <summary>
    /// Streams transactions for a specific account.
    /// </summary>
    /// <param name="account">The account to get transactions for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of transactions for the account</returns>
    public async IAsyncEnumerable<Transaction> GetTransactionsAsync(
        Account account,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        PipeReader pipeReader;

        // Handle async sources (FileSource, StreamSource) properly
        if (_source is FileSource fileSource)
        {
            pipeReader = await fileSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (_source is StreamSource streamSource)
        {
            pipeReader = await streamSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            pipeReader = _source.GetPipeReader(cancellationToken);
        }

        await foreach (var transaction in _reader.GetTransactionsAsync(account, pipeReader, cancellationToken).ConfigureAwait(false))
        {
            yield return transaction;
        }
    }
}

