﻿using Newtonsoft.Json;
using NUnit.Framework;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.EmailAddress;

[TestFixture]
public class EmailAddressExtractorTests
{
    [Test]
    public void Ctor_NoArguments_CreatesExtractor()
    {
        // Arrange

        // Act
        var extractor = new EmailAddressExtractor();

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(1000));
        Assert.That(extractor.Terminator, Is.Null);
    }

    [Test]
    public void Ctor_ValidArgument_CreatesExtractor()
    {
        // Arrange
        TerminatingDelegate method = DemoIsWhiteSpace;

        // Act
        var extractor = new EmailAddressExtractor(method);

        // Assert
        Assert.That(extractor.MaxConsumption, Is.EqualTo(1000));
        Assert.That(extractor.Terminator, Is.SameAs(method));
    }

    [Test]
    public void MaxConsumption_ValidValue_SetsMaxConsumption()
    {
        // Arrange
        var extractor = new EmailAddressExtractor();

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
        var extractor = new EmailAddressExtractor();

        // Act
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => extractor.MaxConsumption = badMaxConsumption);

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("maxConsumption"));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(EmailAddressExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EmailAddressExtractor(terminator);
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
            Assert.That(value!.ToDto(), Is.EqualTo(testDto.ExpectedValue));
            Assert.That(value!.ToString(), Is.EqualTo(testDto.ExpectedValueString));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void Extract_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(EmailAddressExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EmailAddressExtractor(terminator);
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
            var value = Text.EmailAddress.Parse("me@example.com");
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Extract(input, out value));

            Assert.That(value, Is.EqualTo(default(Text.EmailAddress)));

            Assert.That(ex.Message, Is.EqualTo(testDto.ExpectedErrorMessage));
            Assert.That(ex.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(ex.ErrorCode, Is.EqualTo(testDto.ExpectedResult.ErrorCode));
        }
        else
        {
            // success
            var consumed = extractor.Extract(input, out var value);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
            Assert.That(value!.ToDto(), Is.EqualTo(testDto.ExpectedValue));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtosWithoutTerminator))]
    public void TryParse_SomeArgument_ReturnsExpectedResult(EmailAddressExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EmailAddressExtractor(terminator);
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
            Assert.That(value!.ToDto(), Is.EqualTo(testDto.ExpectedValue));
        }
        else
        {
            Assert.That(testDto.ExpectedValue, Is.Null);
        }

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtosWithoutTerminator))]
    public void Parse_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(EmailAddressExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate? terminator =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new EmailAddressExtractor(terminator);
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
            Assert.That(value!.ToDto(), Is.EqualTo(testDto.ExpectedValue));
        }

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
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

    public static IList<EmailAddressExtractorTestDto> GetTestDtosWithoutTerminator() =>
        GetTestDtos()
            .Where(x => x.TestTerminatingChars == null)
            .ToList();

    private static bool DemoIsWhiteSpace(ReadOnlySpan<char> input, int pos) => input[pos].IsIn(' ', '\t', '\r', '\n');
}
