using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.SemanticVersion;

[TestFixture]
public class SemanticVersionExtractorTests
{
    [Test]
    public void Ctor_NoArguments_CreatesExtractor()
    {
        // Arrange

        // Act
        var extractor = new SemanticVersionExtractor();

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(500));
        Assert.That(extractor.Terminator, Is.Null);
    }

    [Test]
    public void Ctor_ValidArgument_CreatesExtractor()
    {
        // Arrange
        TerminatingDelegate method = DemoIsWhiteSpace;

        // Act
        var extractor = new SemanticVersionExtractor(method);

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(500));
        Assert.That(extractor.Terminator, Is.SameAs(method));
    }

    [Test]
    public void MaxConsumption_ValidValue_SetsMaxConsumption()
    {
        // Arrange
        var extractor = new SemanticVersionExtractor();

        // Act
        extractor.MaxConsumption = 222;

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(222));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void MaxConsumption_InvalidValue_SetsMaxConsumption(int badMaxConsumption)
    {
        // Arrange
        var extractor = new SemanticVersionExtractor();

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => extractor.MaxConsumption = badMaxConsumption);

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("maxConsumption"));
    }

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

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void Extract_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(SemanticVersionExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new SemanticVersionExtractor(terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

        // Act
        if (testDto.ExpectedResult.ErrorCode.HasValue)
        {
            // fail
            Text.SemanticVersion value = default;
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Extract(input, out value));

            Assert.That(value, Is.EqualTo(default(Text.SemanticVersion)));

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
    public void TryParse_SomeArgument_ReturnsExpectedResult(SemanticVersionExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new SemanticVersionExtractor(terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

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
    public void Parse_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(SemanticVersionExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new SemanticVersionExtractor(terminator);
        if (testDto.TestMaxConsumption == -1)
        {
            // do nothing
        }
        else
        {
            extractor.MaxConsumption = testDto.TestMaxConsumption;
        }

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

    public static IList<SemanticVersionExtractorTestDto> GetTestDtos()
    {
        var json = typeof(SemanticVersionExtractorTests).Assembly.GetResourceText(
            $".{nameof(SemanticVersionExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<SemanticVersionExtractorTestDto>>(json);

        return dtos;
    }

    public static IList<SemanticVersionExtractorTestDto> GetTestDtosWithoutTerminator() =>
        GetTestDtos()
            .Where(x => x.TestTerminatingChars == null)
            .ToList();

    private static bool DemoIsWhiteSpace(ReadOnlySpan<char> input, int pos) => input[pos].IsIn(' ', '\t', '\r', '\n');
}
