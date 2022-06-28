using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TauCode.Data.Text.Tests.Dto;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

[TestFixture]
public class HostNameTests
{
    #region TryParse

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void TryParse_AnyArgument_ReturnsExpectedResult(HostNameTestDto dto)
    {
        // Arrange

        // Act
        var parsed = HostName.TryParse(dto.TestHostName, out var hostName, out var exception);

        // Assert
        var expectedParsed = dto.ExpectedResult > 0;

        Assert.That(parsed, Is.EqualTo(expectedParsed));

        if (parsed)
        {
            // check dto
            Assert.That(dto.ExpectedHostName, Is.Not.Null);
            Assert.That(dto.ExpectedHostNameKind, Is.Not.Null);
            Assert.That(dto.ExpectedException, Is.Null);


            // check result
            Assert.That(hostName.ToString(), Is.EqualTo(dto.ExpectedHostName));
            Assert.That(hostName.Kind, Is.EqualTo(dto.ExpectedHostNameKind));
            Assert.That(exception, Is.Null);
        }
        else
        {
            // check dto
            Assert.That(dto.ExpectedHostName, Is.Null);
            Assert.That(dto.ExpectedHostNameKind, Is.Null);
            Assert.That(dto.ExpectedException, Is.Not.Null);


            // check result
            Assert.That(hostName, Is.EqualTo(default(HostName)));

            Assert.That(exception.Message, Is.EqualTo(dto.ExpectedException.Message));
            Assert.That(exception.Index, Is.EqualTo(dto.ExpectedException.Index));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetInvalidIPv6TestCases))]
    public void TryParse_InputIsInvalid_ReturnsExpectedResult(HostNameTestDto dto)
    {
        // Arrange

        // Act
        var parsed = HostName.TryParse(dto.TestHostName, out var hostName, out var exception);

        // Assert

        // check dto
        Assert.That(dto.ExpectedResult, Is.Zero);
        Assert.That(dto.ExpectedHostName, Is.Null);
        Assert.That(dto.ExpectedHostNameKind, Is.Null);
        Assert.That(dto.ExpectedException, Is.Not.Null);

        // check result
        Assert.That(parsed, Is.False);
        Assert.That(hostName, Is.EqualTo(default(HostName)));

        Assert.That(exception.Message, Is.EqualTo(dto.ExpectedException.Message));
        Assert.That(exception.Index, Is.EqualTo(dto.ExpectedException.Index));
    }

    [Test]
    [TestCaseSource(nameof(GetValidIPv6TestCases))]
    public void TryParse_ValidIPv6_ReturnsExpectedResult(HostNameTestDto dto)
    {
        // Arrange

        // Act
        var parsed = HostName.TryParse(dto.TestHostName, out var hostName, out var exception);

        // Assert

        // check dto
        Assert.That(dto.ExpectedResult, Is.Not.Zero);
        Assert.That(dto.ExpectedHostName, Is.Not.Null);
        Assert.That(dto.ExpectedHostNameKind, Is.Not.Null);
        Assert.That(dto.ExpectedException, Is.Null);

        // check result
        Assert.That(parsed, Is.True);
        Assert.That(hostName.Value, Is.EqualTo(dto.ExpectedHostName));
        Assert.That(hostName.Kind, Is.EqualTo(dto.ExpectedHostNameKind));

        Assert.That(exception, Is.Null);
    }

    #endregion

    #region TryExtract

    [Test]
    [TestCaseSource(nameof(GetTestCases))]
    public void TryExtract_AnyArgument_ReturnsExpectedResult(HostNameTestDto dto)
    {
        // Arrange
        var input = dto.TestHostName + ";";

        // Act
        var consumed = HostName.TryExtract(
            input,
            out var hostName,
            out var exception,
            (span, position) => span[position] == ';');

        // Assert

        Assert.That(consumed, Is.EqualTo(dto.ExpectedResult));

        if (consumed > 0)
        {
            // check dto
            Assert.That(dto.ExpectedHostName, Is.Not.Null);
            Assert.That(dto.ExpectedHostNameKind, Is.Not.Null);
            Assert.That(dto.ExpectedException, Is.Null);


            // check result
            Assert.That(hostName.ToString(), Is.EqualTo(dto.ExpectedHostName));
            Assert.That(hostName.Kind, Is.EqualTo(dto.ExpectedHostNameKind));
            Assert.That(exception, Is.Null);
        }
        else
        {
            // check dto
            Assert.That(dto.ExpectedHostName, Is.Null);
            Assert.That(dto.ExpectedHostNameKind, Is.Null);
            Assert.That(dto.ExpectedException, Is.Not.Null);


            // check result
            Assert.That(hostName, Is.EqualTo(default(HostName)));

            Assert.That(exception.Message, Is.EqualTo(dto.ExpectedException.Message));
            Assert.That(exception.Index, Is.EqualTo(dto.ExpectedException.Index));
        }
    }

    [Test]
    [TestCaseSource(nameof(GetValidIPv6TestCases))]
    public void TryExtract_ValidIPv6_ReturnsExpectedResult(HostNameTestDto dto)
    {
        // Arrange
        var input = dto.TestHostName + ";";

        // Act
        var consumed = HostName.TryExtract(
            input,
            out var hostName,
            out var exception,
            (span, position) => span[position] == ';');

        // Assert

        // check dto
        Assert.That(dto.ExpectedResult, Is.Not.Zero);
        Assert.That(dto.ExpectedHostName, Is.Not.Null);
        Assert.That(dto.ExpectedHostNameKind, Is.Not.Null);
        Assert.That(dto.ExpectedException, Is.Null);

        // check result
        Assert.That(consumed, Is.EqualTo(dto.ExpectedResult));
        Assert.That(hostName.Value, Is.EqualTo(dto.ExpectedHostName));
        Assert.That(hostName.Kind, Is.EqualTo(dto.ExpectedHostNameKind));

        Assert.That(exception, Is.Null);
    }

    #endregion

    public static IList<HostNameTestDto> GetTestCases()
    {
        var json = typeof(HostNameTests).Assembly.GetResourceText(".HostNameTests.json", true);
        var testCases = JsonConvert.DeserializeObject<IList<HostNameTestDto>>(json);

        foreach (var dto in testCases)
        {
            dto.TestHostName = TestHelper.TransformTestString(dto.TestHostName);
            if (dto.ExpectedHostName != null)
            {
                dto.ExpectedHostName = TestHelper.TransformTestString(dto.ExpectedHostName);
            }
        }

        return testCases;
    }

    public static IList<HostNameTestDto> GetInvalidIPv6TestCases()
    {
        var json = typeof(HostNameTests).Assembly.GetResourceText(@".HostNameTests.IPv6.Fail.json", true);

        var list = JsonConvert.DeserializeObject<IList<HostNameTestDto>>(json);

        return list;
    }

    public static IList<HostNameTestDto> GetValidIPv6TestCases()
    {
        var json = typeof(HostNameTests).Assembly.GetResourceText(@".HostNameTests.IPv6.Success.json", true);

        var list = JsonConvert.DeserializeObject<IList<HostNameTestDto>>(json);

        return list;
    }

    /// <summary>
    /// This method isn't used anymore, and is kept for demonstration how Perl unit test scripts were parsed.
    /// See misc/IPv6-invalid.pl, misc/IPv6-valid.pl
    /// </summary>
    /// <param name="pl">Perl script to transform to list of <see cref="HostNameTestDto"/></param>
    /// <returns>List of test DTO-s</returns>
    /// <exception cref="Exception">Throws if something went wrong</exception>
    private static IList<HostNameTestDto> ParsePerlTestCases(string pl)
    {
        var pattern = @"ipv6test\((?<flag>[!1]*),""(?<ipv6>[^""]*)""(?<second_arg>\,""[^""]*"")?\)(?<comment>.*)";


        var lines = pl
            .Split("\r\n")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !x.StartsWith("#"))
            .ToList();

        var dtos = new List<HostNameTestDto>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            var match = Regex.Match(line, pattern);

            if (!match.Success)
            {
                throw new Exception("Bad input");
            }

            var flag = match.Groups["flag"].Value;
            var ipv6 = match.Groups["ipv6"].Value;
            var comment = match.Groups["comment"].Value.Trim();

            if (comment.StartsWith(";"))
            {
                comment = comment[1..].Trim();
            }

            if (comment.StartsWith("#"))
            {
                comment = comment[1..].Trim();
            }

            var secondArg = match.Groups["second_arg"].Value;

            var secondIpv6 = ipv6;

            if (!string.IsNullOrEmpty(secondArg))
            {
                var secondIpv6Match = Regex.Match(secondArg, @"\,""(?<second_ipv6>[^""]*)""");
                if (!secondIpv6Match.Success)
                {
                    throw new Exception("Bad input");
                }

                secondIpv6 = secondIpv6Match.Groups["second_ipv6"].Value;
            }

            int expectedResult;
            ExceptionDto exceptionDto;
            string expectedHostName;
            HostNameKind? expectedHostNameKind;

            switch (flag)
            {
                case "1":
                    expectedResult = ipv6.Length;
                    expectedHostName = secondIpv6.ToLowerInvariant();
                    expectedHostNameKind = HostNameKind.IPv6;
                    exceptionDto = null;
                    break;

                case "!1":
                    expectedResult = 0;
                    expectedHostName = null;
                    expectedHostNameKind = null;
                    exceptionDto = new ExceptionDto
                    {
                        Message = "<replace-with-real-message>",
                        Index = 1599,
                    };
                    break;

                default:
                    throw new Exception("Bad input");
            }

            var dto = new HostNameTestDto
            {
                Index = i,
                TestHostName = ipv6,
                ExpectedResult = expectedResult,
                ExpectedHostName = expectedHostName,
                ExpectedHostNameKind = expectedHostNameKind,
                Comment = comment,
                ExpectedException = exceptionDto,
            };

            dtos.Add(dto);
        }

        return dtos;
    }
}