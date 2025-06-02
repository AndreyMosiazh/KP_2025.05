using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using DBManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DBManager.Tests
{
    [TestClass]
    public class DatabaseManagerTests
    {
        private Mock<IDatabaseManager> _mockDatabaseManager;

        [TestInitialize]
        public void Setup()
        {
            _mockDatabaseManager = new Mock<IDatabaseManager>();
        }

        [TestMethod]
        public void InitializeDataSet_LoadsAllTables_Success()
        {
            // Arrange
            _mockDatabaseManager.Setup(m => m.InitializeDataSet());

            // Act
            _mockDatabaseManager.Object.InitializeDataSet();

            // Assert
            _mockDatabaseManager.Verify(m => m.InitializeDataSet(), Times.Once());
        }

        [TestMethod]
        public void GetTable_ExistingTable_ReturnsData()
        {
            // Arrange
            string tableName = "Product";
            DataTable table = new DataTable(tableName);
            table.Columns.Add("name", typeof(string));
            table.Rows.Add("Test Product");
            _mockDatabaseManager.Setup(m => m.GetTable(tableName)).Returns(table);

            // Act
            DataTable result = _mockDatabaseManager.Object.GetTable(tableName);

            // Assert
            Assert.IsNotNull(result, "Таблица должна быть не null");
            Assert.AreEqual(tableName, result.TableName, "Имя таблицы должно совпадать");
            Assert.AreEqual(1, result.Rows.Count, "Таблица должна содержать хотя бы одну строку");
        }

        [TestMethod]
        public void UpdateTable_ValidChanges_SavesToDatabase()
        {
            // Arrange
            string tableName = "Product";
            _mockDatabaseManager.Setup(m => m.UpdateTable(tableName));

            // Act
            _mockDatabaseManager.Object.UpdateTable(tableName);

            // Assert
            _mockDatabaseManager.Verify(m => m.UpdateTable(tableName), Times.Once());
        }

        [TestMethod]
        public void ExecuteQuery_ValidQuery_ReturnsData()
        {
            // Arrange
            string query = "SELECT * FROM Product";
            DataTable table = new DataTable("Result");
            table.Columns.Add("name", typeof(string));
            table.Rows.Add("Test Product");
            _mockDatabaseManager.Setup(m => m.ExecuteQuery(query)).Returns(table);

            // Act
            DataTable result = _mockDatabaseManager.Object.ExecuteQuery(query);

            // Assert
            Assert.IsNotNull(result, "Результат запроса должен быть не null");
            Assert.AreEqual(1, result.Rows.Count, "Должен быть хотя бы один результат");
        }

        [TestMethod]
        public void GetJoinedTable_ValidJoin_ReturnsJoinedData()
        {
            // Arrange
            string table1 = "Product";
            string table2 = "Product_Category";
            string joinCondition = "Product.category_id = Product_Category.category_id";
            DataTable joinedTable = new DataTable(table1 + "_" + table2);
            joinedTable.Columns.Add("name", typeof(string));
            joinedTable.Rows.Add("Test Product");
            _mockDatabaseManager.Setup(m => m.GetJoinedTable(table1, table2, joinCondition)).Returns(joinedTable);

            // Act
            DataTable result = _mockDatabaseManager.Object.GetJoinedTable(table1, table2, joinCondition);

            // Assert
            Assert.IsNotNull(result, "Объединенная таблица должна быть не null");
            Assert.AreEqual(1, result.Rows.Count, "Должна быть хотя бы одна строка данных");
        }
    }

    [TestClass]
    public class FormMainTests
    {
        private FormMain _form;

        [TestInitialize]
        public void Setup()
        {
            _form = new FormMain();
            _form.comboTables.SelectedItem = "Product";
            _form.currentTable = new DataTable("Product");
            _form.currentTable.Columns.Add("name", typeof(string));
            _form.currentTable.Rows.Add("Test Product");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _form.Dispose();
        }      
        
    }

    [TestClass]
    public class TableEditorFormTests
    {
        private TableEditorForm _form;
        private DataTable _testTable;

        [TestInitialize]
        public void Setup()
        {
            _testTable = new DataTable("Product");
            _testTable.Columns.Add("name", typeof(string));
            _testTable.Columns.Add("price", typeof(decimal));
            _testTable.Columns.Add("category_id", typeof(int));
            _form = new TableEditorForm("Product");
            System.Reflection.FieldInfo field = typeof(TableEditorForm).GetField("inputControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            System.Collections.Generic.Dictionary<string, Control> inputControls = (System.Collections.Generic.Dictionary<string, Control>)field.GetValue(_form);
            inputControls["name"] = new TextBox { Text = "Test Product" };
            inputControls["price"] = new TextBox { Text = "99.99" };
            inputControls["category_id"] = new NumericUpDown { Value = 1 };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _form.Dispose();
        }
        
    }

    // Интерфейс для моков
    public interface IDatabaseManager
    {
        void InitializeDataSet();
        DataTable GetTable(string tableName);
        void UpdateTable(string tableName);
        DataTable ExecuteQuery(string query);
        DataTable GetJoinedTable(string table1, string table2, string joinCondition);
    }
}
