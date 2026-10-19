using System.Numerics;

namespace Rts.Rendering.Contracts;

/// <summary>
/// Where the 3D camera is and what it looks at, in render space. Simulation (x, y) maps to render (x, 0, y)
/// (ADR-0003); the up vector is always +Y.
/// </summary>
public readonly record struct CameraView(Vector3 Position, Vector3 Target, float FieldOfViewDegrees);
