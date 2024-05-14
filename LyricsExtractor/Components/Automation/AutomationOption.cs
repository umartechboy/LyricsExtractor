using LyricsExtractor.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;

namespace SubtitleEditor.Automation
{
    public class AutomationOption
    {
        public string AcceptedFormats { get; set; } = "";
        public string Label { get; set; } = "";
        public string LoadedDataLabel { get; set; } = "";
        public byte[] Data { get; set; }
        public string DefaultDataSource { get; set; } = "";
        public bool HasDefault { get; set; } = true;
        public async Task FileSelected(IBrowserFile file, byte[] data)
        {
            try
            {
                Data = data;
                LoadedDataLabel = file.Name;
            }
            catch (Exception ex)
            {
            }
            //var data = new byte[file.Size];
            //await file.OpenReadStream(20000000).ReadAsync(data);
            //Data = data;
            //LoadedDataLabel = file.Name;
        }
        public async Task DefaultSelected()
        {
            try
            {
                Data = FileManager.GetBytes(DefaultDataSource);
                LoadedDataLabel = Path.GetFileName(DefaultDataSource);
            }
            catch (Exception ex) 
            {
            }
        }
    }
}
