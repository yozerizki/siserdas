using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientEntryRow : MonoBehaviour
{
    [SerializeField] private Image highlightBackground;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text unitText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private string emptyIngredientLabel = "silakan pilih bahan";
    [SerializeField] private Color normalBackgroundColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color activeBackgroundColor = new Color(1f, 0.93f, 0.65f, 0.8f);

    private IngredientData ingredient;
    private float amount;

    public IngredientData Ingredient => ingredient;
    public float Amount => amount;

    private void Awake()
    {
        if (highlightBackground == null)
            highlightBackground = GetComponent<Image>();

        SetHighlighted(false);
        RefreshTexts();
    }

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlightBackground == null)
            return;

        highlightBackground.color = isHighlighted ? activeBackgroundColor : normalBackgroundColor;
    }

    public void SetIngredient(IngredientData data)
    {
        ingredient = data;
        if (ingredient == null)
            return;

        if (amount <= 0f)
            amount = ingredient.DefaultAmount;

        RefreshTexts();
    }

    public void SetAmount(float value)
    {
        amount = Mathf.Max(0f, value);
        RefreshTexts();
    }

    public void AddAmount(float delta)
    {
        SetAmount(amount + delta);
    }

    public float GetPrice()
    {
        if (ingredient == null)
            return 0f;
        return ingredient.CalculatePrice(amount);
    }

    public float GetCalories()
    {
        if (ingredient == null)
            return 0f;
        return ingredient.CalculateCalories(amount);
    }

    public void RefreshResult(IngredientResultMode mode)
    {
        if (resultText == null)
            return;

        if (ingredient == null)
        {
            resultText.text = "-";
            return;
        }

        if (mode == IngredientResultMode.Edit)
        {
            resultText.text = "";
            return;
        }

        if (mode == IngredientResultMode.Nutrition)
        {
            resultText.text = GetCalories().ToString("0.##") + " kcal";
            return;
        }

        resultText.text = "Rp " + Mathf.RoundToInt(GetPrice()).ToString("N0");
    }

    private void RefreshTexts()
    {
        if (nameText != null)
            nameText.text = ingredient != null ? ingredient.name : emptyIngredientLabel;

        if (amountText != null)
            amountText.text = ingredient != null ? amount.ToString("0.##") : string.Empty;

        if (unitText != null)
            unitText.text = ingredient != null ? ingredient.unit : string.Empty;
    }
}
