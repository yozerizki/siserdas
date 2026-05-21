using UnityEngine;
using UnityEngine.UI;

public class CoinImageLoader : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private string resourcesFolder = "CoinImages";
    [SerializeField] private bool preserveAspect = true;
    [SerializeField] private Sprite fallbackSprite;
    [SerializeField] private MonoBehaviour calculatorController;

    private Sprite[] allSprites;
    private int currentAllIndex = 0;

    private void Start()
    {
        EnsureTargetImage();
        ShowMySereal();
    }

    // Dipanggil saat scene pertama kali muncul dan saat ButtonMySereal diklik
    public void ShowMySereal()
    {
        SwitchCalculatorToImagePanel();

        EnsureTargetImage();
        if (targetImage == null) return;

        if (GameDataHolder.Instance == null)
        {
            Debug.LogWarning("CoinImageLoader: GameDataHolder.Instance null.");
            UseFallback();
            return;
        }

        string coinName = GameDataHolder.Instance.mySereal;
        if (string.IsNullOrWhiteSpace(coinName))
        {
            Debug.LogWarning("CoinImageLoader: mySereal kosong.");
            UseFallback();
            return;
        }

        Sprite sprite = TryLoadSprite(coinName);
        if (sprite == null)
        {
            Debug.LogWarning("CoinImageLoader: Sprite tidak ditemukan untuk " + coinName);
            UseFallback();
            return;
        }

        targetImage.sprite = sprite;
        targetImage.preserveAspect = preserveAspect;
    }

    // Alias untuk kompatibilitas
    public void ApplyCoinImage() => ShowMySereal();

    // Dipanggil setiap kali ButtonAllSereal diklik - tampilkan gambar berikutnya secara berurutan
    public void ShowNextAllSereal()
    {
        SwitchCalculatorToImagePanel();

        EnsureTargetImage();
        if (targetImage == null) return;

        if (allSprites == null || allSprites.Length == 0)
        {
            allSprites = Resources.LoadAll<Sprite>(resourcesFolder);
            currentAllIndex = 0;

            if (allSprites == null || allSprites.Length == 0)
            {
                Debug.LogWarning("CoinImageLoader: Tidak ada sprite di folder " + resourcesFolder);
                UseFallback();
                return;
            }
        }

        targetImage.sprite = allSprites[currentAllIndex];
        targetImage.preserveAspect = preserveAspect;

        currentAllIndex = (currentAllIndex + 1) % allSprites.Length;
    }

    private void EnsureTargetImage()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void SwitchCalculatorToImagePanel()
    {
        if (calculatorController == null)
        {
            MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < allBehaviours.Length; i++)
            {
                MonoBehaviour candidate = allBehaviours[i];
                if (candidate != null && candidate.GetType().Name == "IngredientCalculatorController")
                {
                    calculatorController = candidate;
                    break;
                }
            }
        }

        if (calculatorController != null)
            calculatorController.SendMessage("SwitchToImagePanel", SendMessageOptions.DontRequireReceiver);
    }

    private Sprite TryLoadSprite(string coinName)
    {
        string trimmed = coinName.Trim();
        string noSpace = trimmed.Replace(" ", "");
        string underscore = trimmed.Replace(" ", "_");

        string[] paths =
        {
            resourcesFolder + "/" + trimmed,
            resourcesFolder + "/" + noSpace,
            resourcesFolder + "/" + underscore,
            trimmed,
            noSpace,
            underscore
        };

        foreach (string path in paths)
        {
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    private void UseFallback()
    {
        if (fallbackSprite == null || targetImage == null)
            return;

        targetImage.sprite = fallbackSprite;
        targetImage.preserveAspect = preserveAspect;
    }
}
