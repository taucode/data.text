using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Int64;

[TestFixture]
public class Int64ExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(Int64ExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null
                ? (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                : null;

        var extractor = new Int64Extractor(terminatingPredicate);

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
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(int)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(int)));
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

    public static IList<Int64ExtractorTestDto> GetTestDtos()
    {
        var json = typeof(Int64ExtractorTests).Assembly.GetResourceText($".{nameof(Int64ExtractorTests)}.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<Int64ExtractorTestDto>>(json);

        return dtos;
    }
}
