"..\..\oqtane.framework\oqtane.package\nuget.exe" pack Oqtane.StreamHubs.nuspec 
XCOPY "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\" /Y
