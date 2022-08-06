using System.Diagnostics;

namespace TauCode.Data.Text.EmojiSupport;

[DebuggerDisplay("{Emoji.Value}")]
internal class EmojiDescriptor
{
    internal EmojiDescriptor(Emoji emoji)
    {
        this.Emoji = emoji;
        this.Containers = new List<EmojiDescriptor>();
        this.Contained = new List<EmojiDescriptor>();
    }

    internal Emoji Emoji { get; }

    /// <summary>
    /// Emojis that contain this emoji
    /// </summary>
    internal List<EmojiDescriptor> Containers { get; }

    /// <summary>
    /// Emojis that contained in this emoji
    /// </summary>
    internal List<EmojiDescriptor> Contained { get; }
}