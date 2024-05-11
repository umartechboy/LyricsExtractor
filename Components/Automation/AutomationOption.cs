using LyricsExtractor.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;

namespace SubtitleEditor.Automation
{
    public class AutomationOption
    {
        public bool HasData { get => data != null || selectedFile != null; }
        public string AcceptedFormats { get; set; } = "";
        public string Label { get; set; } = "";
        public string LoadedDataLabel { get; set; } = "";
        public void ClearData()
        {
            data = null;
            selectedFile = null;
        }
        public async Task<byte[]> GetData()
        {
            if (data == null)
                if (selectedFile != null)
                    await selectedFile.OpenReadStream(20000000).ReadAsync(data);
            return data;
        }
        byte[] data;
        IBrowserFile selectedFile;
        public string DefaultDataSource { get; set; } = "";
        public bool HasDefault { get; set; } = true;
        public async Task FileSelected(IBrowserFile file)
        {
            try
            {
                var data = new byte[file.Size];
                selectedFile = file; // Will Read later
                // awaiting this causes UI issue
                //var stream = file.OpenReadStream(20000000);
                //await stream.ReadAsync(data);
                // will read later
                this.data = data;
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
                data = FileManager.GetBytes(DefaultDataSource);
                LoadedDataLabel = Path.GetFileName(DefaultDataSource);
            }
            catch (Exception ex) 
            {
            }
        }
    }
}
