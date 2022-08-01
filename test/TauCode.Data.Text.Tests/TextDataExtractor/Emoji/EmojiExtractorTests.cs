using NUnit.Framework;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Emoji;

[TestFixture]
public class EmojiExtractorTests
{
    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(2)]
    [TestCase(null)]
    public void MaxConsumption_AnyValue_ThrowsException(int? maxConsumption)
    {
        // Arrange
        var extractor = EmojiExtractor.Instance;

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => extractor.MaxConsumption = maxConsumption);

        // Assert
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    [TestCase(null)]
    [TestCase("not_null")]
    public void Terminator_AnyValue_ThrowsException(string? terminatorTag)
    {
        // Arrange
        var extractor = EmojiExtractor.Instance;
        TerminatingDelegate? terminator;
        if (terminatorTag == null)
        {
            terminator = null;
        }
        else
        {
            terminator = DemoIsWhiteSpace;
        }

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => extractor.Terminator = terminator);

        // Assert
        Assert.That(ex, Is.Not.Null);
    }

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
        Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedValue!.Value.Length));
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

    [Test]
    [TestCaseSource(nameof(GetSuccessfulTestDtos))]
    public void Extract_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(EmojiExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;

        var extractor = EmojiExtractor.Instance;

        // Act
        if (testDto.ExpectedResult.ErrorCode.HasValue)
        {
            // fail
            var value = Text.Emoji.EnumerateAll().First(x => x.Name.Contains("woman"));
            var ex = Assert.Throws<TextDataExtractionException>(() => extractor.Extract(input, out value));

            Assert.That(value, Is.EqualTo(default(Text.Emoji)));

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
    [TestCaseSource(nameof(GetSuccessfulTestDtos))]
    public void TryParse_SomeArgument_ReturnsExpectedResult(EmojiExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;

        var extractor = EmojiExtractor.Instance;

        var terminatorBeforeParse = extractor.Terminator;

        // Act
        var parsed = extractor.TryParse(input, out var value);

        var expectedParsed = testDto.ExpectedResult.ErrorCode == null;

        Assert.That(parsed, Is.EqualTo(expectedParsed));
        Assert.That(value.ToDto(), Is.EqualTo(testDto.ExpectedValue));

        Assert.That(extractor.Terminator, Is.SameAs(terminatorBeforeParse));
    }

    [Test]
    [TestCaseSource(nameof(GetSuccessfulTestDtos))]
    public void Parse_SomeArgument_ReturnsExpectedResultOrThrowsExpectedException(EmojiExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;

        var extractor = EmojiExtractor.Instance;

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

    private static bool DemoIsWhiteSpace(ReadOnlySpan<char> input, int pos) => input[pos].IsIn(' ', '\t', '\r', '\n');
}
