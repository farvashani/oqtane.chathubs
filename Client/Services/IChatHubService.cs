using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Services
{
    public interface IChatHubService 
    {
        Task<List<ChatHub>> GetBlogsAsync(int ModuleId);

        Task<ChatHub> GetBlogAsync(int BlogId);

        Task<ChatHub> AddBlogAsync(ChatHub Blog);

        Task<ChatHub> UpdateBlogAsync(ChatHub Blog);

        Task DeleteBlogAsync(int BlogId);
    }
}
