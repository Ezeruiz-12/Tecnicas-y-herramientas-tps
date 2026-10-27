using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Enemigo : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private Transform jugador;
    [SerializeField] private float alcanceDeteccion = 5f;
    [SerializeField] private float distanciaParaDisparar = 5f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosDePatrulla;
    [SerializeField] private float distanciaParaCambiarPunto = 0.5f;

    [Header("Movimiento y vida")]
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float vida = 100f;

    [Header("Ataque")]
    [SerializeField] private float rangoAtaque = 5f;
    [SerializeField] private float dano = 20f;
    [SerializeField] private float cadencia = 2f;
    [SerializeField] private Transform puntoDeDisparo;
    [SerializeField] private LayerMask capasDelJugador;

    private Rigidbody rb;
    private int indicePuntoActual;
    private float proximoDisparo;
    private bool eliminado;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (eliminado)
        {
            DetenerMovimiento();
            return;
        }

        if (jugador != null &&
            Vector3.Distance(transform.position, jugador.position) <= alcanceDeteccion)
        {
            PerseguirJugador();
        }
        else
        {
            Patrullar();
        }
    }

    private void Update()
    {
        if (eliminado || jugador == null)
            return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= alcanceDeteccion &&
            distancia <= rangoAtaque &&
            distancia <= distanciaParaDisparar &&
            Time.time >= proximoDisparo)
        {
            Atacar();
            proximoDisparo = Time.time + cadencia;
        }
    }

    private void Patrullar()
    {
        if (puntosDePatrulla == null || puntosDePatrulla.Length == 0)
        {
            DetenerMovimiento();
            return;
        }

        Transform punto = puntosDePatrulla[indicePuntoActual];

        if (punto == null)
        {
            DetenerMovimiento();
            return;
        }

        Vector3 direccion = punto.position - transform.position;
        direccion.y = 0f;

        if (direccion.magnitude <= distanciaParaCambiarPunto)
        {
            indicePuntoActual = (indicePuntoActual + 1) % puntosDePatrulla.Length;
            return;
        }

        MoverEnDireccion(direccion.normalized);
    }

    private void PerseguirJugador()
    {
        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;

        float distancia = direccion.magnitude;

        // Se detiene al llegar a la distancia de disparo.
        if (distancia <= distanciaParaDisparar)
        {
            DetenerMovimiento();
            MirarAlJugador(direccion);
            return;
        }

        MoverEnDireccion(direccion.normalized);
    }

    private void MoverEnDireccion(Vector3 direccion)
    {
        Vector3 movimiento = direccion * velocidad * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movimiento);

        if (direccion.sqrMagnitude > 0f)
            transform.forward = direccion;
    }

    private void MirarAlJugador(Vector3 direccion)
    {
        if (direccion.sqrMagnitude > 0f)
            transform.forward = direccion.normalized;
    }

    private void DetenerMovimiento()
    {
        // Compatible con versiones de Unity que usan velocity.
        Vector3 velocidadActual = rb.linearVelocity;
        velocidadActual.x = 0f;
        velocidadActual.z = 0f;
        rb.linearVelocity = velocidadActual;
    }

    private void Atacar()
    {
        Vector3 origen = puntoDeDisparo != null
            ? puntoDeDisparo.position
            : transform.position + Vector3.up;

        Vector3 direccion = (jugador.position - origen).normalized;

        if (Physics.Raycast(origen, direccion, out RaycastHit impacto,
                            rangoAtaque, capasDelJugador))
        {
            impacto.collider.SendMessage(
                "RecibirDano",
                dano,
                SendMessageOptions.DontRequireReceiver
            );
        }

        Debug.DrawRay(origen, direccion * rangoAtaque, Color.red, 1f);
    }

    public void RecibirDano(float cantidad)
    {
        if (eliminado)
            return;

        vida = Mathf.Max(0f, vida - cantidad);

        if (vida <= 0f)
        {
            eliminado = true;
            DetenerMovimiento();
        }
    }
}
