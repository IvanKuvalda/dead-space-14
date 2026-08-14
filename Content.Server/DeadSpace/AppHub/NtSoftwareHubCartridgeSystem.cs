using System.Linq;
using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.DeadSpace.AppHub;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.AppHub;

public sealed class NtSoftwareHubCartridgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NtSoftwareHubCartridgeComponent, CartridgeMessageEvent>(OnUiMessage);
        SubscribeLocalEvent<NtSoftwareHubCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
    }

    private void OnUiReady(EntityUid uid, NtSoftwareHubCartridgeComponent component, CartridgeUiReadyEvent args)
    {
        UpdateUiState(uid, args.Loader, component);
    }

    private void OnUiMessage(EntityUid uid, NtSoftwareHubCartridgeComponent component, CartridgeMessageEvent args)
    {
        if (args is not NtSoftwareHubCartridgeUiMessageEvent message)
            return;

        var loaderUid = GetEntity(args.LoaderUid);
        if (!TryComp(loaderUid, out CartridgeLoaderComponent? loader))
            return;

        switch (message.Action)
        {
            case NtSoftwareHubCartridgeUiAction.Install:
            {
                if (!_prototype.TryIndex<AppCatalogEntryPrototype>(message.ProgramId, out var entry))
                    return;

                if (string.IsNullOrEmpty(entry.ProgramId))
                    return;

                foreach (var prog in _cartridgeLoader.GetInstalled(loaderUid))
                {
                    if (Prototype(prog)?.ID == entry.ProgramId)
                        return;
                }

                _cartridgeLoader.InstallProgram(loaderUid, entry.ProgramId, loader: loader);
                break;
            }
            case NtSoftwareHubCartridgeUiAction.Uninstall:
            {
                if (!_prototype.TryIndex<AppCatalogEntryPrototype>(message.ProgramId, out var entry))
                    return;

                if (string.IsNullOrEmpty(entry.ProgramId))
                    return;

                foreach (var prog in _cartridgeLoader.GetInstalled(loaderUid))
                {
                    if (Prototype(prog)?.ID == entry.ProgramId)
                    {
                        _cartridgeLoader.UninstallProgram(loaderUid, prog, loader);
                        break;
                    }
                }
                break;
            }
            case NtSoftwareHubCartridgeUiAction.SelectCategory:
                component.SelectedCategory = message.Category;
                break;
        }

        UpdateUiState(uid, loaderUid, component);
    }

    private void UpdateUiState(EntityUid uid, EntityUid loaderUid, NtSoftwareHubCartridgeComponent component)
    {
        if (!TryComp(loaderUid, out CartridgeLoaderComponent? loader))
            return;

        var usedDisk = 0;
        var installedProgramIds = new HashSet<string>();

        foreach (var prog in _cartridgeLoader.GetInstalled(loaderUid))
        {
            usedDisk++;
            var protoId = Prototype(prog)?.ID;
            if (protoId != null)
                installedProgramIds.Add(protoId);
        }

        var catalogEntries = new List<AppHubCatalogEntry>();
        foreach (var proto in _prototype.EnumeratePrototypes<AppCatalogEntryPrototype>())
        {
            if (component.SelectedCategory != "All" && proto.Category != component.SelectedCategory)
                continue;

            catalogEntries.Add(new AppHubCatalogEntry
            {
                Id = proto.ID,
                Name = Loc.GetString(proto.Name),
                Description = Loc.GetString(proto.Description),
                Category = proto.Category,
                IsInstalled = installedProgramIds.Contains(proto.ProgramId)
            });
        }

        var state = new NtSoftwareHubCartridgeUiState(usedDisk, loader.DiskSpace, component.SelectedCategory, catalogEntries);
        _cartridgeLoader.UpdateCartridgeUiState(loaderUid, state, loader: loader);
    }
}
