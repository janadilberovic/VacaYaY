using System.Security.Cryptography;

namespace VacaYAY.Business.Services.Employee;

internal static class TempPasswordGenerator
{
    public static string Generate()
    {
        Span<byte> bytes = stackalloc byte[18];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
