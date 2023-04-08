dotnet restore

dotnet build TauCode.Data.Text.sln -c Debug
dotnet build TauCode.Data.Text.sln -c Release

dotnet test TauCode.Data.Text.sln -c Debug
dotnet test TauCode.Data.Text.sln -c Release

nuget pack nuget\TauCode.Data.Text.nuspec