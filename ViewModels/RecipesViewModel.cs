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

            List<string> categories = _shoppingListViewModel.Items
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
                "Przyk³adowe instrukcje przygotowania sa³atki greckiej",
                "Przystawki",
                new[]
                {
                    new ListItemModel("Pomidor", 2, "szt", "Warzywa", true, "Kaufland"),
                    new ListItemModel("Ogórek", 1, "szt", "Warzywa", false, "Biedronka"),
                    new ListItemModel("Feta", 200, "g", "Mleczne", false, "Lidl"),
                    new ListItemModel("Oliwki", 150, "g", "Inne", true, "Carrefour")
                }));

            Recipes.Add(new RecipeModel(
                "Spaghetti Bolognese",
                "Przyk³adowe instrukcje przygotowania spaghetti bolognese",
                "Dania G³ówne",
                new[]
                {
                    new ListItemModel("Makaron spaghetti", 500, "g", "Inne", false, "Lidl"),
                    new ListItemModel("Miêso mielone", 400, "g", "Miêso", true, "Selgros"),
                    new ListItemModel("Pomidory krojone", 400, "g", "Warzywa", false, "Biedronka"),
                    new ListItemModel("Cebula", 1, "szt", "Warzywa", true, "Carrefour")
                }));

            Recipes.Add(new RecipeModel(
                "Ciasto Czekoladowe",
                "Przyk³adowe instrukcje przygotowania ciasta czekoladowego",
                "Desery",
                new[]
                {
                    new ListItemModel("M¹ka", 500, "g", "Inne", false, "Dowolny"),
                    new ListItemModel("Czekolada", 100, "g", "Mleczne", false, "Dowolny"),
                    new ListItemModel("Mleko", 500, "ml", "Mleczne", false, "Dowolny"),
                    new ListItemModel("Jajka", 2, "szt", "Inne", false, "Dowolny"),
                }));

            Recipes.Add(new RecipeModel(
                "Woda",
                "Przyk³adowe instrukcje przygotowania wody",
                "Napoje",
                new[]
                {
                    new ListItemModel("Woda", 300, "ml", "Picie", false, "Kaufland"),
                    new ListItemModel("Szklanka", 1, "szt", "Inne", true, "Selgros"),
                }));
        }

        [RelayCommand]
        private void AddRecipeIngredients(RecipeModel recipe)
        {
            if (recipe is null)
                return;

            foreach (ListItemModel ingredient in recipe.Ingredients)
            {
                ListItemModel? existing = _shoppingListViewModel.Items.FirstOrDefault(i =>
                    i.Name == ingredient.Name &&
                    i.Unit == ingredient.Unit &&
                    i.Category == ingredient.Category);

                if (existing is not null)
                {
                    existing.Amount += ingredient.Amount;
                    if (ingredient.IsOptional && !existing.IsOptional)
                        existing.IsOptional = true;
                    if (string.IsNullOrWhiteSpace(existing.Store) && !string.IsNullOrWhiteSpace(ingredient.Store))
                        existing.Store = ingredient.Store;
                }
                else
                {
                    _shoppingListViewModel.Items.Add(
                        new ListItemModel(ingredient.Name, ingredient.Amount, ingredient.Unit, ingredient.Category, ingredient.IsOptional, ingredient.Store));
                }
            }

            List<string> categories = _shoppingListViewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Utils.ToXML(_shoppingListViewModel.Items.ToList(), categories, Recipes.ToList());
        }

        [RelayCommand]
        private void AddCustomRecipe((string Name, string Description, string Category, IReadOnlyList<ListItemModel> Ingredients) data)
        {
            string name = string.IsNullOrWhiteSpace(data.Name) ? "Bez nazwy" : data.Name;
            string description = string.IsNullOrWhiteSpace(data.Description) ? "Brak opisu" : data.Description;
            string category = PresetCategories.Contains(data.Category, StringComparer.OrdinalIgnoreCase) ? data.Category : "Przystawki";

            List<string> categories = _shoppingListViewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Recipes.Add(new RecipeModel(name, description, category, data.Ingredients));
            Utils.ToXML(_shoppingListViewModel.Items.ToList(), categories, Recipes.ToList());
        }
    }
}