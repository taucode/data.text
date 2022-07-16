using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.EmailAddress;

// todo_deferred: ut performance of "123456789012345678901234567890123456789012345678901234567890@12345678901234567890123456789012345678901234567890123456789.12345678901234567890123456789012345678901234567890123456789.12345678901234567890123456789012345678901234567890123456789.12345.iana.org",
// todo_deferred: too much emoji checks on this case.

[TestFixture]
public class EmailAddressExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(EmailAddressExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null
                ? (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                : null;

        var extractor = new EmailAddressExtractor(terminatingPredicate);

        // Act
        var result = extractor.TryExtract(input, out var value);
        string errorMessage = null;
        if (result.ErrorCode.HasValue)
        {
            errorMessage = extractor.GetErrorMessage(result.ErrorCode.Value);
        }

        // Assert
        Assert.That(result.ToDto(), Is.EqualTo(testDto.ExpectedResult));

        if (result.ErrorCode.HasValue)
        {
            // check test dto
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(EmailAddressDto)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);
            Assert.That(testDto.ExpectedValueString, Is.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(Text.EmailAddress)));
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

    public static IList<EmailAddressExtractorTestDto> GetTestDtos()
    {
        var json = typeof(EmailAddressExtractorTests).Assembly.GetResourceText(
            $".{nameof(EmailAddressExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<EmailAddressExtractorTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = dto.TestInput.Replace('␀', '\0');
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);

            if (dto.ExpectedValueString != null)
            {
                dto.ExpectedValueString = dto.ExpectedValueString.Replace('␀', '\0');
                dto.ExpectedValueString = TestHelper.TransformTestString(dto.ExpectedValueString);
            }

            if (dto.ExpectedValue != null)
            {
                dto.ExpectedValue.LocalPart = dto.ExpectedValue.LocalPart.Replace('␀', '\0');
                dto.ExpectedValue.LocalPart = TestHelper.TransformTestString(dto.ExpectedValue.LocalPart);
            }
        }

        return dtos;
    }
}
