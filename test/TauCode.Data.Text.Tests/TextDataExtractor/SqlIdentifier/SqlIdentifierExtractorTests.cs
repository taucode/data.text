using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.SqlIdentifier;

[TestFixture]
public class SqlIdentifierExtractorTests
{
    [Test]
    public void Ctor_NoArguments_CreatesExtractor()
    {
        // Arrange
        Func<string, bool> predictate = s => s == "select";

        // Act
        var extractor = new SqlIdentifierExtractor(predictate);

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(200));
        Assert.That(extractor.Terminator, Is.Null);
        Assert.That(extractor.ReservedWordPredicate, Is.SameAs(predictate));
    }

    [Test]
    public void Ctor_ValidArgument_CreatesExtractor()
    {
        // Arrange
        TerminatingDelegate method = DemoIsWhiteSpace;

        // Act
        var extractor = new SqlIdentifierExtractor(null, method);

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(200));
        Assert.That(extractor.Terminator, Is.SameAs(method));
        Assert.That(extractor.ReservedWordPredicate, Is.Null);
    }

    [Test]
    public void MaxConsumption_ValidValue_SetsMaxConsumption()
    {
        // Arrange
        var extractor = new SqlIdentifierExtractor(null);

        // Act
        extractor.MaxConsumption = 222;

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(222));
    }

    [Test]
    public void Delimiter_InvalidValue_ThrowsException()
    {
        // Arrange
        var extractor = new SqlIdentifierExtractor(s => false);

        // Act
        var ex = Assert.Throws<ArgumentException>(() => extractor.Delimiter = (SqlIdentifierDelimiter)0xff0000);

        // Assert
        Assert.That(ex.Message, Does.StartWith("Delimiter cannot be 0 and must be a combination of valid enum values."));
        Assert.That(ex.ParamName, Is.EqualTo("value"));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void MaxConsumption_InvalidValue_SetsMaxConsumption(int badMaxConsumption)
    {
        // Arrange
        var extractor = new SqlIdentifierExtractor(null);

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => extractor.MaxConsumption = badMaxConsumption);

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("maxConsumption"));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(SqlIdentifierExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        Func<string, bool> reservedWordPredicate =
            testDto.TestReservedWords == null ?
                null :
                s => testDto
                    .TestReservedWords
                    .Select(x => x.ToLowerInvariant())
                    .Contains(s.ToLowerInvariant());

        var extractor = new SqlIdentifierExtractor(reservedWordPredicate, terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

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
        Assert.That(result.ToDto().ToString(), Is.EqualTo(testDto.ExpectedResult.ToString()));

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

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void Extract_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(SqlIdentifierExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        Func<string, bool> reservedWordPredicate =
            testDto.TestReservedWords == null ?
                null :
                s => testDto
                    .TestReservedWords
                    .Select(x => x.ToLowerInvariant())
                    .Contains(s.ToLowerInvariant());

        var extractor = new SqlIdentifierExtractor(reservedWordPredicate, terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

        SqlIdentifierDelimiter delimiter = 0;
        foreach (var testDelimiter in testDto.TestDelimiters)
        {
            delimiter |= testDelimiter;
        }
        extractor.Delimiter = delimiter;

        // Act
        if (testDto.ExpectedResult.ErrorCode.HasValue)
        {
            // fail
            var value = new Text.SqlIdentifier();
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Extract(input, out value));

            Assert.That(value, Is.EqualTo(default(Text.SqlIdentifier)));

            Assert.That(ex.Message, Is.EqualTo(testDto.ExpectedErrorMessage));
            Assert.That(ex.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(ex.ErrorCode, Is.EqualTo(testDto.ExpectedResult.ErrorCode));
        }
        else
        {
            // success
            var consumed = extractor.Extract(input, out var value);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(value.ToDto(), Is.EqualTo(testDto.ExpectedValue));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtosWithoutTerminator))]
    public void TryParse_SomeArgument_ReturnsExpectedResult(SqlIdentifierExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        Func<string, bool> reservedWordPredicate =
            testDto.TestReservedWords == null ?
                null :
                s => testDto
                    .TestReservedWords
                    .Select(x => x.ToLowerInvariant())
                    .Contains(s.ToLowerInvariant());

        var extractor = new SqlIdentifierExtractor(reservedWordPredicate, terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

        SqlIdentifierDelimiter delimiter = 0;
        foreach (var testDelimiter in testDto.TestDelimiters)
        {
            delimiter |= testDelimiter;
        }
        extractor.Delimiter = delimiter;

        var terminatorBeforeParse = extractor.Terminator;

        // Act
        var parsed = extractor.TryParse(input, out var value);

        var expectedParsed = testDto.ExpectedResult.ErrorCode == null;

        Assert.That(parsed, Is.EqualTo(expectedParsed));

        if (expectedParsed)
        {
            Assert.That(value.ToDto(), Is.EqualTo(testDto.ExpectedValue));
        }
        else
        {
            Assert.That(testDto.ExpectedValue, Is.Null);
        }

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtosWithoutTerminator))]
    public void Parse_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(SqlIdentifierExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        Func<string, bool> reservedWordPredicate =
            testDto.TestReservedWords == null ?
                null :
                s => testDto
                    .TestReservedWords
                    .Select(x => x.ToLowerInvariant())
                    .Contains(s.ToLowerInvariant());

        var extractor = new SqlIdentifierExtractor(reservedWordPredicate, terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

        SqlIdentifierDelimiter delimiter = 0;
        foreach (var testDelimiter in testDto.TestDelimiters)
        {
            delimiter |= testDelimiter;
        }
        extractor.Delimiter = delimiter;

        var terminatorBeforeParse = extractor.Terminator;

        // Act
        if (testDto.ExpectedResult.ErrorCode.HasValue)
        {
            // fail
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Parse(input));

            Assert.That(ex.Message, Is.EqualTo(testDto.ExpectedErrorMessage));
            Assert.That(ex.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(ex.ErrorCode, Is.EqualTo(testDto.ExpectedResult.ErrorCode));
        }
        else
        {
            // success
            var value = extractor.Parse(input);
            Assert.That(value.ToDto(), Is.EqualTo(testDto.ExpectedValue));
        }

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
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

            if (dto.ExpectedValueString != null)
            {
                dto.ExpectedValueString = TestHelper.TransformTestString(dto.ExpectedValueString);
            }
        }

        return dtos;
    }

    public static IList<SqlIdentifierExtractorTestDto> GetTestDtosWithoutTerminator() =>
        GetTestDtos()
            .Where(x => x.TestTerminatingChars == null)
            .ToList();

    private static bool DemoIsWhiteSpace(ReadOnlySpan<char> input, int pos) => input[pos].IsIn(' ', '\t', '\r', '\n');
}
