using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Collections;
using System.Text;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

[TestFixture]
public class UtilityFixture
{
    [Test]
    [TestCase("Boolean")]
    [TestCase("CLangString")]
    [TestCase("DateTimeOffset")]
    [TestCase("Decimal")]
    [TestCase("Double")]
    [TestCase("EmailAddress")]
    [TestCase("Emoji")]
    [TestCase("Enum")]
    [TestCase("FilePath")]
    [TestCase("HostName")]
    [TestCase("Identifier")]
    [TestCase("Int32")]
    [TestCase("Int64")]
    [TestCase("Item")]
    [TestCase("JsonString")]
    [TestCase("Key")]
    [TestCase("SemanticVersion")]
    [TestCase("SqlIdentifier")]
    [TestCase("StringItem")]
    [TestCase("Term")]
    [TestCase("TimeSpan")]
    [TestCase("Uri")]
    [TestCase("Word")]
    [Ignore("dev only")]
    public void ReSerializeExtractorUnitTests(string prefix)
    {
        var resourceName = $".{prefix}ExtractorTests.json";
        var fileName = $"{prefix}ExtractorTests.json";
        var dtoTypeName = $"{prefix}ExtractorTestDto";
        var dtoType = this.GetType().Assembly.GetTypes().Single(x => x.Name == dtoTypeName);
        var dtoListType = typeof(IList<>).MakeGenericType(dtoType);
        var json = this.GetType().Assembly.GetResourceText(resourceName, true);

        var list = (IList)JsonConvert.DeserializeObject(json, dtoListType);
        for (var i = 0; i < list.Count; i++)
        {
            var dto = (dynamic)list[i]!;
            dto.Index = i;
        }

        var newJson = JsonConvert.SerializeObject(list, Formatting.Indented, new StringEnumConverter());
        var fullPath = $@"C:\temp\dtos\{fileName}";

        File.WriteAllText(fullPath, newJson, Encoding.UTF8);
    }
}
