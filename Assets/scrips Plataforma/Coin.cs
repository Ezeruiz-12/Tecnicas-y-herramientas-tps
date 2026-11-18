using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private void Update()
    {
        transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // La moneda se recoge al tocarla, aunque no se encuentre WinCondition.
        // Solo la moneda asignada como última activa la victoria.
        WinCondition winCondition = other.GetComponentInParent<WinCondition>();
        if (winCondition != null)
            winCondition.NotifyCoinCollected(gameObject);
        else
            Debug.Log("Moneda recogida. No se encontró WinCondition en el jugador.", this);

        Destroy(gameObject);
    }
}
