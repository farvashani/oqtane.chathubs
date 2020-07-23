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
        private IChatHubRepository _chatHubRepository;
        private ISqlRepository _sql;

        public ChatHubManager(IChatHubRepository chatHubRepository, ISqlRepository sql)
        {
            _chatHubRepository = chatHubRepository;
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
            List<ChatHubRoom> Blogs = _chatHubRepository.GetChatHubRooms(module.ModuleId).ToList();
            if (Blogs != null)
            {
                content = JsonSerializer.Serialize(Blogs);
            }
            return content;
        }

        public void ImportModule(Module module, string content, string version)
        {
            List<ChatHubRoom> chatHubs = null;
            if (!string.IsNullOrEmpty(content))
            {
                chatHubs = JsonSerializer.Deserialize<List<ChatHubRoom>>(content);
            }
            if (chatHubs != null)
            {
                foreach(ChatHubRoom chatHubRoom in chatHubs)
                {
                    ChatHubRoom room = new ChatHubRoom();
                    room.ModuleId = module.ModuleId;
                    room.Title = chatHubRoom.Title;
                    room.Content = chatHubRoom.Content;
                    _chatHubRepository.AddChatHubRoom(room);
                }
            }
        }
    }
}