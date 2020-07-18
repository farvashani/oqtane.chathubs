using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Repository
{
    public class StreamHubRepository : IStreamHubRepository, IService
    {
        private readonly StreamHubContext _db;

        public StreamHubRepository(StreamHubContext context)
        {
            _db = context;
        }

        public IEnumerable<StreamHub> GetBlogs(int ModuleId)
        {
            return _db.StreamHub.Where(item => item.ModuleId == ModuleId);
        }

        public StreamHub GetBlog(int BlogId)
        {
            return _db.StreamHub.Find(BlogId);
        }

        public StreamHub AddBlog(StreamHub Blog)
        {
            _db.StreamHub.Add(Blog);
            _db.SaveChanges();
            return Blog;
        }

        public StreamHub UpdateBlog(StreamHub Blog)
        {
            _db.Entry(Blog).State = EntityState.Modified;
            _db.SaveChanges();
            return Blog;
        }

        public void DeleteBlog(int BlogId)
        {
            StreamHub Blog = _db.StreamHub.Find(BlogId);
            _db.StreamHub.Remove(Blog);
            _db.SaveChanges();
        }
    }
}
