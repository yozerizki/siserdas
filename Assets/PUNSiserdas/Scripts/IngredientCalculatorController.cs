using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientCalculatorController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject imagePanel;
    [SerializeField] private GameObject textPanel;

    [Header("Rows")]
    [SerializeField] private Transform rowsParent;
    [SerializeField] private IngredientEntryRow rowPrefab;

    [Header("Footer")]
    [SerializeField] private TMP_Text footerText;

    [Header("Buttons To Enable After Bahan")]
    [SerializeField] private Button[] groupedActionButtons;

    [Header("Top Navigation Buttons")]
    [SerializeField] private Button bahanButton;
    [SerializeField] private TMP_Text bahanButtonText;
    [SerializeField] private Button resepButton;
    [SerializeField] private TMP_Text resepButtonText;
    [SerializeField] private Button serealiakuButton;
    [SerializeField] private TMP_Text serealiakuButtonText;
    [SerializeField] private Button allSerealiaButton;
    [SerializeField] private Button quizButton;

    [Header("Top Navigation Labels")]
    [SerializeField] private string bahanLabelImageMode = "kalku\nlator";
    [SerializeField] private string bahanLabelCalculatorMode = "ganti bahan";
    [SerializeField] private string resepLabelImageMode = "cari resep";
    [SerializeField] private string resepLabelCalculatorMode = "intip resep";
    [SerializeField] private string serealiakuLabelImageMode = "serealia saya";
    [SerializeField] private string serealiakuLabelCalculatorMode = "back";

    [Header("Resep")]
    [SerializeField] private TMP_Text recipeText;
    [SerializeField] private Image serealImage;

    private readonly List<IngredientEntryRow> rows = new List<IngredientEntryRow>();
    private IngredientResultMode currentMode = IngredientResultMode.Edit;
    private int currentIngredientIndex;
    private bool hasSelectedIngredient;
    private int currentRecipeIndex;
    private bool hasShownRecipe;
    private HoldRepeatButton recipeHoldButton;

    public static IngredientCalculatorController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ResolveTopNavigationReferences();
        SetGroupButtonsState(false);
        SwitchToImagePanel();
        RefreshAllRows();
    }

    public void OnBahanPressed()
    {
        bool wasCalculatorInactive = textPanel == null || !textPanel.activeSelf;

        // From material mode, calculator can only start when recipe mode is currently shown.
        if (wasCalculatorInactive && !IsRecipeModeActive())
        {
            ShowSelectRecipeFirstPrompt();
            return;
        }

        SwitchToTextPanel();
        SetGroupButtonsState(true);
        currentMode = IngredientResultMode.Edit;

        if (IngredientDatabase.Count == 0)
            return;

        if (!hasSelectedIngredient)
        {
            currentIngredientIndex = 0;
            hasSelectedIngredient = true;
        }
        else
        {
            currentIngredientIndex = (currentIngredientIndex + 1) % IngredientDatabase.Count;
        }

        if (wasCalculatorInactive)
            PrepareRowForResume();

        IngredientEntryRow activeRow = EnsureActiveRow();
        activeRow.SetIngredient(IngredientDatabase.GetByIndex(currentIngredientIndex));
        RefreshAllRows();
    }

    public void OnAddPressed()
    {
        IngredientEntryRow activeRow = EnsureActiveRow();
        if (activeRow == null)
            return;

        activeRow.AddAmount(1f);
        RefreshAllRows();
    }

    public void OnSubtractPressed()
    {
        IngredientEntryRow activeRow = EnsureActiveRow();
        if (activeRow == null)
            return;

        activeRow.AddAmount(-1f);
        RefreshAllRows();
    }

    public void OnEnterPressed()
    {
        IngredientEntryRow activeRow = EnsureActiveRow();
        if (activeRow == null)
            return;

        if (activeRow.Ingredient == null)
        {
            RefreshAllRows();
            return;
        }

        IngredientEntryRow newRow = CreateNewRow();
        if (newRow == null)
            return;

        RefreshAllRows();
    }

    public void OnBackspacePressed()
    {
        if (rows.Count == 0)
            return;

        IngredientEntryRow lastRow = rows[rows.Count - 1];
        rows.RemoveAt(rows.Count - 1);

        if (lastRow != null)
            Destroy(lastRow.gameObject);

        RefreshAllRows();
    }

    public void OnClearPressed()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] != null)
                Destroy(rows[i].gameObject);
        }

        rows.Clear();
        hasSelectedIngredient = false;
        currentIngredientIndex = 0;
        currentMode = IngredientResultMode.Edit;

        SetGroupButtonsState(false);
        RefreshFooter();
    }

    public void OnNutrisiPressed()
    {
        currentMode = IngredientResultMode.Nutrition;
        RefreshAllRows();
    }

    public void OnAnalisaUsahaPressed()
    {
        currentMode = IngredientResultMode.Business;
        RefreshAllRows();
    }

    public void OnAnalysisPressed()
    {
        OnAnalisaUsahaPressed();
    }

    public void OnResepPressed()
    {
        if (IsCalculatorModeActive())
            return;

        SwitchToImagePanel();
        SetGroupButtonsState(false);

        if (!hasShownRecipe)
        {
            currentRecipeIndex = 0;
            hasShownRecipe = true;
        }
        else
        {
            currentRecipeIndex = (currentRecipeIndex + 1) % RecipeDatabase.Count;
        }

        if (recipeText != null)
        {
            recipeText.text = RecipeDatabase.GetByIndex(currentRecipeIndex);
            recipeText.gameObject.SetActive(true);
        }

        if (serealImage != null)
            serealImage.gameObject.SetActive(false);
    }

    public void OnRecipePeekStart()
    {
        if (!IsCalculatorModeActive() || !hasShownRecipe || recipeText == null || RecipeDatabase.Count == 0)
            return;

        int recipeIndex = GetCurrentRecipeIndex();
        recipeText.text = RecipeDatabase.GetByIndex(recipeIndex);
        recipeText.gameObject.SetActive(true);

        if (imagePanel != null)
            imagePanel.SetActive(true);

        if (serealImage != null)
            serealImage.gameObject.SetActive(false);
    }

    public void OnRecipePeekEnd()
    {
        if (!IsCalculatorModeActive())
            return;

        if (recipeText != null)
            recipeText.gameObject.SetActive(false);

        if (imagePanel != null)
            imagePanel.SetActive(false);
    }

    public void SwitchToTextPanel()
    {
        if (textPanel != null)
            textPanel.SetActive(true);

        if (imagePanel != null)
            imagePanel.SetActive(false);

        HideRecipeText();
        ApplyTopButtonState(true);
        RefreshActiveRowHighlight();
    }

    public void SwitchToImagePanel()
    {
        SetGroupButtonsState(false);

        if (imagePanel != null)
            imagePanel.SetActive(true);

        if (textPanel != null)
            textPanel.SetActive(false);

        HideRecipeText();

        if (serealImage != null)
            serealImage.gameObject.SetActive(true);

        ApplyTopButtonState(false);
        RefreshActiveRowHighlight();
    }

    private IngredientEntryRow EnsureActiveRow()
    {
        if (rows.Count == 0)
            return CreateNewRow();

        IngredientEntryRow row = rows[rows.Count - 1];
        if (row == null)
            return CreateNewRow();

        return row;
    }

    private IngredientEntryRow CreateNewRow()
    {
        if (rowPrefab == null || rowsParent == null)
            return null;

        IngredientEntryRow row = Instantiate(rowPrefab, rowsParent);
        rows.Add(row);
        row.RefreshResult(currentMode);
        return row;
    }

    private void PrepareRowForResume()
    {
        if (rows.Count == 0)
            return;

        IngredientEntryRow lastRow = rows[rows.Count - 1];
        if (lastRow != null && lastRow.Ingredient != null)
            CreateNewRow();
    }

    private void SetGroupButtonsState(bool enabled)
    {
        if (groupedActionButtons == null)
            return;

        for (int i = 0; i < groupedActionButtons.Length; i++)
        {
            Button button = groupedActionButtons[i];
            if (button == null)
                continue;

            button.interactable = enabled;

            TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);
            for (int j = 0; j < texts.Length; j++)
            {
                if (texts[j] != null)
                    texts[j].enabled = enabled;
            }
        }
    }

    private void ApplyTopButtonState(bool calculatorActive)
    {
        ResolveTopNavigationReferences();

        SetButtonLabel(bahanButtonText, calculatorActive ? bahanLabelCalculatorMode : bahanLabelImageMode);
        SetButtonLabel(resepButtonText, calculatorActive ? resepLabelCalculatorMode : resepLabelImageMode);
        SetButtonLabel(serealiakuButtonText, calculatorActive ? serealiakuLabelCalculatorMode : serealiakuLabelImageMode);

        SetSingleButtonState(allSerealiaButton, !calculatorActive);
        SetSingleButtonState(quizButton, !calculatorActive);
        SetSingleButtonState(bahanButton, true);
        SetSingleButtonState(resepButton, true);
        SetSingleButtonState(serealiakuButton, true);
    }

    private void SetButtonLabel(TMP_Text label, string value)
    {
        if (label != null)
            label.text = value;
    }

    private void SetSingleButtonState(Button button, bool enabled)
    {
        if (button == null)
            return;

        button.interactable = enabled;

        TMP_Text[] texts = button.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
                texts[i].enabled = enabled;
        }
    }

    private void ResolveTopNavigationReferences()
    {
        if (bahanButton == null)
            bahanButton = FindButtonByName("ButtonBahan");

        if (resepButton == null)
            resepButton = FindButtonByName("ButtonResep");

        if (serealiakuButton == null)
            serealiakuButton = FindButtonByName("ButtonMySereal");

        if (allSerealiaButton == null)
            allSerealiaButton = FindButtonByName("ButtonAllSereal");

        if (quizButton == null)
            quizButton = FindButtonByName("ButtonQuiz");

        if (bahanButtonText == null && bahanButton != null)
            bahanButtonText = bahanButton.GetComponentInChildren<TMP_Text>(true);

        if (resepButtonText == null && resepButton != null)
            resepButtonText = resepButton.GetComponentInChildren<TMP_Text>(true);

        if (serealiakuButtonText == null && serealiakuButton != null)
            serealiakuButtonText = serealiakuButton.GetComponentInChildren<TMP_Text>(true);

        EnsureRecipeHoldBinding();
    }

    private Button FindButtonByName(string objectName)
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button button = allButtons[i];
            if (button != null && button.gameObject.name == objectName)
                return button;
        }

        return null;
    }

    private void RefreshAllRows()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] != null)
                rows[i].RefreshResult(currentMode);
        }

        RefreshActiveRowHighlight();
        RefreshFooter();
    }

    private void RefreshActiveRowHighlight()
    {
        bool calculatorVisible = textPanel != null && textPanel.activeSelf;
        int activeIndex = rows.Count - 1;

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] != null)
                rows[i].SetHighlighted(calculatorVisible && i == activeIndex);
        }
    }

    private void RefreshFooter()
    {
        if (footerText == null)
            return;

        if (currentMode == IngredientResultMode.Edit)
        {
            footerText.text = string.Empty;
            return;
        }

        float total = 0f;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i] == null)
                continue;

            total += currentMode == IngredientResultMode.Nutrition ? rows[i].GetCalories() : rows[i].GetPrice();
        }

        if (currentMode == IngredientResultMode.Nutrition)
            footerText.text = "Total Kalori: " + total.ToString("0.##") + " kcal";
        else
        {
            float hpp = total;
            float jual30 = hpp * 1.30f;
            float jual40 = hpp * 1.40f;
            float jual50 = hpp * 1.50f;

            footerText.text =
                "Total Modal / HPP: Rp " + Mathf.RoundToInt(hpp).ToString("N0") +
                "\nHarga Jual (Laba 30%): Rp " + Mathf.RoundToInt(jual30).ToString("N0") +
                "\nHarga Jual (Laba 40%): Rp " + Mathf.RoundToInt(jual40).ToString("N0") +
                "\nHarga Jual (Laba 50%): Rp " + Mathf.RoundToInt(jual50).ToString("N0");
        }
    }

    private bool IsCalculatorModeActive()
    {
        return textPanel != null && textPanel.activeSelf;
    }

    private bool IsRecipeModeActive()
    {
        return hasShownRecipe && recipeText != null && recipeText.gameObject.activeSelf;
    }

    private int GetCurrentRecipeIndex()
    {
        if (RecipeDatabase.Count == 0)
            return 0;

        return Mathf.Clamp(currentRecipeIndex, 0, RecipeDatabase.Count - 1);
    }

    private void HideRecipeText()
    {
        if (recipeText != null)
            recipeText.gameObject.SetActive(false);
    }

    private void ShowSelectRecipeFirstPrompt()
    {
        if (imagePanel != null)
            imagePanel.SetActive(true);

        if (textPanel != null)
            textPanel.SetActive(false);

        if (recipeText != null)
        {
            recipeText.text = "pilih resep terlebih dahulu";
            recipeText.gameObject.SetActive(true);
        }

        if (serealImage != null)
            serealImage.gameObject.SetActive(false);

        SetGroupButtonsState(false);
        ApplyTopButtonState(false);
        RefreshActiveRowHighlight();
    }

    private void EnsureRecipeHoldBinding()
    {
        if (resepButton == null)
            return;

        if (recipeHoldButton == null || recipeHoldButton.gameObject != resepButton.gameObject)
            recipeHoldButton = resepButton.GetComponent<HoldRepeatButton>();

        if (recipeHoldButton == null)
            recipeHoldButton = resepButton.gameObject.AddComponent<HoldRepeatButton>();

        recipeHoldButton.SetRuntimePressCallbacks(OnRecipePeekStart, OnRecipePeekEnd);
    }
}
