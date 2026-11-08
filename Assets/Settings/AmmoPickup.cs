using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 5;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.AddAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }
}