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
        PipeWriter pipeWriter;

        // Handle async targets (FileTarget, StreamTarget) properly
        if (_target is FileTarget fileTarget)
        {
            pipeWriter = await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (_target is StreamTarget streamTarget)
        {
            pipeWriter = await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            pipeWriter = await _target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await _writer.WriteAccountsAsync(pipeWriter, accounts, cancellationToken).ConfigureAwait(false);
            await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await pipeWriter.CompleteAsync().ConfigureAwait(false);
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
        PipeWriter pipeWriter;

        // Handle async targets (FileTarget, StreamTarget) properly
        if (_target is FileTarget fileTarget)
        {
            pipeWriter = await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (_target is StreamTarget streamTarget)
        {
            pipeWriter = await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            pipeWriter = await _target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await _writer.WriteTransactionsAsync(account, pipeWriter, transactions, cancellationToken).ConfigureAwait(false);
            await pipeWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await pipeWriter.CompleteAsync().ConfigureAwait(false);
        }
    }
}

