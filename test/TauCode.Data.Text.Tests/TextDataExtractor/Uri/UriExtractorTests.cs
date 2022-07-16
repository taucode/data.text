using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Uri;

[TestFixture]
public class UriExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(UriExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null
                ? (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                : null;

        var extractor = new UriExtractor(terminatingPredicate);

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
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(System.Uri)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(System.Uri)));
            Assert.That(errorMessage, Is.EqualTo(testDto.ExpectedErrorMessage));
        }
        else
        {
            // check test dto
            Assert.That(testDto.ExpectedErrorMessage, Is.Null);
            Assert.That(testDto.ExpectedResult.ErrorCode, Is.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(testDto.ExpectedValue));
        }
    }

    public static IList<UriExtractorTestDto> GetTestDtos()
    {
        var json = typeof(UriExtractorTests).Assembly.GetResourceText(
            $".{nameof(UriExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<UriExtractorTestDto>>(json);

        return dtos;
    }
}
