using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class BrowserResizeService : ServiceBase, IService
    {
        private readonly IJSRuntime _jsRuntime;

        public static event Func<Task> OnResize;

        public BrowserResizeService(HttpClient http, IJSRuntime jsRuntime) : base(http)
        {
            _jsRuntime = jsRuntime;
        }

        [JSInvokable("OnBrowserResize")]
        public static async Task OnBrowserResize()
        {
            await OnResize?.Invoke();
        }

        public async Task<int> GetInnerHeight()
        {
            return await _jsRuntime.InvokeAsync<int>("browserResize.getInnerHeight");
        }

        public async Task<int> GetInnerWidth()
        {
            return await _jsRuntime.InvokeAsync<int>("browserResize.getInnerWidth");
        }
    }
}