using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace ShopsUtility.Tabs
{
    public class ItemGroupTab : GroupTabBase
    {
        public ICommand ItemGroupRemovedCommand { get; set; }

        public ItemGroupTab(MainWindow window, string connectionString) : base(window, connectionString)
        {
            ItemGroupRemovedCommand = new ActionCommand(x => ItemGroupRemoved((string) x));
        }

        private void ItemGroupRemoved(string itemGroup)
        {

        }
    }
}
