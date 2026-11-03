using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento en 8 direcciones para la vista cenital (5049 D2D3D · UT2).
///  · lee la entrada con Input System;
///  · al añadirlo, Unity añade también Rigidbody 2D y Animator si faltan;
///  · solo actualiza Horizontal y Vertical mientras hay movimiento, así el reposo
///    se queda mirando hacia donde iba el personaje (segundo Blend Tree, de reposo).
/// Parámetros del Animator: Horizontal (Float), Vertical (Float) y Speed (Float).
/// Teclado (WASD y flechas) y mando. Para el táctil basta un On-Screen Stick
/// con Control Path &lt;Gamepad&gt;/leftStick: no hay que tocar este script.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class MovimientoOchoDirecciones : MonoBehaviour
{
    [Tooltip("Unidades de Unity por segundo. Con el personaje de 32 px a PPU 16, 2; con los de Mapa de bits (metros), 1,5.")]
    [SerializeField, Range(0.5f, 10f)] float velocidad = 2f;

    [Tooltip("Hacia dónde mira al empezar (0, -1 = abajo).")]
    [SerializeField] Vector2 direccionInicial = Vector2.down;

    static readonly int ParamHorizontal = Animator.StringToHash("Horizontal");
    static readonly int ParamVertical = Animator.StringToHash("Vertical");
    static readonly int ParamSpeed = Animator.StringToHash("Speed");

    Rigidbody2D cuerpo;
    Animator animator;
    InputAction mover;
    Vector2 entrada;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        cuerpo.gravityScale = 0f;      // vista desde arriba: no hay gravedad
        cuerpo.freezeRotation = true;  // el giro lo pone la hoja de sprites, no la física
        animator = GetComponent<Animator>();

        mover = new InputAction("Mover", InputActionType.Value);
        mover.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        mover.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        mover.AddBinding("<Gamepad>/leftStick");

        Mirar(direccionInicial);
    }

    void OnEnable() => mover.Enable();
    void OnDisable() => mover.Disable();
    void OnDestroy() => mover.Dispose();

    void Update()
    {
        // Se normaliza para que en diagonal no vaya más rápido (√2 ≈ 1,41).
        Vector2 bruto = mover.ReadValue<Vector2>();
        entrada = bruto.sqrMagnitude > 1f ? bruto.normalized : bruto;

        animator.SetFloat(ParamSpeed, entrada.sqrMagnitude);
        if (entrada.sqrMagnitude > 0.001f)
            Mirar(entrada);   // al soltar no se toca: el reposo mira hacia la última dirección
    }

    void FixedUpdate()
    {
        // Con Rigidbody2D se mueve en FixedUpdate, a través de la física (choca con los colisionadores).
        cuerpo.MovePosition(cuerpo.position + entrada * velocidad * Time.fixedDeltaTime);
    }

    void Mirar(Vector2 direccion)
    {
        animator.SetFloat(ParamHorizontal, direccion.x);
        animator.SetFloat(ParamVertical, direccion.y);
    }
}
