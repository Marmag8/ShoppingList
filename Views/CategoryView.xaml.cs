using ShoppingList.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ShoppingList.Views
{
    public partial class CategoryView : ContentView
    {
        public static readonly BindableProperty CategoryProperty =
            BindableProperty.Create(nameof(Category), typeof(string), typeof(CategoryView), propertyChanged: OnCategoryChanged);

        public static readonly BindableProperty AllItemsProperty =
            BindableProperty.Create(nameof(AllItems), typeof(IEnumerable<ListItemModel>), typeof(CategoryView), propertyChanged: OnAllItemsChanged);

        private readonly ObservableCollection<ListItemModel> _filteredItems = [];

        public string Category
        {
            get => (string?)GetValue(CategoryProperty)!;
            set => SetValue(CategoryProperty, value);
        }

        public IEnumerable<ListItemModel>? AllItems
        {
            get => (IEnumerable<ListItemModel>?)GetValue(AllItemsProperty);
            set => SetValue(AllItemsProperty, value);
        }

        public CategoryView()
        {
            InitializeComponent();
            ItemsCollectionView.ItemsSource = _filteredItems;
        }

        private static void OnCategoryChanged(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is CategoryView view)
            {
                view.HeaderLabel.Text = newVal.ToString() ?? string.Empty;
                view.UpdateFilteredItems();
            }
        }

        private static void OnAllItemsChanged(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is not CategoryView view)
                return;

            if (oldVal is IEnumerable<ListItemModel> oldEnum)
            {
                foreach (INotifyPropertyChanged it in oldEnum.OfType<INotifyPropertyChanged>())
                    it.PropertyChanged -= view.OnItemPropertyChanged;
            }

            if (oldVal is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= view.OnSourceCollectionChanged;

            if (newVal is IEnumerable<ListItemModel> newEnum)
            {
                foreach (var it in newEnum.OfType<INotifyPropertyChanged>())
                    it.PropertyChanged += view.OnItemPropertyChanged;
            }

            if (newVal is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += view.OnSourceCollectionChanged;

            view.UpdateFilteredItems();
        }

        private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged old in e.OldItems.OfType<INotifyPropertyChanged>())
                    old.PropertyChanged -= OnItemPropertyChanged;
            }

            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged newNotify in e.NewItems.OfType<INotifyPropertyChanged>())
                    newNotify.PropertyChanged += OnItemPropertyChanged;
            }

            MainThread.BeginInvokeOnMainThread(UpdateFilteredItems);
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ListItemModel.IsBought))
            {
                MainThread.BeginInvokeOnMainThread(UpdateFilteredItems);
            }
        }

        private void UpdateFilteredItems()
        {
            _filteredItems.Clear();
            if (string.IsNullOrWhiteSpace(Category) || AllItems == null)
                return;

            IEnumerable<ListItemModel> matches = AllItems
                .Where(i => string.Equals(i.Category ?? string.Empty, Category ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.IsBought)
                .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase);
            foreach (ListItemModel item in matches)
                _filteredItems.Add(item);
        }
    }
}