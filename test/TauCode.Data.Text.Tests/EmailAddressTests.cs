using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.Tests.Dto;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

[TestFixture]
public class EmailAddressTests
{
    [Test]
    [TestCaseSource(nameof(GetTestCasesSuccess))]
    public void TryExtract_ValidInput_ReturnsExpectedResult(EmailAddressTestDto dto)
    {
        // Arrange
        var testEmailAddress1 = dto.TestEmailAddress;
        var testEmailAddress2 = dto.TestEmailAddress + " ";

        // Act
        var consumed1 = EmailAddress.TryExtract(
            testEmailAddress1,
            out var emailAddress1,
            out var error1,
            (input, position) => input[position] == ' ');

        var consumed2 = EmailAddress.TryExtract(
            testEmailAddress2,
            out var emailAddress2,
            out var error2,
            (input, position) => input[position] == ' ');

        // Assert
        Assert.That(consumed1, Is.Not.Zero);
        Assert.That(error1, Is.Null);

        Assert.That(consumed2, Is.Not.Zero);
        Assert.That(error2, Is.Null);

        Assert.That(consumed1, Is.EqualTo(dto.ExpectedResult));
        Assert.That(consumed2, Is.EqualTo(dto.ExpectedResult));

        Assert.That(emailAddress1.ToString(), Is.EqualTo(dto.ExpectedEmailAddress));
        Assert.That(emailAddress2.ToString(), Is.EqualTo(dto.ExpectedEmailAddress));
    }

    [Test]
    [TestCaseSource(nameof(GetTestCasesFailForParsing))]
    public void Parse_InvalidInput_ThrowsExpectedException(EmailAddressTestDto dto)
    {
        // Arrange
        var testEmailAddress = dto.TestEmailAddress;

        // Act
        EmailAddress emailAddress = null;

        var ex = Assert.Throws<TextDataExtractionException>(() =>
        {
            emailAddress = EmailAddress.Parse(testEmailAddress);
        });

        // Assert
        Assert.That(emailAddress, Is.Null);
        Assert.That(ex, Is.Not.Null);

        Assert.That(ex.Message, Is.EqualTo(dto.ExpectedException.Message));
        Assert.That(ex.Index, Is.EqualTo(dto.ExpectedException.Index));
    }

    [Test]
    public void TryExtract_CommentWithIncompleteEmoji_ReturnsIncompleteEmojiError()
    {
        // Arrange
        var sb = new StringBuilder("(abc🐆");
        sb.Length -= 1;
        var input = sb.ToString();

        // Act
        var consumed = EmailAddress.TryExtract(input, out var emailAddress, out var exception);

        // Assert
        Assert.That(consumed, Is.Zero);
        Assert.That(emailAddress, Is.Null);

        Assert.That(exception.Message, Is.EqualTo("Incomplete emoji."));
        Assert.That(exception.Index, Is.EqualTo(4));
    }

    [Test]
    public void TryExtract_CommentWithBadEmoji_ReturnsBadEmojiError()
    {
        // Arrange
        var sb = new StringBuilder("(abc🐆)a@m.net");
        sb[5] = 'X';
        var input = sb.ToString();

        // Act
        var consumed = EmailAddress.TryExtract(input, out var emailAddress, out var exception);

        // Assert
        Assert.That(consumed, Is.Zero);
        Assert.That(emailAddress, Is.Null);

        Assert.That(exception.Message, Is.EqualTo("Bad emoji."));
        Assert.That(exception.Index, Is.EqualTo(4));
    }

    [Test]
    public void TryExtract_CommentWithLongerBadEmoji_ReturnsBadEmojiError()
    {
        // Arrange
        var sb = new StringBuilder("(abc🇵🇫)a@m.net");
        sb[7] = 'X';
        var input = sb.ToString();

        // Act
        var consumed = EmailAddress.TryExtract(input, out var emailAddress, out var exception);

        // Assert
        Assert.That(consumed, Is.Zero);
        Assert.That(emailAddress, Is.Null);

        Assert.That(exception.Message, Is.EqualTo("Bad emoji."));
        Assert.That(exception.Index, Is.EqualTo(4));
    }

    public static IList<EmailAddressTestDto> GetTestCasesSuccess()
    {
        return GetTestCases(".EmailAddressTests.Success.json");
    }

    public static IList<EmailAddressTestDto> GetTestCasesFailForParsing()
    {
        return GetTestCases(".EmailAddressTests.Fail.ForParsing.json");
    }

    private static IList<EmailAddressTestDto> GetTestCases(string resourceName)
    {
        var json = typeof(EmailAddressTests).Assembly.GetResourceText(resourceName, true);
        var testCases = JsonConvert.DeserializeObject<IList<EmailAddressTestDto>>(json);

        foreach (var testCase in testCases)
        {
            testCase.TestEmailAddress = testCase.TestEmailAddress.Replace('␀', '\0');

            if (testCase.ExpectedEmailAddress != null)
            {
                testCase.ExpectedEmailAddress = testCase.ExpectedEmailAddress.Replace('␀', '\0');
            }

            testCase.TestEmailAddress = TestHelper.TransformTestString(testCase.TestEmailAddress);
            if (testCase.ExpectedEmailAddress != null)
            {
                testCase.ExpectedEmailAddress = TestHelper.TransformTestString(testCase.ExpectedEmailAddress);
            }
        }

        return testCases;
    }
}