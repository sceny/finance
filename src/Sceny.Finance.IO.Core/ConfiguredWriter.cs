using System.IO.Pipelines;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO;

/// <summary>
/// Wraps a target and writer together to provide a configured writer for financial data.
/// This class combines the ITarget (data destination) with ISourceWriter (format serializer).
/// </summary>
public sealed class ConfiguredWriter(ITarget target, ISourceWriter writer)
{
    private readonly ITarget _target = target ?? throw new ArgumentNullException(nameof(target));
    private readonly ISourceWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private PipeWriter? _pipeWriter;
    private bool _isInitialized;

    /// <summary>
    /// Writes accounts to the target as they are provided.
    /// </summary>
    /// <param name="accounts">The accounts to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    public async Task WriteAccountsAsync(
        IAsyncEnumerable<Account> accounts,
        CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            throw new InvalidOperationException("Cannot use batch methods (WriteAccountsAsync) while manual write operation is active. Call EndWriteAsync() first or use manual methods only.");

        PipeWriter pipeWriter = await GetOrCreatePipeWriterAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _writer.WriteAccountsAsync(pipeWriter, accounts, cancellationToken).ConfigureAwait(false);
            await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await CompletePipeWriterAsync(pipeWriter).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Writes transactions for a specific account to the target.
    /// </summary>
    /// <param name="account">The account the transactions belong to</param>
    /// <param name="transactions">The transactions to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    public async Task WriteTransactionsAsync(
        Account account,
        IAsyncEnumerable<Transaction> transactions,
        CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            throw new InvalidOperationException("Cannot use batch methods (WriteTransactionsAsync) while manual write operation is active. Call EndWriteAsync() first or use manual methods only.");

        PipeWriter pipeWriter = await GetOrCreatePipeWriterAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _writer.WriteTransactionsAsync(account, pipeWriter, transactions, cancellationToken).ConfigureAwait(false);
            await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await CompletePipeWriterAsync(pipeWriter).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Begins a manual write operation. Must be called before WriteAccountAsync/WriteTransactionAsync.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the initialization</returns>
    public async Task BeginWriteAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            throw new InvalidOperationException("Write operation has already been started. Call EndWriteAsync() first.");

        _pipeWriter = await GetOrCreatePipeWriterAsync(cancellationToken).ConfigureAwait(false);
        await _writer.WriteHeaderAsync(_pipeWriter, cancellationToken).ConfigureAwait(false);
        _isInitialized = true;
    }

    /// <summary>
    /// Writes a single account. Requires BeginWriteAsync() to be called first.
    /// </summary>
    /// <param name="account">The account to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    public async Task WriteAccountAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized || _pipeWriter == null)
            throw new InvalidOperationException("BeginWriteAsync() must be called before WriteAccountAsync().");

        await _writer.WriteAccountAsync(_pipeWriter, account, cancellationToken).ConfigureAwait(false);
        await _pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes a single transaction. Requires BeginWriteAsync() to be called first.
    /// </summary>
    /// <param name="transaction">The transaction to write</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the write operation</returns>
    public async Task WriteTransactionAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized || _pipeWriter == null)
            throw new InvalidOperationException("BeginWriteAsync() must be called before WriteTransactionAsync().");

        await _writer.WriteTransactionAsync(_pipeWriter, transaction, cancellationToken).ConfigureAwait(false);
        await _pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Ends a manual write operation. Must be called after all writes are complete.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the completion</returns>
    public async Task EndWriteAsync(CancellationToken cancellationToken = default)
    {
        if (!_isInitialized || _pipeWriter == null)
            throw new InvalidOperationException("BeginWriteAsync() must be called before EndWriteAsync().");

        try
        {
            await _writer.WriteFooterAsync(_pipeWriter, cancellationToken).ConfigureAwait(false);
            await _pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await CompletePipeWriterAsync(_pipeWriter).ConfigureAwait(false);
            _pipeWriter = null;
            _isInitialized = false;
        }
    }

    private async Task<PipeWriter> GetOrCreatePipeWriterAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized && _pipeWriter != null)
            return _pipeWriter;

        // Handle async targets (FileTarget, StreamTarget) properly
        if (_target is FileTarget fileTarget)
        {
            return await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (_target is StreamTarget streamTarget)
        {
            return await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return await _target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task CompletePipeWriterAsync(PipeWriter pipeWriter)
    {
        await pipeWriter.CompleteAsync().ConfigureAwait(false);
    }
}

