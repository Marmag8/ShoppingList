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

        private void UpdateDataFile()
        {
            IEnumerable<String> categories = ParentViewModel.Items.Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Utils.ToXML(ParentViewModel?.Items.ToList() ?? new(), categories);
        }

        private void DeleteItem(object sender, EventArgs e)
        {
            ListItemModel? model =
                (sender as MenuFlyoutItem)?.CommandParameter as ListItemModel
                ?? (sender as Button)?.CommandParameter as ListItemModel;

            if (model is null)
                return;

            if (ParentViewModel.DeleteItemCommand.CanExecute(model))
                ParentViewModel.DeleteItemCommand.Execute(model);
            else
            {
                if (ParentViewModel.Items.Contains(model))
                {
                    ParentViewModel.Items.Remove(model);
                    UpdateDataFile();
                }
            }
        }

        private void OnBoughtChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox cb && cb.BindingContext is ListItemModel model)
            {
                UpdateDataFile();
            }
        }

        private void MarkAsBought(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                model.IsBought = !model.IsBought;
                UpdateDataFile();
            }
        }

        private void MarkAsOptional(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                model.IsOptional = !model.IsOptional;
                UpdateDataFile();
            }
        }

        private void Increase(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                model.Amount++;
                UpdateDataFile();
            }
        }

        private void Decrease(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                model.Amount--;
                UpdateDataFile();
            }
        }

        private async void ChangeAmount(object sender, EventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.CommandParameter is ListItemModel model)
            {
                string? result = await Application.Current.MainPage.DisplayPromptAsync("Iloœæ", "Podaj now¹ iloœæ:");
                if (int.TryParse(result, out int newAmount))
                {
                    model.Amount = newAmount;
                    UpdateDataFile();
                }
            }
        }
    }
}