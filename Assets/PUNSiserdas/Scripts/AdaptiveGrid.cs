using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class AdaptiveGrid : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int columns = 3;
    [SerializeField] private Vector2 spacing = new Vector2(12f, 12f);
    [SerializeField] private bool keepSquareCell = true;

    [Header("Adaptive Padding (percent of panel size)")]
    [SerializeField] private float padLeftPercent = 0.04f;
    [SerializeField] private float padRightPercent = 0.04f;
    [SerializeField] private float padTopPercent = 0.04f;
    [SerializeField] private float padBottomPercent = 0.04f;

    private GridLayoutGroup grid;
    private RectTransform rect;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ApplyLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        ApplyLayout();
    }

    private void ApplyLayout()
    {
        if (grid == null || rect == null || columns <= 0)
            return;

        int childCount = Mathf.Max(1, rect.childCount);
        int rows = Mathf.CeilToInt(childCount / (float)columns);

        float panelWidth = rect.rect.width;
        float panelHeight = rect.rect.height;

        int padLeft = Mathf.RoundToInt(panelWidth * padLeftPercent);
        int padRight = Mathf.RoundToInt(panelWidth * padRightPercent);
        int padTop = Mathf.RoundToInt(panelHeight * padTopPercent);
        int padBottom = Mathf.RoundToInt(panelHeight * padBottomPercent);

        grid.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
        grid.spacing = spacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        float usableWidth = panelWidth - padLeft - padRight - spacing.x * (columns - 1);
        float usableHeight = panelHeight - padTop - padBottom - spacing.y * (rows - 1);

        float cellWidth = usableWidth / columns;
        float cellHeight = usableHeight / rows;

        if (keepSquareCell)
        {
            float size = Mathf.Min(cellWidth, cellHeight);
            grid.cellSize = new Vector2(size, size);
        }
        else
        {
            grid.cellSize = new Vector2(cellWidth, cellHeight);
        }
    }
}
