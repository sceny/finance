using System.IO.Pipelines;
using Sceny.Finance.IO.Sources;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO;

/// <summary>
/// Extension methods for fluent copy operations between sources and targets.
/// Provides zero-allocation piped copying with a developer-friendly API.
/// </summary>
public static class CopyExtensions
{
    /// <summary>
    /// Copies data from a SourceBuilder to a TargetBuilder in a zero-allocation way using pipes.
    /// This is a one-hit fluent setup for copying between any source and target.
    /// </summary>
    /// <typeparam name="TSource">The type of source</typeparam>
    /// <typeparam name="TTarget">The type of target</typeparam>
    /// <param name="sourceBuilder">The source builder to read from</param>
    /// <param name="targetBuilder">The target builder to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyToAsync<TSource, TTarget>(
        this SourceBuilder<TSource> sourceBuilder,
        TargetBuilder<TTarget> targetBuilder,
        CancellationToken cancellationToken = default)
        where TSource : ISource
        where TTarget : ITarget
    {
        ArgumentNullException.ThrowIfNull(sourceBuilder);
        ArgumentNullException.ThrowIfNull(targetBuilder);

        await sourceBuilder.Source.CopyToAsync(targetBuilder.Target, cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Copies data from a source to a target in a zero-allocation way using pipes.
    /// This is a one-hit fluent setup for copying between any source and target.
    /// </summary>
    /// <param name="source">The source to read from</param>
    /// <param name="target">The target to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyToAsync(
        this ISource source,
        ITarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        PipeReader reader;
        PipeWriter writer;

        // Get reader from source
        if (source is FileSource fileSource)
        {
            reader = await fileSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (source is StreamSource streamSource)
        {
            reader = await streamSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            reader = source.GetPipeReader(cancellationToken);
        }

        // Get writer from target
        if (target is FileTarget fileTarget)
        {
            writer = await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (target is StreamTarget streamTarget)
        {
            writer = await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            writer = await target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }

        // Copy using zero-allocation pipe operation
        await Utilities.CopyPipeAsync(reader, writer, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a PipeReader from a source for manual zero-allocation copying.
    /// </summary>
    /// <param name="source">The source to get a reader from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeReader for manual copying</returns>
    public static async Task<PipeReader> GetPipeReaderAsync(
        this ISource source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is FileSource fileSource)
        {
            return await fileSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (source is StreamSource streamSource)
        {
            return await streamSource.GetPipeReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return source.GetPipeReader(cancellationToken);
        }
    }

    /// <summary>
    /// Gets a PipeWriter from a target for manual zero-allocation copying.
    /// </summary>
    /// <param name="target">The target to get a writer from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A PipeWriter for manual copying</returns>
    public static async Task<PipeWriter> GetPipeWriterAsync(
        this ITarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target is FileTarget fileTarget)
        {
            return await fileTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else if (target is StreamTarget streamTarget)
        {
            return await streamTarget.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            return await target.GetPipeWriterAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Copies accounts from a ConfiguredReader to a ConfiguredWriter in a zero-allocation way.
    /// This is a one-hit fluent setup for copying accounts between any format.
    /// </summary>
    /// <param name="reader">The configured reader to read accounts from</param>
    /// <param name="writer">The configured writer to write accounts to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyAccountsToAsync(
        this ConfiguredReader reader,
        ConfiguredWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(writer);

        var accounts = reader.GetAccountsAsync(cancellationToken);
        await writer.WriteAccountsAsync(accounts, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Copies transactions for a specific account from a ConfiguredReader to a ConfiguredWriter in a zero-allocation way.
    /// This is a one-hit fluent setup for copying transactions between any format.
    /// </summary>
    /// <param name="reader">The configured reader to read transactions from</param>
    /// <param name="account">The account to copy transactions for</param>
    /// <param name="writer">The configured writer to write transactions to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyTransactionsToAsync(
        this ConfiguredReader reader,
        Account account,
        ConfiguredWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(writer);

        var transactions = reader.GetTransactionsAsync(account, cancellationToken);
        await writer.WriteTransactionsAsync(account, transactions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Copies all accounts and their transactions from a ConfiguredReader to a ConfiguredWriter in a zero-allocation way.
    /// This is a one-hit fluent setup for full hierarchical copy between any format.
    /// </summary>
    /// <param name="reader">The configured reader to read from</param>
    /// <param name="writer">The configured writer to write to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the copy operation</returns>
    public static async Task CopyAllToAsync(
        this ConfiguredReader reader,
        ConfiguredWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(writer);

        await writer.BeginWriteAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await foreach (var account in reader.GetAccountsAsync(cancellationToken).ConfigureAwait(false))
            {
                await writer.WriteAccountAsync(account, cancellationToken).ConfigureAwait(false);

                await foreach (var transaction in reader.GetTransactionsAsync(account, cancellationToken).ConfigureAwait(false))
                {
                    await writer.WriteTransactionAsync(transaction, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            await writer.EndWriteAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}

