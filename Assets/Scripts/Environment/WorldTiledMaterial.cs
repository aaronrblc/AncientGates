using UnityEngine;

// Ajusta el tiling del material según el tamaño real del objeto (localScale),
// evitando que la textura se estire al escalar cubos o planos.
// Usa renderer.material (instancia por objeto) porque _BaseMap_ST vive en el
// CBuffer del SRP Batcher y no puede sobreescribirse con MaterialPropertyBlock.
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class WorldTiledMaterial : MonoBehaviour
{
    [Tooltip("Unidades de mundo que ocupa un tile de textura.")]
    [Min(0.01f)] public float UnidadesPorTile = 1f;

    [Tooltip("Eje que se usa para la U (horizontal del tile). X para paredes frontales/traseras, Z para laterales.")]
    public Eje EjeU = Eje.X;

    [Tooltip("Eje que se usa para la V (vertical del tile). Casi siempre Y.")]
    public Eje EjeV = Eje.Y;

    public enum Eje { X, Y, Z }

    private void OnEnable()   => Aplicar();
    private void OnValidate() => Aplicar();

    private void Aplicar()
    {
        if (!TryGetComponent<Renderer>(out var rend)) return;

        Vector3 s    = transform.localScale;
        float tilesU = ValorEje(s, EjeU) / UnidadesPorTile;
        float tilesV = ValorEje(s, EjeV) / UnidadesPorTile;

        // En edit mode usamos sharedMaterial para no crear instancias; en play mode .material
        var mat = Application.isPlaying ? rend.material : rend.sharedMaterial;
        string prop = EncontrarPropAlbedo(mat);
        if (prop == null) { Debug.LogWarning($"[WorldTiledMaterial] '{mat.shader.name}' no tiene propiedad de albedo conocida. Propiedades de textura: {string.Join(", ", mat.GetTexturePropertyNames())}", this); return; }
        mat.SetTextureScale(prop, new Vector2(tilesU, tilesV));
    }

    // Prueba nombres de albedo en orden de probabilidad antes de hacer fallback
    private static readonly string[] _candidatos = { "_BaseMap", "_MainTex", "_BaseColorMap", "_AlbedoTex", "_Diffuse" };

    private static string EncontrarPropAlbedo(Material mat)
    {
        foreach (var c in _candidatos)
            if (mat.HasProperty(c)) return c;
        string[] todas = mat.GetTexturePropertyNames();
        return todas.Length > 0 ? todas[0] : null;
    }

    private static float ValorEje(Vector3 v, Eje eje) => eje switch
    {
        Eje.X => v.x,
        Eje.Y => v.y,
        Eje.Z => v.z,
        _     => v.x,
    };
}
