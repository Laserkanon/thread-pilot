using System.Security.Cryptography;

namespace Vehicle.IntegrationTests.TesHelpers;

public static class RegistrationNumberGenerator
{
    public static string NewReg(int length = 7)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = RandomNumberGenerator.GetBytes(length);
        var span = new char[length];
        for (int i = 0; i < length; i++) span[i] = chars[bytes[i] % chars.Length];
        return new string(span);
    }
}