using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace Stride.CommunityToolkit.Rendering.ProceduralModels;

/// <summary>
/// Procedurally generates a filled 2D circle mesh oriented on the XY plane.
/// Meshes are cached per (radius, tessellation, UV scale, handedness) for reuse.
/// </summary>
public class CircleProceduralModel : PrimitiveProceduralModelBase
{
    /// <summary>
    /// Circle radius.
    /// </summary>
    public float Radius { get; set; } = 0.5f;
    /// <summary>
    /// Number of perimeter segments (higher = smoother circle).
    /// </summary>
    public int Tessellation { get; set; } = 32;

    private static readonly Dictionary<(float Radius, int Tessellation, float UScale, float VScale, bool IsLeftHanded), GeometricMeshData<VertexPositionNormalTexture>> _meshCache = [];

    /// <summary>
    /// Creates mesh data for the current radius, tessellation, and UV scale settings.
    /// </summary>
    protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData() => New(Radius, Tessellation, UvScale.X, UvScale.Y);

    /// <summary>
    /// Retrieves (or builds and caches) a circle mesh with the specified parameters.
    /// </summary>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="tessellation">Number of segments around the perimeter.</param>
    /// <param name="uScale">Texture U scale factor.</param>
    /// <param name="vScale">Texture V scale factor.</param>
    /// <param name="toLeftHanded">If true, converts to left-handed coordinate system.</param>
    /// <returns>Mesh data for a circle.</returns>
    public static GeometricMeshData<VertexPositionNormalTexture> New(float radius = 0.5f, int tessellation = 32, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        var cacheKey = (radius, tessellation, uScale, vScale, toLeftHanded);

        if (_meshCache.TryGetValue(cacheKey, out var mesh))
            return mesh;

        mesh = CreateMesh(radius, tessellation, uScale, vScale, toLeftHanded);
        _meshCache[cacheKey] = mesh;

        return mesh;
    }

    /// <summary>
    /// Builds a new circle mesh (without caching) using provided parameters.
    /// </summary>
    /// <inheritdoc cref="New(float, int, float, float, bool)"/>
    public static GeometricMeshData<VertexPositionNormalTexture> CreateMesh(float radius = 0.5f, int tessellation = 32, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        // Use stack allocation for small arrays
        var vertices = tessellation <= 128
            ? stackalloc VertexPositionNormalTexture[tessellation + 1]
            : new VertexPositionNormalTexture[tessellation + 1];

        var indices = tessellation <= 128
            ? stackalloc int[tessellation * 3]
            : new int[tessellation * 3];

        var normal = Vector3.UnitZ;

        // Center vertex
        vertices[0] = new VertexPositionNormalTexture(Vector3.Zero, normal, new Vector2(0.5f, 0.5f) * new Vector2(uScale, vScale));

        // Create perimeter-vertices-FLIPPED X and Y coordinates
        for (var i = 0; i < tessellation; i++)
        {
            var angle = (float)(i * 2.0 * Math.PI / tessellation);

            // The key change: swap X and Y coordinates and negate one coordinate
            // to match the orientation of your Rectangle model
            var x = radius * (float)Math.Cos(angle);
            var y = radius * (float)Math.Sin(angle);

            var texCoord = new Vector2(
                0.5f + ((float)Math.Cos(angle) * 0.5f),
                0.5f + ((float)Math.Sin(angle) * 0.5f)
            );

            vertices[i + 1] = new VertexPositionNormalTexture(
                new Vector3(x, y, 0),
                normal,
                texCoord * new Vector2(uScale, vScale)
            );
        }

        // Create triangles
        for (var i = 0; i < tessellation; i++)
        {
            indices[i * 3] = 0; // Center vertex
            indices[i * 3 + 1] = 1 + ((i + 1) % tessellation);
            indices[i * 3 + 2] = 1 + i;
        }

        return new GeometricMeshData<VertexPositionNormalTexture>(vertices.ToArray(), indices.ToArray(), toLeftHanded) { Name = "Circle" };
    }
}