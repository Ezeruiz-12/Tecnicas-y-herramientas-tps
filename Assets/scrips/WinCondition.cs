using UnityEngine;

public class WinCondition : MonoBehaviour

{
    [SerializeField] GameObject winObject;

    private bool done = false;

    private void OnTriggerEnter(Collider other)
    {
        if (done)
            return;

        winObject.SetActive(true);
        done = true;

    }
    
        
    }

