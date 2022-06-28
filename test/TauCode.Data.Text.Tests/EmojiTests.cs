using NUnit.Framework;
using System.Linq;
using System.Text;

namespace TauCode.Data.Text.Tests
{
    [TestFixture]
    public class EmojiTests
    {
        /// <summary>
        /// 'Master' means an emoji that contains other emoji as starting substring
        /// </summary>
        [Test]
        public void TryExtract_IncompleteMasterEmoji_ExtractsPartially()
        {
            // Arrange
            var walesFlagEmoji = Emoji.EnumerateAll().Single(x => x.Name == "flag: Wales");
            var sb = new StringBuilder(walesFlagEmoji.Value);
            sb[^1] = 'X';
            var input = sb.ToString();

            var blackFlagEmoji = Emoji.EnumerateAll().Single(x => x.Name == "black flag");

            // Act
            var consumed = Emoji.TryExtract(input, out var emoji, out var exception);

            // Assert
            Assert.That(consumed, Is.Not.Zero);
            Assert.That(consumed, Is.EqualTo(blackFlagEmoji.Value.Length));

            Assert.That(emoji, Is.EqualTo(blackFlagEmoji));
            Assert.That(exception, Is.Null);
        }

        [Test]
        public void TryExtract_NonEmojiInput_ReturnsNull()
        {
            // Arrange
            var input = "abc";

            // Act
            var consumed = Emoji.TryExtract(input, out var emoji, out var exception);

            // Assert
            Assert.That(consumed, Is.Zero);
            Assert.That(emoji, Is.Null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Index, Is.EqualTo(0));
            Assert.That(exception.Message, Is.EqualTo("Non-emoji character."));
        }
    }
}
