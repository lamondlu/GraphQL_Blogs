using System.Collections.Generic;

namespace chapter1
{
    public interface IDataStore
    {
        IEnumerable<Item> GetItems();
        Item GetItemByBarcode(string barcode);
    }
}
