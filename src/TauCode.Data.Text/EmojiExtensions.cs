using TauCode.Data.Text.EmojiSupport;

namespace TauCode.Data.Text;

public static class EmojiExtensions
{
    public static IReadOnlyList<Emoji> GetContainers(this Emoji emoji)
    {
        var descriptor = EmojiHelper.EmojiDescriptors[emoji.Value];
        return descriptor.Containers.Select(x => x.Emoji).ToList();
    }

    public static IReadOnlyList<Emoji> GetContained(this Emoji emoji)
    {
        var descriptor = EmojiHelper.EmojiDescriptors[emoji.Value];
        return descriptor.Contained.Select(x => x.Emoji).ToList();
    }

    public static bool HasContainers(this Emoji emoji)
    {
        var descriptor = EmojiHelper.EmojiDescriptors[emoji.Value];
        return descriptor.Containers.Count > 0;
    }

    public static bool HasContained(this Emoji emoji)
    {
        var descriptor = EmojiHelper.EmojiDescriptors[emoji.Value];
        return descriptor.Contained.Count > 0;
    }
}