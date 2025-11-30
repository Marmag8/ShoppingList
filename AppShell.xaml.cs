namespace ShoppingList;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Rejestr dodatkowych tras jeśli potrzebne:
        // Routing.RegisterRoute("category", typeof(Views.CategoryViewPage));
    }
}
