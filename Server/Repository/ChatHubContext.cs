using Microsoft.EntityFrameworkCore;
using Oqtane.Modules;
using Microsoft.AspNetCore.Http;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Repository
{
    public class ChatHubContext : DBContextBase, IService
    {

        public virtual DbSet<ChatHubRoom> ChatHubRoom { get; set; }
        public virtual DbSet<ChatHubRoomChatHubUser> ChatHubRoomChatHubUser { get; set; }
        public virtual DbSet<ChatHubUser> ChatHubUser { get; set; }
        public virtual DbSet<ChatHubMessage> ChatHubMessage { get; set; }
        public virtual DbSet<ChatHubConnection> ChatHubConnection { get; set; }
        public virtual DbSet<ChatHubPhoto> ChatHubPhoto { get; set; }
        public virtual DbSet<ChatHubSetting> ChatHubSetting { get; set; }
        public virtual DbSet<ChatHubIgnore> ChatHubIgnore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasDiscriminator<string>("UserType").HasValue<User>("User").HasValue<ChatHubUser>("ChatHubUser");

            // Relations
            // Many-to-many
            // ChatHubRoom / ChatHubUser
            modelBuilder.Entity<ChatHubRoomChatHubUser>()
                .HasKey(t => new { t.ChatHubRoomId, t.ChatHubUserId });

            modelBuilder.Entity<ChatHubRoomChatHubUser>()
                .HasOne(room_user => room_user.Room)
                .WithMany(room => room.RoomUsers)
                .HasForeignKey(room_user => room_user.ChatHubRoomId);

            modelBuilder.Entity<ChatHubRoomChatHubUser>()
                .HasOne(room_user => room_user.User)
                .WithMany(user => user.UserRooms)
                .HasForeignKey(room_user => room_user.ChatHubUserId);

            // Relation
            // One-to-many
            // ChatHubConnection / ChatHubUser
            modelBuilder.Entity<ChatHubConnection>()
                .HasOne(c => c.User)
                .WithMany(u => u.Connections)
                .HasForeignKey(c => c.ChatHubUserId);

            // Relation
            // One-to-many
            // ChatHubMessage / ChatHubPhotos
            modelBuilder.Entity<ChatHubPhoto>()
                .HasOne(p => p.Message)
                .WithMany(m => m.Photos)
                .HasForeignKey(p => p.ChatHubMessageId);

            // Relation
            // One-to-many
            // ChatHubUser / ChatHubIgnore
            modelBuilder.Entity<ChatHubIgnore>()
                .HasOne(i => i.User)
                .WithMany(u => u.Ignores)
                .HasForeignKey(i => i.ChatHubUserId);

            // Relation
            // One-to-one
            // ChatHubSetting / ChatHubUser
            modelBuilder.Entity<ChatHubSetting>()
                .HasOne(s => s.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<ChatHubSetting>(s => s.ChatHubUserId);

        }

        public ChatHubContext(ITenantResolver tenantResolver, IHttpContextAccessor accessor) : base(tenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }

    }
}
