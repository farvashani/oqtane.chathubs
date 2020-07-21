using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using Oqtane.ChatHubs.Models;

namespace Oqtane.ChatHubs.Repository
{
    public class ChatHubRepository : IChatHubRepository, IService
    {
        private readonly ChatHubContext _db;

        public ChatHubRepository(ChatHubContext context)
        {
            _db = context;
        }

        public IEnumerable<ChatHub> GetBlogs(int ModuleId)
        {
            return _db.ChatHub.Where(item => item.ModuleId == ModuleId);
        }

        public ChatHub GetBlog(int BlogId)
        {
            return _db.ChatHub.Find(BlogId);
        }

        public ChatHub AddBlog(ChatHub Blog)
        {
            _db.ChatHub.Add(Blog);
            _db.SaveChanges();
            return Blog;
        }

        public ChatHub UpdateBlog(ChatHub Blog)
        {
            _db.Entry(Blog).State = EntityState.Modified;
            _db.SaveChanges();
            return Blog;
        }

        public void DeleteBlog(int BlogId)
        {
            ChatHub Blog = _db.ChatHub.Find(BlogId);
            _db.ChatHub.Remove(Blog);
            _db.SaveChanges();
        }
    }
}
