using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.StreamHubs.Models;

namespace Oqtane.StreamHubs.Repository
{
    public class StreamHubContext : DBContextBase, IService
    {
        public virtual DbSet<StreamHub> StreamHub { get; set; }

        public StreamHubContext(ITenantResolver tenantResolver, IHttpContextAccessor accessor) : base(tenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}
