using System.Collections.Specialized;
using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;

namespace ShoppingList.Views
{
    public partial class ShoppingListPage : ContentPage
    {
        private readonly ShoppingListViewModel viewModel;

        public ShoppingListPage(ShoppingListViewModel vm)
        {
            InitializeComponent();
            viewModel = vm;
            BindingContext = viewModel;

            BuildCategoryViews();

            if (viewModel.Items is INotifyCollectionChanged collection)
                collection.CollectionChanged += Items_CollectionChanged;
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            AddItemPage addItemPage = new AddItemPage
            {
                OnItemAdded = (name, amount, unit, category) =>
                {
                    ListItemModel item = new ListItemModel(name, amount, unit, category);
                    if (viewModel.AddItemCommand?.CanExecute(item) ?? false)
                        viewModel.AddItemCommand.Execute(item);
                    else
                    {
                        viewModel.Items.Add(item);
                        IEnumerable<string> categories = viewModel.Items
                            .Select(i => i.Category)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
                        Utils.ToXML(viewModel.Items.ToList(), categories);
                    }
                }
            };

            await Navigation.PushModalAsync(addItemPage);
        }

        private void BuildCategoryViews()
        {
            CategoriesLayout.Children.Clear();

            List<string> categories = viewModel.Items
                .Select(i => string.IsNullOrWhiteSpace(i.Category) ? "Inne" : i.Category!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string category in categories)
            {
                CategoryView categoryView = new CategoryView
                {
                    Category = category,
                    AllItems = viewModel.Items.OrderBy(i => i.IsBought),
                    BindingContext = viewModel
                };

                CategoriesLayout.Children.Add(categoryView);
            }
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(BuildCategoryViews);
        }
    }
}