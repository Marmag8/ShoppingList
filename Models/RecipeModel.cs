using System.Collections.Generic;
using ShoppingList.Models;

namespace ShoppingList.Models
{
    public sealed class RecipeModel
    {
        public string Name { get; }
        public string Category { get; }
        public IReadOnlyList<ListItemModel> Ingredients { get; }

        public RecipeModel(string name, string category, IEnumerable<ListItemModel> ingredients)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Bez nazwy" : name;
            Category = string.IsNullOrWhiteSpace(category) ? "Inne" : category;
            Ingredients = new List<ListItemModel>(ingredients ?? []);
        }

        public override string ToString() => $"{Name} ({Category})";
    }
}