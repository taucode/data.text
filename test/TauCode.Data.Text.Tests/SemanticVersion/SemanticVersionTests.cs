using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.Tests.TextDataExtractor.SemanticVersion;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.SemanticVersion;

[TestFixture]
public class SemanticVersionTests
{
    #region operator <

    [Test]
    public void OperatorLess_VersionsAreSequential_ReturnsTrue()
    {
        // Arrange
        var v1 = Text.SemanticVersion.Parse("1.0.0-alpha");
        var v2 = Text.SemanticVersion.Parse("1.0.0-alpha.1");
        var v3 = Text.SemanticVersion.Parse("1.0.0-alpha.beta");
        var v4 = Text.SemanticVersion.Parse("1.0.0-beta");
        var v5 = Text.SemanticVersion.Parse("1.0.0-beta.2");
        var v6 = Text.SemanticVersion.Parse("1.0.0-beta.11");
        var v7 = Text.SemanticVersion.Parse("1.0.0-rc.1");
        var v8 = Text.SemanticVersion.Parse("1.0.0");

        // Act
        var statement =
            v1 < v2 &&
            v2 < v3 &&
            v3 < v4 &&
            v4 < v5 &&
            v5 < v6 &&
            v6 < v7 &&
            v7 < v8 &&
            true;

        // Assert
        Assert.That(statement, Is.True);
    }

    #endregion

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(SemanticVersionExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null
                ? (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                : null;

        var extractor = new SemanticVersionExtractor(terminatingPredicate);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

        // Act
        var result = extractor.TryExtract(input, out var value);
        string errorMessage = null;
        if (result.ErrorCode.HasValue)
        {
            errorMessage = extractor.GetErrorMessage(result.ErrorCode.Value);
        }

        // Assert
        Assert.That(result.ToDto().ToString(), Is.EqualTo(testDto.ExpectedResult.ToString()));

        if (result.ErrorCode.HasValue)
        {
            // check test dto
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(SemanticVersionDto)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);
            Assert.That(testDto.ExpectedValueString, Is.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(Text.SemanticVersion)));
            Assert.That(errorMessage, Is.EqualTo(testDto.ExpectedErrorMessage));
        }
        else
        {
            // check test dto
            Assert.That(testDto.ExpectedErrorMessage, Is.Null);
            Assert.That(testDto.ExpectedResult.ErrorCode, Is.Null);

            // check test itself
            Assert.That(value.ToDto(), Is.EqualTo(testDto.ExpectedValue));
            Assert.That(value.ToString(), Is.EqualTo(testDto.ExpectedValueString));
        }
    }

    public static IList<SemanticVersionExtractorTestDto> GetTestDtos()
    {
        var json = typeof(SemanticVersionExtractorTests).Assembly.GetResourceText(
            $".{nameof(SemanticVersionExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<SemanticVersionExtractorTestDto>>(json);

        return dtos;
    }
}
