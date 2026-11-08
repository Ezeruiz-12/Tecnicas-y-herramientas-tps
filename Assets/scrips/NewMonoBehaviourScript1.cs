using UnityEngine;

public class NewMonoBehaviourScript1 : MonoBehaviour
{
    float Timeractual = 5f;
    

    float Numeronuevo(int a)
    {
        return 0;
    }

    void Update()
    {
        Timeractual = Timeractual - Time.deltaTime;
        Debug.Log(Timeractual);
        if (Timeractual <= 0)
        {

            Debug.Log("pasaron 5 segundos");
        }
    }
    
}
