using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DBManager
{
    public partial class FormMain : Form
    {
        internal IMessageBoxService MessageBoxService { get; set; } = new DefaultMessageBoxService();
        internal ComboBox comboTables = new ComboBox();
        private ComboBox comboTables2 = new ComboBox();
        internal DataGridView dgv = new DataGridView();
        internal TextBox txtSearch = new TextBox();
        private TextBox txtSql = new TextBox();
        private Button btnAdd = new Button();
        private Button btnEdit = new Button();
        private Button btnDelete = new Button();
        private Button btnRefresh = new Button();
        private Button btnExecuteSql = new Button();
        private Button btnExport = new Button();
        private Button btnImport = new Button();
        private Button btnJoin = new Button();
        private Button btnPrev = new Button();
        private Button btnNext = new Button();
        private Button btnCalculateSales = new Button();
        private Button btnCalculateAvgPrice = new Button();
        private Button btnSortByName = new Button();
        private Button btnQueryExample = new Button();
        private Button btnCountByCategory = new Button();
        internal DateTimePicker dtpStartDate = new DateTimePicker();
        internal DateTimePicker dtpEndDate = new DateTimePicker();
        internal DataTable currentTable;
        private Dictionary<(string, string), string> joinConditions;
        private ToolTip toolTip;

        public FormMain()
        {
         
            Width = 1300;
            Height = 850;
            this.Text = "Менеджер бази даних";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255);
            this.Font = new Font("Segoe UI", 10);

            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true
            };

            // Панель для вибору таблиць і пошуку
            var panelTop = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1260, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelTop);

            // Налаштування ComboBox для вибору першої таблиці
            comboTables.Location = new Point(10, 12);
            comboTables.Width = 300;
            comboTables.Font = new Font("Segoe UI", 10);
            comboTables.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTables.BackColor = Color.White;
            comboTables.SelectedIndexChanged += ComboTables_SelectedIndexChanged;
            toolTip.SetToolTip(comboTables, "Виберіть таблицю для відображення");
            panelTop.Controls.Add(comboTables);

            // Налаштування ComboBox для вибору другої таблиці
            comboTables2.Location = new Point(320, 12);
            comboTables2.Width = 300;
            comboTables2.Font = new Font("Segoe UI", 10);
            comboTables2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboTables2.BackColor = Color.White;
            comboTables2.SelectedIndexChanged += ComboTables2_SelectedIndexChanged;
            toolTip.SetToolTip(comboTables2, "Виберіть другу таблицю для об'єднання");
            panelTop.Controls.Add(comboTables2);

            // Налаштування текстового поля для пошуку
            txtSearch.Location = new Point(630, 12);
            txtSearch.Width = 300;
            txtSearch.Text = "Пошук...";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Font = new Font("Segoe UI", 10);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.BackColor = Color.White;
            txtSearch.GotFocus += (s, ev) =>
            {
                if (txtSearch.Text == "Пошук...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
            };
            txtSearch.LostFocus += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Пошук..."; txtSearch.ForeColor = Color.Gray; }
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            toolTip.SetToolTip(txtSearch, "Введіть ключове слово для пошуку в таблиці");
            panelTop.Controls.Add(txtSearch);

            // Панель для керування таблицею (Додати, Редагувати, Видалити, Оновити)
            var panelTableActions = new Panel
            {
                Location = new Point(10, 70),
                Size = new Size(1260, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelTableActions);

            // Налаштування кнопки для додавання запису
            btnAdd.Text = "Додати";
            btnAdd.Location = new Point(10, 12);
            btnAdd.Width = 150;
            btnAdd.Height = 30;
            btnAdd.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnAdd.BackColor = Color.FromArgb(30, 144, 255);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnAdd.Click += BtnAdd_Click;
            toolTip.SetToolTip(btnAdd, "Додати новий запис до вибраної таблиці");
            panelTableActions.Controls.Add(btnAdd);

            // Налаштування кнопки для редагування запису
            btnEdit.Text = "Редагувати";
            btnEdit.Location = new Point(170, 12);
            btnEdit.Width = 150;
            btnEdit.Height = 30;
            btnEdit.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnEdit.BackColor = Color.FromArgb(30, 144, 255);
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnEdit.Click += BtnEdit_Click;
            toolTip.SetToolTip(btnEdit, "Редагувати вибраний запис");
            panelTableActions.Controls.Add(btnEdit);

            // Налаштування кнопки для видалення запису
            btnDelete.Text = "Видалити";
            btnDelete.Location = new Point(330, 12);
            btnDelete.Width = 150;
            btnDelete.Height = 30;
            btnDelete.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnDelete.BackColor = Color.FromArgb(220, 20, 60);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(178, 34, 34);
            btnDelete.Click += BtnDelete_Click;
            toolTip.SetToolTip(btnDelete, "Видалити вибраний запис");
            panelTableActions.Controls.Add(btnDelete);

            // Налаштування кнопки для оновлення даних
            btnRefresh.Text = "Оновити";
            btnRefresh.Location = new Point(490, 12);
            btnRefresh.Width = 150;
            btnRefresh.Height = 30;
            btnRefresh.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnRefresh.BackColor = Color.FromArgb(30, 144, 255);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnRefresh.Click += BtnRefresh_Click;
            toolTip.SetToolTip(btnRefresh, "Оновити дані таблиці");
            panelTableActions.Controls.Add(btnRefresh);

            // Панель для імпорту/експорту та об'єднання
            var panelImportExport = new Panel
            {
                Location = new Point(10, 130),
                Size = new Size(1260, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelImportExport);

            // Налаштування кнопки для експорту в CSV
            btnExport.Text = "Експорт у CSV";
            btnExport.Location = new Point(10, 12);
            btnExport.Width = 200;
            btnExport.Height = 30;
            btnExport.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnExport.BackColor = Color.FromArgb(30, 144, 255);
            btnExport.ForeColor = Color.White;
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnExport.Click += BtnExport_Click;
            toolTip.SetToolTip(btnExport, "Експортувати таблицю у CSV файл");
            panelImportExport.Controls.Add(btnExport);

            // Налаштування кнопки для імпорту з CSV
            btnImport.Text = "Імпорт із CSV";
            btnImport.Location = new Point(220, 12);
            btnImport.Width = 200;
            btnImport.Height = 30;
            btnImport.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnImport.BackColor = Color.FromArgb(30, 144, 255);
            btnImport.ForeColor = Color.White;
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnImport.Click += BtnImport_Click;
            toolTip.SetToolTip(btnImport, "Імпортувати дані з CSV файлу");
            panelImportExport.Controls.Add(btnImport);

            // Налаштування кнопки для об’єднання таблиць
            btnJoin.Text = "Об'єднати таблиці";
            btnJoin.Location = new Point(430, 12);
            btnJoin.Width = 250;
            btnJoin.Height = 30;
            btnJoin.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnJoin.BackColor = Color.FromArgb(30, 144, 255);
            btnJoin.ForeColor = Color.White;
            btnJoin.FlatStyle = FlatStyle.Flat;
            btnJoin.FlatAppearance.BorderSize = 0;
            btnJoin.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnJoin.Click += BtnLoadJoined_Click;
            toolTip.SetToolTip(btnJoin, "Об'єднати дві вибрані таблиці");
            panelImportExport.Controls.Add(btnJoin);

            // Панель для навігації та аналізу
            var panelAnalysis = new Panel
            {
                Location = new Point(10, 190),
                Size = new Size(1260, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelAnalysis);

            // Налаштування кнопки для переходу до попереднього запису
            btnPrev.Text = "Попередній";
            btnPrev.Location = new Point(10, 12);
            btnPrev.Width = 150;
            btnPrev.Height = 30;
            btnPrev.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPrev.BackColor = Color.FromArgb(30, 144, 255);
            btnPrev.ForeColor = Color.White;
            btnPrev.FlatStyle = FlatStyle.Flat;
            btnPrev.FlatAppearance.BorderSize = 0;
            btnPrev.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnPrev.Click += BtnPrev_Click;
            toolTip.SetToolTip(btnPrev, "Перейти до попереднього запису");
            panelAnalysis.Controls.Add(btnPrev);

            // Налаштування кнопки для переходу до наступного запису
            btnNext.Text = "Наступний";
            btnNext.Location = new Point(170, 12);
            btnNext.Width = 150;
            btnNext.Height = 30;
            btnNext.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnNext.BackColor = Color.FromArgb(30, 144, 255);
            btnNext.ForeColor = Color.White;
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnNext.Click += BtnNext_Click;
            toolTip.SetToolTip(btnNext, "Перейти до наступного запису");
            panelAnalysis.Controls.Add(btnNext);

            // Налаштування кнопки для обчислення продажів
            btnCalculateSales.Text = "Сума продажів";
            btnCalculateSales.Location = new Point(330, 12);
            btnCalculateSales.Width = 200;
            btnCalculateSales.Height = 30;
            btnCalculateSales.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCalculateSales.BackColor = Color.FromArgb(30, 144, 255);
            btnCalculateSales.ForeColor = Color.White;
            btnCalculateSales.FlatStyle = FlatStyle.Flat;
            btnCalculateSales.FlatAppearance.BorderSize = 0;
            btnCalculateSales.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnCalculateSales.Click += BtnCalculateSales_Click;
            toolTip.SetToolTip(btnCalculateSales, "Обчислити загальну суму продажів за період");
            panelAnalysis.Controls.Add(btnCalculateSales);

            // Налаштування кнопки для обчислення середньої ціни
            btnCalculateAvgPrice.Text = "Середня ціна за категоріями";
            btnCalculateAvgPrice.Location = new Point(540, 12);
            btnCalculateAvgPrice.Width = 300;
            btnCalculateAvgPrice.Height = 30;
            btnCalculateAvgPrice.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCalculateAvgPrice.BackColor = Color.FromArgb(30, 144, 255);
            btnCalculateAvgPrice.ForeColor = Color.White;
            btnCalculateAvgPrice.FlatStyle = FlatStyle.Flat;
            btnCalculateAvgPrice.FlatAppearance.BorderSize = 0;
            btnCalculateAvgPrice.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnCalculateAvgPrice.Click += BtnCalculateAvgPrice_Click;
            toolTip.SetToolTip(btnCalculateAvgPrice, "Обчислити середню ціну продуктів за категоріями");
            panelAnalysis.Controls.Add(btnCalculateAvgPrice);

            // Налаштування кнопки для сортування за назвою
            btnSortByName.Text = "Сортувати за назвою";
            btnSortByName.Location = new Point(850, 12);
            btnSortByName.Width = 200;
            btnSortByName.Height = 30;
            btnSortByName.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSortByName.BackColor = Color.FromArgb(30, 144, 255);
            btnSortByName.ForeColor = Color.White;
            btnSortByName.FlatStyle = FlatStyle.Flat;
            btnSortByName.FlatAppearance.BorderSize = 0;
            btnSortByName.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnSortByName.Click += BtnSortByName_Click;
            toolTip.SetToolTip(btnSortByName, "Сортувати таблицю за полем 'name'");
            panelAnalysis.Controls.Add(btnSortByName);

            // Налаштування кнопки для виконання прикладу запиту
            btnQueryExample.Text = "Топ-5 продажів";
            btnQueryExample.Location = new Point(1060, 12);
            btnQueryExample.Width = 180;
            btnQueryExample.Height = 30;
            btnQueryExample.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnQueryExample.BackColor = Color.FromArgb(30, 144, 255);
            btnQueryExample.ForeColor = Color.White;
            btnQueryExample.FlatStyle = FlatStyle.Flat;
            btnQueryExample.FlatAppearance.BorderSize = 0;
            btnQueryExample.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnQueryExample.Click += BtnQueryExample_Click;
            toolTip.SetToolTip(btnQueryExample, "Показати топ-5 продажів за сумою");
            panelAnalysis.Controls.Add(btnQueryExample);

            // Панель для вибору дат
            var panelDate = new Panel
            {
                Location = new Point(10, 250),
                Size = new Size(1260, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelDate);

            // Налаштування вибору дат
            dtpStartDate.Location = new Point(10, 12);
            dtpStartDate.Width = 250;
            dtpStartDate.Font = new Font("Segoe UI", 10);
            dtpStartDate.Format = DateTimePickerFormat.Short;
            toolTip.SetToolTip(dtpStartDate, "Виберіть початкову дату для фільтрації продажів");
            panelDate.Controls.Add(dtpStartDate);

            dtpEndDate.Location = new Point(270, 12);
            dtpEndDate.Width = 250;
            dtpEndDate.Font = new Font("Segoe UI", 10);
            dtpEndDate.Format = DateTimePickerFormat.Short;
            toolTip.SetToolTip(dtpEndDate, "Виберіть кінцеву дату для фільтрації продажів");
            panelDate.Controls.Add(dtpEndDate);

            // Налаштування DataGridView
            dgv.Location = new Point(10, 310);
            dgv.Size = new Size(1260, 400);
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 144, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            dgv.RowsDefaultCellStyle.BackColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            toolTip.SetToolTip(dgv, "Відображення даних вибраної таблиці");
            Controls.Add(dgv);

            // Панель для SQL-запиту
            var panelSql = new Panel
            {
                Location = new Point(10, 720),
                Size = new Size(1260, 100),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panelSql);

            // Налаштування текстового поля для SQL-запиту
            txtSql.Location = new Point(10, 10);
            txtSql.Width = 1050;
            txtSql.Height = 80;
            txtSql.Multiline = true;
            txtSql.Font = new Font("Segoe UI", 10);
            txtSql.BorderStyle = BorderStyle.FixedSingle;
            txtSql.BackColor = Color.White;
            toolTip.SetToolTip(txtSql, "Введіть SQL-запит для виконання");
            panelSql.Controls.Add(txtSql);

            // Налаштування кнопки для виконання SQL-запиту
            btnExecuteSql.Text = "Виконати SQL";
            btnExecuteSql.Location = new Point(1070, 40);
            btnExecuteSql.Width = 180;
            btnExecuteSql.Height = 30;
            btnExecuteSql.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnExecuteSql.BackColor = Color.FromArgb(30, 144, 255);
            btnExecuteSql.ForeColor = Color.White;
            btnExecuteSql.FlatStyle = FlatStyle.Flat;
            btnExecuteSql.FlatAppearance.BorderSize = 0;
            btnExecuteSql.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 105, 225);
            btnExecuteSql.Click += BtnExecuteSql_Click;
            toolTip.SetToolTip(btnExecuteSql, "Виконати введений SQL-запит");
            panelSql.Controls.Add(btnExecuteSql);

            Load += FormMain_Load;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            var dt = DatabaseManager.ExecuteQuery("SHOW TABLES");
            comboTables.Items.Clear();
            comboTables2.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                comboTables.Items.Add(row[0].ToString());
                comboTables2.Items.Add(row[0].ToString());
            }

            joinConditions = GetForeignKeyConditions();

            string connStr = ConfigurationManager.ConnectionStrings["AutoShopDB"].ConnectionString;
            using (var conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("Успішне підключення!", "Підключення", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка підключення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            DatabaseManager.SaveDataSetToFile("cached_data.xml");
        }

        private Dictionary<(string, string), string> GetForeignKeyConditions()
        {
            var conditions = new Dictionary<(string, string), string>();
            try
            {
                string query = @"
                    SELECT 
                        TABLE_NAME, COLUMN_NAME, 
                        REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND REFERENCED_TABLE_NAME IS NOT NULL";
                var dt = DatabaseManager.ExecuteQuery(query);
                foreach (DataRow row in dt.Rows)
                {
                    string table1 = row["TABLE_NAME"].ToString();
                    string table2 = row["REFERENCED_TABLE_NAME"].ToString();
                    string condition = $"{table1}.{row["COLUMN_NAME"]} = {table2}.{row["REFERENCED_COLUMN_NAME"]}";
                    conditions[(table1, table2)] = condition;
                    conditions[(table2, table1)] = condition;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при отриманні зв’язків: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return conditions;
        }

        private void ComboTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedTable();
        }

        private void ComboTables2_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        internal void LoadSelectedTable()
        {
            string table = comboTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(table))
            {
                currentTable = DatabaseManager.GetTable(table);
                dgv.DataSource = currentTable;
            }
        }

        internal void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string table = comboTables.SelectedItem?.ToString();
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(keyword) || keyword == "Пошук...")
            {
                LoadSelectedTable();
                return;
            }
            DataView view = currentTable.DefaultView;
            string filter = string.Join(" OR ", currentTable.Columns.Cast<DataColumn>().Select(c => $"Convert([{c.ColumnName}], 'System.String') LIKE '%{keyword}%'"));
            view.RowFilter = filter;
            dgv.DataSource = view;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string table = comboTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(table))
            {
                new TableEditorForm(table).ShowDialog();
                DatabaseManager.UpdateTable(table);
                LoadSelectedTable();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            string table = comboTables.SelectedItem?.ToString();
            if (dgv.SelectedRows.Count > 0)
            {
                DataRowView row = (DataRowView)dgv.SelectedRows[0].DataBoundItem;
                new TableEditorForm(table, row.Row).ShowDialog();
                DatabaseManager.UpdateTable(table);
                LoadSelectedTable();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("Ви впевнені?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int index = dgv.SelectedRows[0].Index;
                    var table = DatabaseManager.GetTable(comboTables.SelectedItem.ToString());
                    table.Rows[index].Delete();
                    DatabaseManager.UpdateTable(comboTables.SelectedItem.ToString());
                    dgv.Refresh();
                    MessageBox.Show("Запис видалено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            string table = comboTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(table))
            {
                currentTable.Clear();
                DatabaseManager.GetTable(table);
                LoadSelectedTable();
                MessageBox.Show("Дані оновлено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExecuteSql_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = DatabaseManager.ExecuteQuery(txtSql.Text);
                dgv.DataSource = dt;
                MessageBox.Show("Запит виконано.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgv.DataSource == null)
            {
                MessageBox.Show("Спочатку виберіть таблицю.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV файл|*.csv",
                FileName = comboTables.SelectedItem.ToString() + ".csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var writer = new System.IO.StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                    {
                        var dt = (dgv.DataSource as DataTable) ?? ((dgv.DataSource as DataView)?.Table);
                        var cols = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                        writer.WriteLine(string.Join(";", cols));

                        foreach (DataRow row in dt.Rows)
                        {
                            var vals = row.ItemArray.Select(f => $"\"{f?.ToString().Replace("\"", "\"\"").Replace("\r\n", " ")}\"");
                            writer.WriteLine(string.Join(";", vals));
                        }
                    }
                    MessageBox.Show("Файл збережено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка експорту: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnImport_Click(object sender, EventArgs e)
        {
            if (comboTables.SelectedItem == null)
            {
                MessageBox.Show("Виберіть таблицю!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "CSV файл|*.csv"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var table = DatabaseManager.GetTable(comboTables.SelectedItem.ToString());
                    using (var reader = new System.IO.StreamReader(ofd.FileName))
                    {
                        string[] headers = reader.ReadLine().Split(';');
                        while (!reader.EndOfStream)
                        {
                            string[] values = reader.ReadLine().Split(';');
                            DataRow newRow = table.NewRow();
                            for (int i = 0; i < headers.Length && i < table.Columns.Count; i++)
                            {
                                newRow[headers[i]] = values[i].Trim('"');
                            }
                            table.Rows.Add(newRow);
                        }
                    }
                    DatabaseManager.UpdateTable(comboTables.SelectedItem.ToString());
                    LoadSelectedTable();
                    MessageBox.Show("Дані імпортовано.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка імпорту: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLoadJoined_Click(object sender, EventArgs e)
        {
            if (comboTables.SelectedItem == null || comboTables2.SelectedItem == null)
            {
                MessageBox.Show("Виберіть обидві таблиці!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string table1 = comboTables.SelectedItem.ToString();
            string table2 = comboTables2.SelectedItem.ToString();
            string joinCondition = GetJoinCondition(table1, table2);
            if (string.IsNullOrEmpty(joinCondition))
            {
                MessageBox.Show($"Таблиці {table1} і {table2} не мають визначеного зв’язку. Об’єднання неможливе.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var joinedTable = DatabaseManager.GetJoinedTable(table1, table2, joinCondition);
            if (joinedTable != null)
            {
                dgv.DataSource = joinedTable;
                MessageBox.Show("Таблиці об'єднано.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetJoinCondition(string table1, string table2)
        {
            return joinConditions.TryGetValue((table1, table2), out var condition) ? condition : string.Empty;
        }

        private void BtnPrev_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow != null && dgv.CurrentRow.Index > 0)
                dgv.CurrentCell = dgv.Rows[dgv.CurrentRow.Index - 1].Cells[0];
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow != null && dgv.CurrentRow.Index < dgv.Rows.Count - 1)
                dgv.CurrentCell = dgv.Rows[dgv.CurrentRow.Index + 1].Cells[0];
        }

        internal void BtnCalculateSales_Click(object sender, EventArgs e)
        {
            if (dtpStartDate.Value > dtpEndDate.Value)
            {
                MessageBox.Show("Початкова дата не може бути пізніше кінцевої дати.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string startDate = dtpStartDate.Value.ToString("yyyy-MM-dd");
                string endDate = dtpEndDate.Value.ToString("yyyy-MM-dd");

                DataTable salesTable = DatabaseManager.GetTable("Sale");
                DataView view = salesTable.DefaultView;
                view.RowFilter = $"sale_date >= '{startDate}' AND sale_date <= '{endDate}'";

                decimal totalSales = 0;
                foreach (DataRowView row in view)
                {
                    totalSales += Convert.ToDecimal(row["total_amount"]);
                }

                if (totalSales > 0)
                {
                    MessageBox.Show($"Загальна сума продажів за період з {startDate} по {endDate}: {totalSales:F2} грн.", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Продажів за період з {startDate} по {endDate} не знайдено.", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка обчислення: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCalculateAvgPrice_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable products = DatabaseManager.GetTable("Product");
                DataTable categories = DatabaseManager.GetTable("Product_Category");

                var grouped = from p in products.AsEnumerable()
                              join c in categories.AsEnumerable()
                              on p.Field<int>("category_id") equals c.Field<int>("category_id")
                              group p by new { category_id = p.Field<int>("category_id"), categoryName = c.Field<string>("name") } into g
                              select new
                              {
                                  CategoryName = g.Key.categoryName,
                                  AvgPrice = g.Average(row => row.Field<decimal>("price"))
                              };

                string result = "Середня ціна за категоріями:\n";
                foreach (var group in grouped)
                {
                    result += $"{group.CategoryName}: {group.AvgPrice:F2} грн.\n";
                }
                MessageBox.Show(result, "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка обчислення: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSortByName_Click(object sender, EventArgs e)
        {
            if (currentTable != null)
            {
                DataView view = currentTable.DefaultView;
                view.Sort = "name ASC";
                dgv.DataSource = view;
                MessageBox.Show("Таблицю відсортовано за назвою.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnQueryExample_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM Sale ORDER BY total_amount DESC LIMIT 5";
            try
            {
                var dt = DatabaseManager.ExecuteQuery(query);
                dgv.DataSource = dt;
                MessageBox.Show("Топ-5 продажів відображено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCountByCategory_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable products = DatabaseManager.GetTable("Product");
                DataTable categories = DatabaseManager.GetTable("Product_Category");

                var grouped = from p in products.AsEnumerable()
                              join c in categories.AsEnumerable()
                              on p.Field<int>("category_id") equals c.Field<int>("category_id")
                              group p by new { category_id = p.Field<int>("category_id"), categoryName = c.Field<string>("name") } into g
                              select new
                              {
                                  CategoryName = g.Key.categoryName,
                                  ProductCount = g.Count()
                              };

                string result = "Кількість продуктів за категоріями:\n";
                foreach (var group in grouped)
                {
                    result += $"{group.CategoryName}: {group.ProductCount} шт.\n";
                }
                MessageBox.Show(result, "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка обчислення: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MaximizeBox = false,
                BackColor = Color.FromArgb(240, 248, 255)
            };
            Label lbl = new Label() { Left = 20, Top = 20, Text = text, Width = 200, Font = new Font("Segoe UI", 10), ForeColor = Color.FromArgb(25, 25, 112) };
            TextBox txt = new TextBox() { Left = 20, Top = 50, Width = 200, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            Button ok = new Button() { Text = "OK", Left = 50, Width = 80, Top = 80, Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(30, 144, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(65, 105, 225) } };
            Button cancel = new Button() { Text = "Скасувати", Left = 140, Width = 80, Top = 80, Font = new Font("Segoe UI", 10, FontStyle.Bold), BackColor = Color.FromArgb(220, 220, 220), ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(200, 200, 200) } };
            ok.Click += (s, e) => { prompt.DialogResult = DialogResult.OK; prompt.Close(); };
            cancel.Click += (s, e) => { prompt.Close(); };
            prompt.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
            return prompt.ShowDialog() == DialogResult.OK ? txt.Text : "";
        }
    }


    public interface IMessageBoxService
    {
        DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);
    }

    public class DefaultMessageBoxService : IMessageBoxService
    {
        public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }
    }

}


