using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.EmojiSupport;

namespace TauCode.Data.Text
{
    public static class EmojiExtensions
    {
        public static IReadOnlyList<Emoji> GetContainers(this Emoji emoji)
        {
            // todo check existence
            var descriptor = EmojiHelper.EmojiDescriptors[emoji.Value];
            return descriptor.Containers.Select(x => x.Emoji).ToList();
        }

        public static IReadOnlyList<Emoji> GetContained(this Emoji emoji)
        {
            // todo check existence
            var descriptor = EmojiHelper.EmojiDescriptors[emoji.Value];
            return descriptor.Contained.Select(x => x.Emoji).ToList();
        }
    }
}
