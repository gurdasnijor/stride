using Stride.CommunityToolkit.Rendering.ProceduralModels;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace Stride.CommunityToolkit.Bullet;

/// <summary>
/// Provides extension methods for the <see cref="Entity"/> class to simplify adding Bullet 2D and 3D physics components.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Adds Bullet 2D physics components to the entity with an appropriate collider shape based on the primitive type.
    /// </summary>
    /// <param name="entity">The entity to add physics components to.</param>
    /// <param name="type">The type of 2D primitive shape for the collider.</param>
    /// <param name="options">Optional physics configuration including the physics component, size, depth, and whether to include a collider. If null, default options are used.</param>
    /// <returns>The entity with the Bullet 2D physics components added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when an unsupported <see cref="Primitive2DModelType"/> is specified.</exception>
    public static Entity AddBullet2DPhysics(this Entity entity, Primitive2DModelType type, Bullet2DPhysicsOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        options ??= new();

        if (type == Primitive2DModelType.Circle)
        {
            entity.Transform.Rotation = Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(90));
        }

        if (options.PhysicsComponent is null) return entity;

        if (!options.IncludeCollider)
        {
            entity.Add(options.PhysicsComponent);

            return entity;
        }

        if (type == Primitive2DModelType.Triangle)
        {
            //var a = new TriangularPrismProceduralModel() { Size = new(options.Size.Value.X, options.Size.Value.Y, options.Depth) };

            var meshData = TriangularPrismProceduralModel.New(options.Size is null ? new(1, 1, options.Depth) : new(options.Size.Value.X, options.Size.Value.Y, options.Depth));

            var points = meshData.Vertices.Select(w => w.Position).ToList();
            var uintIndices = meshData.Indices.Select(w => (uint)w).ToList();
            var collider = new ConvexHullColliderShapeDesc()
            {
                //Model = model, // seems doing nothing
                Scaling = new(0.9f),
                //LocalOffset = new(20, 20, 10),
                ConvexHulls = [],
                ConvexHullsIndices = []
            };

            collider.ConvexHulls.Add([points]);
            collider.ConvexHullsIndices.Add([uintIndices]);

            //var shapee = collider.CreateShape(game.Services);
            //var collider = new ConvexHullColliderShape(points, uintIndices, Vector3.Zero);
            //var cs = new PhysicsColliderShape(descriptions);

            List<IAssetColliderShapeDesc> descriptions = [collider];

            var colliderShapeAsset = new ColliderShapeAssetDesc
            {
                Shape = new PhysicsColliderShape(descriptions)
            };

            options.PhysicsComponent.ColliderShapes.Add(colliderShapeAsset);
            //options.PhysicsComponent.ColliderShape = shapee;
            //options.PhysicsComponent.ColliderShape = collider;
        }
        else
        {
            var colliderShape = Get2DColliderShape(type, options.Size, options.Depth);

            options.PhysicsComponent.ColliderShapes.Add(colliderShape);
        }

        entity.Add(options.PhysicsComponent);

        return entity;
    }

    /// <summary>
    /// Adds Bullet 3D physics components to the entity with an appropriate collider shape based on the primitive type.
    /// </summary>
    /// <param name="entity">The entity to add physics components to.</param>
    /// <param name="type">The type of 3D primitive shape for the collider.</param>
    /// <param name="options">Optional physics configuration including the physics component, size, and whether to include a collider. If null, default options are used.</param>
    /// <returns>The entity with the Bullet 3D physics components added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <remarks>
    /// Some primitive types (Torus, Teapot) do not have a collider shape and will be added without a collider even if <c>IncludeCollider</c> is <see langword="true"/>.
    /// </remarks>
    public static Entity AddBullet3DPhysics(this Entity entity, PrimitiveModelType type, Bullet3DPhysicsOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        options ??= new();

        if (options.PhysicsComponent is null) return entity;

        if (!options.IncludeCollider)
        {
            // Should we add the PhysicsComponent even if no collider is included?
            entity.Add(options.PhysicsComponent);

            return entity;
        }

        var colliderShape = Get3DColliderShape(type, options.Size);

        if (colliderShape is null) return entity;

        options.PhysicsComponent.ColliderShapes.Add(colliderShape);

        entity.Add(options.PhysicsComponent);

        return entity;
    }

    /// <summary>
    /// Creates a 2D collider shape based on the specified primitive type.
    /// </summary>
    /// <param name="type">The type of 2D primitive shape.</param>
    /// <param name="size">Optional size for the collider. Interpretation varies by shape type.</param>
    /// <param name="depth">The depth of the 2D shape in the Z axis.</param>
    /// <returns>An <see cref="IInlineColliderShapeDesc"/> configured for the specified primitive type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unsupported <see cref="Primitive2DModelType"/> is specified.</exception>
    private static IInlineColliderShapeDesc Get2DColliderShape(Primitive2DModelType type, Vector2? size = null, float depth = 0)
        => type switch
        {
            Primitive2DModelType.Rectangle => size is null ? new BoxColliderShapeDesc() { Is2D = true } : new() { Size = new(size.Value.X, size.Value.Y, 0), Is2D = true },
            Primitive2DModelType.Square => size is null ? new BoxColliderShapeDesc() { Is2D = true } : new() { Size = new(size.Value.X, size.Value.Y, 0), Is2D = true },
            Primitive2DModelType.Circle => size is null ? new SphereColliderShapeDesc() : new() { Radius = size.Value.X, Is2D = true },
            Primitive2DModelType.Capsule => size is null ? new CapsuleColliderShapeDesc() : new() { Radius = size.Value.X, Length = size.Value.Y - 2 * size.Value.X, Is2D = true },
            _ => throw new InvalidOperationException(),
        };

    /// <summary>
    /// Creates a 3D collider shape based on the specified primitive type.
    /// </summary>
    /// <param name="type">The type of 3D primitive shape.</param>
    /// <param name="size">Optional size for the collider. Interpretation varies by shape type (e.g., radius, dimensions).</param>
    /// <returns>An <see cref="IInlineColliderShapeDesc"/> configured for the specified primitive type, or <c>null</c> if the shape type does not support colliders (e.g., Torus, Teapot).</returns>
    /// <exception cref="InvalidOperationException">Thrown when an unsupported <see cref="PrimitiveModelType"/> is specified.</exception>
    private static IInlineColliderShapeDesc? Get3DColliderShape(PrimitiveModelType type, Vector3? size = null)
        => type switch
        {
            PrimitiveModelType.Plane => size is null ? new BoxColliderShapeDesc() : new() { Size = new Vector3(size.Value.X, 0, size.Value.Y) },
            PrimitiveModelType.InfinitePlane => new StaticPlaneColliderShapeDesc(),
            PrimitiveModelType.Sphere => size is null ? new SphereColliderShapeDesc() : new() { Radius = size.Value.X },
            PrimitiveModelType.Cube => size is null ? new BoxColliderShapeDesc() : new() { Size = (Vector3)size },
            PrimitiveModelType.Cylinder => size is null ? new CylinderColliderShapeDesc() : new() { Radius = size.Value.X, Height = size.Value.Y },
            PrimitiveModelType.Torus => null,
            PrimitiveModelType.Teapot => null,
            PrimitiveModelType.Cone => size is null ? new ConeColliderShapeDesc() : new() { Radius = size.Value.X, Height = size.Value.Y },
            PrimitiveModelType.Capsule => size is null ? new CapsuleColliderShapeDesc() { Radius = 0.35f } : new() { Radius = size.Value.X, Length = size.Value.Y },
            _ => throw new InvalidOperationException(),
        };
}