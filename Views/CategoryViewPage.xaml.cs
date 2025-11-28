using System.Reflection.Metadata;

namespace ShoppingList.Views;
public partial class CategoryViewPage : ContentPage, IQueryAttributable
{
    public CategoryViewPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("category"))
        {
            CategoryLabel.Text = query["category"].ToString();
        }
    }

    private async void Return(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ShoppingListPage));
    }
}