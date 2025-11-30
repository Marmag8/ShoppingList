using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShoppingList.Models;
using ShoppingList.Services;

namespace ShoppingList.ViewModels
{
    public partial class ShoppingListViewModel : ObservableObject
    {
        public ObservableCollection<ListItemModel> Items { get; } = new();

        [ObservableProperty]
        private ListItemModel? selectedItem;

        public ShoppingListViewModel()
        {
            (List<ListItemModel> items, List<string> categories) = Utils.FromXML();
            foreach (ListItemModel item in items)
                Items.Add(item);
        }

        [RelayCommand]
        private void DeleteItem(ListItemModel item)
        {
            Items.Remove(item);
            IEnumerable<String> categories = Items.Select(i => i.Category).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Utils.ToXML(Items.ToList(), categories);
        }

        [RelayCommand]
        private void AddItem(ListItemModel item)
        {
            Items.Add(item);
            IEnumerable<String> categories = Items.Select(i => i.Category).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            Utils.ToXML(Items.ToList(), categories);
        }
    }
}