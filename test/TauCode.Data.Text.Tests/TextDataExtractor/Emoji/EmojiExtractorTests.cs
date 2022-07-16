using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Emoji;

[TestFixture]
public class EmojiExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetSuccessfulTestDtos))]
    public void TryExtract_ValidArgument_ReturnsExpectedResult(EmojiExtractorTestDto testDto)
    {
        // Arrange

        var input = testDto.TestInput;

        var extractor = EmojiExtractor.Instance;

        // Act
        var result = extractor.TryExtract(input, out var value);

        // Assert
        Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedValue.Value.Length));
        Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));

        Assert.That(value.ToDto(), Is.EqualTo(testDto.ExpectedValue));
    }

    [Test]
    [TestCaseSource(nameof(GetCutTestDtos))]
    public void TryExtract_CutEmoji_ReturnsExpectedError(EmojiExtractorTestDto testDto)
    {
        // Arrange

        var input = testDto.TestInput;

        var extractor = EmojiExtractor.Instance;

        // Act
        var result = extractor.TryExtract(input, out var value);
        var errorMessage = extractor.GetErrorMessage(result.ErrorCode ?? throw new Exception());

        // Assert
        Assert.That(value, Is.EqualTo(default(Text.Emoji)));

        Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
        Assert.That(result.ErrorCode, Is.Not.Null);

        Assert.That(result.ErrorCode.Value, Is.EqualTo(testDto.ExpectedResult.ErrorCode));
        Assert.That(errorMessage, Is.EqualTo(testDto.ExpectedErrorMessage));
    }

    [Test]
    [TestCaseSource(nameof(GetCorruptedTestDtos))]
    public void TryExtract_CorruptedEmoji_ReturnsExpectedError(EmojiExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        var extractor = EmojiExtractor.Instance;

        // Act
        var result = extractor.TryExtract(input, out var value);
        var errorMessage = extractor.GetErrorMessage(result.ErrorCode ?? throw new Exception());

        // Assert
        Assert.That(value, Is.EqualTo(default(Text.Emoji)));

        Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedResult.CharsConsumed));
        Assert.That(result.ErrorCode, Is.Not.Null);

        Assert.That(result.ErrorCode.Value, Is.EqualTo(testDto.ExpectedResult.ErrorCode));
        Assert.That(errorMessage, Is.EqualTo(testDto.ExpectedErrorMessage));
    }

    public static IList<EmojiExtractorTestDto> GetSuccessfulTestDtos()
    {
        var list = Text.Emoji
            .EnumerateAll()
            .Select(x => new EmojiExtractorTestDto
            {
                TestInput = x.Value + "abc",

                ExpectedResult = new TextDataExtractionResultDto
                {
                    CharsConsumed = x.Value.Length,
                    ErrorCode = null,
                },

                ExpectedValue = x.ToDto(),
                ExpectedErrorMessage = null,
            })
            .ToList();

        return list;
    }

    public static IList<EmojiExtractorTestDto> GetCutTestDtos()
    {
        var list = Text.Emoji
            .EnumerateAll()
            .Where(x => x.Value.Length > 1 && !x.HasContained())
            .Select(x => new EmojiExtractorTestDto
            {
                TestInput = x.Value[..^1],

                ExpectedResult = new TextDataExtractionResultDto
                {
                    CharsConsumed = x.Value.Length - 1,
                    ErrorCode = TextDataExtractionErrorCodes.UnexpectedEnd,
                },

                ExpectedValue = null,
                ExpectedErrorMessage = "Unexpected end.",

                Comment = x.Value,
            })
            .GroupBy(x => x.TestInput)
            .Select(x => x.First())
            .ToList();

        return list;
    }

    public static IList<EmojiExtractorTestDto> GetCorruptedTestDtos()
    {
        var list = Text.Emoji
            .EnumerateAll()
            .Where(x => x.Value.Length > 1 && !x.HasContained())
            .Select(x => new EmojiExtractorTestDto
            {
                TestInput = x.Value[..^1] + "abc",

                ExpectedResult = new TextDataExtractionResultDto
                {
                    CharsConsumed = x.Value.Length - 1,
                    ErrorCode = TextDataExtractionErrorCodes.NonEmojiCharacter,
                },

                ExpectedValue = null,
                ExpectedErrorMessage = "Non-emoji character.",

                Comment = x.Value,
            })
            .GroupBy(x => x.TestInput)
            .Select(x => x.First())
            .ToList();

        return list;
    }
}
