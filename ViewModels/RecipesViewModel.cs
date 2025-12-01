using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList.Models;
using ShoppingList.Services;

namespace ShoppingList.ViewModels
{
    public partial class RecipesViewModel : ObservableObject
    {
        private readonly ShoppingListViewModel _shoppingListViewModel;

        public ObservableCollection<RecipeModel> Recipes { get; } = new();

        public static readonly string[] PresetCategories = new[]
        {
            "Przystawki",
            "Dania G³ówne",
            "Desery",
            "Napoje"
        };

        public RecipesViewModel() : this(new ShoppingListViewModel())
        {
        }

        public RecipesViewModel(ShoppingListViewModel shoppingListViewModel)
        {
            _shoppingListViewModel = shoppingListViewModel ?? throw new ArgumentNullException(nameof(shoppingListViewModel));

            List<String> categories = _shoppingListViewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (RecipeModel r in Utils.FromRecipesXML())
                Recipes.Add(r);

            if (Recipes.Count == 0)
                PopulateRecipes(); 

            if (Recipes is INotifyCollectionChanged collection)
                collection.CollectionChanged += (sender, args) => Utils.ToXML(_shoppingListViewModel.Items.ToList(), categories, Recipes.ToList());
        }

        private void PopulateRecipes()
        {
            if (Recipes.Count > 0) return;

            Recipes.Add(new RecipeModel(
                "Sa³atka Grecka",
                "Przystawki",
                new[]
                {
                    new ListItemModel("Pomidor", 2, "szt", "Warzywa", false),
                    new ListItemModel("Ogórek", 1, "szt", "Warzywa", true),
                    new ListItemModel("Feta", 200, "g", "Mleczne", false),
                    new ListItemModel("Oliwki", 150, "g", "Inne", true)
                }));

            Recipes.Add(new RecipeModel(
                "Spaghetti Bolognese",
                "Dania G³ówne",
                new[]
                {
                    new ListItemModel("Makaron spaghetti", 500, "g", "Inne", false),
                    new ListItemModel("Miêso mielone", 400, "g", "Miêso", false),
                    new ListItemModel("Pomidory krojone", 400, "g", "Warzywa", false),
                    new ListItemModel("Cebula", 1, "szt", "Warzywa", true)
                }));
        }

        [RelayCommand]
        private void AddRecipeIngredients(RecipeModel recipe)
        {
            if (recipe is null)
                return;

            foreach (ListItemModel ingredient in recipe.Ingredients)
            {
                var existing = _shoppingListViewModel.Items.FirstOrDefault(i =>
                    i.Name == ingredient.Name &&
                    i.Unit == ingredient.Unit &&
                    i.Category == ingredient.Category);

                if (existing is not null)
                {
                    existing.Amount += ingredient.Amount;
                    if (ingredient.IsOptional && !existing.IsOptional)
                        existing.IsOptional = true;
                }
                else
                {
                    _shoppingListViewModel.Items.Add(
                        new ListItemModel(ingredient.Name, ingredient.Amount, ingredient.Unit, ingredient.Category, ingredient.IsOptional));
                }
            }

            List<String> categories = _shoppingListViewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Utils.ToXML(_shoppingListViewModel.Items.ToList(), categories, Recipes.ToList());
        }

        [RelayCommand]
        private void AddCustomRecipe((string Name, string Category, IReadOnlyList<ListItemModel> Ingredients) data)
        {
            string name = string.IsNullOrWhiteSpace(data.Name) ? "Bez nazwy" : data.Name;
            string category = PresetCategories.Contains(data.Category, StringComparer.OrdinalIgnoreCase)
                ? data.Category
                : "Przystawki";

            List<String> categories = _shoppingListViewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Recipes.Add(new RecipeModel(name, category, data.Ingredients ?? Array.Empty<ListItemModel>()));
            Utils.ToXML(_shoppingListViewModel.Items.ToList(), categories, Recipes.ToList());
        }
    }
}