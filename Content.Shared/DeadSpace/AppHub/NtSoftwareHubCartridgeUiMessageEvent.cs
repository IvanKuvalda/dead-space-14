using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.AppHub;

[Serializable, NetSerializable]
public sealed class NtSoftwareHubCartridgeUiMessageEvent : CartridgeMessageEvent
{
    public readonly NtSoftwareHubCartridgeUiAction Action;
    public readonly string ProgramId;
    public readonly string Category;

    public NtSoftwareHubCartridgeUiMessageEvent(NtSoftwareHubCartridgeUiAction action, string programId = "", string category = "")
    {
        Action = action;
        ProgramId = programId;
        Category = category;
    }
}

[Serializable, NetSerializable]
public enum NtSoftwareHubCartridgeUiAction
{
    Install,
    Uninstall,
    SelectCategory
}
