using System.Numerics;
using Raylib_cs;
using Rts.Client.Camera;
using Rts.Client.Input;
using Rts.Client.Picking;
using Rts.Client.Selection;
using Rts.Client.Timing;
using Rts.Rendering.Contracts;
using Rts.Rendering.Debug;
using Rts.Rendering.Raylib;
using Rts.Rendering.Scene;
using Rts.Simulation.Contracts;
using static Raylib_cs.Raylib;

namespace Rts.Game;

/// <summary>Window and frame loop: input, fixed ticks, interpolated drawing (ADR-0007). Composition only, no game logic.</summary>
internal sealed class GameWindow(GameSession session, GameOptions options)
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;
    private const string Title = "RTS Engine";
    private const int ScreenshotAfterFrames = 120;
    private const double TickSeconds = 1.0 / SimulationClock.TicksPerSecond;
    private const double StatsWindowSeconds = 1.0;

    private static readonly Func<GameSession, CameraBounds> BoundsOf =
        session => new CameraBounds(session.Simulation.Terrain.Width, session.Simulation.Terrain.Height);

    private readonly CameraBounds _bounds = BoundsOf(session);
    private CameraRig _camera = CameraRig.CenteredOn(BoundsOf(session));
    private FixedStepClock _clock = FixedStepClock.Start;
    private SelectionTool _selection = SelectionTool.Empty;
    private ScreenRectangle? _selectionBox;
    private bool _isDebugOverlayVisible = options.IsDebugOverlayVisible;
    private int _ticksInStatsWindow;
    private double _statsWindowElapsed;
    private int _ticksPerSecond;

    public void Run()
    {
        SetConfigFlags(ConfigFlags.VSyncHint | ConfigFlags.Msaa4xHint | ConfigFlags.ResizableWindow);
        InitWindow(WindowWidth, WindowHeight, Title);

        try
        {
            RunFrames();
        }
        finally
        {
            CloseWindow();
        }
    }

    private void RunFrames()
    {
        for (var frame = 1; !WindowShouldClose(); frame++)
        {
            var alpha = Update(GetFrameTime());
            Draw(alpha);

            if (options.ScreenshotFile is { } file && frame == ScreenshotAfterFrames)
            {
                TakeScreenshot(file);
                return;
            }
        }
    }

    private float Update(float frameSeconds)
    {
        _camera = _camera.Update(RaylibInput.ReadCamera(), frameSeconds, _bounds);
        _isDebugOverlayVisible ^= RaylibInput.IsDebugOverlayToggled();
        UpdateSelection();

        var clockFrame = _clock.Advance(frameSeconds, TickSeconds);
        _clock = clockFrame.Next;

        for (var tick = 0; tick < clockFrame.Ticks; tick++)
        {
            session.Step();
        }

        MeasureTickRate(clockFrame.Ticks, frameSeconds);
        return clockFrame.Alpha;
    }

    private void UpdateSelection()
    {
        var screenSize = new Vector2(GetScreenWidth(), GetScreenHeight());

        // A minimized window has no screen area to pick in.
        if (screenSize.X <= 0 || screenSize.Y <= 0)
        {
            return;
        }

        var pointer = RaylibInput.ReadPointer();
        var projection = new ScreenProjection(_camera.ToView(), screenSize);
        var context = new PickingContext(projection, session.Current, GameSession.LocalPlayer, session.Simulation.Terrain);
        var update = _selection.Update(pointer, context);

        _selection = update.Next;
        _selectionBox = _selection.DragRectangle(pointer.Position);

        if (update.Command is { } command)
        {
            session.Simulation.Enqueue(command);
        }
    }

    private void Draw(float alpha)
    {
        var terrain = session.Simulation.Terrain;
        var entities = SnapshotInterpolator.Interpolate(session.Previous, session.Current, alpha);
        var frame = new SceneFrame(terrain, entities, [.. _selection.Selected]);
        Action? debug3D = _isDebugOverlayVisible ? () => DebugOverlayRenderer.Draw3D(terrain, entities) : null;

        BeginDrawing();
        WorldRenderer.Draw(frame, _camera.ToView(), debug3D);

        if (_selectionBox is { } box)
        {
            SelectionRenderer.DrawBox(box);
        }

        if (_isDebugOverlayVisible)
        {
            DebugOverlayRenderer.Draw2D(DebugOverlayText.Lines(session.Current, new FrameStats(GetFPS(), _ticksPerSecond)));
        }

        EndDrawing();
    }

    private void MeasureTickRate(int ticks, double frameSeconds)
    {
        _ticksInStatsWindow += ticks;
        _statsWindowElapsed += frameSeconds;

        if (_statsWindowElapsed < StatsWindowSeconds)
        {
            return;
        }

        _ticksPerSecond = (int)Math.Round(_ticksInStatsWindow / _statsWindowElapsed);
        _ticksInStatsWindow = 0;
        _statsWindowElapsed = 0;
    }
}
