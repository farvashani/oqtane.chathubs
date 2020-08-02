using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared.Enums;
using Blazor.FileReader;
using System.Collections.Generic;
using System.IO;
using Oqtane.Shared.Models;
using Oqtane.Shared;

namespace Oqtane.ChatHubs
{
    public class EditBase : ModuleBase
    {

        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public SiteState SiteState { get; set; }
        [Inject]
        public IFileReaderService fileReaderService { get; set; }

        public ChatHubService ChatHubService { get; set; }

        public override SecurityAccessLevel SecurityAccessLevel { get { return SecurityAccessLevel.Edit; } }
        public override string Actions { get { return "Add,Edit"; } }

        public int roomId = -1;
        public string title;
        public string content;
        public string imageUrl;
        public string createdby;
        public DateTime createdon;
        public string modifiedby;
        public DateTime modifiedon;

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                this.ChatHubService = new ChatHubService(HttpClient, SiteState, NavigationManager, JsRuntime, ModuleState.ModuleId);
                this.ChatHubService.UpdateUI += UpdateUIStateHasChanged;

                await this.InitContextRoomAsync();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        private async Task InitContextRoomAsync()
        {
            try
            {
                this.ChatHubService = new ChatHubService(HttpClient, SiteState, NavigationManager, JsRuntime, ModuleState.ModuleId);
                this.ChatHubService.UpdateUI += UpdateUIStateHasChanged;

                if (PageState.QueryString.ContainsKey("roomid"))
                {
                    this.roomId = Int32.Parse(PageState.QueryString["roomid"]);
                    ChatHubRoom room = await this.ChatHubService.GetChatHubRoomAsync(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        title = room.Title;
                        content = room.Content;
                        imageUrl = room.ImageUrl;
                        createdby = room.CreatedBy;
                        createdon = room.CreatedOn;
                        modifiedby = room.ModifiedBy;
                        modifiedon = room.ModifiedOn;
                    }
                    else
                    {
                        await logger.LogError("Error Loading Room {ChatHubRoomId} {Error}", roomId);
                        ModuleInstance.AddModuleMessage("Error Loading ChatHub", MessageType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        public async Task SaveRoom()
        {
            try
            {                
                if (roomId == -1)
                {
                    ChatHubRoom room = new ChatHubRoom()
                    {
                        ModuleId = ModuleState.ModuleId,
                        Title = title,
                        Content = content,
                        Type = Enum.GetName(typeof(ChatHubRoomType), ChatHubRoomType.Public),
                        Status = Enum.GetName(typeof(ChatHubRoomStatus), ChatHubRoomStatus.Active),
                        ImageUrl = string.Empty
                    };

                    room = await this.ChatHubService.AddChatHubRoomAsync(room);
                    await logger.LogInformation("Room Added {ChatHubRoom}", room);
                    NavigationManager.NavigateTo(NavigateUrl());
                }
                else
                {                    
                    ChatHubRoom room = await this.ChatHubService.GetChatHubRoomAsync(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        room.Title = title;
                        room.Content = content;

                        await this.ChatHubService.UpdateChatHubRoomAsync(room);

                        await logger.LogInformation("Room Updated {ChatHubRoom}", room);
                        NavigationManager.NavigateTo(NavigateUrl());
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Saving Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Saving Room", MessageType.Error);
            }
        }

        [Parameter]
        public int BufferSize { get; set; } = 20480;
        public long max;
        public long value;
        public ElementReference inputElement;

        public string Output { get; set; }

        public async Task ClearFile()
        {
            await fileReaderService.CreateReference(inputElement).ClearValue();
            await this.ChatHubService.DeleteRoomImageAsync(this.roomId, ModuleState.ModuleId);
            await this.InitContextRoomAsync();
        }

        public async Task ReadFile()
        {
            UploadFiles(await fileReaderService.CreateReference(inputElement).EnumerateFilesAsync());
        }

        private async void UploadFiles(IEnumerable<IFileReference> files)
        {
            try
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                foreach (var file in files)
                {
                    var fileInfo = await file.ReadFileInfoAsync();
                    var fs = await file.OpenReadAsync();

                    var nl = Environment.NewLine;
                    var bufferSize = 4096;
                    var buffer = new byte[bufferSize];
                    int read;

                    MemoryStream stream = new MemoryStream(100);
                    while ((read = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await stream.WriteAsync(buffer, 0, read);

                        Output += $"Read {read} bytes. {fs.Position} / {fs.Length}{nl}";
                        this.StateHasChanged();
                    }

                    if (stream.Length == stream.Position)
                    {
                        stream.Position = 0;
                    }

                    content.Add(new StreamContent(stream), "file", fileInfo.Name);
                }
                using (var client = new HttpClient())
                {
                    content.Headers.Add("roomid", this.roomId.ToString());

                    var url = string.Concat(NavigationManager.BaseUri.Substring(0, NavigationManager.BaseUri.LastIndexOf('/')), this.ChatHubService.apiurl, "/postroomimageupload");
                    var result = client.PostAsync(url, content).Result;
                    var remotePath = await result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await this.InitContextRoomAsync();
                this.UpdateUIStateHasChanged();
            }
        }

    }
}
