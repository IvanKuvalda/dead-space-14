using System.Collections.Generic;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.AppHub;

[Serializable, NetSerializable]
public enum NtSoftwareHubLaptopUiKey : byte { Key }

[Serializable, NetSerializable]
public sealed class NtSoftwareHubLaptopUiState : BoundUserInterfaceState
{
    public int TotalLikes;
    public int SpentLikes;
    public List<NtSoftwareHubLaptopCatalogEntry> Entries;

    public NtSoftwareHubLaptopUiState(int totalLikes, int spentLikes, List<NtSoftwareHubLaptopCatalogEntry> entries)
    {
        TotalLikes = totalLikes;
        SpentLikes = spentLikes;
        Entries = entries;
    }
}

[Serializable, NetSerializable]
public sealed class NtSoftwareHubLaptopCatalogEntry
{
    public string Id = string.Empty;
    public string Name = string.Empty;
    public string Description = string.Empty;
    public int LikesCost;
    public bool IsPurchased;
}

[Serializable, NetSerializable]
public sealed class NtSoftwareHubLaptopPurchaseMessage : BoundUserInterfaceMessage
{
    public string EntryId;

    public NtSoftwareHubLaptopPurchaseMessage(string entryId)
    {
        EntryId = entryId;
    }
}
