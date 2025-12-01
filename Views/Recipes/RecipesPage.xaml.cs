using System.Collections.Specialized;
using ShoppingList.Models;
using ShoppingList.ViewModels;

namespace ShoppingList.Views.Recipes
{
    public partial class RecipesPage : ContentPage
    {
        private readonly RecipesViewModel _viewModel;

        public RecipesPage(RecipesViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = _viewModel;

            BuildCategoryViews();

            if (_viewModel.Recipes is INotifyCollectionChanged collection)
                collection.CollectionChanged += (sender, args) => MainThread.BeginInvokeOnMainThread(BuildCategoryViews);
        }

        private async void OnAddRecipeClicked(object sender, EventArgs e)
        {
            AddRecipePage addPage = new AddRecipePage(_viewModel)
            {
                OnRecipeAdded = (name, category, ingredients) =>
                {
                    var recipe = new RecipeModel(name, category, ingredients);
                    _viewModel.Recipes.Add(recipe);
                }
            };

            await Navigation.PushModalAsync(addPage);
        }

        private void BuildCategoryViews()
        {
            CategoriesLayout.Children.Clear();

            List<string> categories = _viewModel.Recipes
                .Select(r => string.IsNullOrWhiteSpace(r.Category) ? "Inne" : r.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (String category in categories)
            {
                RecipeGroupView group = new RecipeGroupView
                {
                    Category = category,
                    AllRecipes = _viewModel.Recipes,
                    BindingContext = _viewModel
                };
                CategoriesLayout.Children.Add(group);
            }
        }
    }
}