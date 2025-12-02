using System.Collections.ObjectModel;
using ShoppingList.Models;
using ShoppingList.ViewModels;
using ShoppingList.Services;

namespace ShoppingList.Views.Recipes;

public partial class AddRecipePage : ContentPage
{
    public Action<string, string, IReadOnlyList<ListItemModel>>? OnRecipeAdded;

    private readonly RecipesViewModel? _recipesVm;
    private readonly ObservableCollection<ListItemModel> _ingredients = new();
    private readonly ObservableCollection<string> _ingredientCategories = new();

    public AddRecipePage()
    {
        InitializeComponent();
        IngredientsCollection.ItemsSource = _ingredients;

        IngredientUnit.SelectedIndex = 5; // sztuki
        RecipeCategory.SelectedIndex = 1; // dania glowne
        IngredientStore.SelectedIndex = 0; // dowolny

        LoadIngredientCategories();
    }

    public AddRecipePage(RecipesViewModel vm) : this()
    {
        _recipesVm = vm;
    }

    private void LoadIngredientCategories()
    {
        List<string> categoriesFromFile = Utils.FromXML().Categories;

        List<string> categoriess = categoriesFromFile
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _ingredientCategories.Clear();
        foreach (string c in categoriess)
            _ingredientCategories.Add(c);

        IngredientCategory.ItemsSource = _ingredientCategories;
        IngredientCategory.SelectedIndex = _ingredientCategories.IndexOf("Inne") >= 0 ? _ingredientCategories.IndexOf("Inne") : 0;
    }

    private async void OnAddIngredient(object sender, EventArgs e)
    {
        string name = IngredientName.Text?.Trim() ?? string.Empty;
        int amount = int.TryParse(IngredientAmount.Text, out int val) ? val : 0;
        string unit = IngredientUnit.SelectedItem as string ?? "szt";
        string category = IngredientCategory.SelectedItem as string ?? "Inne";
        bool isOptional = Optional.IsChecked;
        string store = IngredientStore.SelectedItem as string ?? "Dowolny";

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("B³¹d", "Podaj nazwê sk³adnika.", "OK");
            return;
        }
        if (amount <= 0)
        {
            await DisplayAlert("B³¹d", "Iloœæ sk³adnika musi byæ wiêksza ni¿ 0.", "OK");
            return;
        }

        ListItemModel item = new ListItemModel(name, amount, unit, category, isOptional, store);
        _ingredients.Add(item);

        IngredientName.Text = string.Empty;
        IngredientAmount.Text = string.Empty;
        IngredientUnit.SelectedIndex = 5;
        IngredientCategory.SelectedIndex = _ingredientCategories.IndexOf("Inne") >= 0 ? _ingredientCategories.IndexOf("Inne") : 0;
        IngredientStore.SelectedIndex = 0;
    }

    private void OnRemoveIngredient(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ListItemModel model)
        {
            _ingredients.Remove(model);
        }
    }

    private async void OnConfirm(object sender, EventArgs e)
    {
        string recipeName = RecipeName.Text?.Trim() ?? string.Empty;
        string category = RecipeCategory.SelectedItem as string ?? "Przystawki";
        category = category == "Dania Glowne" ? "Dania G³ówne" : category;

        if (string.IsNullOrWhiteSpace(recipeName))
        {
            await DisplayAlert("B³¹d", "Podaj nazwê przepisu.", "OK");
            return;
        }
        if (_ingredients.Count == 0)
        {
            await DisplayAlert("B³¹d", "Dodaj co najmniej jeden sk³adnik.", "OK");
            return;
        }

        if (_recipesVm?.AddCustomRecipeCommand?.CanExecute((recipeName, category, _ingredients.ToList())) == true)
        {
            _recipesVm.AddCustomRecipeCommand.Execute((recipeName, category, _ingredients.ToList()));
        }
        else
        {
            OnRecipeAdded?.Invoke(recipeName, category, _ingredients.ToList());
        }

        await Navigation.PopModalAsync();
    }

    private async void Return(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}