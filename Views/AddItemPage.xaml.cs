using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShoppingList.Views;

public partial class AddItemPage : ContentPage
{
    public Action<string, int, string, string>? OnItemAdded;
    public AddItemPage()
    {
        InitializeComponent();
        Unit.SelectedIndex = 5;
        Category.SelectedIndex = 5;
    }

    private async void OnConfirm(object sender, EventArgs e)
    {
        string name = Name.Text ?? "Item1";
        int amount = int.TryParse(Amount.Text, out int val) ? val : 0;
        string unit = Unit.SelectedItem as String ?? "szt";
        string category = Category.SelectedItem as String ?? "Inne";

        OnItemAdded?.Invoke(name, amount, unit, category);

        await Navigation.PopModalAsync();
    }

    private async void Return(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}