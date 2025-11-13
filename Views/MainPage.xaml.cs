using System.Diagnostics.Metrics;
using ShoppingList.Models;
using ShoppingList.Services;

namespace ShoppingList.Views
{
    /* TODO
     * context menu -> move deletion to menu
     * move to viewmodels for display
     * mark product as bought (strikethrough) (also in context menu)
     */
    public partial class MainPage : ContentPage
    {
        List<ListItemModel> items = new();
        public MainPage()
        {
            InitializeComponent();

            items = Utils.FromXML();
            foreach (ListItemModel item in items)
            {
                AddItemToUI(item);
            }
        }

        private void OnListChanged()
        {
            Utils.ToXML(items);
        }

        private async void OnAddItemClicked(object sender, EventArgs e)
        {
            AddItemPage addItemPage = new AddItemPage
            {
                OnItemAdded = (name, amount, unit, category) =>
                {
                    ListItemModel item = new ListItemModel(name, amount, unit, category);
                    items.Add(item);
                    AddItemToUI(item);
                    OnListChanged();
                }
            };
            await Navigation.PushModalAsync(addItemPage);
        }

        private void AddItemToUI(ListItemModel item)
        {
            Label label = new Label
            {
                Text = $"{item.Name} {item.Amount}{item.Unit}\nKategoria: {item.Category}",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 20, 0, 0),
            };

            Button deleteBtn = new Button
            {
                Text = "Usuń",
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(5, 5, 5, 5),
            };

            HorizontalStackLayout btns = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Children = { deleteBtn }
            };

            deleteBtn.Clicked += (s, e) =>
            {
                items.Remove(item);
                ListLayout.Children.Remove(label);
                ListLayout.Children.Remove(btns);
                OnListChanged();
            };

            ListLayout.Children.Add(label);
            ListLayout.Children.Add(btns);
        }
    }
}
