using System;
using System.Windows.Forms;

namespace DBManager
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Відкриття форми авторизації
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // Якщо авторизація успішна, відкриваємо головну форму
                    Application.Run(new FormMain());
                }
            }
        }
    }
}