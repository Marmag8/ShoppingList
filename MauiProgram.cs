using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using ShoppingList.ViewModels;
using ShoppingList.Views;

namespace ShoppingList
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<ShoppingListViewModel>();
            builder.Services.AddSingleton<RecipesViewModel>();

            builder.Services.AddTransient<ShoppingListPage>();
            builder.Services.AddTransient<ShoppingList.Views.Recipes.RecipesPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
