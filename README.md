# Oqtane ChatHub Module

The Oqtane Framework allows developers to create external modules.

# Getting Started For Module Dev

- [x] Get familiar with the Oqtane Framework [GitHub](https://github.com/oqtane/oqtane.framework).
- [x] Clone the Oqtane Github Repository in Visual Studio Team Explorer.
- [x] Clone the Oqtane Github Repository in Visual Studio Team Explorer.
- [x] Create a new branch of the Oqtane 1.0.1 release and check it out.
- [x] Clone the Oqtane ChatHub Module in VS Team Explorer and build in debug and release mode.
- [ ] get it work somehow good luck

Edit startup.cs:
```C#
services.AddFileReaderService();

services.AddSignalR()
                .AddHubOptions<ChatHub>(options =>
                {
                    options.EnableDetailedErrors = true;
                    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
                })
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = false;
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });
				
				endpoints.MapHub<ChatHub>("/chathub", options =>
                {
                    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                });
```
			
Edit TenantResolver.cs (workarround for signalr hub http context multiple tenant resolving):
```C#
string[] segments = accessor.HttpContext.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
if (segments.Length > 1 && (segments[1] == "api" || segments[1] == "pages") && segments[0] != "~")
{
	aliasId = int.Parse(segments[0]);
}
else if (segments[0] == "chathub")
{
	aliasId = 1;
}
```

# Example Screenshots

Here's a little peek of the ChatHub module:

![Module](https://github.com/boredanyway/oqtane.chathubs/blob/master/screenshot1.png?raw=true "Module")

![Module](https://github.com/boredanyway/oqtane.chathubs/blob/master/screenshot2.png?raw=true "Module")

# Donation Button

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DZVSWXB4L2GWA)
