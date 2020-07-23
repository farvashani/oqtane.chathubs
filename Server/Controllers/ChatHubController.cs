using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Shared;
using Oqtane.Enums;
using System.Threading.Tasks;
using System.Linq;
using Oqtane.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;
using Oqtane.ChatHubs.Repository;
using Oqtane.Infrastructure;
using System.Drawing;
using Oqtane.ChatHubs.Services;

namespace Oqtane.ChatHubs.Controllers
{

    [Route("{site}/api/[controller]/[action]")]
    public class ChatHubController : Controller
    {

        IWebHostEnvironment webHostEnvironment;
        private readonly IHubContext<Hubs.ChatHub> chatHub;
        private readonly UserManager<IdentityUser> identityUserManager;
        private readonly IChatHubRepository chatHubRepository;
        private readonly IChatHubService chatHubService;
        private readonly ILogManager logger;
        private int EntityId = -1; // passed as a querystring parameter for authorization and used for validation

        public ChatHubController(IWebHostEnvironment webHostEnvironment, IHubContext<Hubs.ChatHub> chatHub, UserManager<IdentityUser> identityUserManager, IChatHubRepository chatHubRepository, IChatHubService chatHubService, IHttpContextAccessor httpContextAccessor, ILogManager logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.chatHub = chatHub;
            this.identityUserManager = identityUserManager;
            this.chatHubRepository = chatHubRepository;
            this.chatHubService = chatHubService;
            this.logger = logger;
            if (httpContextAccessor.HttpContext.Request.Query.ContainsKey("entityid"))
            {
                EntityId = int.Parse(httpContextAccessor.HttpContext.Request.Query["entityid"]);
            }
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [ActionName("GetChatHubRooms")]
        [Authorize(Policy = "ViewModule")]
        public async Task<IEnumerable<ChatHubRoom>> GetAsync(int moduleid)
        {
            try
            {
                IList<ChatHubRoom> chatHubRooms = new List<ChatHubRoom>();
                if (moduleid == EntityId)
                {
                    var rooms = chatHubRepository.GetChatHubRooms(moduleid).ToList();
                    if (rooms != null && rooms.Any())
                    {                        
                        foreach(var room in rooms)
                        {
                            var item = await this.chatHubService.CreateChatHubRoomClientModelAsync(room);
                            chatHubRooms.Add(item);
                        }
                    }
                }

                return chatHubRooms;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Error}", ex.Message);
                throw;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [ActionName("GetChatHubRoom")]
        [Authorize(Policy = "ViewModule")]
        public async Task<ChatHubRoom> GetAsync(int id, int moduleid)
        {
            try
            {
                ChatHubRoom chatHubRoomClientModel = null;
                if (moduleid == EntityId)
                {
                    ChatHubRoom chatHubRoom = chatHubRepository.GetChatHubRoom(id);
                    chatHubRoomClientModel = await this.chatHubService.CreateChatHubRoomClientModelAsync(chatHubRoom);
                }
                return chatHubRoomClientModel;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Error}", ex.Message);
                throw;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [ActionName("AddChatHubRoom")]
        [Authorize(Policy = "EditModule")]
        public async Task<ChatHubRoom> PostAsync([FromBody] ChatHubRoom ChatHubRoom)
        {
            try
            {
                if (ModelState.IsValid && ChatHubRoom.ModuleId == EntityId)
                {
                    ChatHubRoom = chatHubRepository.AddChatHubRoom(ChatHubRoom);
                    logger.Log(LogLevel.Information, this, LogFunction.Create, "ChatHubRoom Added {ChatHubRoom}", ChatHubRoom);
                }
                return await this.chatHubService.CreateChatHubRoomClientModelAsync(ChatHubRoom);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Post Error {Error}", ex.Message);
                throw;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [ActionName("UpdateChatHubRoom")]
        [Authorize(Policy = "EditModule")]
        public async Task<ChatHubRoom> PutAsync(int id, [FromBody] ChatHubRoom ChatHubRoom)
        {
            try
            {
                if (ModelState.IsValid && ChatHubRoom.ModuleId == EntityId)
                {
                    ChatHubRoom = chatHubRepository.UpdateChatHubRoom(ChatHubRoom);
                    logger.Log(LogLevel.Information, this, LogFunction.Update, "ChatHubRoom Updated {ChatHubRoom}", ChatHubRoom);
                }
                return await this.chatHubService.CreateChatHubRoomClientModelAsync(ChatHubRoom);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Put Error {Error}", ex.Message);
                throw;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [ActionName("DeleteChatHubRoom")]
        [Authorize(Policy = "EditModule")]
        public void Delete(int id, int moduleid)
        {
            try
            {
                if (moduleid == EntityId)
                {
                    chatHubRepository.DeleteChatHubRoom(id, moduleid);
                    logger.Log(LogLevel.Information, this, LogFunction.Delete, "ChatHubRoom Deleted {ChatHubRoomId}", id);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Delete Error {Error}", ex.Message);
                throw;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        [ActionName("FixCorruptConnections")]
        [Authorize(Policy = "EditModule")]
        public void FixCorruptConnections(int moduleid)
        {
            try
            {
                if (moduleid == EntityId)
                {
                    List<ChatHubUser> onlineUsers = chatHubRepository.GetOnlineUsers().ToList();
                    foreach (var user in onlineUsers)
                    {
                        foreach (ChatHubConnection connection in user.Connections)
                        {
                            connection.Status = (int)ChatHubConnectionStatus.Inactive;
                            chatHubRepository.UpdateChatHubConnection(connection);

                            logger.Log(LogLevel.Information, this, LogFunction.Delete, "ChatHubConnection Deleted {ChatHubConnection}", connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Delete Error {Error}", ex.Message);
                throw;
            }
        }

        // POST api/<controller>/
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostImageUpload()
        {

            string connectionId = null;
            if (Request.Headers.ContainsKey("connectionId"))
            {
                connectionId = Request.Headers["connectionId"];
                if (string.IsNullOrEmpty(connectionId))
                {
                    return new BadRequestObjectResult(new { Message = "No connection id." });
                }
            }

            ChatHubUser user = await this.chatHubService.IdentifyGuest(connectionId);
            if (user == null)
            {
                return new BadRequestObjectResult(new { Message = "No user found." });
            }

            string displayName = string.Empty;
            if (Request.Headers.ContainsKey("displayName"))
            {
                displayName = Request.Headers["displayName"];
                if (string.IsNullOrEmpty(displayName))
                {
                    return new BadRequestObjectResult(new { Message = "No display name." });
                }
            }

            string roomId = string.Empty;
            if (Request.Headers.ContainsKey("roomId"))
            {
                roomId = Request.Headers["roomId"];
                if (string.IsNullOrEmpty(roomId))
                {
                    return new BadRequestObjectResult(new { Message = "No room id." });
                }
            }

            string moduleId = string.Empty;
            if (Request.Headers.ContainsKey("moduleId"))
            {
                moduleId = Request.Headers["moduleId"];
                if (string.IsNullOrEmpty(moduleId))
                {
                    return new BadRequestObjectResult(new { Message = "No module id." });
                }
            }

            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count <= 0)
            {
                return new BadRequestObjectResult(new { Message = "No files." });
            }

            string content = string.Concat(files.Count, " ", "Photo(s)");
            ChatHubRoom chatHubRoom = this.chatHubRepository.GetChatHubRoom(Int32.Parse(roomId));
            if (chatHubRoom == null)
            {
                return new BadRequestObjectResult(new { Message = "No room found." });
            }

            ChatHubMessage chatHubMessage = new ChatHubMessage()
            {
                ChatHubRoomId = chatHubRoom.ChatHubRoomId,
                ChatHubUserId = user.UserId,
                Type = (int)ChatHubMessageType.Image,
                Content = content,
                User = user
            };
            chatHubMessage = this.chatHubRepository.AddChatHubMessage(chatHubMessage);

            try
            {
                var maxFileSize = 10;
                var maxFileCount = 3;

                string folderName = "modules/oqtane.chathubs/chathub/images/selfies";
                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (files.Count > maxFileCount)
                {
                    return new BadRequestObjectResult(new { Message = "Maximum number of files exceeded." });
                }

                foreach (IFormFile file in files)
                {

                    if (file.Length > (maxFileSize * 1024 * 1024))
                    {
                        return new BadRequestObjectResult(new { Message = "File size Should Be UpTo " + maxFileSize + "MB" });
                    }

                    var supportedFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(file.FileName);
                    if (!supportedFileExtensions.Contains(fileExtension))
                    {
                        return new BadRequestObjectResult(new { Message = "Unknown file type(s)." });
                    }

                    /*
                    ObjectResult result = await this.PostAsync(file);
                    dynamic obj = result.Value;
                    string imageClassification = string.Empty;
                    if (result.StatusCode.Value == 200)
                    {
                        if (obj.predictedLabel == "dickpic")
                        {
                            var percent = string.Format("{0:P2}", Math.Round((float)obj.probability, 3));
                            if ((float)obj.probability >= 0.99)
                            {
                                imageClassification = string.Concat(" | ", "(", "most likely identified as ", obj.predictedLabel, ": ", percent, ")");
                            }
                        }
                    }
                    */

                    int imageWidth, imageHeight;

                    string fileName = string.Concat(Guid.NewGuid().ToString(), fileExtension);
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);

                        FileInfo fileInfo = new FileInfo(file.FileName);
                        var sizeInBytes = file.Length;
                        Bitmap img = new Bitmap(stream);
                        imageWidth = img.Width;
                        imageHeight = img.Height;
                    }

                    ChatHubPhoto chatHubPhoto = new ChatHubPhoto()
                    {
                        ChatHubMessageId = chatHubMessage.ChatHubMessageId,
                        Source = fileName,
                        Size = file.Length,
                        Thumb = fileName,
                        Caption = string.Concat(user.DisplayName, " | ", Math.Round(Convert.ToDecimal(file.Length / (1024.0m * 1024.0m)), 2), "MB"/*, imageClassification*/),
                        Message = chatHubMessage,
                        Width = imageWidth,
                        Height = imageHeight
                    };
                    chatHubPhoto = this.chatHubRepository.AddChatHubPhoto(chatHubPhoto);
                }

                chatHubMessage = this.chatHubService.CreateChatHubMessageClientModel(chatHubMessage);
                await this.chatHub.Clients.Group(chatHubMessage.ChatHubRoomId.ToString()).SendAsync("AddMessage", chatHubMessage);
                return new OkObjectResult(new { Message = "Successfully Uploaded Files." });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { Message = "Error Uploading Files." });
            }
            
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostRoomImageUpload()
        {
            string roomId = string.Empty;
            if (Request.Headers.ContainsKey("roomid"))
            {
                roomId = Request.Headers["roomid"];
                if (string.IsNullOrEmpty(roomId))
                {
                    return new BadRequestObjectResult(new { Message = "No room id." });
                }
            }

            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count <= 0)
            {
                return new BadRequestObjectResult(new { Message = "No files." });
            }

            try
            {
                var maxFileSize = 10;
                var maxFileCount = 3;

                string folderName = "modules/oqtane.chathubs/chathub/images/rooms";
                string webRootPath = string.Concat(this.webHostEnvironment.ContentRootPath, "\\wwwroot");
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (files.Count > maxFileCount)
                {
                    return new BadRequestObjectResult(new { Message = "Maximum number of files exceeded." });
                }

                foreach (IFormFile file in files)
                {
                    if (file.Length > (maxFileSize * 1024 * 1024))
                    {
                        return new BadRequestObjectResult(new { Message = "File size Should Be UpTo " + maxFileSize + "MB" });
                    }

                    var supportedFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(file.FileName);
                    if (!supportedFileExtensions.Contains(fileExtension))
                    {
                        return new BadRequestObjectResult(new { Message = "Unknown file type(s)." });
                    }

                    string fileName = string.Concat(Guid.NewGuid().ToString(), fileExtension);
                    string fullPath = Path.Combine(newPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var room = this.chatHubRepository.GetChatHubRoom(Int32.Parse(roomId));
                    if(room != null)
                    {
                        room.ImageUrl = fileName;
                        this.chatHubRepository.UpdateChatHubRoom(room);
                    }
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { Message = "Error Uploading Files." });
            }

            return new OkObjectResult(new { Message = "Successfully Uploaded Files." });
        }

        [HttpDelete("{id}")]
        [ActionName("DeleteRoomImage")]
        [Authorize(Policy = "EditModule")]
        public async Task<IActionResult> DeleteRoomImage(int id, int moduleid)
        {

            try
            {
                string roomId = string.Empty;
                if (Request.Headers.ContainsKey("roomid"))
                {
                    roomId = Request.Headers["roomid"];
                    if (string.IsNullOrEmpty(roomId))
                    {
                        return new BadRequestObjectResult(new { Message = "No room id." });
                    }
                }

                var room = this.chatHubRepository.GetChatHubRoom(id);
                if (room != null)
                {
                    room.ImageUrl = string.Empty;
                    this.chatHubRepository.UpdateChatHubRoom(room);
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { Message = "Error Removing Image." });
            }

            return new OkObjectResult(new { Message = "Successfully Removed Image." });

        }

    }
}
