using UnityEngine;
using UnityEngine.UI;

public class InteractionHoldUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    void Start()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 0;
            fillImage.gameObject.SetActive(false);
        }
    }

    public void UpdateProgress(float progress)
    {
        if (fillImage == null) return;

        if (!fillImage.gameObject.activeInHierarchy)
        {
            fillImage.gameObject.SetActive(true);
        }
        
        fillImage.fillAmount = Mathf.Clamp01(progress);
    }

    public void ResetAndHide()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 0;
            fillImage.gameObject.SetActive(false);
        }
    }
}