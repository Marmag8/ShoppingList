using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList.Models
{
    class ListItemModel
    {
        private String _name;
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _amount;
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        private String _unit;
        public String Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        private String _category;
        public String Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public ListItemModel(String name, int amount, String unit, String category)
        {
            this.Name = name;
            this.Amount = amount;
            this.Unit = unit;
            this.Category = category;
        }
    }
}
