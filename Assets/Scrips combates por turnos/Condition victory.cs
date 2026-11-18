using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Muestra el cartel de victoria cuando ya no quedan enemigos y, tras una
/// pausa, vuelve a cargar el menú principal.
/// </summary>
public class VictoryCondition : MonoBehaviour
{
    [Header("Enemigos")]
    [Tooltip("Arrastra aquí los tres objetos enemigos de la escena.")]
    [SerializeField] private GameObject[] enemies = new GameObject[3];

    [Header("Victoria")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField, Min(0f)] private float waitBeforeMenu = 3f;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool victoryStarted;
    private bool enemiesConfigured;

    private void Start()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        enemiesConfigured = enemies != null && enemies.Length == 3;
        if (enemiesConfigured)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                {
                    enemiesConfigured = false;
                    break;
                }
            }
        }

        if (!enemiesConfigured)
            Debug.LogError("VictoryCondition: asigna los tres enemigos en el Inspector; la condición de victoria no se activará.", this);
        else
            Debug.Log("VictoryCondition lista: se están vigilando los tres enemigos asignados.", this);
    }

    private void Update()
    {
        if (victoryStarted || !enemiesConfigured)
            return;

        int aliveEnemies = 0;
        foreach (GameObject enemy in enemies)
        {
            // Algunos sistemas desactivan al enemigo al morir en vez de destruirlo.
            if (enemy != null && enemy.activeInHierarchy)
                aliveEnemies++;
        }

        if (aliveEnemies == 0)
            TriggerVictory();
    }

    /// <summary>
    /// Llama a este método desde TurnBasedController justo cuando detecta
    /// que todos los enemigos fueron derrotados.
    /// </summary>
    public void TriggerVictory()
    {
        if (victoryStarted)
            return;

        if (!enemiesConfigured)
        {
            Debug.LogError("VictoryCondition: configura los tres enemigos en el Inspector.", this);
            return;
        }

        Debug.Log("VictoryCondition: victoria activada. Mostrando el cartel.", this);
        StartCoroutine(ShowVictoryAndReturnToMenu());
    }

    private IEnumerator ShowVictoryAndReturnToMenu()
    {
        victoryStarted = true;

        if (victoryPanel != null)
        {
            // Si el Canvas completo estaba apagado, encenderlo antes del panel.
            Canvas parentCanvas = victoryPanel.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
                parentCanvas.gameObject.SetActive(true);

            victoryPanel.SetActive(true);
        }
        else
            Debug.LogError("VictoryCondition: detectó la victoria, pero Victory Panel no está asignado en el Inspector.", this);

        // Tiempo real para que el cartel y el retorno funcionen aunque el juego esté pausado.
        yield return new WaitForSecondsRealtime(waitBeforeMenu);

        if (string.IsNullOrWhiteSpace(mainMenuScene))
        {
            Debug.LogError("VictoryCondition: escribe el nombre de la escena del menú principal en Main Menu Scene.", this);
            yield break;
        }

        SceneManager.LoadScene(mainMenuScene);
    }
}
