using UnityEngine;
using UnityEngine.UI;

public class ProgressManager : MonoBehaviour
{
    [SerializeField]
    private Image _fillImage;
    public float FillAmount { get => _fillImage.fillAmount; set { _fillImage.fillAmount = value; } }

    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
