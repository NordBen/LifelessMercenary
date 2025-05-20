using System;
using System.Collections.Generic;

namespace LM.Inventory
{
    public static class InventoryEvents
    {
        public static Action<Item> ItemSelected;
        public static Action<UIItem> ItemClicked;
        public static Action ScreenEnabled;
        public static Action<EItemType, EItemGrade> ItemsFiltered;
        public static Action<List<Item>> InventoryUpdated;
    }
}