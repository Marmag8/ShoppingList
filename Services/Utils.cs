using ShoppingList.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ShoppingList.Services
{
    class Utils
    {
        public static void ToXML(List<ListItemModel> items)
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            String path = Path.Combine(dir, "itemList.xml");

            if (File.Exists(path))
            {
                File.WriteAllText(path, string.Empty);
            }

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);

            XmlElement root = doc.CreateElement("Items");
            doc.AppendChild(root);

            foreach (ListItemModel i in items)
            {
                XmlElement counterNode = doc.CreateElement("Item");

                XmlElement nameNode = doc.CreateElement("Name");
                nameNode.InnerText = i.Name;
                counterNode.AppendChild(nameNode);

                XmlElement amountNode = doc.CreateElement("Amount");
                amountNode.InnerText = i.Amount.ToString();
                counterNode.AppendChild(amountNode);

                XmlElement unitNode = doc.CreateElement("Unit");
                unitNode.InnerText = i.Unit.ToString();
                counterNode.AppendChild(unitNode);

                XmlElement categoryNode = doc.CreateElement("Category");
                categoryNode.InnerText = i.Category.ToString();
                counterNode.AppendChild(categoryNode);

                root.AppendChild(counterNode);
            }

            doc.Save(path);
        }

        public static List<ListItemModel> FromXML()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShoppingList");
            string path = Path.Combine(dir, "itemList.xml");

            if (File.Exists(path))
            {
                var items = new List<ListItemModel>();

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode? root = doc.SelectSingleNode("Items");
                if (root != null)
                {

                    foreach (XmlNode itemNode in root.SelectNodes("Item")!)
                    {
                        string name = itemNode.SelectSingleNode("Name")?.InnerText ?? "item1";
                        int amount= int.TryParse(itemNode.SelectSingleNode("Amount")?.InnerText, out int res) ? res : 0;
                        string unit = itemNode.SelectSingleNode("Unit")?.InnerText ?? "szt";
                        string category = itemNode.SelectSingleNode("Category")?.InnerText ?? "Inne";
                        items.Add(new ListItemModel(name, amount, unit, category));
                    }
                }

                return (items);
            }
            else
            {
                return (new List<ListItemModel> { new ListItemModel("item1", 0, "szt", "Inne") });
            }
        }
    }
}
