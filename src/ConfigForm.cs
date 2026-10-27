using System;
using System.Drawing;
using System.Windows.Forms;

namespace GorgyLedi
{
    /// <summary>Fenêtre de réglages (bouton « Paramètres » de Windows, argument /c).</summary>
    sealed class ConfigForm : Form
    {
        static readonly string[] PresetKeys = { "red", "green", "amber", "blue", "white" };
        static readonly Color[] PresetColors =
        {
            Color.FromArgb(255, 32, 32), Color.FromArgb(40, 255, 60), Color.FromArgb(255, 170, 0),
            Color.FromArgb(40, 140, 255), Color.FromArgb(240, 240, 240)
        };

        readonly Settings settings;
        readonly Label headerLabel = new Label();
        readonly PreviewPanel preview;
        readonly Label languageLabel = new Label();
        readonly ComboBox languageBox = new ComboBox();
        readonly Label colorLabel = new Label();
        readonly ComboBox colorBox = new ComboBox();
        readonly Button colorButton = new Button();
        readonly CheckBox secondsBox = new CheckBox();
        readonly CheckBox dateBox = new CheckBox();
        readonly CheckBox blinkBox = new CheckBox();
        readonly CheckBox unlitBox = new CheckBox();
        readonly CheckBox logoBox = new CheckBox();
        readonly Button okButton = new Button();
        readonly Button cancelButton = new Button();
        bool updating;

        public ConfigForm(Settings settings)
        {
            this.settings = settings;
            Font = SystemFonts.MessageBoxFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AcceptButton = okButton;
            CancelButton = cancelButton;

            var layout = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12),
            };

            headerLabel.AutoSize = true;
            headerLabel.Font = new Font(Font, FontStyle.Bold);
            headerLabel.Margin = new Padding(3, 3, 3, 9);
            AddRow(layout, headerLabel);

            preview = new PreviewPanel(settings) { Size = new Size(Font.Height * 26, Font.Height * 18) };
            preview.Margin = new Padding(3, 3, 3, 9);
            AddRow(layout, preview);

            languageBox.DropDownStyle = ComboBoxStyle.DropDownList;
            languageBox.Items.AddRange(Strings.LanguageNames);
            languageBox.SelectedIndex = Math.Max(0, Array.IndexOf(Strings.Languages, settings.Language));
            languageBox.SelectedIndexChanged += delegate
            {
                settings.Language = Strings.Languages[languageBox.SelectedIndex];
                ApplyTexts();
            };
            AddRow(layout, languageLabel, languageBox);

            colorBox.DropDownStyle = ComboBoxStyle.DropDownList;
            colorBox.Width = Font.Height * 12;
            colorBox.SelectedIndexChanged += OnColorSelected;
            colorButton.Size = new Size(Font.Height * 2, colorBox.Height);
            colorButton.FlatStyle = FlatStyle.Flat;
            colorButton.Click += delegate { PickCustomColor(); };
            var colorPanel = new FlowLayoutPanel { AutoSize = true, Margin = Padding.Empty, WrapContents = false };
            colorPanel.Controls.Add(colorBox);
            colorPanel.Controls.Add(colorButton);
            AddRow(layout, colorLabel, colorPanel);

            BindCheckBox(layout, secondsBox, settings.ShowSeconds, delegate(bool v) { settings.ShowSeconds = v; });
            BindCheckBox(layout, dateBox, settings.ShowDate, delegate(bool v) { settings.ShowDate = v; });
            BindCheckBox(layout, blinkBox, settings.BlinkColon, delegate(bool v) { settings.BlinkColon = v; });
            BindCheckBox(layout, unlitBox, settings.ShowUnlit, delegate(bool v) { settings.ShowUnlit = v; });
            BindCheckBox(layout, logoBox, settings.ShowLogo, delegate(bool v) { settings.ShowLogo = v; });

            okButton.AutoSize = true;
            okButton.Click += OnOk;
            cancelButton.AutoSize = true;
            cancelButton.DialogResult = DialogResult.Cancel;
            var buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 12, 0, 0),
            };
            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(okButton);
            AddRow(layout, buttons);

            Controls.Add(layout);
            ApplyTexts();
        }

        static void AddRow(TableLayoutPanel layout, Control fullWidth)
        {
            layout.Controls.Add(fullWidth, 0, layout.RowCount);
            layout.SetColumnSpan(fullWidth, 2);
            layout.RowCount++;
        }

        static void AddRow(TableLayoutPanel layout, Label label, Control control)
        {
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            layout.Controls.Add(label, 0, layout.RowCount);
            layout.Controls.Add(control, 1, layout.RowCount);
            layout.RowCount++;
        }

        static void BindCheckBox(TableLayoutPanel layout, CheckBox box, bool value, Action<bool> setter)
        {
            box.AutoSize = true;
            box.Checked = value;
            box.CheckedChanged += delegate { setter(box.Checked); };
            AddRow(layout, box);
        }

        string T(string key)
        {
            return Strings.Get(key, settings.Language);
        }

        void ApplyTexts()
        {
            Text = T("title");
            headerLabel.Text = T("header");
            languageLabel.Text = T("language");
            colorLabel.Text = T("color");
            secondsBox.Text = T("seconds");
            dateBox.Text = T("date");
            blinkBox.Text = T("blink");
            unlitBox.Text = T("unlit");
            logoBox.Text = T("logo");
            okButton.Text = T("ok");
            cancelButton.Text = T("cancel");

            updating = true;
            colorBox.Items.Clear();
            foreach (string key in PresetKeys)
                colorBox.Items.Add(T(key));
            colorBox.Items.Add(T("custom"));
            int preset = Array.FindIndex(PresetColors, c => c.ToArgb() == settings.LedColor.ToArgb());
            colorBox.SelectedIndex = preset >= 0 ? preset : PresetKeys.Length;
            colorButton.BackColor = settings.LedColor;
            updating = false;
        }

        void OnColorSelected(object sender, EventArgs e)
        {
            if (updating)
                return;
            if (colorBox.SelectedIndex < PresetColors.Length)
            {
                settings.LedColor = PresetColors[colorBox.SelectedIndex];
                colorButton.BackColor = settings.LedColor;
            }
            else
            {
                PickCustomColor();
            }
        }

        void PickCustomColor()
        {
            using (var dialog = new ColorDialog { Color = settings.LedColor, FullOpen = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    settings.LedColor = Color.FromArgb(255, dialog.Color);
            }
            ApplyTexts(); // resélectionne le préréglage correspondant, ou « Personnalisée »
        }

        void OnOk(object sender, EventArgs e)
        {
            try
            {
                settings.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, T("saveError") + "\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>Aperçu en direct des réglages en cours.</summary>
        sealed class PreviewPanel : Panel
        {
            readonly Settings settings;
            readonly Timer timer = new Timer { Interval = 100 };

            public PreviewPanel(Settings settings)
            {
                this.settings = settings;
                DoubleBuffered = true;
                ResizeRedraw = true;
                BorderStyle = BorderStyle.FixedSingle;
                timer.Tick += delegate { Invalidate(); };
                timer.Start();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                DateTime now = DateTime.Now;
                bool colonOn = !settings.BlinkColon || now.Millisecond < 500;
                LedRenderer.Render(e.Graphics, ClientRectangle, now, settings, colonOn);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    timer.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
