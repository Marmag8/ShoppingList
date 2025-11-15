using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;

namespace ShoppingList.Views
{
    public partial class ShoppingListPage : ContentPage
    {
        private readonly ShoppingListViewModel viewModel;

        public ShoppingListPage()
        {
            InitializeComponent();

            viewModel = BindingContext as ShoppingListViewModel ?? new ShoppingListViewModel();
            BindingContext = viewModel;
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
                        Utils.ToXML(viewModel.Items.ToList());
                    }
                }
            };

            await Navigation.PushModalAsync(addItemPage);
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.CommandParameter is ListItemModel model)
            {
                if (viewModel.DeleteItemCommand?.CanExecute(model) ?? false)
                {
                    viewModel.DeleteItemCommand.Execute(model);
                }
                else
                {
                    if (viewModel.Items.Contains(model))
                    {
                        viewModel.Items.Remove(model);
                        Utils.ToXML(viewModel.Items.ToList());
                    }
                }
            }
        }
    }
}