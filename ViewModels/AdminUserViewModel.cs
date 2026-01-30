namespace asset_manager.ViewModels;

public class AdminUserViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
}

public class AdminUserEditViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string SelectedRole { get; set; } = "User";
    public IReadOnlyList<string> AvailableRoles { get; init; } = Array.Empty<string>();
}
