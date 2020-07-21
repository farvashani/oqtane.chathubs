XCOPY "..\Client\bin\Debug\netstandard2.1\Oqtane.StreamHubs.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Client\bin\Debug\netstandard2.1\Oqtane.StreamHubs.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\Oqtane.StreamHubs.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\Oqtane.StreamHubs.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\Oqtane.StreamHubs.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\Oqtane.StreamHubs.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\wwwroot\Modules\Oqtane.ChatHubs\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\Oqtane.ChatHubs\" /Y /S /I
