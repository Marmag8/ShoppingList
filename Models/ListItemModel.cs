using CommunityToolkit.Mvvm.ComponentModel;

namespace ShoppingList.Models
{
    public partial class ListItemModel : ObservableObject
    {
        [ObservableProperty]
        private bool isBought;

        private string _name;
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        private int _amount;
        public int Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set => _unit = value;
        }

        private string _category;
        public string Category
        {
            get => _category;
            set => _category = value;
        }

        public ListItemModel(string name, int amount, string unit, string category)
        {
            Name = name;
            Amount = amount;
            Unit = unit;
            Category = category;
        }
    }
}
