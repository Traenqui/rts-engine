using System.Collections.Immutable;
using System.Numerics;
using Rts.Client.Camera;
using Rts.Client.Picking;
using Rts.Client.Selection;
using Rts.Primitives;
using Rts.Simulation.Contracts;

namespace Rts.Client.Tests.Selection;

/// <summary>
/// 16x16 map seen by the start camera: own town center, three own villagers, a tree and an enemy villager.
/// Screen points are computed with the projection, which <see cref="Picking.ScreenProjectionTests"/> covers.
/// </summary>
internal static class SelectionFixture
{
    public const long Tick = 42;
    public const float VillagerPickHeight = 0.4f;

    public static readonly PlayerId LocalPlayer = new(1);
    public static readonly PlayerId Enemy = new(2);

    public static readonly EntityId TownCenter = new(0, 0);
    public static readonly EntityId VillagerA = new(1, 0);
    public static readonly EntityId VillagerB = new(2, 0);
    public static readonly EntityId VillagerC = new(3, 0);
    public static readonly EntityId Tree = new(4, 0);
    public static readonly EntityId EnemyVillager = new(5, 0);

    public static readonly Vector2 ScreenSize = new(1280, 720);

    public static readonly ImmutableArray<EntitySnapshot> Entities =
    [
        Entity(TownCenter, EntityKind.TownCenter, LocalPlayer, 8f, 8f),
        Entity(VillagerA, EntityKind.Villager, LocalPlayer, 4.5f, 4.5f),
        Entity(VillagerB, EntityKind.Villager, LocalPlayer, 5.5f, 4.5f),
        Entity(VillagerC, EntityKind.Villager, LocalPlayer, 12.5f, 12.5f),
        Entity(Tree, EntityKind.Tree, PlayerId.Nature, 2.5f, 10.5f),
        Entity(EnemyVillager, EntityKind.Villager, Enemy, 10.5f, 4.5f),
    ];

    public static PickingContext Context { get; } = CreateContext(Entities);

    public static PickingContext CreateContext(ImmutableArray<EntitySnapshot> entities)
    {
        var view = CameraRig.CenteredOn(new CameraBounds(Width: 16, Height: 16)).ToView();
        var terrain = new TerrainInfo(16, 16, []);
        var snapshot = new WorldSnapshot(Tick, StateHash: 0, RejectedCommandCount: 0, entities);
        return new PickingContext(new ScreenProjection(view, ScreenSize), snapshot, LocalPlayer, terrain);
    }

    /// <summary>Screen position of a ground point at <paramref name="height"/>.</summary>
    public static Vector2 ScreenOf(float x, float y, float height = 0f) =>
        Context.Projection.ToScreen(new Vector3(x, height, y)) ?? throw new InvalidOperationException("Point is not visible.");

    /// <summary>Screen position of the body center of <paramref name="id"/>.</summary>
    public static Vector2 ScreenOf(EntityId id)
    {
        var entity = Entities.Single(candidate => candidate.Id == id);
        return ScreenOf(entity.Position.X.ToSingle(), entity.Position.Y.ToSingle(), VillagerPickHeight);
    }

    public static SelectionTool Click(SelectionTool tool, Vector2 screen) =>
        tool.Update(Press(screen), Context).Next.Update(Release(screen), Context).Next;

    public static SelectionTool Drag(SelectionTool tool, Vector2 from, Vector2 to) =>
        tool.Update(Press(from), Context).Next.Update(Release(to), Context).Next;

    public static PointerInput Press(Vector2 screen) => new(screen, IsSelectPressed: true, IsSelectReleased: false, IsCommandPressed: false);

    public static PointerInput Release(Vector2 screen) => new(screen, IsSelectPressed: false, IsSelectReleased: true, IsCommandPressed: false);

    public static PointerInput Hover(Vector2 screen) => new(screen, IsSelectPressed: false, IsSelectReleased: false, IsCommandPressed: false);

    public static PointerInput RightClick(Vector2 screen) => new(screen, IsSelectPressed: false, IsSelectReleased: false, IsCommandPressed: true);

    public static SelectionTool Selecting(params EntityId[] ids) => SelectionTool.Empty with { Selected = [.. ids] };

    private static EntitySnapshot Entity(EntityId id, EntityKind kind, PlayerId owner, float x, float y)
    {
        var position = new FixedVector2(ToFixed(x), ToFixed(y));
        return new EntitySnapshot(id, kind, owner, position, []);
    }

    // Fixture positions are multiples of 0.5, so the conversion is exact.
    private static Fixed ToFixed(float value) => Fixed.FromRatio((long)(value * 2), 2);
}
