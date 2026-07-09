using Stride.Graphics;
using Stride.Rendering.ProceduralModels;

namespace Stride.CommunityToolkit.Rendering.ProceduralModels;

/// <summary>
/// Generates an isosceles triangle primitive in the XY plane.
/// </summary>
public class TriangleProceduralModel : PrimitiveProceduralModelBase
{
    /// <summary>
    /// Base size of the triangle in local space.
    /// </summary>
    public Vector2 Size { get; set; } = Vector2.One;

    private static readonly Vector2[] _textureCoordinates = [new(0.5f, 1), new(0, 0), new(1, 0)];
    private static readonly Dictionary<MeshCacheKey, GeometricMeshData<VertexPositionNormalTexture>> _meshCache = [];

    private readonly record struct MeshCacheKey(Vector2 Size, float UScale, float VScale, bool ToLeftHanded);

    /// <inheritdoc />
    protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        => New(Size, UvScale.X, UvScale.Y);

    /// <summary>
    /// Creates (or retrieves from cache) a triangle mesh of the given size and UV scale.
    /// </summary>
    /// <param name="size">The size of the triangle.</param>
    /// <param name="uScale">The U coordinate scale factor.</param>
    /// <param name="vScale">The V coordinate scale factor.</param>
    /// <param name="toLeftHanded">Whether to convert the mesh to left-handed coordinates.</param>
    /// <returns>A geometric mesh data instance representing the triangle.</returns>
    public static GeometricMeshData<VertexPositionNormalTexture> New(Vector2 size, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        var key = new MeshCacheKey(size, uScale, vScale, toLeftHanded);

        if (_meshCache.TryGetValue(key, out var mesh)) return mesh;

        mesh = CreateMesh(size, uScale, vScale, toLeftHanded);
        _meshCache[key] = mesh;

        return mesh;
    }

    /// <summary>
    /// Builds a new triangle mesh (no caching).
    /// </summary>
    /// <param name="size">The size of the triangle.</param>
    /// <param name="uScale">The U coordinate scale factor.</param>
    /// <param name="vScale">The V coordinate scale factor.</param>
    /// <param name="toLeftHanded">Whether to convert the mesh to left-handed coordinates.</param>
    /// <returns>A geometric mesh data instance representing the triangle.</returns>
    public static GeometricMeshData<VertexPositionNormalTexture> CreateMesh(Vector2 size, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        Span<VertexPositionNormalTexture> vertices = stackalloc VertexPositionNormalTexture[3];
        Span<int> indices = stackalloc int[3];
        Span<Vector2> textureCoordinates = stackalloc Vector2[3];

        size /= 2.0f;
        var uvScale = new Vector2(uScale, vScale);

        for (var i = 0; i < 3; i++)
            textureCoordinates[i] = _textureCoordinates[i] * uvScale;

        // Three vertices for a triangle
        vertices[0] = new VertexPositionNormalTexture(new Vector3(0, size.Y, 0), Vector3.UnitZ, textureCoordinates[0]);
        vertices[1] = new VertexPositionNormalTexture(new Vector3(-size.X, -size.Y, 0), Vector3.UnitZ, textureCoordinates[1]);
        vertices[2] = new VertexPositionNormalTexture(new Vector3(size.X, -size.Y, 0), Vector3.UnitZ, textureCoordinates[2]);

        // Triangle indices
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;

        // Create the primitive object.
        return new GeometricMeshData<VertexPositionNormalTexture>(vertices.ToArray(), indices.ToArray(), toLeftHanded) { Name = "Triangle" };
    }
}