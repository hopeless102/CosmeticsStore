using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
 
namespace CosmeticsStore
{
    public class CartItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; } = "";
    }

    public class Cart
    {
        private string _filePath;
        private List<CartItem> _items = new List<CartItem>();

        public Cart(string filePath)
        {
            _filePath = filePath;
            LoadCart();
        }

        public void LoadCart()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _items = JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
                }
                else
                {
                    _items = new List<CartItem>();
                }
            }
            catch { _items = new List<CartItem>(); }
        }

        public void SaveCart()
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения корзины", ex);
                throw;
            }
        }

        public List<CartItem> GetItems() => _items.ToList();

        public void AddItem(Product product, int quantity = 1)
        {
            if (quantity <= 0)
                throw new Exception("Количество должно быть больше 0");

            var existing = _items.FirstOrDefault(i => i.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new CartItem
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    Category = product.Category
                });
            }

            SaveCart();
            Logger.Info($"Добавлен в корзину: {product.Name}, {quantity} шт.");
        }

        public void RemoveItem(string productId)
        {
            var item = _items.FirstOrDefault(i => i.Id == productId);
            if (item == null)
                throw new Exception("Товар не найден в корзине");
            _items.Remove(item);
            SaveCart();
            Logger.Info($"Удален из корзины: {item.Name}");
        }

        public void UpdateQuantity(string productId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Количество должно быть больше 0");
            var item = _items.FirstOrDefault(i => i.Id == productId);
            if (item == null)
                throw new Exception("Товар не найден в корзине");
            item.Quantity = quantity;
            SaveCart();
            Logger.Info($"Изменено количество: {item.Name} -> {quantity}");
        }

        public decimal GetTotal() => _items.Sum(i => i.Price * i.Quantity);
        public void Clear() { _items.Clear(); SaveCart(); Logger.Info("Корзина очищена"); }
    }
}
