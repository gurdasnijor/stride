using BepuPhysics;
using BepuPhysics.Collidables;
using Stride.BepuPhysics;
using Stride.BepuPhysics.Components;
using Stride.BepuPhysics.Definitions.Colliders;
using Stride.Engine;
using NRigidPose = BepuPhysics.RigidPose;

namespace Stride.CommunityToolkit.Bepu;

/// <summary>
/// Dynamic body constrained to a 2D plane (XY) while still using Bepu's 3D solver.
/// </summary>
/// <remarks>
/// This component locks angular motion to the Z axis by zeroing the X/Y inverse inertia terms once at attach time
/// and applies a small pre-solve velocity correction each frame to drive the body back onto the Z = 0 plane.
/// The correction avoids post-solve teleports which can inject energy and destabilize piles, especially with convex hulls.
/// </remarks>
[ComponentCategory("Physics - Bepu 2D")]
public class Body2DComponent : BodyComponent, ISimulationUpdate
{
    /// <summary>
    /// Z-position error threshold above which velocity correction is applied. Default is 0.001f (1mm).
    /// </summary>
    /// <remarks>
    /// Smaller values provide tighter constraint but may prevent sleeping. Increase if bodies jitter or fail to sleep.
    /// </remarks>
    public float ZTolerance { get; set; } = 0.001f;

    /// <summary>
    /// Creates a new <see cref="Body2DComponent"/>. Interpolation is enabled by default.
    /// </summary>
    public Body2DComponent()
        => InterpolationMode = BepuPhysics.Definitions.InterpolationMode.Interpolated;

    /// <inheritdoc />
    /// <remarks>
    /// Keeps the shape-derived inertia so roll (Z angular motion) works and zeros X/Y inverse inertia to restrict yaw/pitch.
    /// Also applies mild stability tweaks for convex hull colliders (damping and recovery velocity cap).
    /// </remarks>
    protected override void AttachInner(NRigidPose pose, BodyInertia shapeInertia, TypedIndex shapeIndex)
    {
        // Keep the shape-derived inertia so rotation (including around Z) works.
        base.AttachInner(pose, shapeInertia, shapeIndex);

        // Constrain rotation to Z by removing X/Y inverse inertia (hard lock) and clearing cross terms.
        var inertia = BodyInertia;
        var inverseInertia = inertia.InverseInertiaTensor;
        inverseInertia.XX = 0f;
        inverseInertia.YY = 0f;
        inverseInertia.YX = 0f;
        inverseInertia.ZX = 0f;
        inverseInertia.ZY = 0f; // leave ZZ for roll
        inertia.InverseInertiaTensor = inverseInertia;
        BodyInertia = inertia;

        // Hulls tend to create energetic corrections in dense piles; tame it slightly.
        if (!HasConvexHull(Collider)) return;

        MaximumRecoveryVelocity = MathF.Min(MaximumRecoveryVelocity, 1.5f);
        SpringDampingRatio = MathF.Max(SpringDampingRatio, 1f);
        SpringFrequency = MathF.Min(SpringFrequency, 30f);
    }

    /// <summary>
    /// Returns true if the collider hierarchy contains at least one <see cref="ConvexHullCollider"/>.
    /// </summary>
    private static bool HasConvexHull(object? collider)
    {
        // Direct check for ConvexHullCollider
        if (collider is ConvexHullCollider)
            return true;

        // Recursively check compound colliders
        if (collider is CompoundCollider { Colliders: { } list })
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (HasConvexHull(list[i]))
                    return true;
            }
        }

        return false;
    }

    private static bool HasConvexHullOld(ICollider? collider)
    {
        if (collider is not CompoundCollider { Colliders: { } list })
            return false;

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is ConvexHullCollider)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Called before the physics tick to perform pre-solve corrections.
    /// </summary>
    /// <param name="sim">Active simulation.</param>
    /// <param name="simTimeStep">Fixed time step size in seconds.</param>
    /// <remarks>
    /// Applies a proportional velocity correction on the Z axis to drive the body back to the plane (Z = 0)
    /// only when drift exceeds <see cref="ZTolerance"/>. Also zeros out X/Y angular velocities to prevent
    /// rotation around those axes. This avoids injecting energy while keeping the body constrained to 2D.
    /// Bodies are allowed to sleep naturally for optimal performance.
    /// </remarks>
    public virtual void SimulationUpdate(BepuSimulation sim, float simTimeStep)
    {
        // This was forcing ALL 2D bodies to stay awake every frame, completely disabling Bepu's sleep optimization
        //Awake = true;

        var zError = Position.Z;

        // Only apply Z correction if we've drifted beyond tolerance
        if (MathF.Abs(zError) > ZTolerance)
        {
            var current = LinearVelocity;
            current.Z = -zError; // proportional correction to drive back to Z=0
            LinearVelocity = current;
        }

        // Constrain angular velocity to Z-axis only (remove any X/Y angular drift)
        var angularVel = AngularVelocity;
        if (angularVel.X != 0f || angularVel.Y != 0f)
        {
            angularVel.X = 0f;
            angularVel.Y = 0f;
            AngularVelocity = angularVel;
        }
    }

    /// <summary>
    /// Called after the physics tick. Intentionally left empty for this component.
    /// </summary>
    public virtual void AfterSimulationUpdate(BepuSimulation sim, float simTimeStep) { }
}