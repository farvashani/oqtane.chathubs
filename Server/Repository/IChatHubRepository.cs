using System.Collections.Generic;
using Oqtane.ChatHubs.Models;

namespace Oqtane.ChatHubs.Repository
{
    public interface IChatHubRepository
    {
        IEnumerable<ChatHub> GetBlogs(int ModuleId);
        ChatHub GetBlog(int BlogId);
        ChatHub AddBlog(ChatHub Blog);
        ChatHub UpdateBlog(ChatHub Blog);
        void DeleteBlog(int BlogId);
    }
}
