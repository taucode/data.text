﻿using Newtonsoft.Json;
using NUnit.Framework;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.StringItem;

[TestFixture]
public class StringItemExtractorTests
{
    [Test]
    public void Ctor_ValidArguments_CreatesExtractor()
    {
        // Arrange
        TerminatingDelegate method = DemoIsWhiteSpace;

        // Act
        var extractor = new StringItemExtractor(new[] { "a", "b" }, true, method);

        // Assert
        Assert.That(extractor.MaxConsumption, Is.Null);
        Assert.That(extractor.Items, Is.EqualTo(new[] { "a", "b" }));
        Assert.That(extractor.Terminator, Is.SameAs(method));
        Assert.That(extractor.IgnoreCase, Is.True);
    }

    [Test]
    public void MaxConsumption_ValidValue_SetsMaxConsumption()
    {
        // Arrange
        var extractor = new StringItemExtractor(new[] { "a", "b" }, true);

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
        var extractor = new StringItemExtractor(new[] { "a", "b" }, true);

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => extractor.MaxConsumption = badMaxConsumption);

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("maxConsumption"));
    }

    [Test]
    [TestCaseSource(nameof(GetBadStringCollections))]
    public void Ctor_ItemsIsNull_ThrowsArgumentException(IEnumerable<string> items)
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => new StringItemExtractor(null!, false));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("items"));
    }

    [Test]
    [TestCaseSource(nameof(GetBadStringCollections))]
    public void Ctor_BadItems_ThrowsArgumentException(IEnumerable<string> items)
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentException>(() => new StringItemExtractor(items, false));

        // Assert
        Assert.That(ex, Has.Message.StartWith("'items' cannot be empty or contain empty items."));
        Assert.That(ex.ParamName, Is.EqualTo("items"));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(StringItemExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new StringItemExtractor(
            testDto.TestItems,
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
        string? errorMessage = null;
        if (result.ErrorCode.HasValue)
        {
            errorMessage = extractor.GetErrorMessage(result.ErrorCode.Value);
        }

        // Assert
        Assert.That(result.ToDto().ToString(), Is.EqualTo(testDto.ExpectedResult.ToString()));

        if (result.ErrorCode.HasValue)
        {
            // check test dto
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(string)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(string)));
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
    public void Extract_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(StringItemExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new StringItemExtractor(
            testDto.TestItems,
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
            var value = "initial";
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Extract(input, out value));

            Assert.That(value, Is.EqualTo(default(string)));

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
    public void TryParse_SomeArgument_ReturnsExpectedResult(StringItemExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new StringItemExtractor(
            testDto.TestItems,
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
    public void Parse_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(StringItemExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new StringItemExtractor(
            testDto.TestItems,
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

    public static IList<StringItemExtractorTestDto> GetTestDtos()
    {
        var json = typeof(StringItemExtractorTests).Assembly.GetResourceText(
            $".{nameof(StringItemExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<StringItemExtractorTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);
            if (dto.ExpectedValue != null)
            {
                dto.ExpectedValue = TestHelper.TransformTestString(dto.ExpectedValue);
            }
        }

        return dtos;
    }

    public static IList<IEnumerable<string>> GetBadStringCollections()
    {
        return new List<List<string?>>
            {
                new(),
                new(new[] { "one", "" }),
                new(new[] { "one", null }),
            }
            .Select(x => (IEnumerable<string>)x)
            .ToList();
    }

    public static IList<StringItemExtractorTestDto> GetTestDtosWithoutTerminator() =>
        GetTestDtos()
            .Where(x => x.TestTerminatingChars == null)
            .ToList();

    private static bool DemoIsWhiteSpace(ReadOnlySpan<char> input, int pos) => input[pos].IsIn(' ', '\t', '\r', '\n');
}
