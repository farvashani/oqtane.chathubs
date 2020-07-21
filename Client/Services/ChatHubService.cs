using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Services
{
    public class ChatHubService : ServiceBase, IChatHubService, IService
    {
        private readonly SiteState _siteState;

        public ChatHubService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

         private string Apiurl=> CreateApiUrl(_siteState.Alias, "ChatHub");

        public async Task<List<ChatHub>> GetBlogsAsync(int ModuleId)
        {
            List<ChatHub> Blogs = await GetJsonAsync<List<ChatHub>>($"{Apiurl}?moduleid={ModuleId}");
            return Blogs.OrderBy(item => item.Title).ToList();
        }

        public async Task<ChatHub> GetBlogAsync(int BlogId)
        {
            return await GetJsonAsync<ChatHub>($"{Apiurl}/{BlogId}");
        }

        public async Task<ChatHub> AddBlogAsync(ChatHub Blog)
        {
            return await PostJsonAsync<ChatHub>($"{Apiurl}?entityid={Blog.ModuleId}", Blog);
        }

        public async Task<ChatHub> UpdateBlogAsync(ChatHub Blog)
        {
            return await PutJsonAsync<ChatHub>($"{Apiurl}/{Blog.ChatHubId}?entityid={Blog.ModuleId}", Blog);
        }

        public async Task DeleteBlogAsync(int BlogId)
        {
            await DeleteAsync($"{Apiurl}/{BlogId}");
        }
    }
}
