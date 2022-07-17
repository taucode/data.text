using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.Tests.TextDataExtractor.Item;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Enum;

[TestFixture]
public class EnumExtractorTests
{
    [Test]
    public void Ctor_ValidArguments_CreatesExtractor()
    {
        // Arrange
        Func<string, bool, NumberText> parser = (text, ignoreCase) => new NumberText(text, 11);
        TerminatingDelegate terminator = (input, index) => input[0] == ' ';

        // Act
        var extractor = new EnumExtractor<Color>(
            true,
            terminator);

        // Assert
        Assert.That(extractor.MaxConsumption, Is.Null);
        Assert.That(extractor.Terminator, Is.EqualTo(terminator));
        Assert.That(extractor.IgnoreCase, Is.True);
    }

    [Test]
    public void MaxConsumption_ValidValue_SetsMaxConsumption()
    {
        // Arrange
        Func<string, bool, NumberText> parser = (text, ignoreCase) => new NumberText(text, 11);
        TerminatingDelegate terminator = (input, index) => input[0] == ' ';

        var extractor = new EnumExtractor<Color>(
            true,
            terminator);

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
        Func<string, bool, NumberText> parser = (text, ignoreCase) => new NumberText(text, 11);
        TerminatingDelegate terminator = (input, index) => input[0] == ' ';

        var extractor = new EnumExtractor<Color>(
            true,
            terminator);

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => extractor.MaxConsumption = badMaxConsumption);

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("maxConsumption"));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(EnumExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EnumExtractor<Color>(
            testDto.TestIgnoreCase,
            terminator);

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
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(Color)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(Color)));
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

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void Extract_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(EnumExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EnumExtractor<Color>(
            testDto.TestIgnoreCase,
            terminator);
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
            var value = Color.Black;
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Extract(input, out value));

            Assert.That(value, Is.EqualTo(default(Color)));

            Assert.That(ex.Message, Is.EqualTo(testDto.ExpectedErrorMessage));
            Assert.That(ex.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(ex.ErrorCode, Is.EqualTo(testDto.ExpectedResult.ErrorCode));
        }
        else
        {
            // success
            var consumed = extractor.Extract(input, out var value);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(value, Is.EqualTo(testDto.ExpectedValue));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtosWithoutTerminator))]
    public void TryParse_SomeArgument_ReturnsExpectedResult(EnumExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EnumExtractor<Color>(
            testDto.TestIgnoreCase,
            terminator);
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
        Assert.That(value, Is.EqualTo(testDto.ExpectedValue));

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtosWithoutTerminator))]
    public void Parse_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(EnumExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EnumExtractor<Color>(
            testDto.TestIgnoreCase,
            terminator);
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
            Assert.That(value, Is.EqualTo(testDto.ExpectedValue));
        }

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
    }

    public static IList<EnumExtractorTestDto> GetTestDtos()
    {
        var json = typeof(EnumExtractorTests).Assembly.GetResourceText(
            $".{nameof(EnumExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<EnumExtractorTestDto>>(json);

        return dtos;
    }

    public static IList<EnumExtractorTestDto> GetTestDtosWithoutTerminator() =>
        GetTestDtos()
            .Where(x => x.TestTerminatingChars == null)
            .ToList();

    private static bool DemoIsWhiteSpace(ReadOnlySpan<char> input, int pos) => input[pos].IsIn(' ', '\t', '\r', '\n');
}
