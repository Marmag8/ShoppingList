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
            foreach (ListItemModel item in Utils.FromXML())
                Items.Add(item);
        }

        [RelayCommand]
        private void DeleteItem(ListItemModel item)
        {
            Items.Remove(item);
            Utils.ToXML(Items.ToList());
        }

        [RelayCommand]
        private void AddItem(ListItemModel item)
        {
            Items.Add(item);
            Utils.ToXML(Items.ToList());
        }
    }
}