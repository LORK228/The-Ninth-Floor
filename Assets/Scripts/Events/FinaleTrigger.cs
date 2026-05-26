using UnityEngine;
using UnityEngine.AI;
using Zenject;
using System.Collections;
using UnityEngine.SceneManagement; // Добавлено для работы со сценами

public class FinaleTrigger : MonoBehaviour
{
    public enum FinaleChoice { Window, Door, Bed }
    
    [Header("Настройки финала")]
    [SerializeField] private FinaleChoice choice;
    [Tooltip("Индекс последнего квеста (Лечь спать)")]
    [SerializeField] private int finaleTaskIndex = 17;

    [Header("Для концовок со смертью (Окно/Дверь)")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform monsterSpawnPoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpscareSound;
    [SerializeField] private float reloadDelay = 2f;
    [Tooltip("Точка, на которую будет смотреть игрок (обычно это спавн монстра)")]
    [SerializeField] private Transform lookAtTarget;

    [Header("Движение монстра при смерти")]
    [SerializeField] private bool moveMonsterToPlayer = true;
    [SerializeField] private float monsterMoveSpeed = 4f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Для хорошей концовки (Кровать)")]
    [SerializeField] private GameObject happyEndScreen;
    [Tooltip("Имя сцены с главным меню (например, 'MainMenuScene')")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [Tooltip("Сколько секунд показывать экран концовки перед выходом в меню")]
    [SerializeField] private float timeBeforeMenu = 5f;

    private ITaskManager taskManager;
    private bool isTriggered = false;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;
        
        if (taskManager != null && taskManager.GetCurrentTaskIndex() >= finaleTaskIndex)
        {
            isTriggered = true;
            StartCoroutine(ExecuteFinale(other.gameObject));
        }
    }

    private IEnumerator ExecuteFinale(GameObject player)
    {
        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        if (fpc != null)
        {
            fpc.enabled = false;
        }

        if (choice == FinaleChoice.Bed)
        {
            Debug.Log("ХЭППИ ЭНД!");
            
            if (happyEndScreen != null)
            {
                happyEndScreen.SetActive(true);
            }
            
            yield return new WaitForSeconds(timeBeforeMenu);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else // Window or Door (Death)
        {
            Debug.Log($"Плохая концовка: {choice}");
            
            GameObject monster = null;
            Animator anim = null;
            NavMeshAgent agent = null;

            if (monsterPrefab != null && monsterSpawnPoint != null)
            {
                monster = Instantiate(monsterPrefab, monsterSpawnPoint.position, monsterSpawnPoint.rotation);
                anim = monster.GetComponentInChildren<Animator>();
                agent = monster.GetComponent<NavMeshAgent>();

                if (anim != null)
                {
                    anim.applyRootMotion = false;
                    anim.Play("Jumpscare");
                }
                
                if (agent != null)
                {
                    agent.isStopped = false;
                    agent.stoppingDistance = stopDistance;
                    agent.speed = monsterMoveSpeed;
                }
            }

            if (audioSource != null && jumpscareSound != null)
            {
                audioSource.PlayOneShot(jumpscareSound);
            }

            float timer = 0f;
            Transform cameraTransform = fpc != null ? fpc.playerCamera.transform : player.transform;
            Quaternion startRotation = cameraTransform.rotation;
            
            Transform targetToLookAt = lookAtTarget != null ? lookAtTarget : monsterSpawnPoint;

            while (timer < reloadDelay)
            {
                timer += Time.deltaTime;

                float rotProgress = Mathf.Clamp01(timer / 0.5f);
                if (targetToLookAt != null)
                {
                    Vector3 directionToTarget = (targetToLookAt.position - cameraTransform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotProgress);
                }

                if (moveMonsterToPlayer && monster != null)
                {
                    float distanceToPlayer = Vector3.Distance(monster.transform.position, player.transform.position);

                    if (agent != null)
                    {
                        if (distanceToPlayer > stopDistance)
                        {
                            agent.SetDestination(player.transform.position);
                        }
                        else
                        {
                            if (!agent.isStopped) agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                        }
                    }
                    else
                    {
                        if (distanceToPlayer > stopDistance)
                        {
                            Vector3 targetPos = player.transform.position;
                            targetPos.y = monster.transform.position.y; 
                            monster.transform.position = Vector3.MoveTowards(monster.transform.position, targetPos, monsterMoveSpeed * Time.deltaTime);
                        }
                    }
                    
                    Vector3 directionToPlayer = (player.transform.position - monster.transform.position).normalized;
                    directionToPlayer.y = 0; 
                    if (directionToPlayer != Vector3.zero)
                    {
                        monster.transform.rotation = Quaternion.Slerp(monster.transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);
                    }
                }

                yield return null;
            }

            if (agent != null)
            {
                agent.isStopped = true;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}