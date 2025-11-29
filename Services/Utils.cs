using ShoppingList.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ShoppingList.Services
{
    internal static class Utils
    {
        private static readonly string[] DefaultCategories = new[]
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
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = Path.Combine(dir, "itemList.xml");

            var mergedCategories = DefaultCategories
                .Concat(categories ?? Array.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);

            XmlElement root = doc.CreateElement("ShoppingList");
            doc.AppendChild(root);

            XmlElement categoriesNode = doc.CreateElement("Categories");
            foreach (string c in mergedCategories)
            {
                XmlElement categoryNode = doc.CreateElement("Category");
                categoryNode.InnerText = c;
                categoriesNode.AppendChild(categoryNode);
            }
            root.AppendChild(categoriesNode);

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
                    if (!string.IsNullOrWhiteSpace(c.InnerText) && !categories.Contains(c.InnerText, StringComparer.OrdinalIgnoreCase))
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

                    if (!string.IsNullOrWhiteSpace(category) && !categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                        categories.Add(category);
                }
            }

            foreach (var def in DefaultCategories)
            {
                if (!categories.Contains(def, StringComparer.OrdinalIgnoreCase))
                    categories.Add(def);
            }

            return (items, categories);
        }
    }
}
