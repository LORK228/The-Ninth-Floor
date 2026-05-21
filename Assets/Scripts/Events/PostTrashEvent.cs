using UnityEngine;
using UnityEngine.AI;
using Zenject;
using System.Collections;

public class PostTrashEvent : MonoBehaviour
{
    [Header("Компоненты для скрипта")]
    [Tooltip("Лампочка, которая будет моргать")]
    [SerializeField] private Light flickeringLight;
    [Tooltip("Аниматор монстра")]
    [SerializeField] private Animator monsterAnimator;
    [Tooltip("Триггер, который активирует уход руки")]
    [SerializeField] private Collider hideTrigger;
    [Tooltip("Триггер, который активирует скример")]
    [SerializeField] private Collider jumpscareTrigger;
    [Tooltip("Точка, на которую будет смотреть игрок во время скримера")]
    [SerializeField] private Transform lookAtTarget;

    [Header("Настройки анимаций")]
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string hideAnimationName = "Hide";
    [SerializeField] private string jumpscareAnimationName = "Jumpscare";
    [SerializeField] private float jumpscareDuration = 2f;

    [Header("Движение монстра при скримере")]
    [Tooltip("Должен ли монстр идти к игроку во время скримера?")]
    [SerializeField] private bool moveMonsterToPlayer = true;
    [Tooltip("Скорость движения монстра к игроку")]
    [SerializeField] private float monsterMoveSpeed = 4f;
    [Tooltip("На каком расстоянии от игрока монстр должен остановиться")]
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Настройки моргания")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float flickerSpeed = 0.1f;

    [Header("Настройки квеста")]
    [Tooltip("Индекс квеста, ПОСЛЕ которого запускается это событие")]
    [SerializeField] private int triggerTaskIndex = 10;

    private ITaskManager taskManager;
    private bool eventTriggered = false;
    private bool handIsHiding = false;
    private bool jumpscareInProgress = false;

    [Inject]
    public void Construct(ITaskManager taskManager)
    {
        this.taskManager = taskManager;
    }

    private void OnEnable()
    {
        GameEventManager.OnTaskChanged += HandleTaskChanged;
    }

    private void OnDisable()
    {
        GameEventManager.OnTaskChanged -= HandleTaskChanged;
    }

    private void Start()
    {
        if (flickeringLight != null) flickeringLight.enabled = true;
        if (monsterAnimator != null) monsterAnimator.gameObject.SetActive(false);
        
        if (hideTrigger != null) 
        {
            hideTrigger.enabled = false;
            SetupTriggerRelay(hideTrigger, "Hide");
        }
        if (jumpscareTrigger != null) 
        {
            jumpscareTrigger.enabled = false;
            SetupTriggerRelay(jumpscareTrigger, "Jumpscare");
        }
    }

    private void SetupTriggerRelay(Collider col, string triggerType)
    {
        TriggerRelay relay = col.gameObject.AddComponent<TriggerRelay>();
        relay.manager = this;
        relay.triggerType = triggerType;
    }

    private void HandleTaskChanged(int newTaskIndex)
    {
        if (eventTriggered) return;

        if (newTaskIndex == triggerTaskIndex)
        {
            TriggerEvent();
        }
    }

    private void TriggerEvent()
    {
        eventTriggered = true;
        if (flickeringLight != null)
        {
            flickeringLight.enabled = true;
            StartCoroutine(FlickerCoroutine());
        }

        if (monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(idleAnimationName))
            {
                monsterAnimator.Play(idleAnimationName);
            }
        }

        if (hideTrigger != null) hideTrigger.enabled = true;
        if (jumpscareTrigger != null) jumpscareTrigger.enabled = true;
    }

    private IEnumerator FlickerCoroutine()
    {
        while (true)
        {
            flickeringLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(flickerSpeed);
        }
    }

    public void OnRelayTriggerEnter(string triggerType, Collider other)
    {
        if (!other.CompareTag("Player") || jumpscareInProgress) return;

        if (triggerType == "Hide" && !handIsHiding)
        {
            handIsHiding = true;
            if (monsterAnimator != null && !string.IsNullOrEmpty(hideAnimationName))
            {
                monsterAnimator.Play(hideAnimationName); 
            }
            hideTrigger.enabled = false; 
        }
        else if (triggerType == "Jumpscare")
        {
            jumpscareInProgress = true;
            StartCoroutine(JumpscareSequence(other.gameObject));
        }
    }

    private IEnumerator JumpscareSequence(GameObject player)
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

        if (monsterAnimator != null)
        {
            // ПРИНУДИТЕЛЬНО ОТКЛЮЧАЕМ ROOT MOTION ИЗ СКРИПТА
            monsterAnimator.applyRootMotion = false; 
            
            if (!string.IsNullOrEmpty(jumpscareAnimationName))
            {
                monsterAnimator.Play(jumpscareAnimationName); 
            }
        }

        Transform monsterTransform = monsterAnimator != null ? monsterAnimator.transform : null;
        NavMeshAgent agent = monsterTransform != null ? monsterTransform.GetComponent<NavMeshAgent>() : null;
        
        if (agent != null)
        {
            agent.isStopped = false;
            agent.stoppingDistance = stopDistance;
            agent.speed = monsterMoveSpeed;
        }

        float timer = 0f;
        Transform cameraTransform = fpc != null ? fpc.playerCamera.transform : player.transform;
        Quaternion startRotation = cameraTransform.rotation;

        while (timer < jumpscareDuration)
        {
            timer += Time.deltaTime;

            float rotProgress = Mathf.Clamp01(timer / 0.5f);
            if (lookAtTarget != null)
            {
                Vector3 directionToTarget = (lookAtTarget.position - cameraTransform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotProgress);
            }

            if (moveMonsterToPlayer && monsterTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(monsterTransform.position, player.transform.position);

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
                        targetPos.y = monsterTransform.position.y; 
                        monsterTransform.position = Vector3.MoveTowards(monsterTransform.position, targetPos, monsterMoveSpeed * Time.deltaTime);
                    }
                }
                
                Vector3 directionToPlayer = (player.transform.position - monsterTransform.position).normalized;
                directionToPlayer.y = 0; 
                if (directionToPlayer != Vector3.zero)
                {
                    monsterTransform.rotation = Quaternion.Slerp(monsterTransform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);
                }
            }

            yield return null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
        }

        ReloadScene();
    }

    private void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}

public class TriggerRelay : MonoBehaviour
{
    public PostTrashEvent manager;
    public string triggerType;

    private void OnTriggerEnter(Collider other)
    {
        if (manager != null)
        {
            manager.OnRelayTriggerEnter(triggerType, other);
        }
    }
}