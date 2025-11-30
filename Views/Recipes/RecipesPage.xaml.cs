using System.Collections.Specialized;
using ShoppingList.ViewModels;

namespace ShoppingList.Views.Recipes
{
    public partial class RecipesPage : ContentPage
    {
        private readonly RecipesViewModel viewModel;

        public RecipesPage(RecipesViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            BindingContext = viewModel;

            BuildCategoryViews();

            if (viewModel.Recipes is INotifyCollectionChanged incc)
                incc.CollectionChanged += (_, __) => MainThread.BeginInvokeOnMainThread(BuildCategoryViews);
        }

        private void BuildCategoryViews()
        {
            CategoriesLayout.Children.Clear();

            var categories = viewModel.Recipes
                .Select(r => string.IsNullOrWhiteSpace(r.Category) ? "Inne" : r.Category)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var cat in categories)
            {
                var v = new RecipeView
                {
                    Category = cat,
                    AllRecipes = viewModel.Recipes,
                    BindingContext = viewModel
                };
                CategoriesLayout.Children.Add(v);
            }
        }
    }
}