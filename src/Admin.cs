using System;
using System.Collections.Generic;

namespace CosmeticsStore
{
    public class Admin
    {
        private Catalog _catalog;
        private OrderModule _order;

        public Admin(Catalog catalog, OrderModule order)
        {
            _catalog = catalog;
            _order = order;
        }

        public List<Product> GetAllProducts() => _catalog.GetAll();

        public void AddProduct(string name, string category, decimal price, int stock, string description)
        {
            var product = new Product
            {
                Id = GenerateId(),
                Name = name,
                Category = category,
                Price = price,
                Stock = stock,
                Description = description
            };
            _catalog.Add(product);
        }

        public void UpdateProduct(string id, string name, string category, decimal price, int stock, string description)
        {
            var product = new Product
            {
                Id = id,
                Name = name,
                Category = category,
                Price = price,
                Stock = stock,
                Description = description
            };
            _catalog.Update(product);
        }

        public void DeleteProduct(string id) => _catalog.Delete(id);
        public List<Order> GetAllOrders() => _order.GetAll();
        public void UpdateOrderStatus(string number, string status) => _order.UpdateStatus(number, status);

        private string GenerateId()
        {
            return $"pr_{DateTime.Now.Ticks.ToString().Substring(10, 4)}";
        }
    }
}