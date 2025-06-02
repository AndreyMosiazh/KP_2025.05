using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace DBManager
{
    public partial class TableEditorForm : Form
    {
        private string tableName;
        private DataRow existingRow;
        internal Dictionary<string, Control> inputControls = new Dictionary<string, Control>();
        private ToolTip toolTip;

        public TableEditorForm(string tableName, DataRow rowToEdit = null)
        {
            this.tableName = tableName;
            this.existingRow = rowToEdit;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = existingRow == null ? $"Додавання в {tableName}" : $"Редагування в {tableName}";
            this.Width = 600;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterParent;
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

            GenerateForm();
        }

        private void GenerateForm()
        {
            var panel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(560, 350),
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(panel);

            var tableData = DatabaseManager.GetTable(tableName);
            int top = 20;

            Label lblTitle = new Label
            {
                Text = existingRow == null ? $"Додавання нового запису в {tableName}" : $"Редагування запису в {tableName}",
                Location = new Point(10, 10),
                Width = 540,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(25, 25, 112)
            };
            panel.Controls.Add(lblTitle);
            top += 30;

            foreach (DataColumn col in tableData.Columns)
            {
                if (!col.AllowDBNull && col.AutoIncrement && existingRow == null)
                    continue;

                Label label = new Label
                {
                    Text = col.ColumnName,
                    Left = 10,
                    Top = top,
                    Width = 150,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(25, 25, 112)
                };
                panel.Controls.Add(label);

                if (col.ColumnName.ToLower() == "category_id" || col.ColumnName.ToLower() == "warranty_months")
                {
                    NumericUpDown nud = new NumericUpDown
                    {
                        Left = 170,
                        Top = top,
                        Width = 350,
                        Minimum = 0,
                        Maximum = 1000,
                        Font = new Font("Segoe UI", 10),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    if (existingRow != null)
                        nud.Value = Convert.ToDecimal(existingRow[col.ColumnName] ?? 0);
                    panel.Controls.Add(nud);
                    inputControls[col.ColumnName] = nud;
                    toolTip.SetToolTip(nud, $"Введіть числове значення для {col.ColumnName} (0-1000)");
                }
                else
                {
                    TextBox textbox = new TextBox
                    {
                        Left = 170,
                        Top = top,
                        Width = 350,
                        Font = new Font("Segoe UI", 10),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White
                    };
                    if (existingRow != null)
                        textbox.Text = existingRow[col.ColumnName]?.ToString();
                    panel.Controls.Add(textbox);
                    inputControls[col.ColumnName] = textbox;
                    toolTip.SetToolTip(textbox, $"Введіть значення для {col.ColumnName}");
                }

                top += 40;
            }

            Button btnSave = new Button
            {
                Text = "Зберегти",
                Top = top + 10,
                Left = 170,
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 144, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(65, 105, 225) }
            };
            btnSave.Click += BtnSave_Click;
            toolTip.SetToolTip(btnSave, "Зберегти зміни та закрити форму");
            panel.Controls.Add(btnSave);

            Button btnCancel = new Button
            {
                Text = "Скасувати",
                Top = top + 10,
                Left = 280,
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 220, 220),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(200, 200, 200) }
            };
            btnCancel.Click += (s, e) => this.Close();
            toolTip.SetToolTip(btnCancel, "Закрити форму без збереження");
            panel.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var table = DatabaseManager.GetTable(tableName);

            try
            {
                if (existingRow == null)
                {
                    DataRow newRow = table.NewRow();
                    foreach (var col in inputControls.Keys)
                    {
                        string textValue = inputControls[col] is NumericUpDown nud ? nud.Value.ToString() : inputControls[col].Text.Trim();
                        if (col.ToLower() == "price" || col.ToLower() == "weight_kg")
                        {
                            textValue = textValue.Replace(",", ".");
                            if (!decimal.TryParse(textValue, out _))
                            {
                                throw new Exception($"Невірний формат для {col}. Введіть числове значення.");
                            }
                        }
                        if (col.ToLower() == "category_id" || col.ToLower() == "warranty_months")
                        {
                            if (!int.TryParse(textValue, out int value) || value < 0)
                            {
                                throw new Exception($"Невірне значення для {col}. Введіть додатнє ціле число.");
                            }
                        }
                        if (col.ToLower() == "is_active")
                        {
                            textValue = (textValue.ToLower() == "true") ? "1" : "0";
                        }
                        if (col.ToLower() == "sale_date")
                        {
                            if (!DateTime.TryParse(textValue, out _))
                            {
                                throw new Exception($"Невірний формат для {col}. Введіть дату у форматі РРРР-ММ-ДД.");
                            }
                        }
                        newRow[col] = textValue;
                    }
                    table.Rows.Add(newRow);
                    MessageBox.Show("Запис додано.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    foreach (var col in inputControls.Keys)
                    {
                        string textValue = inputControls[col] is NumericUpDown nud ? nud.Value.ToString() : inputControls[col].Text.Trim();
                        if (col.ToLower() == "price" || col.ToLower() == "weight_kg")
                        {
                            textValue = textValue.Replace(",", ".");
                            if (!decimal.TryParse(textValue, out _))
                            {
                                throw new Exception($"Невірний формат для {col}. Введіть числове значення.");
                            }
                        }
                        if (col.ToLower() == "category_id" || col.ToLower() == "warranty_months")
                        {
                            if (!int.TryParse(textValue, out int value) || value < 0)
                            {
                                throw new Exception($"Невірне значення для {col}. Введіть додатнє ціле число.");
                            }
                        }
                        if (col.ToLower() == "is_active")
                        {
                            textValue = (textValue.ToLower() == "true") ? "1" : "0";
                        }
                        if (col.ToLower() == "sale_date")
                        {
                            if (!DateTime.TryParse(textValue, out _))
                            {
                                throw new Exception($"Невірний формат для {col}. Введіть дату у форматі РРРР-ММ-ДД.");
                            }
                        }
                        existingRow[col] = textValue;
                    }
                    MessageBox.Show("Запис оновлено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}