using ShoppingList.Models;
using ShoppingList.ViewModels;

namespace ShoppingList.Views.Recipes;

public partial class RecipeItemView : ContentView
{
    public static readonly BindableProperty ParentViewModelProperty =
        BindableProperty.Create(nameof(ParentViewModel), typeof(RecipesViewModel), typeof(RecipeItemView));

    public RecipesViewModel? ParentViewModel
    {
        get => (RecipesViewModel?)GetValue(ParentViewModelProperty);
        set => SetValue(ParentViewModelProperty, value);
    }

    public RecipeItemView()
    {
        InitializeComponent();
        AddButton.Clicked += OnAddClicked;
    }

    private void OnAddClicked(object? sender, EventArgs e)
    {
        if (BindingContext is RecipeModel recipe && ParentViewModel?.AddRecipeIngredientsCommand?.CanExecute(recipe) == true)
        {
            ParentViewModel.AddRecipeIngredientsCommand.Execute(recipe);
        }
    }
}