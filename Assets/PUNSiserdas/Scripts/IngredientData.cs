using System;

[Serializable]
public class IngredientData
{
    public string name;
    public float basePrice;
    public float baseQuantity;
    public string unit;
    public float baseCalories;

    public IngredientData(string name, float basePrice, float baseQuantity, string unit, float baseCalories)
    {
        this.name = name;
        this.basePrice = basePrice;
        this.baseQuantity = baseQuantity;
        this.unit = unit;
        this.baseCalories = baseCalories;
    }

    public float DefaultAmount
    {
        get
        {
            if (IsWeightOrVolumeUnit(unit))
            {
                if (baseQuantity > 0f)
                    return Math.Min(100f, Math.Max(1f, baseQuantity));

                return 100f;
            }
            return Math.Max(1f, baseQuantity);
        }
    }

    public static bool IsWeightOrVolumeUnit(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
            return false;

        string normalized = unitName.Trim().ToLowerInvariant();
        return normalized == "gram" || normalized == "gr" || normalized == "g" || normalized == "ml" || normalized == "ltr" || normalized == "cc" || normalized == "kg";
    }

    public float CalculatePrice(float amount)
    {
        if (baseQuantity <= 0f)
            return 0f;

        return (amount / baseQuantity) * basePrice;
    }

    public float CalculateCalories(float amount)
    {
        if (baseQuantity <= 0f)
            return 0f;

        return (amount / baseQuantity) * baseCalories;
    }
}
