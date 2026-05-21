using System.Collections.Generic;

public static class IngredientDatabase
{
    private static List<IngredientData> items;

    public static IReadOnlyList<IngredientData> Items
    {
        get
        {
            if (items == null)
                items = BuildDefaults();
            return items;
        }
    }

    public static int Count => Items.Count;

    public static IngredientData GetByIndex(int index)
    {
        if (Count == 0)
            return null;

        int wrappedIndex = ((index % Count) + Count) % Count;
        return items[wrappedIndex];
    }

    private static List<IngredientData> BuildDefaults()
    {
        return new List<IngredientData>
        {
            new IngredientData("Terigu Protein Tinggi (Cakra)", 15000f, 1000f, "gram", 3620f),
            new IngredientData("Terigu Protein Sedang (Sgt Biru)", 12000f, 1000f, "gram", 3640f),
            new IngredientData("Terigu Protein Rendah", 11000f, 1000f, "gram", 3640f),
            new IngredientData("Tepung Tapioka", 12000f, 1000f, "gram", 3580f),
            new IngredientData("Jagung Pipil", 25000f, 1000f, "gram", 3500f),
            new IngredientData("Barley", 30000f, 1000f, "gram", 3500f),
            new IngredientData("Agar-agar", 5000f, 7f, "gram", 25f),
            new IngredientData("Gula", 18000f, 1000f, "gram", 3870f),
            new IngredientData("Telur", 29000f, 1000f, "gram", 1550f),
            new IngredientData("Ragi", 5000f, 11f, "gram", 17.3f),
            new IngredientData("Vanili", 1000f, 1f, "gram", 0f),
            new IngredientData("Bread Improver", 9000f, 100f, "gram", 0f),
            new IngredientData("Margarin", 35000f, 1000f, "gram", 7170f),
            new IngredientData("Room Butter", 45000f, 1000f, "gram", 7170f),
            new IngredientData("Susu Bubuk", 100000f, 1000f, "gram", 4960f),
            new IngredientData("Air", 0f, 100f, "ml", 0f),
            new IngredientData("Garam", 2500f, 250f, "gram", 0f),
            new IngredientData("Santan", 8000f, 100f, "ml", 230f),
            new IngredientData("Daun Pandan", 1000f, 1f, "lembar", 0f),
            new IngredientData("Selai Nanas", 10000f, 250f, "gram", 675f),
            new IngredientData("Pisang Raja", 10000f, 1500f, "gram", 1890f),
            new IngredientData("Coklat batang", 28000f, 250f, "gram", 1375f),
            new IngredientData("Meses", 9000f, 500f, "gram", 2250f),
            new IngredientData("Tepung Beras", 14000f, 1000f, "gram", 3660f),
            new IngredientData("Tepung Ketan", 18000f, 1000f, "gram", 3660f),
            new IngredientData("Gula Kelapa", 25000f, 500f, "gram", 1875f),
            new IngredientData("Kinoa", 75000f, 500f, "gram", 1840f),
            new IngredientData("Fonio", 95000f, 500f, "gram", 1835f),
            new IngredientData("Jewawut", 30000f, 500f, "gram", 1890f),
            new IngredientData("Tepung Buckwheat", 55000f, 500f, "gram", 1675f),
            new IngredientData("Jali", 35000f, 500f, "gram", 1760f),
            new IngredientData("Millet", 40000f, 500f, "gram", 1890f),
            new IngredientData("Oat Gulung", 35000f, 1000f, "gram", 3890f),
            new IngredientData("Sorgum Biji", 30000f, 1000f, "gram", 3290f),
            new IngredientData("Udang Rebon", 35000f, 250f, "gram", 725f),
            new IngredientData("Nanas", 12000f, 1000f, "gram", 500f),
            new IngredientData("Pepaya Muda", 8000f, 1000f, "gram", 430f),
            new IngredientData("Kacang Mede", 75000f, 500f, "gram", 2765f),
            new IngredientData("Daun Mint", 7000f, 50f, "gram", 22f),
            new IngredientData("Madu", 45000f, 250f, "gram", 760f),
            new IngredientData("Jeruk Nipis", 18000f, 1000f, "gram", 300f),
            new IngredientData("Sambal Terasi", 12000f, 200f, "gram", 300f),
            new IngredientData("Kelapa Parut", 10000f, 250f, "gram", 885f),
            new IngredientData("Bawang Merah", 45000f, 1000f, "gram", 400f),
            new IngredientData("Cabai Merah", 60000f, 1000f, "gram", 400f),
            new IngredientData("Tomat", 15000f, 1000f, "gram", 180f),
            new IngredientData("Terasi", 8000f, 100f, "gram", 170f),
            new IngredientData("Minyak Goreng", 19000f, 1000f, "ml", 8840f),
            new IngredientData("Cabai Rawit", 80000f, 1000f, "gram", 400f),
            new IngredientData("Gula Aren", 28000f, 500f, "gram", 1875f),
            new IngredientData("Durian", 65000f, 1000f, "gram", 1470f),
            new IngredientData("Kelapa Gading", 15000f, 500f, "gram", 1770f),
            new IngredientData("Gula Merah", 22000f, 500f, "gram", 1850f),
            new IngredientData("Jamur Kancing", 30000f, 500f, "gram", 110f),
            new IngredientData("Ayam Giling", 45000f, 500f, "gram", 1075f),
            new IngredientData("Teriyaki Sauce", 20000f, 250f, "gram", 223f),
            new IngredientData("Lada Hitam", 22000f, 100f, "gram", 251f),
            new IngredientData("Dark Chocolate", 45000f, 250f, "gram", 1365f),
            new IngredientData("Gula Jawa", 22000f, 500f, "gram", 1850f)
        };
    }
}
