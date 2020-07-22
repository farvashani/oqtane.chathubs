using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Shared.Models;
using Oqtane.ChatHubs.Repository;

namespace Oqtane.ChatHubs.Manager
{
    public class ChatHubManager : IInstallable, IPortable
    {
        private IChatHubRepository _Blogs;
        private ISqlRepository _sql;

        public ChatHubManager(IChatHubRepository Blogs, ISqlRepository sql)
        {
            _Blogs = Blogs;
            _sql = sql;
        }

        public bool Install(Tenant tenant, string version)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "Oqtane.ChatHubs." + version + ".sql");
        }

        public bool Uninstall(Tenant tenant)
        {
            return _sql.ExecuteScript(tenant, GetType().Assembly, "Oqtane.ChatHubs.Uninstall.sql");
        }

        public string ExportModule(Module module)
        {
            string content = "";
            List<ChatHubRoom> Blogs = _Blogs.GetChatHubRooms(module.ModuleId).ToList();
            if (Blogs != null)
            {
                content = JsonSerializer.Serialize(Blogs);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            List<ChatHubRoom> Blogs = null;
            if (!string.IsNullOrEmpty(content))
            {
                Blogs = JsonSerializer.Deserialize<List<ChatHubRoom>>(content);
            }
            if (Blogs != null)
            {
                foreach(ChatHubRoom Blog in Blogs)
                {
                    ChatHubRoom _Blog = new ChatHubRoom();
                    _Blog.ModuleId = module.ModuleId;
                    _Blog.Title = Blog.Title;
                    _Blog.Content = Blog.Content;
                    _Blogs.AddChatHubRoom(_Blog);
                }
            }
        }
    }
}