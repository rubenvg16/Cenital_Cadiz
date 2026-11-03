using UnityEngine;

/// <summary>
/// Cámara del nivel cenital: sigue al personaje en X y en Y sin salirse del nivel.
/// Va en la Main Camera (ortográfica). El zoom se ajusta con «Tamaño» (Orthographic Size): 4,5 ve 16 x 9 m;
/// 5,4 ve 19,2 x 10,8 m, la escala del documento. Más pequeño = más cerca.
/// El nivel se marca con un rectángulo (Límites) o, si se prefiere, con el SpriteRenderer del suelo.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CamaraCenital : MonoBehaviour
{
    public Transform objetivo;

    [Tooltip("Orthographic Size: medio alto de lo que se ve, en metros. Más pequeño = personaje más grande.")]
    [Range(2f, 6f)] public float tamano = 4.5f;

    [Tooltip("Rectángulo del nivel en metros (x, y = esquina de abajo-izquierda). Se usa si Ancho > 0.")]
    public Rect limites;

    [Tooltip("Alternativa a Límites: el suelo de la sala como un solo sprite.")]
    public SpriteRenderer sala;

    [Tooltip("Segundos que tarda en alcanzar al personaje. 0 = pegada a él.")]
    public float suavizado = 0.15f;

    Camera cam;
    Vector3 velocidad;

    void Awake() { cam = GetComponent<Camera>(); cam.orthographicSize = tamano; }

    void OnValidate() { var c = GetComponent<Camera>(); if (c != null) c.orthographicSize = tamano; }

    void LateUpdate()
    {
        if (objetivo == null) return;
        cam.orthographicSize = tamano;

        Vector3 destino = new Vector3(objetivo.position.x, objetivo.position.y, transform.position.z);

        Bounds b;
        bool hay = true;
        if (limites.width > 0) b = new Bounds(limites.center, limites.size);
        else if (sala != null) b = sala.bounds;
        else { b = new Bounds(); hay = false; }

        if (hay)
        {
            float medioAlto = cam.orthographicSize;
            float medioAncho = medioAlto * cam.aspect;
            destino.x = Limitar(destino.x, b.min.x + medioAncho, b.max.x - medioAncho, b.center.x);
            destino.y = Limitar(destino.y, b.min.y + medioAlto, b.max.y - medioAlto, b.center.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidad, suavizado);
    }

    // Si el nivel es más pequeño que lo que ve la cámara en ese eje, se queda centrada.
    static float Limitar(float valor, float min, float max, float centro) =>
        min > max ? centro : Mathf.Clamp(valor, min, max);
}
