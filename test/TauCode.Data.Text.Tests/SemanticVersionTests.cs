using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using TauCode.Data.Text.Tests.Dto;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

[TestFixture]
public class SemanticVersionTests
{
    #region TryExtract

    [Test]
    [TestCaseSource(nameof(GetTestCasesSuccess))]
    public void TryExtract_ValidInput_ReturnsExpectedResult(SemanticVersionTestDto dto)
    {
        // Arrange
        var testSemanticVersion1 = dto.TestSemanticVersion;
        var testSemanticVersion2 = dto.TestSemanticVersion + " ";

        // Act

        var consumed1 = SemanticVersion.TryExtract(
            testSemanticVersion1,
            out var semanticVersion1,
            out var error1,
            (input, position) => input[position] == ' ');

        var consumed2 = SemanticVersion.TryExtract(
            testSemanticVersion2,
            out var semanticVersion2,
            out var error2,
            (input, position) => input[position] == ' ');

        // Assert
        // check dto
        Assert.That(dto.ExpectedSemanticVersion, Is.Not.Null);
        Assert.That(dto.ExpectedMajor, Is.Not.Null);
        Assert.That(dto.ExpectedMinor, Is.Not.Null);
        Assert.That(dto.ExpectedPatch, Is.Not.Null);
        Assert.That(dto.ExpectedException, Is.Null);

        // assertions themselves
        Assert.That(consumed1, Is.GreaterThan(0));
        Assert.That(error1, Is.Null);

        Assert.That(consumed2, Is.GreaterThan(0));
        Assert.That(error2, Is.Null);

        Assert.That(consumed1, Is.EqualTo(dto.ExpectedResult));
        Assert.That(consumed2, Is.EqualTo(dto.ExpectedResult));

        Assert.That(semanticVersion1.ToString(), Is.EqualTo(dto.ExpectedSemanticVersion));
        Assert.That(semanticVersion1.Major, Is.EqualTo(dto.ExpectedMajor));
        Assert.That(semanticVersion1.Minor, Is.EqualTo(dto.ExpectedMinor));
        Assert.That(semanticVersion1.Patch, Is.EqualTo(dto.ExpectedPatch));
        Assert.That(semanticVersion1.PreRelease, Is.EqualTo(dto.ExpectedPreRelease));
        Assert.That(semanticVersion1.BuildMetadata, Is.EqualTo(dto.ExpectedBuildMetadata));

        Assert.That(semanticVersion2.ToString(), Is.EqualTo(dto.ExpectedSemanticVersion));
        Assert.That(semanticVersion2.Major, Is.EqualTo(dto.ExpectedMajor));
        Assert.That(semanticVersion2.Minor, Is.EqualTo(dto.ExpectedMinor));
        Assert.That(semanticVersion2.Patch, Is.EqualTo(dto.ExpectedPatch));
        Assert.That(semanticVersion2.PreRelease, Is.EqualTo(dto.ExpectedPreRelease));
        Assert.That(semanticVersion2.BuildMetadata, Is.EqualTo(dto.ExpectedBuildMetadata));
    }

    [Test]
    [TestCaseSource(nameof(GetTestCasesFail))]
    public void TryExtract_InvalidInput_ReturnsExpectedError(SemanticVersionTestDto dto)
    {
        // Arrange
        var testSemanticVersion1 = dto.TestSemanticVersion;
        var testSemanticVersion2 = dto.TestSemanticVersion + " ";

        // Act
        var consumed1 = SemanticVersion.TryExtract(
            testSemanticVersion1,
            out var semanticVersion1,
            out var error1,
            (input, position) => input[position] == ' ');

        var consumed2 = SemanticVersion.TryExtract(
            testSemanticVersion2,
            out var semanticVersion2,
            out var error2,
            (input, position) => input[position] == ' ');

        // Assert
        // check dto
        Assert.That(dto.ExpectedSemanticVersion, Is.Null);
        Assert.That(dto.ExpectedResult, Is.Zero);
        Assert.That(dto.ExpectedMajor, Is.Null);
        Assert.That(dto.ExpectedMinor, Is.Null);
        Assert.That(dto.ExpectedPatch, Is.Null);
        Assert.That(dto.ExpectedPreRelease, Is.Null);
        Assert.That(dto.ExpectedBuildMetadata, Is.Null);
        Assert.That(dto.ExpectedException, Is.Not.Null);

        // assertions themselves
        Assert.That(consumed1, Is.Zero);
        Assert.That(error1, Is.Not.Null);

        Assert.That(consumed2, Is.Zero);
        Assert.That(error2, Is.Not.Null);

        Assert.That(semanticVersion1, Is.Null);
        Assert.That(error1.Message, Is.EqualTo(dto.ExpectedException.Message));
        Assert.That(error1.Index, Is.EqualTo(dto.ExpectedException.Index));

        Assert.That(semanticVersion2, Is.Null);
        Assert.That(error2.Message, Is.EqualTo(dto.ExpectedException.Message));
        Assert.That(error2.Index, Is.EqualTo(dto.ExpectedException.Index));
    }

    #endregion

    #region operator <

    [Test]
    public void OperatorLess_VersionsAreSequential_ReturnsTrue()
    {
        // Arrange
        var v1 = SemanticVersion.Parse("1.0.0-alpha");
        var v2 = SemanticVersion.Parse("1.0.0-alpha.1");
        var v3 = SemanticVersion.Parse("1.0.0-alpha.beta");
        var v4 = SemanticVersion.Parse("1.0.0-beta");
        var v5 = SemanticVersion.Parse("1.0.0-beta.2");
        var v6 = SemanticVersion.Parse("1.0.0-beta.11");
        var v7 = SemanticVersion.Parse("1.0.0-rc.1");
        var v8 = SemanticVersion.Parse("1.0.0");

        // Act
        var statement =
            v1 < v2 &&
            v2 < v3 &&
            v3 < v4 &&
            v4 < v5 &&
            v5 < v6 &&
            v6 < v7 &&
            v7 < v8 &&
            true;

        // Assert
        Assert.That(statement, Is.True);
    }

    #endregion

    public static IList<SemanticVersionTestDto> GetTestCasesSuccess()
    {
        return GetTestCases(".SemanticVersionTests.Success.json");
    }

    public static IList<SemanticVersionTestDto> GetTestCasesFail()
    {
        return GetTestCases(".SemanticVersionTests.Fail.json");
    }

    private static IList<SemanticVersionTestDto> GetTestCases(string resourceName)
    {
        var json = typeof(SemanticVersionTests).Assembly.GetResourceText(resourceName, true);
        var testCases = JsonConvert.DeserializeObject<IList<SemanticVersionTestDto>>(json);

        foreach (var testCase in testCases)
        {
            testCase.TestSemanticVersion = TestHelper.TransformTestString(testCase.TestSemanticVersion);
            if (testCase.ExpectedSemanticVersion != null)
            {
                testCase.ExpectedSemanticVersion = TestHelper.TransformTestString(testCase.ExpectedSemanticVersion);
            }
        }

        return testCases;
    }
}
