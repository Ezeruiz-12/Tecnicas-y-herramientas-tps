using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TurnBasedCombat : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] GameObject player;
    [SerializeField] int playerHealth = 15;
    [SerializeField] int maxPlayerHealth = 15;
    [SerializeField] TextMeshProUGUI playerHealthText;

    [Header("Wave 1 Enemies")]
    [SerializeField] GameObject enemy1;
    [SerializeField] GameObject enemy2;
    [SerializeField] GameObject enemy3;

    [SerializeField] int enemy1Health = 10;
    [SerializeField] int enemy2Health = 5;
    [SerializeField] int enemy3Health = 5;
    [SerializeField] int maxEnemyHealth = 10;

    [Header("Wave 1 Health Texts")]
    [SerializeField] TextMeshProUGUI enemy1HealthText;
    [SerializeField] TextMeshProUGUI enemy2HealthText;
    [SerializeField] TextMeshProUGUI enemy3HealthText;

    [Header("Target Arrow")]
    [SerializeField] GameObject targetArrow;
    [SerializeField] Vector3 arrowOffset = new Vector3(0f, 0.3f, 0f);

    [Header("Attack Buttons")]
    [SerializeField] Button attackButton;
    [SerializeField] Button strongAttackButton;
    [SerializeField] Button healButton;

    [Header("Healing")]
    [SerializeField] int healingAmount = 4;

    [Header("Enemy Turns")]
    [SerializeField] float enemyThinkingTime = 2f;
    [SerializeField] float enemyDeathAnimationTime = 1f;

    private bool playerTurn = true;
    private bool actionInProgress;
    private bool gameOver;
    private VictoryCondition victoryCondition;
    private DefeatCondition defeatCondition;
    private int selectedEnemyIndex;

    private int playerStrongCooldown;
    private int healCooldown;

    private readonly List<GameObject> currentEnemies =
        new List<GameObject>();

    private readonly List<int> currentEnemyHealth =
        new List<int>();

    private readonly List<int> currentEnemyMaxHealth =
        new List<int>();

    private readonly List<int> enemyStrongCooldowns =
        new List<int>();

    private readonly List<TMP_Text> currentEnemyHealthTexts =
        new List<TMP_Text>();

    private Vector3[] enemySpawnPositions;
    private Quaternion[] enemySpawnRotations;

    void Start()
    {
        // VictoryCondition está en este mismo objeto, como en la configuración de la escena.
        victoryCondition = GetComponent<VictoryCondition>();
        if (victoryCondition == null)
            Debug.LogError("Falta agregar el componente VictoryCondition a este objeto.", this);

        // DefeatCondition también debe estar agregado a este objeto.
        defeatCondition = GetComponent<DefeatCondition>();
        if (defeatCondition == null)
            Debug.LogError("Falta agregar el componente DefeatCondition a este objeto.", this);

        AddEnemy(enemy1, enemy1Health, maxEnemyHealth, enemy1HealthText);
        AddEnemy(enemy2, enemy2Health, 5, enemy2HealthText);
        AddEnemy(enemy3, enemy3Health, 5, enemy3HealthText);

        attackButton.onClick.AddListener(PlayerBasicAttack);
        strongAttackButton.onClick.AddListener(PlayerStrongAttack);
        healButton.onClick.AddListener(PlayerHeal);

        SelectFirstLivingEnemy();
        UpdateUI();
        UpdateTargetArrow();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TrySelectEnemyWithClick();
    }

    void LateUpdate()
    {
        UpdateTargetArrow();
    }

    void AddEnemy(
        GameObject enemyObject,
        int health,
        int maxHealth,
        TMP_Text healthText = null
    )
    {
        currentEnemies.Add(enemyObject);
        currentEnemyHealth.Add(health);
        currentEnemyMaxHealth.Add(maxHealth);
        enemyStrongCooldowns.Add(0);

        if (healthText == null && enemyObject != null)
            healthText = enemyObject.GetComponentInChildren<TMP_Text>(true);

        currentEnemyHealthTexts.Add(healthText);
        UpdateEnemyHealthText(currentEnemyHealth.Count - 1);
    }

    void TrySelectEnemyWithClick()
    {
        if (!playerTurn || actionInProgress)
            return;

        if (EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

        if (hit2D.collider != null)
        {
            SelectEnemyFromTransform(hit2D.collider.transform);
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit3D))
            SelectEnemyFromTransform(hit3D.collider.transform);
    }

    void SelectEnemyFromTransform(Transform clickedTransform)
    {
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            bool clickedThisEnemy =
                clickedTransform == currentEnemies[i].transform
                || clickedTransform.IsChildOf(currentEnemies[i].transform);

            if (clickedThisEnemy && currentEnemyHealth[i] > 0)
            {
                selectedEnemyIndex = i;
                UpdateButtons();
                UpdateTargetArrow();
                return;
            }
        }
    }

    void UpdateTargetArrow()
    {
        if (targetArrow == null)
            return;

        if (GetFirstLivingEnemyIndex() == -1)
        {
            targetArrow.SetActive(false);
            return;
        }

        if (selectedEnemyIndex < 0
            || selectedEnemyIndex >= currentEnemies.Count
            || currentEnemyHealth[selectedEnemyIndex] <= 0)
        {
            SelectFirstLivingEnemy();
        }

        targetArrow.SetActive(true);
        targetArrow.transform.position =
            currentEnemies[selectedEnemyIndex].transform.position
            + arrowOffset;
    }

    void PlayerBasicAttack()
    {
        if (!CanPlayerAct())
            return;

        StartPlayerAction();

        int targetIndex = selectedEnemyIndex;

        StartCoroutine(DoAttack(
            player,
            currentEnemies[targetIndex],
            player.transform.position,
            currentEnemies[targetIndex].transform.position,
            () => FinishPlayerAttack(targetIndex, 2)
        ));
    }

    void PlayerStrongAttack()
    {
        if (!CanPlayerAct() || playerStrongCooldown > 0)
            return;

        StartPlayerAction();
        playerStrongCooldown = 3;

        int targetIndex = selectedEnemyIndex;

        StartCoroutine(DoAttack(
            player,
            currentEnemies[targetIndex],
            player.transform.position,
            currentEnemies[targetIndex].transform.position,
            () => FinishPlayerAttack(targetIndex, 5)
        ));
    }

    void PlayerHeal()
    {
        if (!CanPlayerAct() || healCooldown > 0)
            return;

        StartPlayerAction();

        playerHealth = Mathf.Min(
            playerHealth + healingAmount,
            maxPlayerHealth
        );

        healCooldown = 4;

        UpdateUI();
        StartCoroutine(EndPlayerTurn());
    }

    bool CanPlayerAct()
    {
        return playerTurn
            && !actionInProgress
            && !gameOver
            && playerHealth > 0
            && GetFirstLivingEnemyIndex() != -1
            && selectedEnemyIndex >= 0
            && selectedEnemyIndex < currentEnemyHealth.Count
            && currentEnemyHealth[selectedEnemyIndex] > 0;
    }

    void StartPlayerAction()
    {
        actionInProgress = true;
        UpdateButtons();
    }

    void FinishPlayerAttack(int targetIndex, int damage)
    {
        if (targetIndex < 0 || targetIndex >= currentEnemyHealth.Count)
            return;

        currentEnemyHealth[targetIndex] =
            Mathf.Max(0, currentEnemyHealth[targetIndex] - damage);

        UpdateEnemyHealthText(targetIndex);

        if (currentEnemyHealth[targetIndex] <= 0)
            StartCoroutine(PlayEnemyDeath(currentEnemies[targetIndex]));

        if (currentEnemyHealth[selectedEnemyIndex] <= 0)
            SelectFirstLivingEnemy();

        UpdateUI();
        UpdateTargetArrow();

        if (GetFirstLivingEnemyIndex() == -1)
        {
            gameOver = true;
            Debug.Log("You defeated all enemies!");
            if (victoryCondition != null)
                victoryCondition.TriggerVictory();
            UpdateButtons();
            return;
        }

        StartCoroutine(EndPlayerTurn());
    }

    IEnumerator PlayEnemyDeath(GameObject enemy)
    {
        if (enemy == null)
            yield break;

        Animator enemyAnimator = enemy.GetComponentInChildren<Animator>();

        if (HasValidAnimator(enemyAnimator))
        {
            enemyAnimator.SetBool("Death", true);
            yield return new WaitForSeconds(enemyDeathAnimationTime);
            enemyAnimator.SetBool("Death", false);
        }

        if (enemy != null)
            enemy.SetActive(false);
    }

    IEnumerator PlayPlayerDeath()
    {
        Animator playerAnimator = player.GetComponentInChildren<Animator>();

        if (HasValidAnimator(playerAnimator))
        {
            // Keep Death enabled so the player remains in the death state.
            playerAnimator.SetBool("Death", true);
            yield return new WaitForSeconds(enemyDeathAnimationTime);
        }
    }

    bool HasValidAnimator(Animator animator)
    {
        return animator != null &&
               animator.runtimeAnimatorController != null;
    }
    IEnumerator EndPlayerTurn()
    {
        yield return null;

        playerTurn = false;
        actionInProgress = false;
        UpdateButtons();

        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            if (playerHealth <= 0)
            {
                gameOver = true;
                UpdateButtons();
                yield break;
            }

            if (currentEnemyHealth[i] <= 0)
                continue;

            // Each living enemy waits before choosing and using its attack.
            yield return new WaitForSeconds(enemyThinkingTime);

            if (playerHealth <= 0)
            {
                gameOver = true;
                UpdateButtons();
                yield break;
            }

            bool enemyHasStrongAttack = currentEnemyMaxHealth[i] > 5;

            bool useStrongAttack =
                enemyHasStrongAttack
                && enemyStrongCooldowns[i] <= 0
                && Random.value < 0.5f;

            int damage = enemyHasStrongAttack
                ? (useStrongAttack ? 2 : 1)
                : 1;
            if (useStrongAttack)
                enemyStrongCooldowns[i] = 3;

            Vector3 enemyStart =
                currentEnemies[i].transform.position;

            Vector3 playerStart =
                player.transform.position;

            yield return DoAttack(
                currentEnemies[i],
                player,
                enemyStart,
                playerStart,
                () =>
                {
                    playerHealth = Mathf.Max(0, playerHealth - damage);
                    UpdateUI();
                }
            );

            if (playerHealth <= 0)
            {
                yield return PlayPlayerDeath();
                Debug.Log("Player defeated!");
                if (defeatCondition != null)
                    defeatCondition.CheckPlayerHealth(playerHealth);
                gameOver = true;
                UpdateButtons();
                yield break;
            }

            enemyStrongCooldowns[i] =
                Mathf.Max(0, enemyStrongCooldowns[i] - 1);
        }

        playerStrongCooldown =
            Mathf.Max(0, playerStrongCooldown - 1);

        healCooldown =
            Mathf.Max(0, healCooldown - 1);

        playerTurn = true;
        actionInProgress = false;
        UpdateButtons();
    }

    void UpdateEnemyHealthText(int enemyIndex)
    {
        if (enemyIndex < 0
            || enemyIndex >= currentEnemyHealthTexts.Count)
        {
            return;
        }

        TMP_Text healthText = currentEnemyHealthTexts[enemyIndex];

        if (healthText != null)
        {
            healthText.text =
                currentEnemyHealth[enemyIndex]
                + " / "
                + currentEnemyMaxHealth[enemyIndex];
        }
    }

    IEnumerator DoAttack(
        GameObject attacker,
        GameObject target,
        Vector3 attackerStart,
        Vector3 targetStart,
        System.Action onComplete
    )
    {
        Animator attackerAnimator = attacker.GetComponentInChildren<Animator>();
        bool canAnimateAttacker = HasValidAnimator(attackerAnimator);

        if (canAnimateAttacker)
            attackerAnimator.SetBool("Attack", true);
        Vector3 attackPosition =
            attackerStart
            + (targetStart - attackerStart).normalized * 0.5f;

        Vector3 targetPushPosition =
            targetStart
            + (targetStart - attackerStart).normalized * 0.3f;

        yield return MoveOverTime(
            attacker,
            attackerStart,
            attackPosition,
            0.6f
        );

        yield return MoveOverTime(
            attacker,
            attackPosition,
            attackerStart,
            0.6f
        );

        Animator targetAnimator = target.GetComponentInChildren<Animator>();
        bool canAnimateTarget = HasValidAnimator(targetAnimator);

        if (canAnimateTarget)
            targetAnimator.SetBool("Hit", true);

        yield return MoveOverTime(
            target,
            targetStart,
            targetPushPosition,
            0.65f
        );

        yield return MoveOverTime(
            target,
            targetPushPosition,
            targetStart,
            0.6f
        );

        if (canAnimateTarget)
            targetAnimator.SetBool("Hit", false);

        if (canAnimateAttacker)
            attackerAnimator.SetBool("Attack", false);

        onComplete?.Invoke();
    }

    IEnumerator MoveOverTime(
        GameObject obj,
        Vector3 startPosition,
        Vector3 endPosition,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                elapsed / duration
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    int GetFirstLivingEnemyIndex()
    {
        for (int i = 0; i < currentEnemyHealth.Count; i++)
        {
            if (currentEnemyHealth[i] > 0)
                return i;
        }

        return -1;
    }

    void SelectFirstLivingEnemy()
    {
        int firstLivingEnemy = GetFirstLivingEnemyIndex();

        if (firstLivingEnemy != -1)
            selectedEnemyIndex = firstLivingEnemy;
    }

    void UpdateUI()
    {
        if (playerHealthText != null)
        {
            playerHealthText.text =
                playerHealth + " / " + maxPlayerHealth;
        }


        for (int i = 0; i < currentEnemyHealth.Count; i++)
            UpdateEnemyHealthText(i);

        UpdateButtons();
    }

    void UpdateButtons()
    {
        bool playerCanAct =
            playerTurn
            && !actionInProgress
            && !gameOver
            && playerHealth > 0
            && GetFirstLivingEnemyIndex() != -1;

        bool selectedEnemyIsAlive =
            selectedEnemyIndex >= 0
            && selectedEnemyIndex < currentEnemyHealth.Count
            && currentEnemyHealth[selectedEnemyIndex] > 0;

        attackButton.interactable =
            playerCanAct && selectedEnemyIsAlive;

        strongAttackButton.interactable =
            playerCanAct
            && selectedEnemyIsAlive
            && playerStrongCooldown <= 0;

        healButton.interactable =
            playerCanAct
            && healCooldown <= 0
            && playerHealth < maxPlayerHealth;
    }
}


