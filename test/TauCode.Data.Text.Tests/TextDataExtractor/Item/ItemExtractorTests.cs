using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.Item;

[TestFixture]
public class ItemExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(ItemExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null
                ? (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                : null;

        var extractor = new ItemExtractor<NumberText>(
            testDto.TestItems.ToHashSet(),
            testDto.TestIgnoreCase,
            x => NumberText.Parse(x, testDto.TestIgnoreCase),
            terminator: terminatingPredicate);

        // Act
        var result = extractor.TryExtract(input, out var value);
        string errorMessage = null;
        if (result.ErrorCode.HasValue)
        {
            errorMessage = extractor.GetErrorMessage(result.ErrorCode.Value);
        }

        // Assert
        Assert.That(result.ToDto(), Is.EqualTo(testDto.ExpectedResult));

        if (result.ErrorCode.HasValue)
        {
            // check test dto
            Assert.That(testDto.ExpectedValue, Is.EqualTo(default(NumberText)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(default(NumberText)));
            Assert.That(errorMessage, Is.EqualTo(testDto.ExpectedErrorMessage));
        }
        else
        {
            // check test dto
            Assert.That(testDto.ExpectedErrorMessage, Is.Null);
            Assert.That(testDto.ExpectedResult.ErrorCode, Is.Null);

            // check test itself
            Assert.That(value, Is.EqualTo(testDto.ExpectedValue));
        }
    }

    public static IList<ItemExtractorTestDto> GetTestDtos()
    {
        var json = typeof(ItemExtractorTests).Assembly.GetResourceText(
            $".{nameof(ItemExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<ItemExtractorTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);
        }

        return dtos;
    }
}
