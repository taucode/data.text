namespace TauCode.Data.Text.EmailAddressSupport
{
    internal enum SegmentType : byte
    {
        Period = 1,
        Comment,

        LocalPartSpace,
        LocalPartFoldingWhiteSpace,
        LocalPartWord,
        LocalPartQuotedString,

        At, // '@' symbol

        DomainLabel, // part of sub-domain. e.g. in 'mail.google.com' labels are: 'mail', 'google', 'com'.
        DomainIPAddress,

        Termination,
    }
}
