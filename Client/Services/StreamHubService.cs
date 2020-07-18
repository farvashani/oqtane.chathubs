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
    public class StreamHubService : ServiceBase, IStreamHubService, IService
    {
        private readonly SiteState _siteState;

        public StreamHubService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

         private string Apiurl=> CreateApiUrl(_siteState.Alias, "StreamHub");

        public async Task<List<StreamHub>> GetBlogsAsync(int ModuleId)
        {
            List<StreamHub> Blogs = await GetJsonAsync<List<StreamHub>>($"{Apiurl}?moduleid={ModuleId}");
            return Blogs.OrderBy(item => item.Title).ToList();
        }

        public async Task<StreamHub> GetBlogAsync(int BlogId)
        {
            return await GetJsonAsync<StreamHub>($"{Apiurl}/{BlogId}");
        }

        public async Task<StreamHub> AddBlogAsync(StreamHub Blog)
        {
            return await PostJsonAsync<StreamHub>($"{Apiurl}?entityid={Blog.ModuleId}", Blog);
        }

        public async Task<StreamHub> UpdateBlogAsync(StreamHub Blog)
        {
            return await PutJsonAsync<StreamHub>($"{Apiurl}/{Blog.StreamHubId}?entityid={Blog.ModuleId}", Blog);
        }

        public async Task DeleteBlogAsync(int BlogId)
        {
            await DeleteAsync($"{Apiurl}/{BlogId}");
        }
    }
}
