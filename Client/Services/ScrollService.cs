using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class ScrollService : ServiceBase, IService
    {
        private readonly IJSRuntime _jsRuntime;

        public ScrollService(HttpClient http, IJSRuntime jsRuntime) : base(http)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task ScrollToBottom(string element, int time)
        {
            await _jsRuntime.InvokeAsync<object>("scroll.scrollToBottom", element, time);
        }

    }
}