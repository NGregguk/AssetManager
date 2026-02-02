using Microsoft.AspNetCore.Http;

namespace asset_manager.ViewModels;

public class FloorPlanUploadViewModel
{
    public string Name { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}
