using System.Drawing;
using System.Windows.Forms;
using StilsoftIRS.Infrastructure;

namespace StilsoftIRS.Utilities
{
    internal static class GridHelper
    {
        private static readonly Color LightOrange = Color.FromArgb(255, 229, 204);

        public static void ConfigureReadOnlyGrid(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.BackgroundColor = DesktopTheme.CardBackground;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = DesktopTheme.BorderColor;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = DesktopTheme.AccentDarkColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = DesktopTheme.AccentDarkColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersHeight = 40;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = DesktopTheme.TextColor;
            grid.DefaultCellStyle.SelectionForeColor = DesktopTheme.TextColor;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 232, 240);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 251);
        }

        public static void ApplyPriorityRowColor(DataGridViewRow row, string priority)
        {
            if (row == null)
            {
                return;
            }

            var color = Color.White;
            switch (priority)
            {
                case SystemConstants.CriticalPriority:
                    color = Color.LightCoral;
                    break;
                case SystemConstants.HighPriority:
                    color = LightOrange;
                    break;
                case SystemConstants.MediumPriority:
                    color = Color.LightYellow;
                    break;
                case SystemConstants.LowPriority:
                    color = Color.White;
                    break;
            }

            row.DefaultCellStyle.BackColor = color;
            row.DefaultCellStyle.SelectionBackColor = ControlPaint.Dark(color);
            row.DefaultCellStyle.SelectionForeColor = Color.Black;
        }
    }
}
