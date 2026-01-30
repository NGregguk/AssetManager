using Microsoft.AspNetCore.Http;

namespace asset_manager.ViewModels;

public class PcInfoUploadViewModel
{
    public IFormFile? File { get; set; }
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; } = new();
}
