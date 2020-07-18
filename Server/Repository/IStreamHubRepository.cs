using System.Collections.Generic;
using Oqtane.Blogs.Models;

namespace Oqtane.Blogs.Repository
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
