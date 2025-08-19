using System.Globalization;

namespace Insurance.IntegrationTests.TestHelpers;

public static class PersonalIdentityNumberGenerator
{
    /// <summary>
    /// Returns a random valid Swedish personal number (12 digits): YYYYMMDDNNNC.
    /// - Luhn checksum is computed over YYMMDDNNN.
    /// - By default: age 18..90, no coordination numbers.
    /// </summary>
    public static string Generate(int minAgeYears = 18, int maxAgeYears = 90, bool allowCoordinationNumber = false)
    {
        if (minAgeYears < 0 || maxAgeYears < 0 || maxAgeYears < minAgeYears)
            throw new ArgumentOutOfRangeException(nameof(minAgeYears), "Invalid age range.");

        var rnd = Random.Shared;

        var today  = DateTime.UtcNow.Date;
        var minDob = today.AddYears(-maxAgeYears);
        var maxDob = today.AddYears(-minAgeYears);
        var dob    = RandomDate(minDob, maxDob, rnd);

        int shownDay = dob.Day;
        if (allowCoordinationNumber && rnd.Next(0, 2) == 1)
            shownDay += 60; // coordination number: DD = calendar day + 60

        string yyyymmdd = $"{dob:yyyyMM}{shownDay:00}";
        string yymmdd   = $"{dob:yyMM}{shownDay:00}";

        string nnn = rnd.Next(0, 1000).ToString("000", CultureInfo.InvariantCulture);

        int checksum = LuhnCheckDigit(yymmdd + nnn); // Luhn over YYMMDDNNN

        return yyyymmdd + nnn + checksum.ToString(CultureInfo.InvariantCulture);
    }

    private static DateTime RandomDate(DateTime min, DateTime max, Random rnd)
    {
        if (min > max) (min, max) = (max, min);
        int range = (max - min).Days;
        return min.AddDays(rnd.Next(range + 1));
    }

    private static int LuhnCheckDigit(string digits)
    {
        int sum = 0;
        bool dbl = false; // start from rightmost, don't double first
        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int d = digits[i] - '0';
            if (dbl) { d *= 2; if (d > 9) d -= 9; }
            sum += d;
            dbl = !dbl;
        }
        return (10 - (sum % 10)) % 10;
    }
}