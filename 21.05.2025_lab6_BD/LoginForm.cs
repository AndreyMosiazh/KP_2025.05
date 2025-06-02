using System;
using System.Drawing;
using System.Windows.Forms;

namespace DBManager
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername; // Поле для введення імені користувача
        private TextBox txtPassword; // Поле для введення пароля
        private Button btnLogin; // Кнопка для входу
        private ToolTip toolTip; // Підказки для користувача

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Налаштування форми
            this.Text = "Авторизація";
            this.Width = 350;
            this.Height = 250;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255); // Світло-блакитний фон
            this.Font = new Font("Segoe UI", 10);

            // Ініціалізація ToolTip
            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true
            };

            // Налаштування текстового поля для імені користувача
            txtUsername = new TextBox
            {
                Location = new Point(75, 30),
                Width = 200,
                Text = "admin",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            toolTip.SetToolTip(txtUsername, "Введіть ім'я користувача (наприклад, admin)");
            Controls.Add(txtUsername);

            // Налаштування текстового поля для пароля
            txtPassword = new TextBox
            {
                Location = new Point(75, 80),
                Width = 200,
                UseSystemPasswordChar = true,
                Text = "1234",
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            toolTip.SetToolTip(txtPassword, "Введіть пароль (наприклад, 1234)");
            Controls.Add(txtPassword);

            // Налаштування кнопки для входу
            btnLogin = new Button
            {
                Text = "Увійти",
                Location = new Point(125, 130),
                Width = 100,
                Height = 35,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 144, 255), // Синій фон
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(65, 105, 225) }
            };
            btnLogin.Click += BtnLogin_Click;
            toolTip.SetToolTip(btnLogin, "Натисніть для входу в систему");
            Controls.Add(btnLogin);

            // Додавання назви форми
            Label lblTitle = new Label
            {
                Text = "Вхід до системи",
                Location = new Point(75, 10),
                Width = 200,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(25, 25, 112)
            };
            Controls.Add(lblTitle);
        }

        // Перевірка логіну та пароля
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "admin" && txtPassword.Text == "1234")
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUsername.BackColor = Color.FromArgb(255, 245, 238); // Легкий червоний фон при помилці
                txtPassword.BackColor = Color.FromArgb(255, 245, 238);
            }
        }
    }
}