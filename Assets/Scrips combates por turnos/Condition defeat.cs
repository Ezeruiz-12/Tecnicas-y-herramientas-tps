using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Muestra el cartel de derrota cuando la vida del jugador llega a cero
/// y reinicia el nivel actual tras una pausa.
/// </summary>
public class DefeatCondition : MonoBehaviour
{
    [Header("Derrota")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField, Min(0f)] private float waitBeforeRestart = 3f;

    private bool defeatStarted;

    private void Start()
    {
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }

    /// <summary>
    /// Llama a este método desde el sistema de vida del jugador cuando
    /// sus puntos de vida sean cero o menos.
    /// </summary>
    public void CheckPlayerHealth(float currentHealth)
    {
        if (currentHealth <= 0f && !defeatStarted)
            StartCoroutine(ShowDefeatAndRestart());
    }

    private IEnumerator ShowDefeatAndRestart()
    {
        defeatStarted = true;

        if (defeatPanel != null)
        {
            // Si el Canvas completo estaba apagado, encenderlo antes del panel.
            Canvas parentCanvas = defeatPanel.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
                parentCanvas.gameObject.SetActive(true);

            defeatPanel.SetActive(true);
        }
        else
            Debug.LogWarning("DefeatCondition: asigna el cartel de derrota en el Inspector.");

        yield return new WaitForSecondsRealtime(waitBeforeRestart);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
