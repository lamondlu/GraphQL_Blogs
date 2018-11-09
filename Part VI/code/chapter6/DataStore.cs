using System.Collections.Generic;
using System.Linq;

namespace chapter1
{
    public class DataStore : IDataStore
    {
        private ApplicationDbContext _context;

        public DataStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public Item GetItemByBarcode(string barcode)
        {
            return _context.Items.First(i => i.Barcode == barcode);
        }

        public IEnumerable<Item> GetItems()
        {
            return _context.Items.ToList();
        }
    }
}
