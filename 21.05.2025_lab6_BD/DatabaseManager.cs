using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DBManager 
{
    public static class DatabaseManager
    {
        // Рядок підключення до бази даних
        private static string _connStr;
        private static string ConnStr
        {
            get
            {
                if (_connStr == null)
                {
                    _connStr = ConfigurationManager.ConnectionStrings["AutoShopDB"]?.ConnectionString;
                    if (string.IsNullOrEmpty(_connStr))
                        throw new InvalidOperationException("Строка подключения AutoShopDB не настроена.");
                }
                return _connStr;
            }
        }
        // Локальний кеш для всіх таблиць
        private static DataSet dataSet = new DataSet();

        // Ініціалізація DataSet із усіма таблицями при запуску програми
        public static void InitializeDataSet()
        {
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                // Отримуємо лише таблиці, виключаючи представлення
                var tablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE'";
                var tablesAdapter = new MySqlDataAdapter(tablesQuery, conn);
                var tables = new DataTable();
                tablesAdapter.Fill(tables);

                foreach (DataRow tableRow in tables.Rows)
                {
                    string tableName = tableRow["TABLE_NAME"].ToString();
                    try
                    {
                        var adapter = new MySqlDataAdapter($"SELECT * FROM `{tableName}`", conn);
                        var commandBuilder = new MySqlCommandBuilder(adapter);
                        adapter.InsertCommand = commandBuilder.GetInsertCommand();
                        adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                        adapter.DeleteCommand = commandBuilder.GetDeleteCommand();
                        var table = new DataTable(tableName);
                        adapter.Fill(table);
                        dataSet.Tables.Add(table);
                        Console.WriteLine($"Таблиця {tableName} успішно завантажена.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при завантаженні таблиці {tableName}: {ex.Message}");
                        try
                        {
                            System.IO.File.AppendAllText(@"C:\Users\YourUsername\Desktop\error_log.txt", $"[{DateTime.Now}] Помилка для {tableName}: {ex.Message}\n");
                        }
                        catch (Exception fileEx)
                        {
                            MessageBox.Show($"Помилка запису в лог-файл: {fileEx.Message}");
                        }
                    }
                }
            }
        }

        // Отримання локальної копії таблиці
        public static DataTable GetTable(string tableName)
        {
            if (!dataSet.Tables.Contains(tableName))
            {
                using (var conn = new MySqlConnection(ConnStr))
                {
                    conn.Open();
                    try
                    {
                        var adapter = new MySqlDataAdapter($"SELECT * FROM `{tableName}`", conn);
                        var commandBuilder = new MySqlCommandBuilder(adapter);
                        adapter.InsertCommand = commandBuilder.GetInsertCommand();
                        adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                        adapter.DeleteCommand = commandBuilder.GetDeleteCommand();
                        var table = new DataTable(tableName);
                        adapter.Fill(table);
                        dataSet.Tables.Add(table);
                        Console.WriteLine($"Таблиця {tableName} успішно завантажена.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при завантаженні таблиці {tableName}: {ex.Message}");
                    }
                }
            }
            return dataSet.Tables[tableName];
        }

        // Синхронізація змін із базою даних
        public static void UpdateTable(string tableName)
        {
            try
            {
                if (dataSet.Tables.Contains(tableName))
                {
                    using (var conn = new MySqlConnection(ConnStr))
                    {
                        conn.Open();
                        var adapter = new MySqlDataAdapter($"SELECT * FROM `{tableName}`", conn);
                        var commandBuilder = new MySqlCommandBuilder(adapter);
                        adapter.InsertCommand = commandBuilder.GetInsertCommand();
                        adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                        adapter.DeleteCommand = commandBuilder.GetDeleteCommand();
                        adapter.Update(dataSet.Tables[tableName]);
                        Console.WriteLine($"Таблиця {tableName} успішно оновлена.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Помилка синхронізації таблиці {tableName}: {ex.Message}");
            }
        }

        // Виконання SQL-запиту для ручного введення
        public static DataTable ExecuteQuery(string query)
        {
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                var adapter = new MySqlDataAdapter(query, conn);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        // Збереження DataSet у файл для кешування між сеансами
        public static void SaveDataSetToFile(string filePath)
        {
            dataSet.WriteXml(filePath, XmlWriteMode.WriteSchema);
        }

        // Завантаження DataSet із файлу
        public static void LoadDataSetFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                dataSet.Clear();
                dataSet.ReadXml(filePath, XmlReadMode.ReadSchema);
            }
        }

        // Отримання об’єднаної таблиці з двох таблиць
        public static DataTable GetJoinedTable(string table1, string table2, string joinCondition)
        {
            string query = $"SELECT * FROM `{table1}` INNER JOIN `{table2}` ON {joinCondition}";
            using (var conn = new MySqlConnection(ConnStr))
            {
                conn.Open();
                var adapter = new MySqlDataAdapter(query, conn);
                var table = new DataTable($"{table1}_{table2}");
                adapter.Fill(table);
                dataSet.Tables.Add(table);
                return table;
            }
        }
    }
}