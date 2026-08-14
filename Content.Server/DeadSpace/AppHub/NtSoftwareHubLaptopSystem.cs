using System.Linq;
using Content.Server.UserInterface;
using Content.Shared.DeadSpace.AppHub;
using Content.Shared.MassMedia.Components;
using Content.Shared.Station;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.DeadSpace.AppHub;

public sealed class NtSoftwareHubLaptopSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly NtSoftwareHubCartridgeSystem _cartridgeSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NtSoftwareHubLaptopComponent, AfterActivatableUIOpenEvent>(OnUiOpen);
        SubscribeLocalEvent<NtSoftwareHubLaptopComponent, NtSoftwareHubLaptopPurchaseMessage>(OnPurchase);
    }

    private void OnUiOpen(EntityUid uid, NtSoftwareHubLaptopComponent comp, AfterActivatableUIOpenEvent args)
    {
        UpdateUiState(uid);
    }

    private void OnPurchase(EntityUid uid, NtSoftwareHubLaptopComponent comp, NtSoftwareHubLaptopPurchaseMessage msg)
    {
        if (!_prototype.TryIndex<AppCatalogEntryPrototype>(msg.EntryId, out var entry))
            return;

        if (entry.LikesCost <= 0)
            return;

        var grid = _transform.GetGrid(uid);
        if (grid == null)
            return;

        var gridCatalog = EnsureComp<NtSoftwareHubGridCatalogComponent>(grid.Value);

        if (gridCatalog.PurchasedEntryIds.Contains(entry.ID))
            return;

        var totalLikes = GetTotalLikes(uid);
        if (totalLikes - gridCatalog.SpentLikes < entry.LikesCost)
            return;

        gridCatalog.PurchasedEntryIds.Add(entry.ID);
        gridCatalog.SpentLikes += entry.LikesCost;

        UpdateUiState(uid);
        _cartridgeSystem.RefreshCartridgesOnGrid(grid.Value);
    }

    private int GetTotalLikes(EntityUid uid)
    {
        if (_station.GetOwningStation(uid) is not { } station ||
            !TryComp(station, out StationNewsComponent? stationNews))
            return 0;

        var total = 0;
        foreach (var article in stationNews.Articles)
            total += article.Likes;

        return total;
    }

    private void UpdateUiState(EntityUid uid)
    {
        if (!_ui.HasUi(uid, NtSoftwareHubLaptopUiKey.Key))
            return;

        var grid = _transform.GetGrid(uid);
        var purchased = new HashSet<string>();
        var spentLikes = 0;
        if (grid != null && TryComp<NtSoftwareHubGridCatalogComponent>(grid.Value, out var gridCatalog))
        {
            spentLikes = gridCatalog.SpentLikes;
            foreach (var id in gridCatalog.PurchasedEntryIds)
                purchased.Add(id);
        }

        var totalLikes = GetTotalLikes(uid);

        var entries = new List<NtSoftwareHubLaptopCatalogEntry>();
        foreach (var proto in _prototype.EnumeratePrototypes<AppCatalogEntryPrototype>())
        {
            if (proto.LikesCost <= 0)
                continue;

            entries.Add(new NtSoftwareHubLaptopCatalogEntry
            {
                Id = proto.ID,
                Name = Loc.GetString(proto.Name),
                Description = Loc.GetString(proto.Description),
                LikesCost = proto.LikesCost,
                IsPurchased = purchased.Contains(proto.ID)
            });
        }

        var state = new NtSoftwareHubLaptopUiState(totalLikes, spentLikes, entries);
        _ui.SetUiState(uid, NtSoftwareHubLaptopUiKey.Key, state);
    }
}
