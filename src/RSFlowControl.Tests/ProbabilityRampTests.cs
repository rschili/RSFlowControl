namespace RSFlowControl.Tests;

public class ProbabilityRampTests
{
    [Test]
    [Arguments(-0.1, 0.5, 10)]
    [Arguments(1.1, 0.5, 10)]
    [Arguments(0.1, -0.1, 10)]
    [Arguments(0.1, 1.1, 10)]
    public void Constructor_InvalidChanceRange_ThrowsArgumentOutOfRangeException(double minChance, double maxChance, int seconds)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new ProbabilityRamp(minChance, maxChance, TimeSpan.FromSeconds(seconds)));
    }

    [Test]
    public void Constructor_MinimumGreaterThanMaximum_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new ProbabilityRamp(0.8, 0.2, TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void Constructor_ZeroOrNegativeTimespan_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new ProbabilityRamp(0.1, 0.8, TimeSpan.Zero));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            new ProbabilityRamp(0.1, 0.8, TimeSpan.FromSeconds(-1)));
    }

    [Test]
    public async Task CurrentChance_InitiallyReturnsMinimumChance()
    {
        // Arrange
        var ramp = new ProbabilityRamp(0.2, 0.8, TimeSpan.FromSeconds(10));

        // Act
        var currentChance = ramp.CurrentChance;

        // Assert
        await Assert.That(currentChance).IsEqualTo(0.2).Within(0.001);
    }

    [Test]
    public async Task CurrentChance_AfterMaxTimespan_ReturnsMaximumChance()
    {
        // Arrange
        var ramp = new ProbabilityRamp(0.1, 0.9, TimeSpan.FromMilliseconds(50));

        // Act
        Thread.Sleep(100); // Wait longer than max timespan
        var currentChance = ramp.CurrentChance;

        // Assert
        await Assert.That(currentChance).IsEqualTo(0.9).Within(0.001);
    }

    [Test]
    public async Task CurrentChance_HalfwayThroughTimespan_ReturnsMiddleValue()
    {
        // Arrange
        var ramp = new ProbabilityRamp(0.2, 0.8, TimeSpan.FromMilliseconds(200));

        // Act
        Thread.Sleep(100); // Wait for half the timespan
        var currentChance = ramp.CurrentChance;

        // Assert
        var expectedChance = 0.2 + (0.5 * (0.8 - 0.2)); // Should be 0.5
        await Assert.That(currentChance).IsEqualTo(expectedChance).Within(0.05); // Allow some tolerance for timing
    }

    [Test]
    public async Task Reset_ResetsTimerAndCurrentChance()
    {
        // Arrange
        var ramp = new ProbabilityRamp(0.1, 0.9, TimeSpan.FromMilliseconds(100));
        Thread.Sleep(150); // Wait past max timespan
        
        // Act
        ramp.Reset();
        var currentChance = ramp.CurrentChance;

        // Assert
        await Assert.That(currentChance).IsEqualTo(0.1).Within(0.001);
    }

    [Test]
    public async Task Check_WithZeroChance_AlwaysReturnsFalse()
    {
        // Arrange
        var ramp = new ProbabilityRamp(0.0, 0.0, TimeSpan.FromSeconds(1));

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            await Assert.That(ramp.Check()).IsFalse();
        }
    }

    [Test]
    public async Task Check_WithMaxChance_AlwaysReturnsTrue()
    {
        // Arrange
        var ramp = new ProbabilityRamp(1.0, 1.0, TimeSpan.FromSeconds(1));

        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            await Assert.That(ramp.Check()).IsTrue();
        }
    }

    [Test]
    public async Task Check_WithModerateChance_ReturnsVariedResults()
    {
        // Arrange
        var ramp = new ProbabilityRamp(0.3, 0.7, TimeSpan.FromMilliseconds(100));
        Thread.Sleep(50); // Get to middle probability (~0.5)

        // Act
        var trueCount = 0;
        var totalChecks = 1000;
        
        for (int i = 0; i < totalChecks; i++)
        {
            if (ramp.Check())
                trueCount++;
        }

        // Assert
        var actualRate = (double)trueCount / totalChecks;
        var expectedRate = ramp.CurrentChance;
        
        // Should be within reasonable statistical range (±10%)
        await Assert.That(actualRate).IsGreaterThan(expectedRate - 0.1);
        await Assert.That(actualRate).IsLessThan(expectedRate + 0.1);
    }
}