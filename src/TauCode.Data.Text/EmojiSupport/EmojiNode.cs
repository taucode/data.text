using System;
using System.Collections.Generic;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text.EmojiSupport
{
    // todo: clean, regions, rearrange
    internal class EmojiNode
    {
        private Emoji? _emoji;
        private readonly Dictionary<char, EmojiNode> _followers;

        internal EmojiNode(char? c)
        {
            this.Char = c;
            _followers = new Dictionary<char, EmojiNode>();
        }

        internal char? Char { get; }

        internal Emoji? Emoji
        {
            get => _emoji;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (_emoji != null)
                {
                    throw new InvalidOperationException("Emoji already set.");
                }

                _emoji = value;
            }
        }

        internal IReadOnlyDictionary<char, EmojiNode> Followers => _followers;

        internal void AddEmoji(Emoji emoji)
        {
            this.CheckIsRoot();
            this.AddEmojiTail(emoji, 0);
        }

        internal int TryExtract(ReadOnlySpan<char> input, out Emoji? emoji, out TextDataExtractionException exception)
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applicable to the root node.");
            }

            if (input.Length == 0)
            {
                emoji = null;
                exception = Helper.CreateException(ExtractionErrorTag.EmptyInput, null);
                return 0;
            }

            Emoji? lastEmoji = null;
            int? lastSuccessfulOffset = null;

            var offset = 0;
            var current = this;

            while (true)
            {
                if (offset == input.Length)
                {
                    if (lastEmoji == null)
                    {
                        emoji = null;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, offset);
                        return 0;
                    }
                    else
                    {
                        emoji = lastEmoji.Value;
                        exception = null;
                        return lastSuccessfulOffset.Value + 1;
                    }
                }

                var c = input[offset];

                var follower = current._followers.GetValueOrDefault(c);

                if (follower == null)
                {
                    if (current.Emoji.HasValue)
                    {
                        emoji = current.Emoji.Value;
                        exception = null;
                        return offset; // todo: mistake? should be 'offset + 1'?
                    }
                    else
                    {
                        if (lastEmoji == null)
                        {
                            emoji = null;
                            exception = Helper.CreateException(ExtractionErrorTag.NonEmojiChar, offset); // todo: sure?
                            return 0;
                        }
                        else
                        {
                            emoji = lastEmoji.Value;
                            exception = null;
                            return lastSuccessfulOffset.Value + 1;
                        }
                    }
                }
                else
                {
                    if (follower.Emoji.HasValue)
                    {
                        lastEmoji = follower.Emoji.Value;
                        lastSuccessfulOffset = offset;
                    }

                    offset++;

                    if (offset == input.Length)
                    {
                        if (lastEmoji == null)
                        {
                            emoji = null;
                            exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, offset);
                            return 0;
                        }
                        else
                        {
                            emoji = lastEmoji.Value;
                            exception = null;
                            return lastSuccessfulOffset.Value + 1;
                        }
                    }

                    current = follower;
                }
            }
        }

        private void AddEmojiTail(Emoji emoji, int offset)
        {
            var c = emoji.Value[offset];
            var follower = this.GetOrCreateFollower(c);

            if (offset == emoji.Value.Length - 1)
            {
                follower.Emoji = emoji;
            }
            else
            {
                follower.AddEmojiTail(emoji, offset + 1);
            }
        }

        private EmojiNode GetOrCreateFollower(char c)
        {
            var follower = _followers.GetValueOrDefault(c);
            if (follower == null)
            {
                follower = new EmojiNode(c);
                _followers.Add(c, follower);
            }

            return follower;
        }

        internal bool HasPath(ReadOnlySpan<char> path, int length)
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applicable to the root node.");
            }

            var cleanPath = path[..Math.Min(path.Length, length)];

            return this.HasPathInternal(cleanPath);
        }

        private bool HasPathInternal(ReadOnlySpan<char> cleanPath)
        {
            if (cleanPath.Length == 0)
            {
                return true;
            }

            var follower = _followers.GetValueOrDefault(cleanPath[0]);
            if (follower == null)
            {
                return false;
            }

            return follower.HasPathInternal(cleanPath[1..]);
        }

        internal int Skip(ReadOnlySpan<char> input, out ExtractionErrorTag? errorTag)
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applicable to the root node.");
            }

            if (input.Length == 0)
            {
                errorTag = ExtractionErrorTag.EmptyInput;
                return 0;
            }

            Emoji? lastEmoji = null;
            var lastSuccessfulOffset = 0;

            var offset = 0;
            var current = this;

            while (true)
            {
                if (offset == input.Length)
                {
                    if (lastEmoji == null)
                    {
                        errorTag = ExtractionErrorTag.IncompleteEmoji;
                        return offset;
                    }
                    else
                    {
                        errorTag = null;
                        return lastSuccessfulOffset + 1;
                    }
                }

                var c = input[offset];

                var follower = current._followers.GetValueOrDefault(c);

                if (follower == null)
                {
                    if (current.Emoji.HasValue)
                    {
                        errorTag = null;
                        return offset;
                    }
                    else
                    {
                        if (lastEmoji == null)
                        {
                            errorTag = ExtractionErrorTag.NonEmojiChar;
                            return offset;
                        }
                        else
                        {
                            errorTag = null;
                            return lastSuccessfulOffset + 1;
                        }
                    }
                }
                else
                {
                    if (follower.Emoji.HasValue)
                    {
                        lastEmoji = follower.Emoji.Value;
                        lastSuccessfulOffset = offset;
                    }

                    offset++;
                    current = follower;
                }
            }
        }

        private void CheckIsRoot()
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applies to the root node."); // should never happen
            }
        }
    }
}
