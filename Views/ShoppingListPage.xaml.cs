using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;
using System.Collections.Specialized;

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

        private void UpdateDataFile()
        {
            List<string> categories = viewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Utils.ToXML(viewModel.Items.ToList(), categories);
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            AddItemPage addItemPage = new AddItemPage
            {
                OnItemAdded = (name, amount, unit, category, isOptional, store) =>
                {
                    ListItemModel item = new ListItemModel(name, amount, unit, category, isOptional, store);
                    if (viewModel.AddItemCommand?.CanExecute(item) ?? false)
                        viewModel.AddItemCommand.Execute(item);
                    else
                    {
                        viewModel.Items.Add(item);
                        UpdateDataFile();
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

        private async void OpenStorePage(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(StorePage)}");
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                await ExportOptions.ImportAsync(item => viewModel.Items.Add(item));

                UpdateDataFile();

                BuildCategoryViews();
                await DisplayAlert("Import", "Zaimportowano listê zakupów.", "OK");
            }
            catch (OperationCanceledException)
            {
                await DisplayAlert("Import", "Anulowano import", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Import", $"B³¹d importu: {ex.Message}", "OK");
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            try
            {
                UpdateDataFile();
                await ExportOptions.ExportAsync();
                await DisplayAlert("Eksport", "Zapisano plik.", "OK");
            }
            catch (OperationCanceledException)
            {
                await DisplayAlert("Eksport", "Anulowano zapis", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Eksport", $"B³¹d eksportu: {ex.Message}", "OK");
            }
        }
    }
}