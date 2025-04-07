using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.Generics;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;
using static StartSmartDeliveryForm.Generics.TableDefinition;

namespace StartSmartDeliveryForm.PresentationLayer.TemplateViews
{
    public partial class GenericDataFormTemplate : Form, IGenericDataForm
    {
        public FormMode Mode { get; set; }
        public readonly ILogger<GenericDataFormTemplate> _logger;
        private readonly IMessageBox _messageBox;
        private readonly Type _entityType;
        private readonly TableConfig _tableConfig;
        private readonly Dictionary<string, Control> _dynamicControls = [];

        public GenericDataFormTemplate() : this(typeof(object), TableConfigs.Empty, NullLogger<GenericDataFormTemplate>.Instance, new MessageBoxWrapper()) { }
        public GenericDataFormTemplate(Type entityType, TableConfig tableConfig, ILogger<GenericDataFormTemplate>? logger = null, IMessageBox? messageBox = null)
        {
            InitializeComponent();
            _logger = logger ?? NullLogger<GenericDataFormTemplate>.Instance;
            _messageBox = messageBox ?? new MessageBoxWrapper();
            _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            _tableConfig = tableConfig ?? throw new ArgumentNullException(nameof(tableConfig));
            GenerateDynamicFields();
        }

        private void GenericDataForm_Load(object sender, EventArgs e)
        {
            btnSubmit.BackColor = GlobalConstants.SoftBeige;
            btnSubmit.FlatAppearance.BorderSize = 0;
        }

        private event EventHandler<SubmissionCompletedEventArgs>? _submitClicked;
        public event EventHandler<SubmissionCompletedEventArgs>? SubmitClicked
        {
            add { _submitClicked += value; }
            remove { _submitClicked -= value; }
        }

        public void btnSubmit_Click(object? sender, EventArgs e)
        {
            _logger.LogInformation("btnSubmit clicked");
            _submitClicked?.Invoke(this, SubmissionCompletedEventArgs.Empty);
        }

        public virtual void OnSubmissionComplete(object? sender, SubmissionCompletedEventArgs e) { }

        public void ShowMessageBox(string Text, string Caption, MessageBoxButtons Buttons, MessageBoxIcon Icon)
        {
            _messageBox.Show(Text, Caption, Buttons, Icon);
        }

        public void InitializeEditing(object data)
        {
            if (data == null) return;
            _tableConfig.MapToForm(data, _dynamicControls);
        }

        public void ClearData()
        {
            foreach ((ColumnConfig column, Control control) in _tableConfig.Columns.Zip(_dynamicControls.Values))
            {
                switch (control)
                {
                    case TextBox textBox:
                        textBox.Clear();
                        break;
                    case ComboBox comboBox:
                        object? defaultValue = _tableConfig.GetDefaultValue(column);
                        if (defaultValue != null)
                            comboBox.SelectedItem = defaultValue.ToString();
                        else
                            comboBox.SelectedIndex = column.SqlType == SqlDbType.Bit ? 1 : -1; // False for bool, -1 for enums
                        break;
                }
            }
        }

        public object GetData()
        {
            return _tableConfig.CreateFromForm(_dynamicControls);
        }

        private void GenerateDynamicFields()
        {
            // Clear existing (If any)
            tlpDynamicFields.Controls.Clear();
            _dynamicControls.Clear();
            tlpDynamicFields.ColumnStyles.Clear();

            // Layout constants
            const int MAX_COLUMN_PAIRS = 2;
            const int CONTROLS_PER_PAIR = 2;
            const int CONTROL_WIDTH = 150;
            const int LABEL_WIDTH = 100;
            const int CONTROL_HEIGHT_ESTIMATE = 55;
            int totalFields = _tableConfig.Columns.Count;
            int columnsPerRow = Math.Min(MAX_COLUMN_PAIRS, (totalFields + 1) / 2) * CONTROLS_PER_PAIR;
            int rowsNeeded = (int)Math.Ceiling((double)totalFields / (columnsPerRow / CONTROLS_PER_PAIR));

            // Configure TableLayoutPanel
            tlpDynamicFields.ColumnCount = columnsPerRow;
            tlpDynamicFields.RowCount = rowsNeeded;

            for (int i = 0; i < columnsPerRow; i += CONTROLS_PER_PAIR)
            {
                tlpDynamicFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LABEL_WIDTH));
                tlpDynamicFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CONTROL_WIDTH));
            }
            tlpDynamicFields.RowStyles.Clear();
            for (int i = 0; i < rowsNeeded; i++)
            {
                tlpDynamicFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Generate controls
            int row = 0, col = 0;
            var entityProperties = _entityType.GetProperties().ToDictionary(p => p.Name, p => p);

            _logger.LogInformation("TableConfig Columns: {Columns}", string.Join(", ", _tableConfig.Columns.Select(col => col.Name)));
            foreach (ColumnConfig column in _tableConfig.Columns)
            {
                Label label = new()
                {
                    Text = column.Name,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(3, 6, 3, 3)
                };
                tlpDynamicFields.Controls.Add(label, col, row);

                Control control;
                if (column.SqlType == SqlDbType.Bit)
                {
                    control = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Name = $"cbo{column.Name}",
                        Width = CONTROL_WIDTH,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Margin = new Padding(3)
                    };
                    ((ComboBox)control).Items.AddRange(["False", "True"]); // Putting True first means 0 index is True

                    object? defaultValue = _tableConfig.GetDefaultValue(column);
                    if (defaultValue is bool boolValue && boolValue) { ((ComboBox)control).SelectedIndex = 1; }
                }
                else if (entityProperties.TryGetValue(column.Name, out System.Reflection.PropertyInfo? prop) && prop.PropertyType.IsEnum)
                {
                    control = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Name = $"cbo{column.Name}",
                        Width = CONTROL_WIDTH,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Margin = new Padding(3)
                    };
                    ((ComboBox)control).Items.AddRange(Enum.GetNames(prop.PropertyType));
                    ((ComboBox)control).SelectedIndex = 0;
                }
                else
                {
                    control = new TextBox
                    {
                        Name = $"txt{column.Name}",
                        Width = CONTROL_WIDTH,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Margin = new Padding(3)
                    };
                }

                tlpDynamicFields.Controls.Add(control, col + 1, row);
                _dynamicControls[column.Name] = control;

                col += CONTROLS_PER_PAIR;
                if (col >= columnsPerRow)
                {
                    col = 0;
                    row++;
                }
            }

            // Calculate required size
            int requiredWidth = (columnsPerRow / 2 * LABEL_WIDTH) + (columnsPerRow / 2 * CONTROL_WIDTH) + 20;
            int requiredHeight = rowsNeeded * CONTROL_HEIGHT_ESTIMATE; // Height for fields + button/padding

            // Set form size and minimum size
            tlpDynamicFields.AutoScroll = true; // Scroll if exceeds screen
            AutoSize = false; // Manual sizing with scrolling
            MinimumSize = new Size(requiredWidth, requiredHeight); // Minimum size to fit all fields
            Size = new Size(requiredWidth, requiredHeight); // Initial size matches content
        }


    }
}
