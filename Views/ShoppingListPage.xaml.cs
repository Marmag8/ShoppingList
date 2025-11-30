using Microsoft.Maui.Storage;
using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;
using System.Collections.Specialized;
using System.IO;

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

        private async void OpenStorePage(object sender, EventArgs e)
        {
            string? storeName = await DisplayPromptAsync("Generuj Listê", "Podaj nazwê sklepu, do którego chcesz wygenerowaæ listê (zostaw puste, aby wygenerowaæ listê wszystkich produktów):");
            if (storeName == String.Empty)
                await Shell.Current.GoToAsync($"{nameof(StorePage)}");
            else
                await Shell.Current.GoToAsync($"{nameof(StorePage)}?store={storeName}");
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Wybierz plik .xml do importu",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xml" } },
                    { DevicePlatform.Android, new[] { "application/xml", "text/xml" } },
                    { DevicePlatform.iOS, new[] { "public.xml" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.xml" } }
                })
            });

            if (result == null || string.IsNullOrWhiteSpace(result.FullPath) || !File.Exists(result.FullPath))
                return;

            string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            Directory.CreateDirectory(appDir);
            string appPath = Path.Combine(appDir, "itemList.xml");
            File.Copy(result.FullPath, appPath, overwrite: true);

            (List<ListItemModel> itemsFromFile, List<string> categoriesFromFile) = Utils.FromXML();
            foreach (ListItemModel item in itemsFromFile)
                viewModel.Items.Add(item);

            List<String> categories = viewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Utils.ToXML(viewModel.Items.ToList(), categories);

            BuildCategoryViews();
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            List<String> categories = viewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Utils.ToXML(viewModel.Items.ToList(), categories);

            string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            string appPath = Path.Combine(appDir, "itemList.xml");
            if (!File.Exists(appPath))
                return;

            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Directory.CreateDirectory(docs);
            string target = Path.Combine(docs, $"itemList-export-{DateTime.Now:yyyyMMddHHmmss}.xml");
            File.Copy(appPath, target, overwrite: true);

            await DisplayAlert("Eksport", $"Zapisano do: {target}", "OK");
        }
    }
}