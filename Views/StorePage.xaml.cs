using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ShoppingList.Models;
using ShoppingList.Services;
using ShoppingList.ViewModels;

namespace ShoppingList.Views;

public partial class StorePage : ContentPage, IQueryAttributable
{
    private readonly ShoppingListViewModel _viewModel;
    private readonly ObservableCollection<ListItemModel> _storeItems = new();

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

        RebuildStoreItems();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("store"))
        {
            String storeName = query["store"].ToString();
            // jakas inna logika
        }
    }

    private void RebuildStoreItems()
    {
        _storeItems.Clear();
        foreach (ListItemModel item in _viewModel.Items
                     .Where(i => !i.IsBought)
                     .OrderBy(i => i.Category, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
        {
            _storeItems.Add(item);
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
            var cats = _viewModel.Items
                .Select(i => i.Category)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            Utils.ToXML(_viewModel.Items.ToList(), cats);
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

    private async void Return(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}