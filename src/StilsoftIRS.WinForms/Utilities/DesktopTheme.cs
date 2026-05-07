using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace StilsoftIRS.Utilities
{
    internal static class DesktopTheme
    {
        public const string SkipThemeTag = "desktop-theme-skip";

        public static readonly Color AppBackground = Color.FromArgb(16, 27, 42);
        public static readonly Color WorkspaceBackground = Color.FromArgb(24, 38, 56);
        public static readonly Color SurfaceBackground = Color.FromArgb(236, 242, 247);
        public static readonly Color CardBackground = Color.FromArgb(250, 252, 253);
        public static readonly Color BorderColor = Color.FromArgb(203, 214, 225);
        public static readonly Color AccentColor = Color.FromArgb(0, 168, 150);
        public static readonly Color AccentDarkColor = Color.FromArgb(10, 77, 102);
        public static readonly Color AccentWarmColor = Color.FromArgb(214, 166, 70);
        public static readonly Color TextColor = Color.FromArgb(33, 46, 60);
        public static readonly Color MutedTextColor = Color.FromArgb(108, 120, 133);
        public static readonly Color DangerColor = Color.FromArgb(184, 75, 61);

        public static readonly Font UiFont = new Font("Bahnschrift", 10F, FontStyle.Regular);
        public static readonly Font UiSemiboldFont = new Font("Bahnschrift SemiBold", 10F, FontStyle.Regular);
        public static readonly Font TitleFont = new Font("Bahnschrift SemiBold", 24F, FontStyle.Regular);
        public static readonly Font SubtitleFont = new Font("Bahnschrift", 10.5F, FontStyle.Regular);
        public static readonly Font MetricFont = new Font("Bahnschrift SemiBold", 18F, FontStyle.Regular);

        public static void ApplyFormTheme(Form form)
        {
            if (form == null)
            {
                return;
            }

            form.BackColor = SurfaceBackground;
            form.ForeColor = TextColor;
            form.Font = UiFont;
            ApplyToControls(form.Controls);

            if (form.IsMdiContainer)
            {
                StyleMdiWorkspace(form);
            }
        }

        public static void ApplyDialogTheme(Form form)
        {
            if (form == null)
            {
                return;
            }

            form.BackColor = SurfaceBackground;
            form.ForeColor = TextColor;
            form.Font = UiFont;
            ApplyToControls(form.Controls);
        }

        public static Panel CreateHeroPanel(string title, string subtitle, string badgeText = null)
        {
            var hasSubtitle = !string.IsNullOrWhiteSpace(subtitle);
            var panel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Top,
                Height = hasSubtitle ? 104 : 76,
                Padding = new Padding(24, hasSubtitle ? 18 : 16, 24, 16),
                Margin = Padding.Empty,
                Tag = SkipThemeTag
            };
            panel.Paint += (sender, args) =>
            {
                using (var brush = new LinearGradientBrush(panel.ClientRectangle, AccentDarkColor, AppBackground, LinearGradientMode.Horizontal))
                {
                    args.Graphics.FillRectangle(brush, panel.ClientRectangle);
                }

                using (var stripeBrush = new SolidBrush(Color.FromArgb(28, AccentWarmColor)))
                {
                    args.Graphics.FillRectangle(stripeBrush, new Rectangle(0, 0, panel.Width / 4, panel.Height));
                    args.Graphics.FillRectangle(stripeBrush, new Rectangle(panel.Width - 12, 0, 12, panel.Height));
                }

                using (var pen = new Pen(Color.FromArgb(180, AccentWarmColor), 2F))
                {
                    args.Graphics.DrawLine(pen, 24, panel.Height - 8, panel.Width - 24, panel.Height - 8);
                }
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = hasSubtitle ? 2 : 1,
                Tag = SkipThemeTag
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = TitleFont,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Tag = SkipThemeTag
            };

            layout.Controls.Add(titleLabel, 0, 0);
            if (hasSubtitle)
            {
                var subtitleLabel = new Label
                {
                    Text = subtitle,
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Font = SubtitleFont,
                    ForeColor = Color.FromArgb(220, 233, 240),
                    BackColor = Color.Transparent,
                    Tag = SkipThemeTag
                };

                layout.Controls.Add(subtitleLabel, 0, 1);
            }

            panel.Controls.Add(layout);
            return panel;
        }

        public static Panel CreateCardPanel(Control content, Padding margin)
        {
            var card = CreateCardPanel(margin);
            if (content != null)
            {
                content.Dock = DockStyle.Fill;
                card.Controls.Add(content);
            }

            return card;
        }

        public static Panel CreateCardPanel(Padding margin)
        {
            var panel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBackground,
                Padding = new Padding(16),
                Margin = margin
            };
            panel.Paint += (sender, args) =>
            {
                using (var pen = new Pen(BorderColor))
                {
                    args.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, panel.Width - 1), Math.Max(0, panel.Height - 1));
                }
            };
            return panel;
        }

        public static Button CreateButton(string text, bool primary = false, bool danger = false)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(18, 9, 18, 9),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 10),
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            ApplyButtonStyle(button, primary, danger);
            return button;
        }

        public static void StyleToolStrip(ToolStrip toolStrip)
        {
            if (toolStrip == null)
            {
                return;
            }

            toolStrip.Font = UiFont;
            toolStrip.BackColor = AppBackground;
            toolStrip.ForeColor = Color.White;
            toolStrip.RenderMode = ToolStripRenderMode.Professional;
            toolStrip.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable());
            toolStrip.Padding = new Padding(10, toolStrip is MenuStrip ? 8 : 4, 10, toolStrip is MenuStrip ? 8 : 4);
        }

        public static void ApplyButtonStyle(Button button, bool primary = false, bool danger = false)
        {
            if (button == null)
            {
                return;
            }

            var backColor = CardBackground;
            var foreColor = TextColor;

            if (primary)
            {
                backColor = AccentColor;
                foreColor = Color.White;
            }
            else if (danger)
            {
                backColor = DangerColor;
                foreColor = Color.White;
            }

            button.Font = UiSemiboldFont;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.MinimumSize = new Size(108, 40);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyControlTheme(control);
                if (control.HasChildren)
                {
                    ApplyToControls(control.Controls);
                }
            }
        }

        private static void ApplyControlTheme(Control control)
        {
            if (control == null || Equals(control.Tag, SkipThemeTag))
            {
                return;
            }

            control.Font = UiFont;

            if (control is Button button)
            {
                var isPrimary = ReferenceEquals(button, control.FindForm()?.AcceptButton);
                if (button.BackColor == default(Color) || button.UseVisualStyleBackColor)
                {
                    ApplyButtonStyle(button, isPrimary);
                }

                return;
            }

            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = Color.White;
                textBox.ForeColor = TextColor;
                return;
            }

            if (control is ComboBox comboBox)
            {
                comboBox.FlatStyle = FlatStyle.Flat;
                comboBox.BackColor = Color.White;
                comboBox.ForeColor = TextColor;
                return;
            }

            if (control is DateTimePicker dateTimePicker)
            {
                dateTimePicker.CalendarForeColor = TextColor;
                dateTimePicker.CalendarMonthBackground = Color.White;
                return;
            }

            if (control is CheckBox checkBox)
            {
                checkBox.ForeColor = TextColor;
                checkBox.BackColor = Color.Transparent;
                return;
            }

            if (control is Label label)
            {
                label.ForeColor = TextColor;
                if (label.BackColor == default(Color))
                {
                    label.BackColor = Color.Transparent;
                }

                return;
            }

            if (control is DataGridView grid)
            {
                StyleGrid(grid);
                return;
            }

            if (control is TabControl tabControl)
            {
                StyleTabControl(tabControl);
                return;
            }

            if (control is GroupBox groupBox)
            {
                groupBox.ForeColor = AccentDarkColor;
                groupBox.BackColor = CardBackground;
                groupBox.Font = UiSemiboldFont;
                return;
            }

            if (control is MenuStrip menuStrip)
            {
                StyleToolStrip(menuStrip);
                return;
            }

            if (control is StatusStrip statusStrip)
            {
                StyleToolStrip(statusStrip);
                statusStrip.BackColor = Color.FromArgb(12, 21, 34);
                return;
            }

            if ((control is FlowLayoutPanel || control is TableLayoutPanel || control is SplitContainer || control is Panel) &&
                control.BackColor == default(Color))
            {
                control.BackColor = SurfaceBackground;
            }
        }

        private static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = CardBackground;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = BorderColor;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = AccentDarkColor;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = AccentDarkColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = UiSemiboldFont;
            grid.ColumnHeadersHeight = 42;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = TextColor;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 240);
            grid.DefaultCellStyle.SelectionForeColor = TextColor;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 251);
            grid.RowTemplate.Height = 34;
            grid.DefaultCellStyle.Padding = new Padding(4);
        }

        private static void StyleTabControl(TabControl tabControl)
        {
            if (tabControl.DrawMode == TabDrawMode.OwnerDrawFixed)
            {
                return;
            }

            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.ItemSize = new Size(180, 36);
            tabControl.SizeMode = TabSizeMode.Fixed;
            tabControl.Padding = new Point(18, 6);
            tabControl.DrawItem += OnTabControlDrawItem;

            foreach (TabPage page in tabControl.TabPages.Cast<TabPage>())
            {
                page.BackColor = CardBackground;
                page.Padding = new Padding(0);
            }
        }

        private static void OnTabControlDrawItem(object sender, DrawItemEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null || e.Index < 0 || e.Index >= tabControl.TabPages.Count)
            {
                return;
            }

            var bounds = e.Bounds;
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var backColor = selected ? AccentColor : Color.FromArgb(219, 228, 236);
            var textColor = selected ? Color.White : TextColor;

            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            using (var borderPen = new Pen(BorderColor))
            {
                e.Graphics.DrawRectangle(borderPen, bounds);
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabControl.TabPages[e.Index].Text,
                UiSemiboldFont,
                bounds,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static void StyleMdiWorkspace(Form form)
        {
            foreach (Control control in form.Controls)
            {
                if (control.GetType().Name == "MdiClient")
                {
                    control.BackColor = WorkspaceBackground;
                }
            }
        }

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        private sealed class ThemeColorTable : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin => AppBackground;

            public override Color MenuStripGradientEnd => AppBackground;

            public override Color ToolStripDropDownBackground => Color.FromArgb(21, 33, 49);

            public override Color ImageMarginGradientBegin => Color.FromArgb(21, 33, 49);

            public override Color ImageMarginGradientMiddle => Color.FromArgb(21, 33, 49);

            public override Color ImageMarginGradientEnd => Color.FromArgb(21, 33, 49);

            public override Color MenuItemSelected => AccentColor;

            public override Color MenuItemBorder => AccentColor;

            public override Color MenuItemSelectedGradientBegin => AccentColor;

            public override Color MenuItemSelectedGradientEnd => AccentColor;

            public override Color MenuItemPressedGradientBegin => AccentDarkColor;

            public override Color MenuItemPressedGradientEnd => AccentDarkColor;

            public override Color SeparatorDark => BorderColor;

            public override Color SeparatorLight => BorderColor;

            public override Color StatusStripGradientBegin => Color.FromArgb(12, 21, 34);

            public override Color StatusStripGradientEnd => Color.FromArgb(12, 21, 34);
        }
    }
}
