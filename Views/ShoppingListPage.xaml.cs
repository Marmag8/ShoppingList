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

        private void ContextMenuHandler(object sender, EventArgs e)
        {
            if (sender is not MenuFlyoutItem menuItem)
                return;

            switch (menuItem.StyleId)
            {
                case "Delete":
                    Delete(menuItem);
                    break;
                case "MarkAsBought":
                    MarkAsBought(menuItem);
                    break;
            }
        }

        private void Delete(object sender)
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

        private void MarkAsBought(object sender)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.CommandParameter is ListItemModel model)
            {
                model.IsBought = !model.IsBought;
                Utils.ToXML(viewModel.Items.ToList());
            }
        }

        private async void OpenCategoryView(object sender, EventArgs e)
        {
            if (sender is TapGestureRecognizer tgr && tgr.CommandParameter is string category)
            {
                await Shell.Current.GoToAsync($"{nameof(CategoryViewPage)}?category={category}");
                return;
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(CategoryViewPage)}?category={"Inne"}");
            }
        }
    }
}