dotnet new maui
dotnet restore ComicReader.sln
dotnet build ComicReader.sln --no-restore -c Release
dotnet publish ComicReader/ComicReader.csproj --configuration "Release" --framework "net9.0-android" --output "bin/Release/net9.0-android/publish"