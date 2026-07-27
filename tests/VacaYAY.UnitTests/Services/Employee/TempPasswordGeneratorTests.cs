using VacaYAY.Business.Services.Employee;

namespace VacaYAY.UnitTests;

public class TempPasswordGeneratorTests
{
    [Fact]
    public void Generate_ReturnsNonEmptyValue()
    {
        Assert.False(string.IsNullOrWhiteSpace(TempPasswordGenerator.Generate()));
    }

    [Fact]
    public void Generate_ProducesUrlSafeValue()
    {
        // The base64 output is remapped so it can travel in URLs and JSON without escaping.
        var password = TempPasswordGenerator.Generate();

        Assert.DoesNotContain('+', password);
        Assert.DoesNotContain('/', password);
        Assert.DoesNotContain('=', password);
    }

    [Fact]
    public void Generate_ProducesDistinctValuesAcrossCalls()
    {
        // 18 random bytes make collisions astronomically unlikely; a batch must be all-unique.
        var passwords = Enumerable.Range(0, 100).Select(_ => TempPasswordGenerator.Generate()).ToList();

        Assert.Equal(passwords.Count, passwords.Distinct().Count());
    }

    [Fact]
    public void Generate_EncodesEighteenBytesWithoutPadding()
    {
        // 18 bytes → 24 base64 chars, a clean multiple of 3 so nothing is trimmed as padding.
        Assert.Equal(24, TempPasswordGenerator.Generate().Length);
    }
}
