using System.Xml.Linq;
using ShoppingList.Models;

namespace ShoppingList.Services
{
    internal static class Utils
    {
        private static readonly string[] DefaultCategories =
        {
            "Mleczne",
            "Owoce",
            "Warzywa",
            "Mięso",
            "Picie",
            "Inne"
        };

        public static string GetAppDataPath()
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        public static string GetDataFilePath()
        {
            return Path.Combine(GetAppDataPath(), "itemList.xml");
        }

        public static void ToXML(List<ListItemModel> items, IEnumerable<string> categories)
        {
            List<RecipeModel> recipes = FromRecipesXML();
            ToXML(items, categories, recipes);
        }

        public static void ToXML(List<ListItemModel> items, IEnumerable<string> categories, List<RecipeModel> recipes)
        {
            string path = GetDataFilePath();

            List<string> mergedCategories = DefaultCategories
                .Concat(categories)
                .Where((string s) => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            XDeclaration declaration = new XDeclaration("1.0", "UTF-8", null);

            XElement categoriesElement = new XElement(
                "Categories",
                mergedCategories.Select(c => new XElement("Category", c))
            );

            XElement recipesElement = new XElement(
                "Recipes",
                recipes.Select(recipe =>
                    new XElement(
                        "Recipe",
                        new XElement("Name", recipe.Name),
                        new XElement("Description", recipe.Description),
                        new XElement("Category", string.IsNullOrWhiteSpace(recipe.Category) ? "Inne" : recipe.Category),
                        new XElement(
                            "Ingredients",
                            recipe.Ingredients.Select(ingredient =>
                                new XElement(
                                    "Ingredient",
                                    new XElement("Name", ingredient.Name),
                                    new XElement("Amount", ingredient.Amount),
                                    new XElement("Unit", ingredient.Unit),
                                    new XElement("Category", string.IsNullOrWhiteSpace(ingredient.Category) ? "Inne" : ingredient.Category),
                                    new XElement("IsOptional", ingredient.IsOptional),
                                    new XElement("Store", ingredient.Store ?? "Dowolny")
                                )
                            )
                        )
                    )
                )
            );

            XElement itemsElement = new XElement(
                "Items",
                items.Select(item =>
                    new XElement(
                        "Item",
                        new XElement("Name", item.Name),
                        new XElement("Amount", item.Amount),
                        new XElement("Unit", item.Unit),
                        new XElement("Category", item.Category),
                        new XElement("IsBought", item.IsBought),
                        new XElement("IsOptional", item.IsOptional),
                        new XElement("Store", item.Store)
                    )
                )
            );

            XElement rootElement = new XElement("ShoppingList", categoriesElement, recipesElement, itemsElement);
            XDocument document = new XDocument(declaration, rootElement);

            document.Save(path);
        }

        public static (List<ListItemModel> Items, List<string> Categories) FromXML()
        {
            string path = GetDataFilePath();

            List<ListItemModel> items = new List<ListItemModel>();
            List<string> categories = new List<string>();

            if (!File.Exists(path))
            {
                categories.AddRange(DefaultCategories);
                items.Add(new ListItemModel("item1", 0, "szt", "Inne", false, "Dowolny"));
                return (items, categories);
            }

            XDocument document = XDocument.Load(path);
            XElement root = document.Root ?? new XElement("ShoppingList");

            IEnumerable<XElement> categoryNodes = root.Element("Categories")?.Elements("Category") ?? Enumerable.Empty<XElement>();
            List<string> fileCategories = categoryNodes
                .Select((XElement x) => (x.Value ?? string.Empty).Trim())
                .Where((string s) => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            categories.AddRange(fileCategories);

            foreach (string defaultCategory in DefaultCategories)
            {
                if (!categories.Contains(defaultCategory, StringComparer.OrdinalIgnoreCase))
                {
                    categories.Add(defaultCategory);
                }
            }

            IEnumerable<XElement> itemNodes = root.Element("Items")?.Elements("Item") ?? Enumerable.Empty<XElement>();
            foreach (XElement node in itemNodes)
            {
                string name = (node.Element("Name")?.Value ?? "item1").Trim();
                int amount = int.TryParse(node.Element("Amount")?.Value, out int parsedAmount) ? parsedAmount : 0;
                string unit = (node.Element("Unit")?.Value ?? "szt").Trim();
                string category = (node.Element("Category")?.Value ?? "Inne").Trim();

                ListItemModel item = new ListItemModel(name, amount, unit, category);

                bool parsedIsBought;
                if (bool.TryParse(node.Element("IsBought")?.Value, out parsedIsBought))
                {
                    item.IsBought = parsedIsBought;
                }

                bool parsedIsOptional;
                if (bool.TryParse(node.Element("IsOptional")?.Value, out parsedIsOptional))
                {
                    item.IsOptional = parsedIsOptional;
                }

                item.Store = (node.Element("Store")?.Value ?? "Dowolny").Trim();

                items.Add(item);

                if (!string.IsNullOrWhiteSpace(category) && !categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                {
                    categories.Add(category);
                }
            }

            return (items, categories);
        }

        public static List<RecipeModel> FromRecipesXML()
        {
            string path = GetDataFilePath();

            List<RecipeModel> recipes = new List<RecipeModel>();
            if (!File.Exists(path))
            {
                return recipes;
            }

            XDocument document = XDocument.Load(path);
            XElement root = document.Root ?? new XElement("ShoppingList");

            IEnumerable<XElement> recipeNodes = root.Element("Recipes")?.Elements("Recipe") ?? Enumerable.Empty<XElement>();
            foreach (XElement recipeElement in recipeNodes)
            {
                string name = (recipeElement.Element("Name")?.Value ?? "Bez nazwy").Trim();
                string description = (recipeElement.Element("Description")?.Value ?? "Brak opisu").Trim();
                string category = (recipeElement.Element("Category")?.Value ?? "Inne").Trim();

                IEnumerable<XElement> ingredientNodes = recipeElement.Element("Ingredients")?.Elements("Ingredient") ?? Enumerable.Empty<XElement>();
                List<ListItemModel> ingredients = ingredientNodes.Select((XElement ingredientElement) =>
                {
                    string ingredientName = (ingredientElement.Element("Name")?.Value ?? "Składnik").Trim();
                    int ingredientAmount = int.TryParse(ingredientElement.Element("Amount")?.Value, out int parsedIngredientAmount) ? parsedIngredientAmount : 0;
                    string ingredientUnit = (ingredientElement.Element("Unit")?.Value ?? "szt").Trim();
                    string ingredientCategory = (ingredientElement.Element("Category")?.Value ?? "Inne").Trim();

                    ListItemModel ingredientItem = new ListItemModel(ingredientName, ingredientAmount, ingredientUnit, ingredientCategory);

                    bool parsedIngredientIsOptional;
                    if (bool.TryParse(ingredientElement.Element("IsOptional")?.Value, out parsedIngredientIsOptional))
                    {
                        ingredientItem.IsOptional = parsedIngredientIsOptional;
                    }

                    string ingredientStore = (ingredientElement.Element("Store")?.Value ?? "Dowolny").Trim();
                    ingredientItem.Store = ingredientStore;

                    return ingredientItem;
                }).ToList();

                RecipeModel recipe = new RecipeModel(name, description, category, ingredients);
                recipes.Add(recipe);
            }

            return recipes;
        }
    }
}