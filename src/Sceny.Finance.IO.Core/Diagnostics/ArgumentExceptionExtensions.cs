using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sceny.Finance.IO;

public static class ArgumentExceptionExtensions
{
    private const string WhitespaceMessage = "Value cannot be empty or whitespace.";

    extension(ArgumentException)
    {
        public static void ThrowIfNullOrWhiteSpace(
            [NotNull] string? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null,
            string? whiteSpaceMessage = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException(whiteSpaceMessage ?? WhitespaceMessage, paramName);
        }

        public static void ThrowIfNullOrWhiteSpace(
            ReadOnlySpan<char> argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null,
            string? whiteSpaceMessage = null)
        {
                if (argument.IsEmpty)
                    throw new ArgumentException(whiteSpaceMessage ?? WhitespaceMessage, paramName);

            for (int i = 0; i < argument.Length; i++)
            {
                if (!char.IsWhiteSpace(argument[i]))
                    return;
            }

            throw new ArgumentException(whiteSpaceMessage ?? WhitespaceMessage, paramName);
        }
    }
}


