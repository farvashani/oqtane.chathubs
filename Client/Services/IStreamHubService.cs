using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Services
{
    public interface IStreamHubService 
    {
        Task<List<StreamHub>> GetBlogsAsync(int ModuleId);

        Task<StreamHub> GetBlogAsync(int BlogId);

        Task<StreamHub> AddBlogAsync(StreamHub Blog);

        Task<StreamHub> UpdateBlogAsync(StreamHub Blog);

        Task DeleteBlogAsync(int BlogId);
    }
}
