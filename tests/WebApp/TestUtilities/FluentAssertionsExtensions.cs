using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Sceny.Finance.WebApp.Tests;

public static class FluentAssertionsExtensions
{
    [PureAttribute]
    public static async Task<GenericCollectionAssertions<T>> ShouldAsync<T>(this IAsyncEnumerable<T> actualValue)
    {
        var array = await actualValue.ToArrayAsync().ConfigureAwait(false);
        return array.Should();
    }
}