using NUnit.Framework;
using System.Collections.Generic;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.Tests.TextDataExtractor.HostName;

namespace TauCode.Data.Text.Tests.HostName;

[TestFixture]
public class HostNameTests
{
    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void TryParse_AnyArgument_ReturnsExpectedResult(HostNameExtractorTestDto dto)
    {
        // Arrange

        // Act
        var parsed = Text.HostName.TryParse(dto.TestInput, out var hostName);

        // Assert
        if (dto.ExpectedResult.ErrorCode.HasValue)
        {
            // check dto
            Assert.That(dto.ExpectedValue, Is.Null);
            Assert.That(dto.ExpectedValueString, Is.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(parsed, Is.False);
            Assert.That(hostName, Is.EqualTo(default(Text.HostName)));
        }
        else
        {
            // check dto
            Assert.That(dto.ExpectedValue, Is.Not.Null);
            Assert.That(dto.ExpectedValueString, Is.Not.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Null);

            // check test itself
            Assert.That(parsed, Is.True);
            Assert.That(hostName.ToDto(), Is.EqualTo(dto.ExpectedValue));
            Assert.That(hostName.ToString(), Is.EqualTo(dto.ExpectedValueString));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void Parse_AnyArgument_ReturnsExpectedResult(HostNameExtractorTestDto dto)
    {
        if (dto.Comment.Contains("[Not for Parse]"))
        {
            Assert.Pass("Skipped");
        }

        // Arrange
        var successExpected = dto.ExpectedResult.ErrorCode == null;
        var input = dto.TestInput;

        if (successExpected)
        {
            // Act
            var hostName = Text.HostName.Parse(input);

            // Assert
            // check dto
            Assert.That(dto.ExpectedValue, Is.Not.Null);
            Assert.That(dto.ExpectedValueString, Is.Not.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Null);

            // check test itself
            Assert.That(hostName.ToDto(), Is.EqualTo(dto.ExpectedValue));
            Assert.That(hostName.ToString(), Is.EqualTo(dto.ExpectedValueString));
        }
        else
        {
            // Act
            var ex = Assert.Throws<TextDataExtractionException>(() => Text.HostName.Parse(input));

            // Assert
            // check dto
            Assert.That(dto.ExpectedValue, Is.Null);
            Assert.That(dto.ExpectedValueString, Is.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(ex.Message, Is.EqualTo(dto.ExpectedErrorMessage));
            Assert.That(ex.ErrorCode, Is.EqualTo(dto.ExpectedResult.ErrorCode));
            Assert.That(ex.CharsConsumed, Is.EqualTo(dto.ExpectedResult.CharsConsumed));
        }
    }

    public static IList<HostNameExtractorTestDto> GetTestCases()
    {
        return HostNameExtractorTests.GetTestDtos();
    }
}
