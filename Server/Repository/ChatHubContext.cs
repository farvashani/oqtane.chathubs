using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.ChatHubs.Models;

namespace Oqtane.ChatHubs.Repository
{
    public class ChatHubContext : DBContextBase, IService
    {
        public virtual DbSet<ChatHub> ChatHub { get; set; }

        public ChatHubContext(ITenantResolver tenantResolver, IHttpContextAccessor accessor) : base(tenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}
