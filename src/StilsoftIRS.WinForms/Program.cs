using System;
using System.Data.Common;
using System.Linq;
using System.Windows.Forms;
using StilsoftIRS.Forms;
using StilsoftIRS.Infrastructure;

namespace StilsoftIRS
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var initOnly = args != null && args.Any(arg => string.Equals(arg, "--init-db", StringComparison.OrdinalIgnoreCase));

            try
            {
                DatabaseInitializer.Initialize();

                if (initOnly)
                {
                    return;
                }

                var services = new AppServices();
                Application.Run(new LoginForm(services));
            }
            catch (DbException ex)
            {
                if (initOnly)
                {
                    Environment.ExitCode = 1;
                    return;
                }

                MessageBox.Show(
                    BuildDatabaseErrorMessage(ex),
                    "Ошибка запуска",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                if (initOnly)
                {
                    Environment.ExitCode = 1;
                    return;
                }

                MessageBox.Show(
                    "Не удалось инициализировать приложение или базу данных.\r\n\r\n" + ex.Message,
                    "Ошибка запуска",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string BuildDatabaseErrorMessage(DbException ex)
        {
            return
                "Не удалось подключиться к базе данных SQL Server.\r\n\r\n" +
                "Проверьте строку подключения, доступность SQL Server / LocalDB и права пользователя.\r\n\r\n" +
                "Если это локальный запуск без выделенного SQL Server, используйте Start-StilsoftIRS.cmd: он подбирает MSSQLLocalDB автоматически.\r\n\r\n" +
                "Техническая причина:\r\n" + ex.Message;
        }
    }
}
