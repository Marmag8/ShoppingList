using ShoppingList.Models;
using ShoppingList.Services;

namespace ShoppingList.Views;

public partial class AddItemPage : ContentPage
{
    public Action<string, int, string, string, bool>? OnItemAdded;
    private List<string> _categories = new();

    public AddItemPage()
    {
        InitializeComponent();
        Unit.SelectedIndex = 5;

        List<string> categoriesFromFile = Utils.FromXML().Categories;

        _categories = categoriesFromFile
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        _categories.Add("Nowa...");

        Category.ItemsSource = _categories;
        Category.SelectedIndex = 0;
        Category.SelectedIndexChanged += Category_SelectedIndexChanged;
    }

    private async void Category_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (Category.SelectedItem is string selection && selection == "Nowa...")
        {
            string? newCategory = await DisplayPromptAsync("Nowa kategoria", "Podaj nazwê nowej kategorii:");
            if (string.IsNullOrWhiteSpace(newCategory))
            {
                Category.SelectedItem = _categories.FirstOrDefault(c => c != "Nowa...");
                return;
            }

            if (!_categories.Contains(newCategory, StringComparer.OrdinalIgnoreCase))
            {
                int index = _categories.Count - 1; 
                _categories.Insert(index, newCategory);
            }

            Category.ItemsSource = null;
            Category.ItemsSource = _categories;
            Category.SelectedItem = newCategory;

            List<ListItemModel> itemsFromFile = Utils.FromXML().Items;
            List<String> categoriesToSave = _categories.Where(c => c != "Nowa...").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Utils.ToXML(itemsFromFile, categoriesToSave);
        }
    }

    private async void OnConfirm(object sender, EventArgs e)
    {
        string name = Name.Text ?? "Item1";
        int amount = int.TryParse(Amount.Text, out int val) ? val : 0;
        string unit = Unit.SelectedItem as string ?? "szt";
        string category = Category.SelectedItem as string ?? "Inne";
        bool isOptional = Optional.IsChecked;

        OnItemAdded?.Invoke(name, amount, unit, category, isOptional);

        await Navigation.PopModalAsync();
    }

    private async void Return(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}