using System.Security.Cryptography;

namespace VacaYAY.Business.Services.Employee;

internal static class TempPasswordGenerator
{
    public static string Generate()
    {
        Span<byte> bytes = stackalloc byte[18]; //allocates 18 byte buffer stack (its freed automatically when the method returns)
        RandomNumberGenerator.Fill(bytes); // fills bytes buffer with cryptographically random bytes

        return Convert.ToBase64String(bytes) //convert to string
            .Replace('+', '-') 
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
