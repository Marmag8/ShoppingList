using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ShoppingList.Models;

namespace ShoppingList.Views.Recipes;

public partial class RecipeView : ContentView
{
    public static readonly BindableProperty CategoryProperty =
        BindableProperty.Create(nameof(Category), typeof(string), typeof(RecipeView), propertyChanged: OnCategoryChanged);

    public static readonly BindableProperty AllRecipesProperty =
        BindableProperty.Create(nameof(AllRecipes), typeof(IEnumerable<RecipeModel>), typeof(RecipeView), propertyChanged: OnAllRecipesChanged);

    private readonly ObservableCollection<RecipeModel> _filtered = new();

    public string? Category
    {
        get => (string?)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public IEnumerable<RecipeModel>? AllRecipes
    {
        get => (IEnumerable<RecipeModel>?)GetValue(AllRecipesProperty);
        set => SetValue(AllRecipesProperty, value);
    }

    public RecipeView()
    {
        InitializeComponent();
        RecipesCollectionView.ItemsSource = _filtered;
    }

    private static void OnCategoryChanged(BindableObject bindable, object oldVal, object newVal)
    {
        if (bindable is RecipeView v)
        {
            v.HeaderLabel.Text = newVal?.ToString() ?? string.Empty;
            v.UpdateFiltered();
        }
    }

    private static void OnAllRecipesChanged(BindableObject bindable, object oldVal, object newVal)
    {
        if (bindable is RecipeView v)
        {
            if (oldVal is INotifyCollectionChanged oldIncc)
                oldIncc.CollectionChanged -= v.OnSourceCollectionChanged;

            if (newVal is INotifyCollectionChanged newIncc)
                newIncc.CollectionChanged += v.OnSourceCollectionChanged;

            v.UpdateFiltered();
        }
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateFiltered);
    }

    private void UpdateFiltered()
    {
        _filtered.Clear();
        if (string.IsNullOrWhiteSpace(Category) || AllRecipes is null)
            return;

        foreach (var r in AllRecipes.Where(r => string.Equals(r.Category ?? string.Empty, Category ?? string.Empty, System.StringComparison.OrdinalIgnoreCase)))
            _filtered.Add(r);
    }
}