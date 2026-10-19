using System.Collections.Immutable;
using System.Numerics;
using Rts.Client.Picking;
using Rts.Navigation.Grid;
using Rts.Rendering.Contracts;
using Rts.Simulation.Contracts;

namespace Rts.Client.Selection;

/// <summary>Mouse state of one frame: left button selects, right button commands.</summary>
public readonly record struct PointerInput(Vector2 Position, bool IsSelectPressed, bool IsSelectReleased, bool IsCommandPressed);

/// <summary>Result of one frame: the new tool state and the command to enqueue, if any.</summary>
public sealed record SelectionUpdate(SelectionTool Next, Command? Command);

/// <summary>
/// Click and box selection plus right-click move (PRD US-03, US-04). Immutable; <see cref="Update"/> returns the next
/// state. Commands are only created here, the simulation validates them again (ADR-0009).
/// </summary>
/// <param name="Selected">Selected entities in ascending id order.</param>
/// <param name="DragStart">Screen point where the left button went down, null when it is up.</param>
public sealed record SelectionTool(ImmutableArray<EntityId> Selected, Vector2? DragStart)
{
    /// <summary>Boxes narrower and lower than this count as a click (PRD E15), so a shaky click still picks.</summary>
    public const float DragThresholdPixels = 4f;

    public static SelectionTool Empty { get; } = new([], null);

    public SelectionUpdate Update(PointerInput input, PickingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tool = this;

        if (input.IsSelectPressed)
        {
            tool = tool with { DragStart = input.Position };
        }

        if (input.IsSelectReleased && tool.DragStart is { } start)
        {
            tool = new SelectionTool(Select(start, input.Position, context), DragStart: null);
        }

        var command = input.IsCommandPressed ? tool.CreateMoveCommand(input.Position, context) : null;
        return new SelectionUpdate(tool, command);
    }

    /// <summary>The selection box to draw while dragging, null while not dragging or below the threshold.</summary>
    public ScreenRectangle? DragRectangle(Vector2 pointer)
    {
        if (DragStart is not { } start)
        {
            return null;
        }

        var rectangle = ScreenRectangle.FromCorners(start, pointer);
        return IsClick(rectangle) ? null : rectangle;
    }

    private static ImmutableArray<EntityId> Select(Vector2 start, Vector2 end, PickingContext context)
    {
        var rectangle = ScreenRectangle.FromCorners(start, end);

        if (!IsClick(rectangle))
        {
            return EntityPicker.PickInRectangle(rectangle, context);
        }

        return EntityPicker.PickAt(end, context) is { } picked ? [picked] : [];
    }

    private static bool IsClick(ScreenRectangle rectangle) =>
        rectangle.Size.X < DragThresholdPixels && rectangle.Size.Y < DragThresholdPixels;

    /// <summary>Move for the selected villagers to the clicked tile; null without villagers or off the map (PRD E12).</summary>
    private MoveCommand? CreateMoveCommand(Vector2 screen, PickingContext context)
    {
        var villagers = context.Snapshot.Entities
            .Where(entity => entity.Kind == EntityKind.Villager && entity.Owner == context.LocalPlayer)
            .Select(entity => entity.Id)
            .Where(Selected.Contains)
            .ToImmutableArray();

        if (villagers.IsEmpty || context.Projection.GroundAt(screen) is not { } ground)
        {
            return null;
        }

        var tile = new GridPosition((int)MathF.Floor(ground.X), (int)MathF.Floor(ground.Y));

        if (tile.X < 0 || tile.Y < 0 || tile.X >= context.Terrain.Width || tile.Y >= context.Terrain.Height)
        {
            return null;
        }

        // The snapshot shows the state before the next tick, so the command runs in that tick.
        return new MoveCommand(context.LocalPlayer, context.Snapshot.Tick, villagers, tile);
    }
}
