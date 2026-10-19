using System.Numerics;
using Rts.Client.Selection;
using Rts.Navigation.Grid;
using Rts.Simulation.Contracts;
using static Rts.Client.Tests.Selection.SelectionFixture;

namespace Rts.Client.Tests.Selection;

/// <summary>PRD US-03 (selection), US-04 scenario 1 (right-click move) and edge cases E12, E15.</summary>
public sealed class SelectionToolTests
{
    private const float DragPadding = 30f;

    [Fact]
    public void Click_OnOwnVillager_SelectsOnlyIt()
    {
        var tool = Click(Selecting(VillagerB, VillagerC), ScreenOf(VillagerA));

        Assert.Equal<EntityId>([VillagerA], tool.Selected);
    }

    [Fact]
    public void Click_OnVillagerFeet_SelectsIt()
    {
        var tool = Click(SelectionTool.Empty, ScreenOf(4.5f, 4.5f));

        Assert.Equal<EntityId>([VillagerA], tool.Selected);
    }

    [Fact]
    public void Click_OnEmptyGround_ClearsSelection()
    {
        var tool = Click(Selecting(VillagerA, VillagerB), ScreenOf(1.5f, 1.5f));

        Assert.Empty(tool.Selected);
    }

    [Fact]
    public void Click_OnTownCenter_SelectsIt()
    {
        var tool = Click(Selecting(VillagerA), ScreenOf(7.5f, 8f));

        Assert.Equal<EntityId>([TownCenter], tool.Selected);
    }

    [Fact]
    public void Click_OnTree_ClearsSelection()
    {
        var tool = Click(Selecting(VillagerA), ScreenOf(2.5f, 10.5f, VillagerPickHeight));

        Assert.Empty(tool.Selected);
    }

    [Fact]
    public void Click_OnEnemyVillager_ClearsSelection()
    {
        var tool = Click(Selecting(VillagerA), ScreenOf(EnemyVillager));

        Assert.Empty(tool.Selected);
    }

    [Fact]
    public void Drag_AroundTwoOfThreeVillagers_SelectsExactlyThose()
    {
        var from = ScreenOf(VillagerA) - new Vector2(DragPadding);
        var to = ScreenOf(VillagerB) + new Vector2(DragPadding);

        var tool = Drag(Selecting(VillagerC), from, to);

        Assert.Equal<EntityId>([VillagerA, VillagerB], tool.Selected);
    }

    [Fact]
    public void Drag_FromBottomRightToTopLeft_SelectsTheSameVillagers()
    {
        var topLeft = ScreenOf(VillagerA) - new Vector2(DragPadding);
        var bottomRight = ScreenOf(VillagerB) + new Vector2(DragPadding);

        var tool = Drag(SelectionTool.Empty, bottomRight, topLeft);

        Assert.Equal<EntityId>([VillagerA, VillagerB], tool.Selected);
    }

    [Fact]
    public void Drag_OverWholeScreen_SelectsOnlyOwnVillagers()
    {
        var tool = Drag(SelectionTool.Empty, Vector2.Zero, ScreenSize);

        Assert.Equal<EntityId>([VillagerA, VillagerB, VillagerC], tool.Selected);
    }

    [Fact]
    public void Drag_OverEmptyArea_ClearsSelection()
    {
        var tool = Drag(Selecting(VillagerA), ScreenOf(1f, 14f), ScreenOf(2f, 15f));

        Assert.Empty(tool.Selected);
    }

    [Fact]
    public void Drag_WithZeroSize_IsTreatedAsClick()
    {
        var point = ScreenOf(VillagerC);

        var tool = SelectionTool.Empty.Update(Press(point), Context).Next.Update(Release(point), Context).Next;

        Assert.Equal<EntityId>([VillagerC], tool.Selected);
    }

    [Fact]
    public void PressAndReleaseInSameFrame_IsTreatedAsClick()
    {
        var point = ScreenOf(VillagerB);
        var input = new PointerInput(point, IsSelectPressed: true, IsSelectReleased: true, IsCommandPressed: false);

        var tool = SelectionTool.Empty.Update(input, Context).Next;

        Assert.Equal<EntityId>([VillagerB], tool.Selected);
    }

