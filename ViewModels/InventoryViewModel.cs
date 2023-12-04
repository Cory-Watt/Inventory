using Inventory.Models;

namespace Inventory.ViewModels
{
    public class InventoryViewModel
    {
        public List<ItemModel> Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }

        public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);
    }
}
