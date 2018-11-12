using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Item> AddItem(Item item)
        {
            var addedItem = await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
            return addedItem.Entity;
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers
                .FindAsync(customerId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