    [Fact]
    public void Release_WithoutPress_KeepsSelection()
    {
        var tool = Selecting(VillagerA).Update(Release(ScreenOf(1.5f, 1.5f)), Context).Next;

        Assert.Equal<EntityId>([VillagerA], tool.Selected);
    }

    [Fact]
    public void DragRectangle_WhileDragging_SpansPressAndPointer()
    {
        var from = new Vector2(100, 200);
        var pointer = new Vector2(50, 300);

        var dragging = SelectionTool.Empty.Update(Press(from), Context).Next.Update(Hover(pointer), Context).Next;

        var rectangle = dragging.DragRectangle(pointer);
        Assert.NotNull(rectangle);
        Assert.Equal(new Vector2(50, 200), rectangle.Value.Min);
        Assert.Equal(new Vector2(100, 300), rectangle.Value.Max);
    }

    [Fact]
    public void DragRectangle_BelowThresholdOrNotDragging_IsNull()
    {
        var from = new Vector2(100, 200);
        var dragging = SelectionTool.Empty.Update(Press(from), Context).Next;

        Assert.Null(dragging.DragRectangle(from + Vector2.One));
        Assert.Null(SelectionTool.Empty.DragRectangle(from));
    }

    [Fact]
    public void RightClick_WithSelectedVillagers_MovesThemToClickedTile()
    {
        var update = Selecting(VillagerB, VillagerA).Update(RightClick(ScreenOf(10.5f, 13.5f)), Context);

        var command = Assert.IsType<MoveCommand>(update.Command);
        Assert.Equal(LocalPlayer, command.Issuer);
        Assert.Equal(Tick, command.TargetTick);
        Assert.Equal<EntityId>([VillagerA, VillagerB], command.Units);
        Assert.Equal(new GridPosition(10, 13), command.Target);
    }

    [Fact]
    public void RightClick_KeepsSelection()
    {
        var update = Selecting(VillagerA).Update(RightClick(ScreenOf(10.5f, 13.5f)), Context);

        Assert.Equal<EntityId>([VillagerA], update.Next.Selected);
    }

    [Fact]
    public void RightClick_WithMixedSelection_MovesOnlyVillagers()
    {
        var update = Selecting(TownCenter, VillagerC).Update(RightClick(ScreenOf(1.5f, 1.5f)), Context);

        var command = Assert.IsType<MoveCommand>(update.Command);
        Assert.Equal<EntityId>([VillagerC], command.Units);
    }

    [Fact]
    public void RightClick_WithoutSelection_CreatesNoCommand()
    {
        var update = SelectionTool.Empty.Update(RightClick(ScreenOf(1.5f, 1.5f)), Context);

        Assert.Null(update.Command);
    }

    [Fact]
    public void RightClick_WithOnlyTownCenterSelected_CreatesNoCommand()
    {
        var update = Selecting(TownCenter).Update(RightClick(ScreenOf(1.5f, 1.5f)), Context);

        Assert.Null(update.Command);
    }

    [Fact]
    public void RightClick_WithVanishedEntitySelected_CreatesNoCommand()
    {
        var context = CreateContext([.. Entities.Where(entity => entity.Id != VillagerA)]);

        var update = Selecting(VillagerA).Update(RightClick(ScreenOf(1.5f, 1.5f)), context);

        Assert.Null(update.Command);
    }

    [Theory]
    [InlineData(-0.5f, 3.5f)]
    [InlineData(3.5f, -0.5f)]
    [InlineData(16.5f, 3.5f)]
    public void RightClick_OutsideMap_CreatesNoCommand(float x, float y)
    {
        var update = Selecting(VillagerA).Update(RightClick(ScreenOf(x, y)), Context);

        Assert.Null(update.Command);
    }

    [Fact]
    public void Update_WithoutButtons_ChangesNothing()
    {
        var tool = Selecting(VillagerA);

        var update = tool.Update(Hover(ScreenOf(VillagerB)), Context);

        Assert.Same(tool, update.Next);
        Assert.Null(update.Command);
    }
}
