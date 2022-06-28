namespace TauCode.Data.Text
{
    internal enum ExtractionErrorTag
    {
        // Common
        EmptyInput = 1000,
        InputTooLong,
        UnexpectedChar,
        UnexpectedEnd,
        UnclosedString, // todo: message for this + ut
        NewLineInString, // todo: message for this + ut
        BadEscape, // todo: message for this + ut

        /// <summary>
        /// This error should never happen
        /// </summary>
        InternalError,

        // HostName
        HostNameTooLong = 2000,
        DomainLabelTooLong,
        InvalidHostName,
        InvalidIPv4Address,
        InvalidIPv6Address,

        // EmailAddress
        EmptyLocalPart = 3000,
        LocalPartTooLong,
        EmailAddressTooLong,
        InvalidDomain,
        UnescapedSpecialCharacter,
        UnclosedQuotedString, // todo: we have already 'UnclosedString', maybe use that one?
        EmptyQuotedString,
        IPv4MustBeEnclosedInBrackets,

        // Emoji
        NonEmojiChar = 4000,
        IncompleteEmoji, // todo ut
        BadEmoji,

        // Semantic version
        InvalidSemanticVersion = 5000,
    }
}
