using System;
using System.Runtime.CompilerServices;

namespace WebApp.Domain
{
    public static class Check
    {
        public static T EFLoaded<T>(
            T? value,
            [CallerMemberName] string propertyName = "") where T : class
            => value ?? throw new InvalidOperationException($"Uninitialized {propertyName}.");
    }
}