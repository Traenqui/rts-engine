using System.Numerics;
using Rts.Rendering.Contracts;

namespace Rts.Client.Picking;

/// <summary>
/// Converts between screen pixels and render space for one <see cref="CameraView"/>. It uses the same perspective
/// frustum as Raylib (vertical field of view, aspect from the screen size), so picks match what is drawn.
/// </summary>
public sealed class ScreenProjection
{
    private const float DegreesToRadians = MathF.PI / 180f;

    // Rays flatter than this never reach the ground within a sensible distance.
    private const float MinDownwardRayComponent = 1e-4f;
    private const float MinDepth = 1e-4f;

    private readonly Vector3 _forward;
    private readonly Vector3 _right;
    private readonly Vector3 _up;
    private readonly float _tanHalfHeight;
    private readonly float _tanHalfWidth;

    /// <exception cref="ArgumentOutOfRangeException">A component of <paramref name="screenSize"/> is not positive.</exception>
    public ScreenProjection(CameraView view, Vector2 screenSize)
    {
        if (!(screenSize.X > 0 && screenSize.Y > 0))
        {
            throw new ArgumentOutOfRangeException(nameof(screenSize), screenSize, "Screen size must be positive.");
        }

        View = view;
        ScreenSize = screenSize;
        _forward = Vector3.Normalize(view.Target - view.Position);
        _right = Vector3.Normalize(Vector3.Cross(_forward, Vector3.UnitY));
        _up = Vector3.Cross(_right, _forward);
        _tanHalfHeight = MathF.Tan(view.FieldOfViewDegrees * DegreesToRadians / 2);
        _tanHalfWidth = _tanHalfHeight * screenSize.X / screenSize.Y;
    }

    public CameraView View { get; }

    public Vector2 ScreenSize { get; }

    /// <summary>
    /// Where the ray through <paramref name="screen"/> meets the horizontal plane at <paramref name="height"/>,
    /// as a simulation-space point (render x, render z); null if the ray does not reach the plane.
    /// </summary>
    public Vector2? GroundAt(Vector2 screen, float height = 0f)
    {
        var ndcX = (2 * screen.X / ScreenSize.X) - 1;
        var ndcY = 1 - (2 * screen.Y / ScreenSize.Y);
        var direction = _forward + (_right * ndcX * _tanHalfWidth) + (_up * ndcY * _tanHalfHeight);

        if (direction.Y > -MinDownwardRayComponent || View.Position.Y <= height)
        {
            return null;
        }

        var hit = View.Position + (direction * ((height - View.Position.Y) / direction.Y));
        return new Vector2(hit.X, hit.Z);
    }

    /// <summary>Screen pixel of a render-space point; null if the point is behind the camera.</summary>
    public Vector2? ToScreen(Vector3 point)
    {
        var offset = point - View.Position;
        var depth = Vector3.Dot(offset, _forward);

        if (depth < MinDepth)
        {
            return null;
        }

        var ndcX = Vector3.Dot(offset, _right) / (depth * _tanHalfWidth);
        var ndcY = Vector3.Dot(offset, _up) / (depth * _tanHalfHeight);
        return new Vector2((ndcX + 1) / 2 * ScreenSize.X, (1 - ndcY) / 2 * ScreenSize.Y);
    }
}
