using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CosmeticsStore
{
    public class Order
    {
        public string Number { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }
        public string Status { get; set; } = "новый";
        public string Date { get; set; } = "";
    }

    public class OrderModule
    {
        private string _filePath;
        private List<Order> _orders = new List<Order>();

        public OrderModule(string filePath)
        {
            _filePath = filePath;
            LoadOrders();
        }

        public void LoadOrders()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _orders = JsonSerializer.Deserialize<List<Order>>(json) ?? new List<Order>();
                }
                else
                {
                    _orders = new List<Order>();
                }
            }
            catch { _orders = new List<Order>(); }
        }

        public void SaveOrders()
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(_orders, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения заказов", ex);
                throw;
            }
        }

        public string CreateOrder(string customerName, string address, string phone, string email, List<CartItem> items)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new Exception("Введите имя");
            if (string.IsNullOrWhiteSpace(address))
                throw new Exception("Введите адрес");
            if (string.IsNullOrWhiteSpace(phone))
                throw new Exception("Введите телефон");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new Exception("Введите корректный email");

            var order = new Order
            {
                Number = GenerateOrderNumber(),
                CustomerName = customerName,
                Address = address,
                Phone = phone,
                Email = email,
                Items = items.ToList(),
                Total = items.Sum(i => i.Price * i.Quantity),
                Status = "новый",
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            _orders.Add(order);
            SaveOrders();
            Logger.Info($"Создан заказ {order.Number} на сумму {order.Total:F2}");
            return order.Number;
        }

        private string GenerateOrderNumber()
        {
            return $"BEAUTY-{DateTime.Now:yyyyMMddHHmmss}-{_orders.Count + 1:000}";
        }

        public List<Order> GetAll() => _orders.ToList();
        public Order? GetByNumber(string number) => _orders.FirstOrDefault(o => o.Number == number);

        public void UpdateStatus(string orderNumber, string status)
        {
            var validStatuses = new[] { "новый", "в обработке", "доставлен", "отменен" };
            if (!validStatuses.Contains(status))
                throw new Exception("Некорректный статус");

            var order = GetByNumber(orderNumber);
            if (order == null)
                throw new Exception("Заказ не найден");

            order.Status = status;
            SaveOrders();
            Logger.Info($"Статус заказа {orderNumber} изменен на {status}");
        }
    }
}