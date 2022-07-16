using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.SqlIdentifier;

[TestFixture]
public class SqlIdentifierExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(SqlIdentifierExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new SqlIdentifierExtractor(terminator: terminatingPredicate);

        SqlIdentifierDelimiter delimiter = 0;
        foreach (var testDelimiter in testDto.TestDelimiters)
        {
            delimiter |= testDelimiter;
        }
        extractor.Delimiter = delimiter;

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
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(SqlIdentifierDto)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);
            Assert.That(testDto.ExpectedValueString, Is.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(Text.SqlIdentifier)));
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

    public static IList<SqlIdentifierExtractorTestDto> GetTestDtos()
    {
        var json = typeof(SqlIdentifierExtractorTests).Assembly.GetResourceText(
            $".{nameof(SqlIdentifierExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<SqlIdentifierExtractorTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);
            if (dto.ExpectedValue != null)
            {
                dto.ExpectedValue.Value = TestHelper.TransformTestString(dto.ExpectedValue.Value);
            }
        }

        return dtos;
    }
}