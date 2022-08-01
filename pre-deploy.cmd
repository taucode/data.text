dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\test\TauCode.Data.Text.Tests\TauCode.Data.Text.Tests.csproj
dotnet test -c Release .\test\TauCode.Data.Text.Tests\TauCode.Data.Text.Tests.csproj

nuget pack nuget\TauCode.Data.Text.nuspec