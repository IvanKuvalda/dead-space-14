using Content.Shared.DeadSpace.AppHub;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.DeadSpace.AppHub;

[UsedImplicitly]
public sealed class NtSoftwareHubLaptopBoundUserInterface : BoundUserInterface
{
    private NtSoftwareHubLaptopMenu? _menu;

    public NtSoftwareHubLaptopBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<NtSoftwareHubLaptopMenu>();
        _menu.OnPurchasePressed += OnPurchase;
        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NtSoftwareHubLaptopUiState laptopState)
            return;

        _menu?.UpdateState(laptopState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _menu?.Dispose();
    }

    private void OnPurchase(string entryId)
    {
        SendMessage(new NtSoftwareHubLaptopPurchaseMessage(entryId));
    }
}
