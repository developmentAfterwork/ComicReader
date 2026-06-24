dotnet new maui
dotnet restore ComicReader.sln
dotnet publish ComicReader/ComicReader.csproj -c Release -f net10.0-android --no-restore --output "bin/Release/net10.0-android/publish"