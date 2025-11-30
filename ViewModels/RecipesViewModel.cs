using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList.Models;
using ShoppingList.Services;

namespace ShoppingList.ViewModels
{
    public partial class RecipesViewModel : ObservableObject
    {
        private readonly ShoppingListViewModel shoppingViewModel;

        public ObservableCollection<RecipeModel> Recipes { get; } = new();

        public RecipesViewModel() : this(new ShoppingListViewModel())
        {
        }

        public RecipesViewModel(ShoppingListViewModel shoppingViewModel)
        {
            this.shoppingViewModel = shoppingViewModel ?? throw new ArgumentNullException(nameof(shoppingViewModel));
            Seed();
        }

        private void Seed()
        {
            if (Recipes.Count > 0) return;

            Recipes.Add(new RecipeModel(
                "Sa³atka Grecka",
                "Sa³atki",
                new[]
                {
                    new ListItemModel("Pomidor", 2, "szt", "Warzywa"),
                    new ListItemModel("Ogórek", 1, "szt", "Warzywa"),
                    new ListItemModel("Feta", 200, "g", "Mleczne"),
                    new ListItemModel("Oliwki", 150, "g", "Inne")
                }));

            Recipes.Add(new RecipeModel(
                "Spaghetti Bolognese",
                "Dania g³ówne",
                new[]
                {
                    new ListItemModel("Makaron spaghetti", 500, "g", "Inne"),
                    new ListItemModel("Miêso mielone", 400, "g", "Miêso"),
                    new ListItemModel("Pomidory krojone", 400, "g", "Warzywa"),
                    new ListItemModel("Cebula", 1, "szt", "Warzywa")
                }));
        }

        [RelayCommand]
        private void AddRecipeIngredients(RecipeModel recipe)
        {
            if (recipe is null)
                return;

            foreach (ListItemModel ing in recipe.Ingredients)
            {
                ListItemModel? existing = shoppingViewModel.Items.FirstOrDefault(i =>
                    i.Name == ing.Name &&
                    i.Unit == ing.Unit &&
                    i.Category == ing.Category);

                if (existing is not null)
                    existing.Amount += ing.Amount;
                else
                    shoppingViewModel.Items.Add(new ListItemModel(ing.Name, ing.Amount, ing.Unit, ing.Category));
            }

            var cats = shoppingViewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Utils.ToXML(shoppingViewModel.Items.ToList(), cats);
        }
    }
}