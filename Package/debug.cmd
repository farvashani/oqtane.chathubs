XCOPY "..\Client\bin\Debug\netstandard2.1\Oqtane.ChatHubs.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Client\bin\Debug\netstandard2.1\Oqtane.ChatHubs.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\Oqtane.ChatHubs.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\bin\Debug\netcoreapp3.1\Oqtane.ChatHubs.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\Oqtane.ChatHubs.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Shared\bin\Debug\netstandard2.1\Oqtane.ChatHubs.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\BlazorAlerts\bin\Debug\netstandard2.1\BlazorAlerts.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\BlazorAlerts\bin\Debug\netstandard2.1\BlazorAlerts.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\netcoreapp3.1\" /Y
XCOPY "..\Server\wwwroot\Modules\Oqtane.ChatHubs\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\Oqtane.ChatHubs\" /Y /S /I