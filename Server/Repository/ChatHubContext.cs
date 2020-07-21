using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Repository
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
