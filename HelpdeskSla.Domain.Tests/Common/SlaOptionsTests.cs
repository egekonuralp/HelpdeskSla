using HelpdeskSla.Application.Common.Options;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskSla.Domain.Tests.Common;

public class SlaOptionsTest
{
    [Fact]
    public void SlaOptions_Binding_UsesConfiguredValues()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Sla:LowHours"] = "48",
            ["Sla:MediumHours"] = "16",
            ["Sla:HighHours"] = "6",
            ["Sla:CriticalHours"] = "3"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        // Act
        var options = new SlaOptions();

        configuration
            .GetSection(SlaOptions.SectionName)
            .Bind(options);

        // Assert
        Assert.Equal(48, options.LowHours);
        Assert.Equal(16, options.MediumHours);
        Assert.Equal(6, options.HighHours);
        Assert.Equal(3, options.CriticalHours);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void SlaOptions_InvalidCriticalHours_FailsValidation(int criticalHours)
    {
        // Arrange
        var options = new SlaOptions
        {
            CriticalHours = criticalHours
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(options);

        // Act
        var isValid = Validator.TryValidateObject(options, validationContext, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(SlaOptions.CriticalHours)));
    }
}