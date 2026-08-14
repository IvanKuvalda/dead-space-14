using System.Collections.Generic;

namespace Content.Shared.DeadSpace.AppHub;

[RegisterComponent]
public sealed partial class NtSoftwareHubGridCatalogComponent : Component
{
    [DataField]
    public List<string> PurchasedEntryIds = new();

    [DataField]
    public int SpentLikes;
}
