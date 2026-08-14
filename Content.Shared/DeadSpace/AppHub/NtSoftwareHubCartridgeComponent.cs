namespace Content.Shared.DeadSpace.AppHub;

[RegisterComponent]
public sealed partial class NtSoftwareHubCartridgeComponent : Component
{
    [DataField]
    public string SelectedCategory = "All";
}
