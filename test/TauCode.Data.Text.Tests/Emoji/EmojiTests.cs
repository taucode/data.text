using NUnit.Framework;
using System.Linq;
using System.Text;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text.Tests.Emoji;

[TestFixture]
public class EmojiTests
{
    /// <summary>
    /// 'Master' means an emoji that contains other emoji as starting substring
    /// </summary>
    [Test]
    public void Parse_IncompleteMasterEmoji_ThrowsException()
    {
        // Arrange
        var walesFlagEmoji = Text.Emoji.EnumerateAll().Single(x => x.Name == "flag: Wales");
        var sb = new StringBuilder(walesFlagEmoji.Value);
        sb.Length -= 1;
        var input = sb.ToString();

        var blackFlagEmoji = Text.Emoji.EnumerateAll().Single(x => x.Name == "black flag");

        // Act
        var ex = Assert.Throws<TextDataExtractionException>(() => Text.Emoji.Parse(input));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Unexpected character."));
        Assert.That(ex.ErrorCode, Is.EqualTo(TextDataExtractionErrorCodes.UnexpectedCharacter));
        Assert.That(ex.CharsConsumed, Is.EqualTo(blackFlagEmoji.Value.Length));
    }

    [Test]
    public void TryExtract_NonEmojiInput_ReturnsNull()
    {
        // Arrange
        var input = "abc";

        // Act
        var ex = Assert.Throws<TextDataExtractionException>(() => Text.Emoji.Parse(input));

        // Assert
        Assert.That(ex.CharsConsumed, Is.EqualTo(0));
        Assert.That(ex.ErrorCode, Is.EqualTo(TextDataExtractionErrorCodes.NonEmojiCharacter));
        Assert.That(ex.Message, Is.EqualTo("Non-emoji character."));
    }
}
