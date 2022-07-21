namespace TauCode.Data.Text
{
    public static class TextDataExtractionErrorCodes
    {
        // Common: 0
        public const int InputIsTooLong = 2;
        public const int UnclosedString = 3;
        public const int NewLineInString = 4;
        public const int UnexpectedEnd = 5;
        public const int BadEscape = 6;
        public const int UnexpectedCharacter = 7;
        public const int InternalError = 8;

        // Emoji: 100
        public const int NonEmojiCharacter = 102;

        // EmailAddress: 200
        public const int LocalPartIsTooLong = 201;
        public const int EmailAddressIsTooLong = 202;
        public const int IPv4MustBeEnclosedInBrackets = 203;
        public const int EmptyLocalPart = 204;
        public const int EmptyString = 205;
        public const int InvalidIPv6Prefix = 207;

        // HostName: 300
        public const int InvalidDomain = 301;
        public const int HostNameIsTooLong = 302;
        public const int InvalidHostName = 303;
        public const int DomainLabelIsTooLong = 304;
        public const int InvalidIPv4Address = 305;
        public const int InvalidIPv6Address = 306;

        // int: 400
        public const int FailedToExtractInt32 = 401;

        // long: 500
        public const int FailedToExtractInt64 = 501;

        // decimal: 600
        public const int FailedToExtractDecimal = 601;

        // double: 700
        public const int FailedToExtractDouble = 701;

        // FilePath: 800
        public const int FailedToExtractFilePath = 801;
        public const int FilePathIsTooLong = 802;

        // Key: 900
        public const int FailedToExtractKey = 901;

        // Term: 1000
        public const int FailedToExtractTerm = 1001;

        // JsonString: 1100
        public const int FailedToExtractJsonString = 1101;

        // DateTimeOffset: 1200
        public const int FailedToExtractDateTimeOffset = 1201;

        // Uri: 1300
        public const int FailedToExtractUri = 1301;
        public const int UriIsTooLong = 1302;

        // Item: 1400
        public const int FailedToExtractItem = 1401;

        // TimeSpan: 1500
        public const int FailedToExtractTimeSpan = 1501;

        // SemanticVersion: 1600
        public const int FailedToExtractSemanticVersion = 1601;

        // StringItem: 1700
        public const int ItemNotFound = 1701;

        // Identifier: 1800
        public const int ValueIsReservedWord = 1801;
    }
}
