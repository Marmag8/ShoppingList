using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;

namespace ShoppingList.Views
{
    public partial class ItemView : ContentView
    {
        public static readonly BindableProperty ParentViewModelProperty =
            BindableProperty.Create(nameof(ParentViewModel), typeof(ShoppingListViewModel), typeof(ItemView));

        public ShoppingListViewModel ParentViewModel
        {
            get => (ShoppingListViewModel)GetValue(ParentViewModelProperty)!;
            set => SetValue(ParentViewModelProperty, value);
        }

        public ItemView()
        {
            InitializeComponent();
        }

        private void DeleteItem(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                if (ParentViewModel.DeleteItemCommand.CanExecute(model))
                    ParentViewModel.DeleteItemCommand.Execute(model);
                else
                {
                    if (ParentViewModel.Items.Contains(model))
                    {
                        ParentViewModel.Items.Remove(model);
                        IEnumerable<String> categories = ParentViewModel.Items.Select(i => i.Category).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                        Utils.ToXML(ParentViewModel.Items.ToList(), categories);
                    }
                }
            }
        }

        private void MarkAsBought(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                model.IsBought = !model.IsBought;
                IEnumerable<String> categories = ParentViewModel.Items.Select(i => i.Category).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                Utils.ToXML(ParentViewModel?.Items.ToList() ?? new(), categories);
            }
        }
    }
}