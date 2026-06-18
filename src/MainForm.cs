using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CosmeticsStore
{
    public partial class MainForm : Form
    {
        private Catalog _catalog;
        private Cart _cart;
        private OrderModule _order;
        private Admin _admin;

        private DataGridView _catalogGrid;
        private DataGridView _cartGrid;
        private TextBox _searchBox;
        private ComboBox _categoryFilter;
        private NumericUpDown _priceFilter;
        private Label _totalLabel;
        private Button _addBtn, _removeBtn, _checkoutBtn, _adminBtn, _exitBtn;

        public MainForm()
        {
            InitializeComponent();
            InitializeModules();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "🌸 BeautyShop";
            this.Size = new Size(1100, 630);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Разделитель
            var split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 650;
            split.Padding = new Padding(10);

            // ===== ЛЕВАЯ ЧАСТЬ - КАТАЛОГ =====
            var catalogPanel = new Panel();
            catalogPanel.Dock = DockStyle.Fill;

            // Фильтры
            var filterPanel = new FlowLayoutPanel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 40;
            filterPanel.Padding = new Padding(5);

            filterPanel.Controls.Add(new Label { Text = "Поиск:", AutoSize = true });
            _searchBox = new TextBox { Width = 120 };
            _searchBox.TextChanged += (s, e) => ApplyFilters();
            filterPanel.Controls.Add(_searchBox);

            filterPanel.Controls.Add(new Label { Text = "Категория:", AutoSize = true });
            _categoryFilter = new ComboBox { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _categoryFilter.SelectedIndexChanged += (s, e) => ApplyFilters();
            filterPanel.Controls.Add(_categoryFilter);

            filterPanel.Controls.Add(new Label { Text = "Цена до:", AutoSize = true });
            _priceFilter = new NumericUpDown { Width = 70, Maximum = 10000 };
            _priceFilter.ValueChanged += (s, e) => ApplyFilters();
            filterPanel.Controls.Add(_priceFilter);

            // Таблица каталога
            _catalogGrid = new DataGridView();
            _catalogGrid.Dock = DockStyle.Fill;
            _catalogGrid.AllowUserToAddRows = false;
            _catalogGrid.ReadOnly = true;
            _catalogGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _catalogGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _catalogGrid.Columns.Add("Id", "ID");
            _catalogGrid.Columns.Add("Name", "Название");
            _catalogGrid.Columns.Add("Category", "Категория");
            _catalogGrid.Columns.Add("Price", "Цена");
            _catalogGrid.Columns.Add("Stock", "Наличие");

            catalogPanel.Controls.Add(_catalogGrid);
            catalogPanel.Controls.Add(filterPanel);

            _addBtn = new Button();
            _addBtn.Text = "➜ Добавить в корзину";
            _addBtn.Dock = DockStyle.Bottom;
            _addBtn.Height = 35;
            _addBtn.Click += AddToCart_Click;
            catalogPanel.Controls.Add(_addBtn);

            // ===== ПРАВАЯ ЧАСТЬ - КОРЗИНА =====
            var cartPanel = new Panel();
            cartPanel.Dock = DockStyle.Fill;

            _cartGrid = new DataGridView();
            _cartGrid.Dock = DockStyle.Fill;
            _cartGrid.AllowUserToAddRows = false;
            _cartGrid.ReadOnly = true;
            _cartGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _cartGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _cartGrid.Columns.Add("Name", "Название");
            _cartGrid.Columns.Add("Price", "Цена");
            _cartGrid.Columns.Add("Qty", "Кол-во");
            _cartGrid.Columns.Add("Total", "Сумма");

            _totalLabel = new Label();
            _totalLabel.Text = "Итого: 0.00 ₽";
            _totalLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            _totalLabel.Dock = DockStyle.Bottom;
            _totalLabel.Height = 35;
            _totalLabel.TextAlign = ContentAlignment.MiddleRight;
            _totalLabel.BackColor = Color.LightYellow;

            var cartButtons = new FlowLayoutPanel();
            cartButtons.Dock = DockStyle.Bottom;
            cartButtons.Height = 40;
            cartButtons.FlowDirection = FlowDirection.RightToLeft;
            cartButtons.Padding = new Padding(5);

            // Кнопка Выход (добавлена)
            _exitBtn = new Button();
            _exitBtn.Text = "✖ Выход";
            _exitBtn.Width = 80;
            _exitBtn.BackColor = Color.LightCoral;
            _exitBtn.Click += (s, e) =>
            {
                if (MessageBox.Show("Выйти из приложения?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };
            cartButtons.Controls.Add(_exitBtn);

            _adminBtn = new Button { Text = "Админка", Width = 80 };
            _adminBtn.Click += Admin_Click;
            cartButtons.Controls.Add(_adminBtn);

            _checkoutBtn = new Button { Text = "Оформить заказ", Width = 120 };
            _checkoutBtn.Click += Checkout_Click;
            cartButtons.Controls.Add(_checkoutBtn);

            _removeBtn = new Button { Text = "Удалить", Width = 80 };
            _removeBtn.Click += RemoveFromCart_Click;
            cartButtons.Controls.Add(_removeBtn);

            cartPanel.Controls.Add(_cartGrid);
            cartPanel.Controls.Add(_totalLabel);
            cartPanel.Controls.Add(cartButtons);

            split.Panel1.Controls.Add(catalogPanel);
            split.Panel2.Controls.Add(cartPanel);
            this.Controls.Add(split);
        }

        private void InitializeModules()
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            string productsFile = Path.Combine(dataDir, "products.json");
            string cartFile = Path.Combine(dataDir, "cart.json");
            string ordersFile = Path.Combine(dataDir, "orders.json");

            try
            {
                _catalog = new Catalog(productsFile);
                _cart = new Cart(cartFile);
                _order = new OrderModule(ordersFile);
                _admin = new Admin(_catalog, _order);
                Logger.Info("Приложение запущено");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                throw;
            }
        }

        private void LoadData()
        {
            var cats = _catalog.GetCategories();
            _categoryFilter.Items.Clear();
            _categoryFilter.Items.Add("Все");
            foreach (var c in cats)
                _categoryFilter.Items.Add(c);
            _categoryFilter.SelectedIndex = 0;

            RefreshCatalog();
            RefreshCart();
        }

        private void RefreshCatalog()
        {
            _catalogGrid.Rows.Clear();
            foreach (var p in _catalog.GetAll())
                _catalogGrid.Rows.Add(p.Id, p.Name, p.Category, p.Price.ToString("F2"), p.Stock);
        }

        private void ApplyFilters()
        {
            string search = _searchBox.Text.Trim();
            string category = _categoryFilter.SelectedItem?.ToString() ?? "Все";
            decimal maxPrice = _priceFilter.Value;

            var products = _catalog.GetAll();

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (category != "Все")
                products = products.Where(p => p.Category == category).ToList();

            if (maxPrice > 0)
                products = products.Where(p => p.Price <= maxPrice).ToList();

            _catalogGrid.Rows.Clear();
            foreach (var p in products)
                _catalogGrid.Rows.Add(p.Id, p.Name, p.Category, p.Price.ToString("F2"), p.Stock);
        }

        private void RefreshCart()
        {
            _cartGrid.Rows.Clear();
            decimal total = 0;

            foreach (var item in _cart.GetItems())
            {
                decimal subtotal = item.Price * item.Quantity;
                total += subtotal;
                _cartGrid.Rows.Add(item.Name, item.Price.ToString("F2"), item.Quantity, subtotal.ToString("F2"));
            }

            _totalLabel.Text = $"Итого: {total:F2} ₽";
        }

        private void AddToCart_Click(object sender, EventArgs e)
        {
            if (_catalogGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            string id = _catalogGrid.SelectedRows[0].Cells["Id"].Value?.ToString();
            if (string.IsNullOrEmpty(id)) return;

            var product = _catalog.GetById(id);
            if (product == null)
            {
                MessageBox.Show("Товар не найден");
                return;
            }

            if (product.Stock <= 0)
            {
                MessageBox.Show("Товара нет на складе");
                return;
            }

            try
            {
                _cart.AddItem(product, 1);
                RefreshCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void RemoveFromCart_Click(object sender, EventArgs e)
        {
            if (_cartGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите товар в корзине");
                return;
            }

            string name = _cartGrid.SelectedRows[0].Cells["Name"].Value?.ToString();
            if (string.IsNullOrEmpty(name)) return;

            var items = _cart.GetItems();
            var item = items.FirstOrDefault(i => i.Name == name);
            if (item == null) return;

            try
            {
                _cart.RemoveItem(item.Id);
                RefreshCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Checkout_Click(object sender, EventArgs e)
        {
            if (_cart.GetItems().Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            var form = new Form
            {
                Text = "Оформление заказа",
                Size = new Size(400, 350),
                StartPosition = FormStartPosition.CenterParent
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 5,
                ColumnCount = 2
            };

            string[] labels = { "Имя:", "Адрес:", "Телефон:", "Email:" };
            var inputs = new TextBox[4];

            for (int i = 0; i < 4; i++)
            {
                panel.Controls.Add(new Label { Text = labels[i], AutoSize = true }, 0, i);
                inputs[i] = new TextBox { Dock = DockStyle.Fill };
                panel.Controls.Add(inputs[i], 1, i);
            }

            var okBtn = new Button { Text = "Подтвердить", DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };

            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            btnPanel.Controls.Add(okBtn);
            btnPanel.Controls.Add(cancelBtn);
            panel.Controls.Add(btnPanel, 1, 4);
            panel.SetColumnSpan(btnPanel, 2);

            form.Controls.Add(panel);

            if (form.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string name = inputs[0].Text.Trim();
                    string address = inputs[1].Text.Trim();
                    string phone = inputs[2].Text.Trim();
                    string email = inputs[3].Text.Trim();

                    var items = _cart.GetItems();
                    string orderNumber = _order.CreateOrder(name, address, phone, email, items);

                    _cart.Clear();
                    RefreshCart();

                    MessageBox.Show($"Заказ №{orderNumber} оформлен!\nСпасибо за покупку!", "Успех");
                    Logger.Info($"Заказ {orderNumber} оформлен");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        // ===== АДМИНКА =====
        private void Admin_Click(object sender, EventArgs e)
        {
            // Создаем форму входа
            Form loginForm = new Form();
            loginForm.Text = "Вход в админ-панель";
            loginForm.Size = new Size(350, 180);
            loginForm.StartPosition = FormStartPosition.CenterParent;
            loginForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            loginForm.MaximizeBox = false;
            loginForm.MinimizeBox = false;

            // Панель для элементов
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);
            panel.RowCount = 3;
            panel.ColumnCount = 2;

            // Метка
            Label lblPassword = new Label();
            lblPassword.Text = "Пароль:";
            lblPassword.AutoSize = true;
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;

            // Поле для пароля
            TextBox txtPassword = new TextBox();
            txtPassword.PasswordChar = '*';
            txtPassword.Dock = DockStyle.Fill;

            // Кнопки
            FlowLayoutPanel btnPanel = new FlowLayoutPanel();
            btnPanel.Dock = DockStyle.Fill;
            btnPanel.FlowDirection = FlowDirection.RightToLeft;
            btnPanel.Padding = new Padding(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Text = "Войти";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Width = 80;
            btnOk.Height = 30;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Width = 80;
            btnCancel.Height = 30;

            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);

            // Добавляем элементы на панель
            panel.Controls.Add(lblPassword, 0, 0);
            panel.Controls.Add(txtPassword, 1, 0);
            panel.Controls.Add(btnPanel, 1, 1);
            panel.SetColumnSpan(btnPanel, 2);

            loginForm.Controls.Add(panel);

            // Показываем форму
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                if (txtPassword.Text == "admin123")
                {
                    ShowAdminForm();
                }
                else
                {
                    MessageBox.Show("Неверный пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowAdminForm()
        {
            Form adminForm = new Form();
            adminForm.Text = "Административная панель";
            adminForm.Size = new Size(900, 630);
            adminForm.StartPosition = FormStartPosition.CenterParent;

            // ===== КНОПКА ВЫХОДА В АДМИНКЕ =====
            // Создаем панель с кнопкой выхода внизу
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 50;
            bottomPanel.BackColor = Color.LightGray;
            bottomPanel.Padding = new Padding(10);

            Button exitAdminBtn = new Button();
            exitAdminBtn.Text = "✖ Закрыть админку";
            exitAdminBtn.Width = 150;
            exitAdminBtn.Height = 35;
            exitAdminBtn.BackColor = Color.LightCoral;
            exitAdminBtn.Location = new Point(adminForm.Width - 170, 8);
            exitAdminBtn.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            exitAdminBtn.Click += (s, e) => adminForm.Close();
            bottomPanel.Controls.Add(exitAdminBtn);

            // Основное содержимое
            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;

            // ===== ВКЛАДКА ТОВАРЫ =====
            TabPage productsTab = new TabPage("Товары");

            DataGridView productsGrid = new DataGridView();
            productsGrid.Dock = DockStyle.Fill;
            productsGrid.AllowUserToAddRows = false;
            productsGrid.ReadOnly = true;
            productsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            productsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            productsGrid.Columns.Add("Id", "ID");
            productsGrid.Columns.Add("Name", "Название");
            productsGrid.Columns.Add("Category", "Категория");
            productsGrid.Columns.Add("Price", "Цена");
            productsGrid.Columns.Add("Stock", "Наличие");

            void LoadProducts()
            {
                productsGrid.Rows.Clear();
                foreach (var p in _catalog.GetAll())
                    productsGrid.Rows.Add(p.Id, p.Name, p.Category, p.Price.ToString("F2"), p.Stock);
            }
            LoadProducts();

            FlowLayoutPanel productBtns = new FlowLayoutPanel();
            productBtns.Dock = DockStyle.Bottom;
            productBtns.Height = 45;
            productBtns.Padding = new Padding(5);
            productBtns.FlowDirection = FlowDirection.LeftToRight;

            // Кнопка Добавить
            Button btnAdd = new Button();
            btnAdd.Text = "Добавить";
            btnAdd.Width = 100;
            btnAdd.Height = 30;
            btnAdd.Click += (s, e) =>
            {
                var result = ShowProductDialog(null);
                if (result.name != null)
                {
                    try
                    {
                        _admin.AddProduct(result.name, result.category, result.price, result.stock, result.description);
                        LoadProducts();
                        RefreshCatalog();
                        MessageBox.Show("Товар добавлен!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            };
            productBtns.Controls.Add(btnAdd);

            // Кнопка Редактировать
            Button btnEdit = new Button();
            btnEdit.Text = "Редактировать";
            btnEdit.Width = 100;
            btnEdit.Height = 30;
            btnEdit.Click += (s, e) =>
            {
                if (productsGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите товар");
                    return;
                }
                string id = productsGrid.SelectedRows[0].Cells["Id"].Value?.ToString();
                var product = _catalog.GetById(id);
                if (product == null) return;

                var result = ShowProductDialog(product);
                if (result.name != null)
                {
                    try
                    {
                        _admin.UpdateProduct(product.Id, result.name, result.category, result.price, result.stock, result.description);
                        LoadProducts();
                        RefreshCatalog();
                        MessageBox.Show("Товар обновлен!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            };
            productBtns.Controls.Add(btnEdit);

            // Кнопка Удалить
            Button btnDelete = new Button();
            btnDelete.Text = "Удалить";
            btnDelete.Width = 100;
            btnDelete.Height = 30;
            btnDelete.Click += (s, e) =>
            {
                if (productsGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите товар");
                    return;
                }
                string id = productsGrid.SelectedRows[0].Cells["Id"].Value?.ToString();
                if (MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        _admin.DeleteProduct(id);
                        LoadProducts();
                        RefreshCatalog();
                        MessageBox.Show("Товар удален!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            };
            productBtns.Controls.Add(btnDelete);

            // Кнопка Обновить
            Button btnRefresh = new Button();
            btnRefresh.Text = "Обновить";
            btnRefresh.Width = 100;
            btnRefresh.Height = 30;
            btnRefresh.Click += (s, e) => LoadProducts();
            productBtns.Controls.Add(btnRefresh);

            productsTab.Controls.Add(productsGrid);
            productsTab.Controls.Add(productBtns);

            // ===== ВКЛАДКА ЗАКАЗЫ =====
            TabPage ordersTab = new TabPage("Заказы");

            DataGridView ordersGrid = new DataGridView();
            ordersGrid.Dock = DockStyle.Fill;
            ordersGrid.AllowUserToAddRows = false;
            ordersGrid.ReadOnly = true;
            ordersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ordersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ordersGrid.Columns.Add("Number", "Номер");
            ordersGrid.Columns.Add("Customer", "Клиент");
            ordersGrid.Columns.Add("Total", "Сумма");
            ordersGrid.Columns.Add("Status", "Статус");
            ordersGrid.Columns.Add("Date", "Дата");

            void LoadOrders()
            {
                ordersGrid.Rows.Clear();
                foreach (var o in _order.GetAll())
                {
                    ordersGrid.Rows.Add(o.Number, o.CustomerName, o.Total.ToString("F2"), o.Status, o.Date);
                }
            }
            LoadOrders();

            FlowLayoutPanel orderBtns = new FlowLayoutPanel();
            orderBtns.Dock = DockStyle.Bottom;
            orderBtns.Height = 45;
            orderBtns.Padding = new Padding(5);
            orderBtns.FlowDirection = FlowDirection.LeftToRight;

            // Кнопка Изменить статус
            Button btnStatus = new Button();
            btnStatus.Text = "Изменить статус";
            btnStatus.Width = 130;
            btnStatus.Height = 30;
            btnStatus.Click += (s, e) =>
            {
                if (ordersGrid.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите заказ");
                    return;
                }
                string number = ordersGrid.SelectedRows[0].Cells["Number"].Value?.ToString();

                Form statusForm = new Form();
                statusForm.Text = "Изменение статуса";
                statusForm.Size = new Size(300, 150);
                statusForm.StartPosition = FormStartPosition.CenterParent;
                statusForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                statusForm.MaximizeBox = false;
                statusForm.MinimizeBox = false;

                TableLayoutPanel statusPanel = new TableLayoutPanel();
                statusPanel.Dock = DockStyle.Fill;
                statusPanel.Padding = new Padding(20);
                statusPanel.RowCount = 2;
                statusPanel.ColumnCount = 1;

                ComboBox comboStatus = new ComboBox();
                comboStatus.Dock = DockStyle.Fill;
                comboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
                comboStatus.Items.AddRange(new object[] { "новый", "в обработке", "доставлен", "отменен" });
                comboStatus.SelectedIndex = 0;

                FlowLayoutPanel statusBtnPanel = new FlowLayoutPanel();
                statusBtnPanel.Dock = DockStyle.Fill;
                statusBtnPanel.FlowDirection = FlowDirection.RightToLeft;

                Button btnStatusOk = new Button();
                btnStatusOk.Text = "OK";
                btnStatusOk.DialogResult = DialogResult.OK;
                btnStatusOk.Width = 80;
                btnStatusOk.Height = 30;

                Button btnStatusCancel = new Button();
                btnStatusCancel.Text = "Отмена";
                btnStatusCancel.DialogResult = DialogResult.Cancel;
                btnStatusCancel.Width = 80;
                btnStatusCancel.Height = 30;

                statusBtnPanel.Controls.Add(btnStatusOk);
                statusBtnPanel.Controls.Add(btnStatusCancel);

                statusPanel.Controls.Add(new Label { Text = "Выберите статус:", AutoSize = true }, 0, 0);
                statusPanel.Controls.Add(comboStatus, 0, 1);
                statusPanel.Controls.Add(statusBtnPanel, 0, 2);

                statusForm.Controls.Add(statusPanel);

                if (statusForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _admin.UpdateOrderStatus(number, comboStatus.SelectedItem.ToString());
                        LoadOrders();
                        MessageBox.Show("Статус обновлен!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            };
            orderBtns.Controls.Add(btnStatus);

            // Кнопка Обновить заказы
            Button btnOrdersRefresh = new Button();
            btnOrdersRefresh.Text = "Обновить";
            btnOrdersRefresh.Width = 100;
            btnOrdersRefresh.Height = 30;
            btnOrdersRefresh.Click += (s, e) => LoadOrders();
            orderBtns.Controls.Add(btnOrdersRefresh);

            ordersTab.Controls.Add(ordersGrid);
            ordersTab.Controls.Add(orderBtns);

            tabs.TabPages.Add(productsTab);
            tabs.TabPages.Add(ordersTab);

            // Сборка админки
            adminForm.Controls.Add(tabs);
            adminForm.Controls.Add(bottomPanel);
            adminForm.ShowDialog();
        }

        // Диалог для добавления/редактирования товара
        private (string name, string category, decimal price, int stock, string description) ShowProductDialog(Product? product)
        {
            Form form = new Form();
            form.Text = product == null ? "Добавить товар" : "Редактировать товар";
            form.Size = new Size(400, 380);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);
            panel.RowCount = 6;
            panel.ColumnCount = 2;

            string[] labels = { "Название:", "Категория:", "Цена:", "Наличие:", "Описание:" };
            TextBox[] inputs = new TextBox[5];

            for (int i = 0; i < 5; i++)
            {
                Label lbl = new Label();
                lbl.Text = labels[i];
                lbl.AutoSize = true;
                panel.Controls.Add(lbl, 0, i);

                inputs[i] = new TextBox();
                inputs[i].Dock = DockStyle.Fill;
                panel.Controls.Add(inputs[i], 1, i);
            }

            if (product != null)
            {
                inputs[0].Text = product.Name;
                inputs[1].Text = product.Category;
                inputs[2].Text = product.Price.ToString();
                inputs[3].Text = product.Stock.ToString();
                inputs[4].Text = product.Description;
            }

            FlowLayoutPanel btnPanel = new FlowLayoutPanel();
            btnPanel.Dock = DockStyle.Fill;
            btnPanel.FlowDirection = FlowDirection.RightToLeft;
            btnPanel.Padding = new Padding(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Width = 80;
            btnOk.Height = 30;

            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Width = 80;
            btnCancel.Height = 30;

            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);
            panel.Controls.Add(btnPanel, 1, 5);
            panel.SetColumnSpan(btnPanel, 2);

            form.Controls.Add(panel);

            if (form.ShowDialog() == DialogResult.OK)
            {
                string name = inputs[0].Text.Trim();
                string category = inputs[1].Text.Trim();
                decimal price = decimal.TryParse(inputs[2].Text, out decimal p) ? p : 0;
                int stock = int.TryParse(inputs[3].Text, out int s) ? s : 0;
                string description = inputs[4].Text.Trim();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(category))
                {
                    MessageBox.Show("Название и категория обязательны!");
                    return (null!, null!, 0, 0, null!);
                }

                return (name, category, price, stock, description);
            }

            return (null!, null!, 0, 0, null!);
        }
    }
}