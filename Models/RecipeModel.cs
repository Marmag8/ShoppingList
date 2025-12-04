namespace ShoppingList.Models
{
    public sealed class RecipeModel
    {
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        public IReadOnlyList<ListItemModel> Ingredients { get; }

        public RecipeModel(string name, string description, string category, IEnumerable<ListItemModel> ingredients)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Bez nazwy" : name;
            Description = string.IsNullOrWhiteSpace(description) ? "Brak opisu" : description;
            Category = string.IsNullOrWhiteSpace(category) ? "Inne" : category;
            Ingredients = new List<ListItemModel>(ingredients ?? []);
        }

        public override string ToString() => $"{Name} ({Category})";
    }
}