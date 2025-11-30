using System.Xml;
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

        public static void ToXML(List<ListItemModel> items, IEnumerable<string> categories)
        {
            List<RecipeModel> recipes = FromRecipesXML();
            ToXML(items, categories, recipes);
        }

        public static void ToXML(List<ListItemModel> items, IEnumerable<string> categories, List<RecipeModel> recipes)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, "itemList.xml");

            List<string> mergedCategories = DefaultCategories
                .Concat(categories ?? Array.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);

            XmlElement root = doc.CreateElement("ShoppingList");
            doc.AppendChild(root);

            // Kategorie
            XmlElement categoriesNode = doc.CreateElement("Categories");
            foreach (string c in mergedCategories)
            {
                XmlElement categoryNode = doc.CreateElement("Category");
                categoryNode.InnerText = c;
                categoriesNode.AppendChild(categoryNode);
            }
            root.AppendChild(categoriesNode);

            // Przepisy
            XmlElement recipesNode = doc.CreateElement("Recipes");
            foreach (RecipeModel r in recipes)
            {
                XmlElement recipeNode = doc.CreateElement("Recipe");

                XmlElement nameNode = doc.CreateElement("Name");
                nameNode.InnerText = r.Name ?? string.Empty;
                recipeNode.AppendChild(nameNode);

                XmlElement categoryNode = doc.CreateElement("Category");
                categoryNode.InnerText = r.Category ?? "Inne";
                recipeNode.AppendChild(categoryNode);

                XmlElement ingredientsNode = doc.CreateElement("Ingredients");
                foreach (ListItemModel i in r.Ingredients)
                {
                    XmlElement ingNode = doc.CreateElement("Ingredient");

                    XmlElement inName = doc.CreateElement("Name");
                    inName.InnerText = i.Name ?? string.Empty;
                    ingNode.AppendChild(inName);

                    XmlElement inAmount = doc.CreateElement("Amount");
                    inAmount.InnerText = i.Amount.ToString();
                    ingNode.AppendChild(inAmount);

                    XmlElement inUnit = doc.CreateElement("Unit");
                    inUnit.InnerText = i.Unit ?? string.Empty;
                    ingNode.AppendChild(inUnit);

                    XmlElement inCategory = doc.CreateElement("Category");
                    inCategory.InnerText = i.Category ?? "Inne";
                    ingNode.AppendChild(inCategory);

                    ingredientsNode.AppendChild(ingNode);
                }

                recipeNode.AppendChild(ingredientsNode);
                recipesNode.AppendChild(recipeNode);
            }
            root.AppendChild(recipesNode);

            // Produkty
            XmlElement itemsNode = doc.CreateElement("Items");
            foreach (ListItemModel i in items)
            {
                XmlElement itemNode = doc.CreateElement("Item");

                XmlElement nameNode = doc.CreateElement("Name");
                nameNode.InnerText = i.Name ?? string.Empty;
                itemNode.AppendChild(nameNode);

                XmlElement amountNode = doc.CreateElement("Amount");
                amountNode.InnerText = i.Amount.ToString();
                itemNode.AppendChild(amountNode);

                XmlElement unitNode = doc.CreateElement("Unit");
                unitNode.InnerText = i.Unit ?? string.Empty;
                itemNode.AppendChild(unitNode);

                XmlElement categoryNode = doc.CreateElement("Category");
                categoryNode.InnerText = i.Category ?? string.Empty;
                itemNode.AppendChild(categoryNode);

                XmlElement boughtNode = doc.CreateElement("IsBought");
                boughtNode.InnerText = i.IsBought.ToString();
                itemNode.AppendChild(boughtNode);

                itemsNode.AppendChild(itemNode);
            }
            root.AppendChild(itemsNode);

            doc.Save(path);
        }

        public static (List<ListItemModel> Items, List<string> Categories) FromXML()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            string path = Path.Combine(dir, "itemList.xml");

            var items = new List<ListItemModel>();
            var categories = new List<string>();

            if (!File.Exists(path))
            {
                categories.AddRange(DefaultCategories);
                items.Add(new ListItemModel("item1", 0, "szt", "Inne"));
                return (items, categories);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            var categoryNodes = doc.SelectNodes("/ShoppingList/Categories/Category");
            if (categoryNodes != null)
            {
                foreach (XmlNode c in categoryNodes)
                {
                    if (!string.IsNullOrWhiteSpace(c.InnerText) &&
                        !categories.Contains(c.InnerText, StringComparer.OrdinalIgnoreCase))
                        categories.Add(c.InnerText);
                }
            }

            var itemNodes = doc.SelectNodes("/ShoppingList/Items/Item");
            if (itemNodes != null)
            {
                foreach (XmlNode itemNode in itemNodes)
                {
                    string name = itemNode.SelectSingleNode("Name")?.InnerText ?? "item1";
                    int amount = int.TryParse(itemNode.SelectSingleNode("Amount")?.InnerText, out int res) ? res : 0;
                    string unit = itemNode.SelectSingleNode("Unit")?.InnerText ?? "szt";
                    string category = itemNode.SelectSingleNode("Category")?.InnerText ?? "Inne";

                    var item = new ListItemModel(name, amount, unit, category);

                    if (bool.TryParse(itemNode.SelectSingleNode("IsBought")?.InnerText, out bool isBought))
                        item.IsBought = isBought;

                    items.Add(item);

                    if (!string.IsNullOrWhiteSpace(category) &&
                        !categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                        categories.Add(category);
                }
            }

            foreach (var def in DefaultCategories)
                if (!categories.Contains(def, StringComparer.OrdinalIgnoreCase))
                    categories.Add(def);

            return (items, categories);
        }

        public static List<RecipeModel> FromRecipesXML()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            string path = Path.Combine(dir, "itemList.xml");

            var recipes = new List<RecipeModel>();
            if (!File.Exists(path))
                return recipes;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            var recipeNodes = doc.SelectNodes("ShoppingList/Recipes/Recipe");
            if (recipeNodes == null) return recipes;

            foreach (XmlNode rNode in recipeNodes)
            {
                string name = rNode.SelectSingleNode("Name")?.InnerText ?? "Bez nazwy";
                string category = rNode.SelectSingleNode("Category")?.InnerText ?? "Inne";

                var ingredients = new List<ListItemModel>();
                var ingNodes = rNode.SelectNodes("Ingredients/Ingredient");
                if (ingNodes != null)
                {
                    foreach (XmlNode ingNode in ingNodes)
                    {
                        string inName = ingNode.SelectSingleNode("Name")?.InnerText ?? "Składnik";
                        int inAmount = int.TryParse(ingNode.SelectSingleNode("Amount")?.InnerText, out int val) ? val : 0;
                        string inUnit = ingNode.SelectSingleNode("Unit")?.InnerText ?? "szt";
                        string inCategory = ingNode.SelectSingleNode("Category")?.InnerText ?? "Inne";
                        ingredients.Add(new ListItemModel(inName, inAmount, inUnit, inCategory));
                    }
                }

                recipes.Add(new RecipeModel(name, category, ingredients));
            }

            return recipes;
        }
    }
}
