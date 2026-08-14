using System.Collections.Generic;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.AppHub;

[Serializable, NetSerializable]
public sealed class NtSoftwareHubCartridgeUiState : BoundUserInterfaceState
{
    public int UsedDiskSpace;
    public int MaxDiskSpace;
    public string SelectedCategory;
    public List<AppHubCatalogEntry> CatalogEntries;

    public NtSoftwareHubCartridgeUiState(
        int usedDiskSpace,
        int maxDiskSpace,
        string selectedCategory,
        List<AppHubCatalogEntry> catalogEntries)
    {
        UsedDiskSpace = usedDiskSpace;
        MaxDiskSpace = maxDiskSpace;
        SelectedCategory = selectedCategory;
        CatalogEntries = catalogEntries;
    }
}
