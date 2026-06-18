using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace CosmeticsStore
{
    public class Product
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; } = "";
    }

    public class Catalog
    {
        private string _filePath;
        private List<Product> _products = new List<Product>();

        public Catalog(string filePath)
        {
            _filePath = filePath;
            LoadProducts();
        }

        public void LoadProducts()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    CreateDemoData();
                    return;
                }

                string json = File.ReadAllText(_filePath);
                _products = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
                Logger.Info($"Загружено {_products.Count} товаров");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка загрузки каталога", ex);
                throw;
            }
        }

        private void CreateDemoData()
        {
            _products = new List<Product>
            {
                new Product { Id = "1", Name = "Крем увлажняющий", Category = "Уход за кожей", Price = 45.99m, Stock = 20, Description = "С гиалуроновой кислотой" },
                new Product { Id = "2", Name = "Тональный крем", Category = "Декоративная косметика", Price = 35.50m, Stock = 15, Description = "SPF 15" },
                new Product { Id = "3", Name = "Духи Chanel №5", Category = "Парфюмерия", Price = 129.99m, Stock = 5, Description = "Легендарный аромат" },
                new Product { Id = "4", Name = "Гель для душа", Category = "Мужская косметика", Price = 15.99m, Stock = 30, Description = "Свежий аромат" },
                new Product { Id = "5", Name = "Сыворотка витамин С", Category = "Уход за кожей", Price = 65.00m, Stock = 12, Description = "Антиоксидантная" },
                new Product { Id = "6", Name = "Помада матовая", Category = "Декоративная косметика", Price = 22.99m, Stock = 25, Description = "Стойкая" },
                new Product { Id = "7", Name = "Туалетная вода", Category = "Парфюмерия", Price = 89.99m, Stock = 8, Description = "Цветочный аромат" },
                new Product { Id = "8", Name = "Бальзам после бритья", Category = "Мужская косметика", Price = 19.99m, Stock = 18, Description = "Успокаивающий" }
            };

            SaveProducts();
            Logger.Info("Созданы демо-данные");
        }

        public void SaveProducts()
        {
            try
            {
                string dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(_products, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка сохранения", ex);
                throw;
            }
        }

        public List<Product> GetAll() => _products.ToList();
        public List<string> GetCategories() => _products.Select(p => p.Category).Distinct().ToList();

        public List<Product> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetAll();
            return _products.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Product> FilterByCategory(string category)
        {
            if (string.IsNullOrEmpty(category) || category == "Все")
                return GetAll();
            return _products.Where(p => p.Category == category).ToList();
        }

        public List<Product> FilterByPrice(decimal maxPrice)
        {
            if (maxPrice <= 0)
                return GetAll();
            return _products.Where(p => p.Price <= maxPrice).ToList();
        }

        public Product? GetById(string id) => _products.FirstOrDefault(p => p.Id == id);

        public void Add(Product product)
        {
            if (_products.Any(p => p.Id == product.Id))
                throw new Exception("Товар с таким ID уже существует");
            _products.Add(product);
            SaveProducts();
            Logger.Info($"Добавлен товар: {product.Name}");
        }

        public void Update(Product product)
        {
            int index = _products.FindIndex(p => p.Id == product.Id);
            if (index == -1)
                throw new Exception("Товар не найден");
            _products[index] = product;
            SaveProducts();
            Logger.Info($"Обновлен товар: {product.Name}");
        }

        public void Delete(string id)
        {
            var product = GetById(id);
            if (product == null)
                throw new Exception("Товар не найден");
            _products.Remove(product);
            SaveProducts();
            Logger.Info($"Удален товар: {product.Name}");
        }
    }
}
