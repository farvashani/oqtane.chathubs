using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Modules;
using System;
using System.Threading.Tasks;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Repository
{
    public class ChatHubRepository : IChatHubRepository, IService
    {

        private readonly ChatHubContext db;

        public ChatHubRepository(ChatHubContext context)
        {
            db = context;
        }

        #region GET

        public IQueryable<ChatHubRoom> GetChatHubRooms(int ModuleId)
        {
            try
            {
                return db.ChatHubRoom.Where(item => item.ModuleId == ModuleId);
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubRoom> GetChatHubRoomsByUser(ChatHubUser user)
        {
            return db.Entry(user)
                      .Collection(u => u.UserRooms)
                      .Query().Select(u => u.Room);            
        }
        public IQueryable<ChatHubUser> GetChatHubUsersByRoom(ChatHubRoom room)
        {
            return db.Entry(room)
                      .Collection(r => r.RoomUsers)
                      .Query().Select(r => r.User);
        }
        public ChatHubRoom GetChatHubRoom(int ChatHubRoomId)
        {
            try
            {
                return db.ChatHubRoom.Where(item => item.ChatHubRoomId == ChatHubRoomId).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoom GetChatHubRoomOneVsOne(string OneVsOneId)
        {
            try
            {
                return db.ChatHubRoom.FirstOrDefault(item => item.OneVsOneId == OneVsOneId);
            }
            catch
            {
                throw;
            }
        }
        public IEnumerable<ChatHubMessage> GetChatHubMessages(int ChatHubRoomId)
        {
            try
            {
                return db.ChatHubMessage.Where(item => item.ChatHubRoomId == ChatHubRoomId);
            }
            catch
            {
                throw;
            }
        }
        public ChatHubMessage GetChatHubMessage(int ChatHubMessageId)
        {
            try
            {
                return db.ChatHubMessage.Where(item => item.ChatHubMessageId == ChatHubMessageId).Include(item => item.Photos).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
        public IQueryable<ChatHubUser> GetOnlineUsers()
        {
            return db.ChatHubUser.Include(u => u.Connections).Where(u => u.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)));
        }
        public IQueryable<ChatHubUser> GetOnlineUsers(ChatHubRoom room)
        {
            IQueryable<ChatHubUser> users = db.Entry(room).Collection(r => r.RoomUsers).Query().Include(ru => ru.User).ThenInclude(u => u.Connections).Select(ru => ru.User).Where(u => u.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)));
            return users;
        }
        public IQueryable<ChatHubConnection> GetConnectionsByUserId(int userId)
        {
            ChatHubUser user = db.ChatHubUser.Where(u => u.UserId == userId).Include(u => u.Connections).FirstOrDefault();

            return db.Entry(user)
                      .Collection(u => u.Connections)
                      .Query().Select(c => c);
        }
        public async Task<ChatHubConnection> GetConnectionByConnectionId(string connectionId)
        {
            return await db.ChatHubConnection
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
        }
        public ChatHubRoomChatHubUser GetChatHubRoomChatHubUser(int chatHubRoomId, int chatHubUserId)
        {
            return db.ChatHubRoomChatHubUser
                    .Where(item => item.ChatHubRoomId == chatHubRoomId)
                    .Where(item => item.ChatHubUserId == chatHubUserId)
                    .FirstOrDefault();
        }
        public IQueryable<ChatHubIgnore> GetIgnoredUsers(ChatHubUser user)
        {
            return db.Entry(user)
                      .Collection(u => u.Ignores)
                      .Query().Select(i => i);
        }
        public IQueryable<ChatHubUser> GetIgnoredApplicationUsers(ChatHubUser user)
        {
            IQueryable<int> chatIgnoreIdList = db.Entry(user)
                      .Collection(u => u.Ignores)
                      .Query()
                      .Where(i => (i.ModifiedOn.AddDays(7)) >= DateTime.Now)
                      .Select(i => i.ChatHubIgnoredUserId);

            return db.ChatHubUser.Where(u => chatIgnoreIdList.Contains(u.UserId)).Include(u => u.Connections);
        }
        public IQueryable<ChatHubUser> GetIgnoredByApplicationUsers(ChatHubUser user)
        {
            IQueryable<int> chatIgnoredByIdList = db.ChatHubIgnore
                      .Include(i => i.User)
                      .Where(i => i.ChatHubIgnoredUserId == user.UserId && (i.ModifiedOn.AddDays(7)) >= DateTime.Now)
                      .Select(i => i.ChatHubUserId);

            return db.ChatHubUser.Where(u => chatIgnoredByIdList.Contains(u.UserId)).Include(u => u.Connections);
        }
        public IQueryable<ChatHubIgnore> GetIgnoredByUsers(ChatHubUser user)
        {
            return db.ChatHubIgnore.Include(i => i.User).Select(i => i).Where(i => i.ChatHubIgnoreId == user.UserId && (i.ModifiedOn.AddDays(7)) >= DateTime.Now);
        }
        public ChatHubSetting GetChatHubSetting(int ChatHubUserId)
        {
            try
            {
                return db.ChatHubSetting.Include(item => item.User).Where(item => item.ChatHubUserId == ChatHubUserId).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
        public async Task<ChatHubUser> GetUserByIdAsync(int id)
        {
            var item = await db.ChatHubUser
                            .Include(u => u.Connections)
                            .Include(u => u.Settings)
                            .Include(u => u.UserRooms)
                            .Where(u => u.UserId == id)
                            .Select(u => new
                            {
                                User = u,
                                Connections = u.Connections.OrderByDescending(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)).Take(100),
                            })
                            .FirstOrDefaultAsync();

            if (item != null)
            {
                ChatHubUser user = item.User;
                user.Connections = item.User.Connections;

                return user;
            }

            return null;
        }
        public async Task<ChatHubUser> GetUserByUserNameAsync(string username)
        {
            var item = await db.ChatHubUser
                            .Include(u => u.Connections)
                            .Where(u => u.Username == username)
                            .Select(u => new
                            {
                                User = u,
                                Connections = u.Connections.OrderByDescending(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)).Take(100),
                            })
                            .FirstOrDefaultAsync();

            if (item != null)
            {
                ChatHubUser user = item.User;
                user.Connections = item.User.Connections;

                return user;
            }

            return null;
        }
        public async Task<ChatHubUser> GetUserByDisplayName(string displayName)
        {
            ChatHubUser user = await db.ChatHubUser.Include(u => u.Connections).Where(u => u.DisplayName == displayName).Where(u => u.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).FirstOrDefaultAsync();
            return user;
        }

        #endregion

        #region ADD

        public ChatHubRoom AddChatHubRoom(ChatHubRoom ChatHubRoom)
        {
            try
            {
                db.ChatHubRoom.Add(ChatHubRoom);
                db.SaveChanges();
                return ChatHubRoom;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubMessage AddChatHubMessage(ChatHubMessage ChatHubMessage)
        {
            try
            {
                db.ChatHubMessage.Add(ChatHubMessage);
                db.SaveChanges();
                return ChatHubMessage;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubConnection AddChatHubConnection(ChatHubConnection ChatHubConnection)
        {
            try
            {
                db.ChatHubConnection.Add(ChatHubConnection);
                db.SaveChanges();
                return ChatHubConnection;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubUser AddChatHubUser(ChatHubUser ChatHubUser)
        {
            try
            {
                db.ChatHubUser.Add(ChatHubUser);
                db.SaveChanges();
                return ChatHubUser;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubRoomChatHubUser AddChatHubRoomChatHubUser(ChatHubRoomChatHubUser ChatHubRoomChatHubUser)
        {
            try
            {
                var item = this.GetChatHubRoomChatHubUser(ChatHubRoomChatHubUser.ChatHubRoomId, ChatHubRoomChatHubUser.ChatHubUserId);
                if (item == null)
                {
                    db.ChatHubRoomChatHubUser.Add(ChatHubRoomChatHubUser);
                    db.SaveChanges();
                    return ChatHubRoomChatHubUser;
                }

                return item;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubPhoto AddChatHubPhoto(ChatHubPhoto ChatHubPhoto)
        {
            try
            {
                db.ChatHubMessage.Attach(ChatHubPhoto.Message);
                db.Entry(ChatHubPhoto.Message).State = EntityState.Modified;

                db.ChatHubPhoto.Add(ChatHubPhoto);
                db.SaveChanges();
                return ChatHubPhoto;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubIgnore AddChatHubIgnore(ChatHubIgnore chatHubIgnore)
        {
            try
            {
                db.ChatHubUser.Attach(chatHubIgnore.User);
                db.Entry(chatHubIgnore.User).State = EntityState.Modified;

                db.ChatHubIgnore.Add(chatHubIgnore);
                db.SaveChanges();
                return chatHubIgnore;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubSetting AddChatHubSetting(ChatHubSetting ChatHubSetting)
        {
            try
            {
                db.ChatHubSetting.Add(ChatHubSetting);
                db.SaveChanges();
                return ChatHubSetting;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region DELETE

        public void DeleteChatHubRoom(int ChatHubRoomId, int ModuleId)
        {
            try
            {
                ChatHubRoom ChatHubRoom = db.ChatHubRoom.Where(item => item.ChatHubRoomId == ChatHubRoomId)
                    .Where(item => item.ModuleId == ModuleId).FirstOrDefault();
                db.ChatHubRoom.Remove(ChatHubRoom);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubMessage(int ChatHubMessageId, int ChatHubRoomId)
        {
            try
            {
                ChatHubMessage ChatHubMessage = db.ChatHubMessage.Where(item => item.ChatHubMessageId == ChatHubMessageId)
                    .Where(item => item.ChatHubRoomId == ChatHubRoomId).FirstOrDefault();
                db.ChatHubMessage.Remove(ChatHubMessage);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubConnection(int ChatHubConnectionId, int ChatHubUserId)
        {
            try
            {
                ChatHubConnection ChatHubConnection = db.ChatHubConnection.Where(item => item.ChatHubConnectionId == ChatHubConnectionId)
                    .Where(item => item.ChatHubUserId == ChatHubUserId).FirstOrDefault();
                db.ChatHubConnection.Remove(ChatHubConnection);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubRoomChatHubUser(int ChatHubRoomId, int ChatHubUserId)
        {
            try
            {
                ChatHubRoomChatHubUser item = this.GetChatHubRoomChatHubUser(ChatHubRoomId, ChatHubUserId);

                if (item != null)
                {
                    db.ChatHubRoomChatHubUser.Remove(item);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
        public void DeleteChatHubIgnore(ChatHubIgnore chatHubIgnore)
        {
            try
            {
                db.ChatHubIgnore.Remove(chatHubIgnore);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region UPDATE

        public ChatHubRoom UpdateChatHubRoom(ChatHubRoom ChatHubRoom)
        {
            try
            {
                db.Entry(ChatHubRoom).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubRoom;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubMessage UpdateChatHubMessage(ChatHubMessage ChatHubMessage)
        {
            try
            {
                db.Entry(ChatHubMessage).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubMessage;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubConnection UpdateChatHubConnection(ChatHubConnection ChatHubConnection)
        {
            try
            {
                db.Entry(ChatHubConnection).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubConnection;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubIgnore UpdateChatHubIgnore(ChatHubIgnore chatHubIgnore)
        {
            try
            {
                db.Entry(chatHubIgnore).State = EntityState.Modified;
                db.SaveChanges();
                return chatHubIgnore;
            }
            catch
            {
                throw;
            }
        }
        public ChatHubSetting UpdateChatHubSetting(ChatHubSetting ChatHubSetting)
        {
            try
            {
                db.Entry(ChatHubSetting).State = EntityState.Modified;
                db.SaveChanges();
                return ChatHubSetting;
            }
            catch
            {
                throw;
            }
        }

        #endregion

    }
}
 