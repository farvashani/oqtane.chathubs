using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.StreamHubs.Models;
using Oqtane.StreamHubs.Repository;

namespace Oqtane.StreamHubs.Manager
{
    public class StreamHubManager : IInstallable, IPortable
    {
        private IStreamHubRepository _Blogs;
        private ISqlRepository _sql;

        public StreamHubManager(IStreamHubRepository Blogs, ISqlRepository sql)
        {
            _Blogs = Blogs;
            _sql = sql;
        }

        public bool Install(Tenant tenant, string version)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "Oqtane.StreamHubs." + version + ".sql");
        }

        public bool Uninstall(Tenant tenant)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "Oqtane.StreamHubs.Uninstall.sql");
        }

        public string ExportModule(Module module)
        {
            string content = "";
            List<StreamHub> Blogs = _Blogs.GetBlogs(module.ModuleId).ToList();
            if (Blogs != null)
            {
                content = JsonSerializer.Serialize(Blogs);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            List<StreamHub> Blogs = null;
            if (!string.IsNullOrEmpty(content))
            {
                Blogs = JsonSerializer.Deserialize<List<StreamHub>>(content);
            }
            if (Blogs != null)
            {
                foreach(StreamHub Blog in Blogs)
                {
                    StreamHub _Blog = new StreamHub();
                    _Blog.ModuleId = module.ModuleId;
                    _Blog.Title = Blog.Title;
                    _Blog.Content = Blog.Content;
                    _Blogs.AddBlog(_Blog);
                }
            }
        }
    }
}