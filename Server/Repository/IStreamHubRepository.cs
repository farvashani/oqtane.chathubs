using System.Collections.Generic;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Repository
{
    public interface IStreamHubRepository
    {
        IEnumerable<StreamHub> GetBlogs(int ModuleId);
        StreamHub GetBlog(int BlogId);
        StreamHub AddBlog(StreamHub Blog);
        StreamHub UpdateBlog(StreamHub Blog);
        void DeleteBlog(int BlogId);
    }
}
