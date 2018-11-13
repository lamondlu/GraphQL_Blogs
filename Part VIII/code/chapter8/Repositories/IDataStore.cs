using System.Collections.Generic;
using System.Threading.Tasks;

namespace chapter1
{
    public interface IDataStore
    {
        IEnumerable<Item> GetItems();
        Item GetItemByBarcode(string barcode);
        Task<Item> AddItem(Item item);

        Task<IEnumerable<Order>> GetOrdersAsync();

        Task<IEnumerable<Customer>> GetCustomersAsync();

        Task<Customer> GetCustomerByIdAsync(int customerId);

        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);

        Task<Order> AddOrderAsync(Order order);

        Task<Customer> AddCustomerAsync(Customer customer);
    }
}
