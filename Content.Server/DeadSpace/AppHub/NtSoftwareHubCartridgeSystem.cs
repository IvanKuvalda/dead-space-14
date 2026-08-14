using System.Linq;
using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Content.Shared.DeadSpace.AppHub;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.AppHub;

public sealed class NtSoftwareHubCartridgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

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

    public void RefreshCartridgesOnGrid(EntityUid grid)
    {
        var query = EntityQueryEnumerator<NtSoftwareHubCartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!TryComp<CartridgeComponent>(uid, out var cartridge) || cartridge.LoaderUid is not { } loader)
                continue;

            var loaderGrid = _transform.GetGrid(loader);
            if (loaderGrid != grid)
                continue;

            UpdateUiState(uid, loader, comp);
        }
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

        var purchased = GetPurchasedEntryIds(loaderUid);

        var catalogEntries = new List<AppHubCatalogEntry>();
        foreach (var proto in _prototype.EnumeratePrototypes<AppCatalogEntryPrototype>())
        {
            if (component.SelectedCategory != "All" && proto.Category != component.SelectedCategory)
                continue;

            if (proto.LikesCost > 0 && !purchased.Contains(proto.ID))
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

    private HashSet<string> GetPurchasedEntryIds(EntityUid loaderUid)
    {
        var purchased = new HashSet<string>();

        var grid = _transform.GetGrid(loaderUid);
        if (grid != null && TryComp<NtSoftwareHubGridCatalogComponent>(grid.Value, out var gridCatalog))
        {
            foreach (var id in gridCatalog.PurchasedEntryIds)
                purchased.Add(id);
        }

        return purchased;
    }
}
