namespace ShoppingList;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Views.StorePage), typeof(Views.StorePage));
        Routing.RegisterRoute(nameof(Views.ShoppingListPage), typeof(Views.ShoppingListPage));
    }
}
