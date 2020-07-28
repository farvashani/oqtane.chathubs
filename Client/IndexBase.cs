using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.ChatHubs;
using BlazorStrap;
using System.Text.RegularExpressions;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs
{
    public partial class IndexBase : ModuleBase, IDisposable
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
        public ISettingService SettingService { get; set; }

        public Dictionary<string, string> settings { get; set; }

        public ChatHubService ChatHubService { get; set; }
        public BrowserResizeService BrowserResizeService { get; set; }
        public ScrollService ScrollService { get; set; }

        public string GuestUsername { get; set; } = string.Empty;

        public ChatHubRoom contextRoom { get; set; }

        public List<ChatHubRoom> rooms;
        public int maxUserNameCharacters;

        public int InnerHeight = 0;
        public int InnerWidth = 0;

        [Parameter]
        public string MessageWindowHeight { get; set; }
        [Parameter]
        public string UserlistWindowHeight { get; set; }

        public static string ChatWindowDatePattern = @"HH:mm:ss";

        public IndexBase()
        {
            
        }

        protected override void OnInitialized()
        {
            this.ChatHubService = new ChatHubService(HttpClient, SiteState, NavigationManager, ModuleState.ModuleId);
            this.BrowserResizeService = new BrowserResizeService(HttpClient, JsRuntime);
            this.ScrollService = new ScrollService(HttpClient, JsRuntime);
            
            this.ChatHubService.UpdateUI += UpdateUIStateHasChanged;
            this.ChatHubService.OnAddChatHubMessageEvent += OnAddChatHubMessageExecute;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (firstRender)
            {
                BrowserResizeService.OnResize += BrowserHasResized;
                await JsRuntime.InvokeAsync<object>("browserResize.registerResizeCallback");
                await BrowserHasResized();

                //await JsRuntime.InvokeAsync<object>("showChatPage");

                /*
                var browserDateTime = await browserDateTimeProvider.GetInstance();
                currentTimeZone = browserDateTime.LocalTimeZoneInfo.DisplayName;
                currentTimeZoneOffSet = browserDateTime.LocalTimeZoneInfo.GetUtcOffset(DateTime.Now);
                currentLocalTime = browserDateTime.Now;
                */
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                this.settings = await this.SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);
                maxUserNameCharacters = int.Parse(this.SettingService.GetSetting(settings, "MaxUserNameCharacters", "500"));
                
                if (PageState.QueryString.ContainsKey("moduleid") && PageState.QueryString.ContainsKey("roomid") && int.Parse(PageState.QueryString["moduleid"]) == ModuleState.ModuleId)
                {
                    this.contextRoom = await this.ChatHubService.GetChatHubRoomAsync(int.Parse(PageState.QueryString["roomid"]), ModuleState.ModuleId);
                }
                else
                {
                    await this.ChatHubService.GetLobbyRooms();
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Rooms {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Rooms", MessageType.Error);
            }

            await base.OnParametersSetAsync();
        }

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public async Task DeleteRoom(int id)
        {
            try
            {
                ChatHubService BlogService = new ChatHubService(HttpClient, SiteState, NavigationManager, ModuleState.ModuleId);
                await ChatHubService.DeleteChatHubRoomAsync(id, ModuleState.ModuleId);
                await logger.LogInformation("Room Deleted {ChatHubRoomId}", id);
                NavigationManager.NavigateTo(NavigateUrl());
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Deleting Room {ChatHubRoomId} {Error}", id, ex.Message);
                ModuleInstance.AddModuleMessage("Error Deleting Room", MessageType.Error);
            }
        }

        public async Task ConnectAsGuest()
        {

            try
            {
                if (this.ChatHubService.Connection?.State == HubConnectionState.Connected)
                {
                    return;
                }                

                this.ChatHubService.BuildGuestConnection(GuestUsername);
                this.ChatHubService.RegisterHubConnectionHandlers();
                await this.ChatHubService.ConnectAsync();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Connecting To ChatHub {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error Connecting To ChatHub", MessageType.Error);
            }

        }

        public async Task EnterRoom_Clicked(int roomId, int moduleid)
        {
            if(ChatHubService.Connection?.State == HubConnectionState.Connected)
            {
                await this.ChatHubService.EnterChatRoom(roomId, moduleid);
            }
        }

        public async Task LeaveRoom_Clicked(int roomId, int moduleId)
        {
            await this.ChatHubService.LeaveChatRoom(roomId, moduleId);
        }

        public async void KeyDown(KeyboardEventArgs e, ChatHubRoom room)
        {
            if (!e.ShiftKey && e.Key == "Enter")
            {
                this.SendMessage_Clicked(room.MessageInput, room);
            }
        }

        public void SendMessage_Clicked(string messageInput, ChatHubRoom room)
        {
            this.ChatHubService.SendMessage(messageInput, room.ChatHubRoomId, ModuleState.ModuleId);
            room.MessageInput = string.Empty;
        }
        
        private async Task BrowserHasResized()
        {
            try
            {
                InnerHeight = await this.BrowserResizeService.GetInnerHeight();
                InnerWidth = await this.BrowserResizeService.GetInnerWidth();

                SetChatTabElementsHeight();
                this.UpdateUIStateHasChanged();
            }
            catch(Exception ex)
            {
                await logger.LogError(ex, "Error On Browser Resize {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error On Browser Resize", MessageType.Error);
            }
        }

        private void SetChatTabElementsHeight()
        {
            MessageWindowHeight = 520 + "px";
            UserlistWindowHeight = 570 + "px";
        }

        private async void OnAddChatHubMessageExecute(object sender, ChatHubMessage message)
        {
            if(message.ChatHubRoomId.ToString() != ChatHubService.ContextRoomId)
            {
                ChatHubService.Rooms.FirstOrDefault(room => room.ChatHubRoomId == message.ChatHubRoomId).UnreadMessages++;
                this.UpdateUIStateHasChanged();
            }

            string elementId = string.Concat("#message-window-", ModuleState.ModuleId.ToString(), "-", message.ChatHubRoomId.ToString());
            int animationTime = 1000;
            await this.ScrollService.ScrollToBottom(elementId, animationTime);
        }

        public void UserlistItem_Clicked(MouseEventArgs e, ChatHubRoom room, ChatHubUser user)
        {
            if (user.UserlistItemCollapsed)
            {
                user.UserlistItemCollapsed = false;
            }
            else
            {
                foreach (var chatUser in room.Users.Where(x => x.UserlistItemCollapsed == true))
                {
                    chatUser.UserlistItemCollapsed = false;
                }

                user.UserlistItemCollapsed = true;
            }

            this.UpdateUIStateHasChanged();
        }

        public async Task FixCorruptConnections_ClickedAsync()
        {

            try
            {
                await this.ChatHubService.FixCorruptConnections(ModuleState.ModuleId);
            }
            catch
            {
                throw;
            }

        }

        public string ReplaceYoutubeLinksAsync(string message)
        {
            try
            {
                //var youtubeRegex = @"(?:http?s?:\/\/)?(?:www.)?(?:m.)?(?:music.)?youtu(?:\.?be)(?:\.com)?(?:(?:\w*.?:\/\/)?\w*.?\w*-?.?\w*\/(?:embed|e|v|watch|.*\/)?\??(?:feature=\w*\.?\w*)?&?(?:v=)?\/?)([\w\d_-]{11})(?:\S+)?";
                List<string> regularExpressions = this.SettingService.GetSetting(this.settings, "RegularExpression", "").Split(";delimiter;", StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var regularExpression in regularExpressions)
                {
                    string pattern = regularExpression;
                    string replacement = string.Format("<a href=\"{0}\" target=\"_blank\" title=\"{0}\">{0}</a>", "$0");
                    message = Regex.Replace(message, pattern, replacement);
                }
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }

            return message;
        }

        public string HighlightOwnUsername(string message, string username)
        {
            try
            {
                string pattern = username;
                string replacement = string.Format("<strong>{0}</strong>", "$0");
                message = Regex.Replace(message, pattern, replacement);
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }

            return message;
        }

        public void Dispose()
        {
            if (this.ChatHubService.Connection?.State != HubConnectionState.Disconnected)
            {
                this.ChatHubService.Connection?.StopAsync();
            }

            BrowserResizeService.OnResize -= BrowserHasResized;
        }

        public ChatHubMessage messageItem;
        public bool dialogIsOpen = false;

        public void OpenDialog(ChatHubMessage item)
        {
            this.messageItem = item;
            dialogIsOpen = true;
        }

        public void CloseDialogClicked()
        {
            dialogIsOpen = false;
        }

        public BSModal CenteredBSModal;
        public void BSModalOnToggle(MouseEventArgs e)
        {
            this.BSModalOnToggle();
        }
        private void BSModalOnToggle()
        {
            CenteredBSModal.Toggle();
        }
        public void OpenProfile_Clicked(int userId, int roomId)
        {
            this.BSModalOnToggle();
        }

        public void Show(BSTabEvent e)
        {
            Console.WriteLine($"Show   -> Activated: {e.Activated?.Id.ToString()} , Deactivated: {e.Deactivated?.Id.ToString()}");
        }
        public void Shown(BSTabEvent e)
        {
            var shownRoomId = e.Activated.Id;
            this.ChatHubService.ContextRoomId = shownRoomId;
            var room = this.ChatHubService.Rooms.FirstOrDefault(item => item.ChatHubRoomId.ToString() == this.ChatHubService.ContextRoomId);
            if(room != null)
            {
                room.UnreadMessages = 0;
            }
            Console.WriteLine($"Shown  -> Activated: {e.Activated?.Id.ToString()} , Deactivated: {e.Deactivated?.Id.ToString()}");
        }
        public void Hide(BSTabEvent e)
        {
            Console.WriteLine($"Hide   ->  Activated: {e.Activated?.Id.ToString()} , Deactivated: {e.Deactivated?.Id.ToString()}");
        }
        public void Hidden(BSTabEvent e)
        {
            Console.WriteLine($"Hidden -> Activated: {e.Activated?.Id.ToString()} , Deactivated: {e.Deactivated?.Id.ToString()}");
        }

    }
}