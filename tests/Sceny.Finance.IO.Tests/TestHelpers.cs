namespace Sceny.Finance.IO.Tests;

internal static class TestHelpers
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Waits a short time for background async operations (like StringTarget writes) to complete.
    /// </summary>
    public static async Task WaitForAsyncWrites()
    {
        await Task.Delay(100); // Wait 100ms for background writes to complete
    }
}

