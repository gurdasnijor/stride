using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace Stride.CommunityToolkit.Rendering.ProceduralModels;

/// <summary>
/// Generates a textured rectangle (quad) in the XY plane.
/// </summary>
public class RectangleProceduralModel : PrimitiveProceduralModelBase
{
    /// <summary>
    /// Gets or sets the size of the object as a two-dimensional vector.
    /// </summary>
    public Vector2 Size { get; set; } = new(0.5f, 1);

    private static readonly Vector2[] _textureCoordinates = [new(1, 0), new(1, 1), new(0, 1), new(0, 0)];
    private static readonly Dictionary<MeshCacheKey, GeometricMeshData<VertexPositionNormalTexture>> _meshCache = [];

    private readonly record struct MeshCacheKey(Vector2 Size, float UScale, float VScale, bool ToLeftHanded);

    /// <inheritdoc />
    protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        => New(Size, UvScale.X, UvScale.Y);

    /// <summary>
    /// Creates (or retrieves from cache) a rectangle mesh of the given size and UV scale.
    /// </summary>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="uScale">The U coordinate scale factor.</param>
    /// <param name="vScale">The V coordinate scale factor.</param>
    /// <param name="toLeftHanded">Whether to convert the mesh to left-handed coordinates.</param>
    /// <returns>A geometric mesh data instance representing the rectangle.</returns>
    public static GeometricMeshData<VertexPositionNormalTexture> New(Vector2 size, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        var key = new MeshCacheKey(size, uScale, vScale, toLeftHanded);

        if (_meshCache.TryGetValue(key, out var mesh)) return mesh;

        mesh = CreateMesh(size, uScale, vScale, toLeftHanded);
        _meshCache[key] = mesh;

        return mesh;
    }

    /// <summary>
    /// Builds a new rectangle mesh (no caching).
    /// </summary>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="uScale">The U coordinate scale factor.</param>
    /// <param name="vScale">The V coordinate scale factor.</param>
    /// <param name="toLeftHanded">Whether to convert the mesh to left-handed coordinates.</param>
    /// <returns>A geometric mesh data instance representing the rectangle.</returns>
    public static GeometricMeshData<VertexPositionNormalTexture> CreateMesh(Vector2 size, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        Span<VertexPositionNormalTexture> vertices = stackalloc VertexPositionNormalTexture[4];
        Span<int> indices = stackalloc int[6];
        Span<Vector2> textureCoordinates = stackalloc Vector2[4];

        size /= 2.0f;
        var uvScale = new Vector2(uScale, vScale);

        for (var i = 0; i < 4; i++)
            textureCoordinates[i] = _textureCoordinates[i] * uvScale;

        // Four vertices
        vertices[0] = new VertexPositionNormalTexture(new Vector3(-size.X, -size.Y, 0), Vector3.UnitZ, textureCoordinates[0]);
        vertices[1] = new VertexPositionNormalTexture(new Vector3(-size.X, size.Y, 0), Vector3.UnitZ, textureCoordinates[1]);
        vertices[2] = new VertexPositionNormalTexture(new Vector3(size.X, size.Y, 0), Vector3.UnitZ, textureCoordinates[2]);
        vertices[3] = new VertexPositionNormalTexture(new Vector3(size.X, -size.Y, 0), Vector3.UnitZ, textureCoordinates[3]);

        // Triangle indices
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;

        // Create the primitive object.
        return new GeometricMeshData<VertexPositionNormalTexture>(vertices.ToArray(), indices.ToArray(), toLeftHanded) { Name = "Rectangle" };
    }
}