using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.DeadSpace.AppHub;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.AppHub;

public sealed partial class NtSoftwareHubCartridgeUi : UIFragment
{
    private NtSoftwareHubCartridgeFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NtSoftwareHubCartridgeFragment();
        _fragment.OnInstallPressed += programId =>
        {
            var message = new NtSoftwareHubCartridgeUiMessageEvent(NtSoftwareHubCartridgeUiAction.Install, programId);
            userInterface.SendMessage(new CartridgeUiMessage(message));
        };
        _fragment.OnUninstallPressed += programId =>
        {
            var message = new NtSoftwareHubCartridgeUiMessageEvent(NtSoftwareHubCartridgeUiAction.Uninstall, programId);
            userInterface.SendMessage(new CartridgeUiMessage(message));
        };
        _fragment.OnCategorySelected += category =>
        {
            var message = new NtSoftwareHubCartridgeUiMessageEvent(NtSoftwareHubCartridgeUiAction.SelectCategory, category: category);
            userInterface.SendMessage(new CartridgeUiMessage(message));
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NtSoftwareHubCartridgeUiState hubState)
            return;

        _fragment?.UpdateState(hubState);
    }
}
