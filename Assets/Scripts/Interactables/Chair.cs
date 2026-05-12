using UnityEngine;

public class Chair : MonoBehaviour, IInteractable
{
    [Header("Настройки кресла")]
    [SerializeField] private string prompt = "Сесть";
    [SerializeField] private Transform sitPoint; 
    [SerializeField] private KeyCode standUpKey = KeyCode.Space;
    
    [Header("Визуализация")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(0.8f, 0.8f, 0.5f, 1f);
    
    private Color[][] originalColors;
    private bool isOccupied = false;
    private GameObject currentPlayerObj;
    private FirstPersonController fpc;
    
    private Vector3 standPosition;
    
    private bool wasPlayerCanMove;
    private bool wasEnableJump;
    private bool wasEnableCrouch;
    private bool wasEnableHeadBob;
    
    public string InteractionPrompt => prompt;

    private void Awake()
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
        {
            meshRenderers = GetComponentsInChildren<Renderer>();
        }

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length][];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null)
                {
                    Material[] mats = meshRenderers[i].materials;
                    originalColors[i] = new Color[mats.Length];
                    for (int j = 0; j < mats.Length; j++)
                    {
                        if (mats[j].HasProperty("_BaseColor"))
                            originalColors[i][j] = mats[j].GetColor("_BaseColor");
                        else if (mats[j].HasProperty("_Color"))
                            originalColors[i][j] = mats[j].color;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (isOccupied && Input.GetKeyDown(standUpKey))
        {
            StandUp();
        }
    }

    public bool Interact(GameObject interactor)
    {
        if (isOccupied) return false;
        
        SitDown(interactor);
        return true; 
    }

    private void SitDown(GameObject interactor)
    {
        fpc = interactor.GetComponentInParent<FirstPersonController>();
        if (fpc == null) return;
        
        currentPlayerObj = fpc.gameObject;
        standPosition = currentPlayerObj.transform.position;
        
        wasPlayerCanMove = fpc.playerCanMove;
        wasEnableJump = fpc.enableJump;
        wasEnableCrouch = fpc.enableCrouch;
        wasEnableHeadBob = fpc.enableHeadBob;
        
        fpc.playerCanMove = false;
        fpc.enableJump = false;
        fpc.enableCrouch = false;
        fpc.enableHeadBob = false;
        
        Rigidbody rb = currentPlayerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (sitPoint != null)
        {
            currentPlayerObj.transform.position = sitPoint.position;
        }
        else
        {
            currentPlayerObj.transform.position = transform.position + Vector3.up * 1.5f; 
        }

        isOccupied = true;
        OnHoverExit();
    }

    private void StandUp()
    {
        if (fpc != null)
        {
            fpc.playerCanMove = wasPlayerCanMove;
            fpc.enableJump = wasEnableJump;
            fpc.enableCrouch = wasEnableCrouch;
            fpc.enableHeadBob = wasEnableHeadBob;
        }

        Rigidbody rb = currentPlayerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        currentPlayerObj.transform.position = standPosition;
        
        isOccupied = false;
        currentPlayerObj = null;
        fpc = null;
    }

    public void OnHoverEnter()
    {
        if (isOccupied || meshRenderers == null) return;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
            {
                Material[] mats = meshRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j].HasProperty("_BaseColor"))
                        mats[j].SetColor("_BaseColor", highlightColor);
                    else if (mats[j].HasProperty("_Color"))
                        mats[j].color = highlightColor;
                }
            }
        }
    }

    public void OnHoverExit()
    {
        if (meshRenderers == null || originalColors == null) return;
        
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && originalColors[i] != null)
            {
                Material[] mats = meshRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (j < originalColors[i].Length)
                    {
                        if (mats[j].HasProperty("_BaseColor"))
                            mats[j].SetColor("_BaseColor", originalColors[i][j]);
                        else if (mats[j].HasProperty("_Color"))
                            mats[j].color = originalColors[i][j];
                    }
                }
            }
        }
    }
}