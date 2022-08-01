using NUnit.Framework;
using System.Text;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.Tests.TextDataExtractor.EmailAddress;

namespace TauCode.Data.Text.Tests.EmailAddress;

[TestFixture]
public class EmailAddressTests
{
    [Test]
    public void TryExtract_CommentWithIncompleteEmoji_ReturnsIncompleteEmojiError()
    {
        // Arrange
        var sb = new StringBuilder("(abc🐆");
        sb.Length -= 1;
        var input = sb.ToString();

        // Act
        var ex = Assert.Throws<TextDataExtractionException>(() => Text.EmailAddress.Parse(input));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Unexpected end."));
        Assert.That(ex.CharsConsumed, Is.EqualTo(input.Length));
        Assert.That(ex.ErrorCode, Is.EqualTo(TextDataExtractionErrorCodes.UnexpectedEnd));
    }

    [Test]
    public void TryExtract_CommentWithBadEmoji_ReturnsBadEmojiError()
    {
        // Arrange
        var sb = new StringBuilder("(abc🐆)a@m.net");
        sb[5] = 'X';
        var input = sb.ToString();

        // Act
        var ex = Assert.Throws<TextDataExtractionException>(() => Text.EmailAddress.Parse(input));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Non-emoji character."));
        Assert.That(ex.CharsConsumed, Is.EqualTo(5));
        Assert.That(ex.ErrorCode, Is.EqualTo(TextDataExtractionErrorCodes.NonEmojiCharacter));
    }

    [Test]
    public void TryExtract_CommentWithLongerBadEmoji_ReturnsBadEmojiError()
    {
        // Arrange
        var sb = new StringBuilder("(abc🇵🇫)a@m.net");
        sb[7] = 'X';
        var input = sb.ToString();

        // Act
        var ex = Assert.Throws<TextDataExtractionException>(() => Text.EmailAddress.Parse(input));

        Assert.That(ex.Message, Is.EqualTo("Non-emoji character."));
        Assert.That(ex.ErrorCode, Is.EqualTo(TextDataExtractionErrorCodes.NonEmojiCharacter));
        Assert.That(ex.CharsConsumed, Is.EqualTo(7));
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void TryParse_AnyArgument_ReturnsExpectedResult(EmailAddressExtractorTestDto dto)
    {
        // Arrange

        // Act
        var parsed = Text.EmailAddress.TryParse(dto.TestInput, out var hostName);

        // Assert
        if (dto.ExpectedResult.ErrorCode.HasValue)
        {
            // check dto
            Assert.That(dto.ExpectedValue, Is.Null);
            Assert.That(dto.ExpectedValueString, Is.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(parsed, Is.False);
            Assert.That(hostName, Is.EqualTo(default(Text.EmailAddress)));
        }
        else
        {
            // check dto
            Assert.That(dto.ExpectedValue, Is.Not.Null);
            Assert.That(dto.ExpectedValueString, Is.Not.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Null);

            // check test itself
            Assert.That(parsed, Is.True);
            Assert.That(hostName!.ToDto(), Is.EqualTo(dto.ExpectedValue));
            Assert.That(hostName!.ToString(), Is.EqualTo(dto.ExpectedValueString));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void Parse_AnyArgument_ReturnsExpectedResult(EmailAddressExtractorTestDto dto)
    {
        if (dto.Comment.Contains("[Not for Parse]"))
        {
            Assert.Pass("Skipped");
        }

        // Arrange
        var successExpected = dto.ExpectedResult.ErrorCode == null;
        var input = dto.TestInput;

        if (successExpected)
        {
            // Act
            var hostName = Text.EmailAddress.Parse(input);

            // Assert
            // check dto
            Assert.That(dto.ExpectedValue, Is.Not.Null);
            Assert.That(dto.ExpectedValueString, Is.Not.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Null);

            // check test itself
            Assert.That(hostName.ToDto(), Is.EqualTo(dto.ExpectedValue));
            Assert.That(hostName.ToString(), Is.EqualTo(dto.ExpectedValueString));
        }
        else
        {
            // Act
            var ex = Assert.Throws<TextDataExtractionException>(() => Text.EmailAddress.Parse(input));

            // Assert
            // check dto
            Assert.That(dto.ExpectedValue, Is.Null);
            Assert.That(dto.ExpectedValueString, Is.Null);
            Assert.That(dto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(ex.Message, Is.EqualTo(dto.ExpectedErrorMessage));
            Assert.That(ex.ErrorCode, Is.EqualTo(dto.ExpectedResult.ErrorCode));
            Assert.That(ex.CharsConsumed, Is.EqualTo(dto.ExpectedResult.CharsConsumed));
        }
    }

    public static IList<EmailAddressExtractorTestDto> GetTestCases()
    {
        return EmailAddressExtractorTests
            .GetTestDtos()
            .Where(x => !x.Comment.Contains("[Not for Parse]"))
            .ToList();
    }
}
