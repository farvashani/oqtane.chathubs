"..\..\oqtane.framework\oqtane.package\nuget.exe" pack Oqtane.ChatHubs.nuspec 
XCOPY "*.nupkg" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\" /Y
