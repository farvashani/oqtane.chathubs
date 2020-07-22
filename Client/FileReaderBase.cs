using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.FileReader;
using Oqtane.Shared;
using Oqtane.ChatHubs.Services;
using Microsoft.AspNetCore.Components;

namespace Oqtane.ChatHubs
{
    public class FileReaderBase : ComponentBase
    {

        [Inject]
        public IFileReaderService fileReaderService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public SiteState SiteState { get; set; }

        [Parameter]
        public ChatHubService ChatHubService { get; set; }
        [Parameter]
        public string ModuleId { get; set; }
        [Parameter]
        public string ChatHubRoomId { get; set; }

        public ElementReference dropTargetElement;
        public IFileReaderRef dropReference;
        public bool Additive { get; set; }

        public const string dropTargetDragClass = "droptarget-drag";
        public const string dropTargetClass = "droptarget";

        private List<string> _dropClasses = new List<string>() { dropTargetClass };

        public string DropClass => string.Join(" ", _dropClasses);
        public string Output { get; set; }
        public List<IFileInfo> FileList { get; } = new List<IFileInfo>();

        protected override void OnInitialized()
        {
        }

        protected override async Task OnAfterRenderAsync(bool isFirstRender)
        {
            if (isFirstRender)
            {
                dropReference = fileReaderService.CreateReference(dropTargetElement);
                await dropReference.RegisterDropEventsAsync();
            }
        }

        public async Task OnAdditiveChange(ChangeEventArgs e)
        {
            Additive = (bool)e.Value;
            await dropReference.UnregisterDropEventsAsync();
            await dropReference.RegisterDropEventsAsync(Additive);
        }

        public async Task ClearFile()
        {
            await dropReference.ClearValue();
            await this.RefreshFileList();

            Output = string.Empty;
        }

        public void OnDragEnter(EventArgs e)
        {
            _dropClasses.Add(dropTargetDragClass);
        }

        public void OnDragLeave(EventArgs e)
        {
            _dropClasses.Remove(dropTargetDragClass);
        }

        public async Task OnDrop(EventArgs e)
        {
            Output += "Dropped a file.";
            _dropClasses.Remove(dropTargetDragClass);
            this.StateHasChanged();
            await this.RefreshFileList();
        }

        private async Task RefreshFileList()
        {
            this.FileList.Clear();
            foreach (var file in await dropReference.EnumerateFilesAsync())
            {
                var fileInfo = await file.ReadFileInfoAsync();
                this.FileList.Add(fileInfo);
            }
            this.StateHasChanged();
        }

        public async Task ReadFile()
        {
            UploadFiles(await dropReference.EnumerateFilesAsync(), this.ChatHubRoomId);
        }

        private async void UploadFiles(IEnumerable<IFileReference> files, string chatHubRoomId)
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

                    //MemoryStream ms = await file.CreateMemoryStreamAsync(4096);
                    //content.Add(new StreamContent(stream), "file", fileInfo.Name);
                }
                using (var httpClient = new HttpClient())
                {
                    content.Headers.Add("connectionId", this.ChatHubService.Connection?.ConnectionId);
                    content.Headers.Add("displayName", this.ChatHubService.ConnectedUser?.DisplayName);
                    content.Headers.Add("roomId", this.ChatHubRoomId);
                    content.Headers.Add("moduleId", this.ModuleId);

                    var url = this.ChatHubService.apiurl + "/PostImageUpload";
                    var result = httpClient.PostAsync(url, content).Result;
                    var remotePath = await result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

            }

        }

    }
}
