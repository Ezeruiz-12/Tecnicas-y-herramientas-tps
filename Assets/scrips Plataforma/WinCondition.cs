using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Se coloca en el jugador y se le asigna la última moneda.
/// Al tocarla, muestra el panel de victoria y luego carga el menú.
/// </summary>
public class WinCondition : MonoBehaviour
{
    [Header("Última moneda")]
    [SerializeField] private GameObject lastCoin;

    [Header("Cartel de victoria")]
    [SerializeField] private GameObject winObject;
    [SerializeField, Min(0f)] private float waitBeforeMenu = 3f;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool done;

    private void Start()
    {
        if (winObject != null)
            winObject.SetActive(false);
    }

    /// <summary>
    /// Lo llama Coin justo antes de destruirse. Solo la moneda asignada
    /// como última moneda activa la victoria.
    /// </summary>
    public void NotifyCoinCollected(GameObject coin)
    {
        if (done || lastCoin == null || coin == null)
            return;

        bool touchedLastCoin = coin == lastCoin
            || coin.transform.IsChildOf(lastCoin.transform);

        if (!touchedLastCoin)
            return;

        done = true;
        Debug.Log("Se recogió la última moneda. Mostrando victoria.", this);
        StartCoroutine(ShowWinAndLoadMenu());
    }

    private IEnumerator ShowWinAndLoadMenu()
    {
        if (winObject != null)
        {
            // Enciende también el Canvas si estaba apagado.
            Canvas parentCanvas = winObject.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
                parentCanvas.gameObject.SetActive(true);

            winObject.SetActive(true);
        }
        else
        {
            Debug.LogError("WinCondition: asigna el panel de victoria en Win Object.", this);
        }

        yield return new WaitForSecondsRealtime(waitBeforeMenu);

        if (string.IsNullOrWhiteSpace(mainMenuScene))
        {
            Debug.LogError("WinCondition: escribe el nombre de la escena del menú en Main Menu Scene.", this);
            yield break;
        }

        SceneManager.LoadScene(mainMenuScene);
    }
}
