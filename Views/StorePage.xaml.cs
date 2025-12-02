using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;

namespace ShoppingList.Views;

public partial class StorePage : ContentPage
{
    private readonly ShoppingListViewModel _viewModel;
    private readonly ObservableCollection<ListItemModel> _storeItems = new();
    private string _store;
    private string _sorting;

    public StorePage(ShoppingListViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;

        ItemsCollectionView.ItemsSource = _storeItems;

        if (_viewModel.Items is INotifyCollectionChanged collection)
            collection.CollectionChanged += OnItemsCollectionChanged;

        foreach (var item in _viewModel.Items)
            SubscribeItem(item);

        RebuildStoreItems();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Store.SelectedItem = "Dowolny";
        Sorting.SelectedItem = "Według Kategorii";
        RebuildStoreItems();
    }

    private void RebuildStoreItems()
    {
        _storeItems.Clear();

        List <ListItemModel> items = new();

        switch (_store)
        {
            case "Dowolny":
                items = _viewModel.Items.ToList();
                break;
            case "Biedronka":
                items = _viewModel.Items.Where(i => i.Store == "Biedronka" || i.Store == "Dowolny").ToList();
                break;
            case "Lidl":
                items = _viewModel.Items.Where(i => i.Store == "Lidl" || i.Store == "Dowolny").ToList();
                break;
            case "Selgros":
                items = _viewModel.Items.Where(i => i.Store == "Selgros" || i.Store == "Dowolny").ToList();
                break;
            case "Carrefour":
                items = _viewModel.Items.Where(i => i.Store == "Carrefour" || i.Store == "Dowolny").ToList();
                break;
            case "Kaufland":
                items = _viewModel.Items.Where(i => i.Store == "Kaufland" || i.Store == "Dowolny").ToList();
                break;
            default:
                items = _viewModel.Items.ToList();
                break;
        }
            
        switch (_sorting)
        {
            case "category":
                foreach (ListItemModel item in items.Where(i => !i.IsBought).OrderBy(i => i.Category, StringComparer.OrdinalIgnoreCase).ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
                {
                    _storeItems.Add(item);
                }
                break;
            case "name":
                foreach (ListItemModel item in items.Where(i => !i.IsBought).OrderByDescending(i => i.Name, StringComparer.OrdinalIgnoreCase).ThenBy(i => i.Category, StringComparer.OrdinalIgnoreCase))
                {
                    _storeItems.Add(item);
                }
                break;
            case "amount":
                foreach (ListItemModel item in items.Where(i => !i.IsBought).OrderBy(i => i.Amount).ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
                {
                    _storeItems.Add(item);
                }
                break;
            default:
                foreach (ListItemModel item in items.Where(i => !i.IsBought).OrderBy(i => i.Category, StringComparer.OrdinalIgnoreCase).ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
                {
                    _storeItems.Add(item);
                }
                break;
        }
        
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (ListItemModel oldItem in e.OldItems.OfType<ListItemModel>())
                UnsubscribeItem(oldItem);

        if (e.NewItems != null)
            foreach (ListItemModel newItem in e.NewItems.OfType<ListItemModel>())
                SubscribeItem(newItem);

        RebuildStoreItems();
    }

    private void SubscribeItem(ListItemModel item)
    {
        if (item is INotifyPropertyChanged npc)
            npc.PropertyChanged += OnItemPropertyChanged;
    }

    private void UnsubscribeItem(ListItemModel item)
    {
        if (item is INotifyPropertyChanged npc)
            npc.PropertyChanged -= OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ListItemModel item)
            return;

        if (e.PropertyName == nameof(ListItemModel.IsBought))
        {
            if (item.IsBought)
            {
                _storeItems.Remove(item);
            }
            else
            {
                InsertSorted(item);
            }
            List<string> categories = _viewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Utils.ToXML(_viewModel.Items.ToList(), categories);
        }
        else if (e.PropertyName == nameof(ListItemModel.Category) ||
                 e.PropertyName == nameof(ListItemModel.Name))
        {
            if (!item.IsBought && _storeItems.Contains(item))
            {
                _storeItems.Remove(item);
                InsertSorted(item);
            }
        }
    }

    private void InsertSorted(ListItemModel item)
    {
        if (item.IsBought) return;
        int index = 0;
        while (index < _storeItems.Count &&
               (string.Compare(_storeItems[index].Category, item.Category, StringComparison.OrdinalIgnoreCase) < 0 ||
               (string.Equals(_storeItems[index].Category, item.Category, StringComparison.OrdinalIgnoreCase) &&
                string.Compare(_storeItems[index].Name, item.Name, StringComparison.OrdinalIgnoreCase) < 0)))
        {
            index++;
        }
        _storeItems.Insert(index, item);
    }

    private void OnStoreChanged(object sender, EventArgs e)
    {
        _store = Store.SelectedItem as string ?? "Dowolny";
        RebuildStoreItems();
    }

    private void OnSortingChanged(object sender, EventArgs e)
    {
        string selected = Sorting.SelectedItem as string;
        _sorting = selected switch
        {
            "Według Kategorii" => "category",
            "Według Nazwy" => "name",
            "Według Ilości" => "amount",
            _ => "category"
        };
        RebuildStoreItems();
    }

    private async void Return(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}