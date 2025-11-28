namespace ShoppingList
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(Views.CategoryViewPage), typeof(Views.CategoryViewPage));
            Routing.RegisterRoute(nameof(Views.ShoppingListPage), typeof(Views.ShoppingListPage));
        }
    }
}
