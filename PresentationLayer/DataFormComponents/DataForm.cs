using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.SharedLayer;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public partial class DataForm : Form, IDataForm
    {
        public FormMode Mode { get; set; }
        public readonly ILogger<IDataForm> _logger;
        private readonly IMessageBox _messageBox;
        private readonly Dictionary<string, Control> _dynamicControls = [];
        public DataForm() : this(NullLogger<IDataForm>.Instance, new MessageBoxWrapper()) { }
        public DataForm(ILogger<IDataForm>? logger = null, IMessageBox? messageBox = null)
        {
            InitializeComponent();
            _logger = logger ?? NullLogger<IDataForm>.Instance;
            _messageBox = messageBox ?? new MessageBoxWrapper();
            _logger.LogInformation("DataForm created");
        }

        private void DataForm_Load(object sender, EventArgs e)
        {
            btnSubmit.BackColor = GlobalConstants.SoftBeige;
            btnSubmit.FlatAppearance.BorderSize = 0;
            _logger.LogInformation("DataForm loaded");
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

        public void ShowMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _messageBox.Show(text, caption, buttons, icon);
            _logger.LogInformation("Message box shown: {Caption}", caption);
        }

        public void InitializeEditing(Dictionary<string, object> values)
        {
            _logger.LogDebug("Initializing editing with {ValueCount} values", values.Count);
            foreach ((string name, object value) in values)
            {
                if (_dynamicControls.TryGetValue(name, out Control? control))
                {
                    switch (control)
                    {
                        case TextBox textBox:
                            textBox.Text = value?.ToString() ?? string.Empty;
                            break;
                        case ComboBox comboBox:
                            comboBox.SelectedItem = value?.ToString();
                            if (value == null || comboBox.SelectedItem == null)
                            {
                                comboBox.SelectedIndex = -1;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            _logger.LogInformation("Editing initialized with {ValueCount} values", values.Count);
        }

        public void ClearData(Dictionary<string, object?>? defaultValues = null)
        {
            _logger.LogDebug("Clearing data for {ControlCount} controls", _dynamicControls.Count);

            if (defaultValues == null)
            {
                _logger.LogWarning("No default values found");
            }

            foreach ((string key, Control control) in _dynamicControls)
            {
                object? defaultValueObject = null;
                defaultValues?.TryGetValue(key, out defaultValueObject);
                string? defaultValue = defaultValueObject?.ToString();
                _logger.LogDebug("Default Value for {Key}: {defaultValue}", key, defaultValue);

                switch (control)
                {
                    case TextBox textBox:
                        textBox.Text = defaultValue?.ToString() ?? string.Empty;
                        _logger.LogDebug("Set {ControlName}.Text to {DefaultValue}", key, defaultValue);
                        break;
                    case ComboBox comboBox:
                        string? stringValue = defaultValue?.ToString();
                        if (comboBox.Items.Contains(stringValue))
                        {
                            comboBox.SelectedItem = stringValue;
                        }
                        else
                        {
                            comboBox.SelectedIndex = -1;
                        }
                        _logger.LogDebug("Set {ControlName}.SelectedItem to {DefaultValue}", key, defaultValue);
                        break;
                    default:
                        _logger.LogWarning("Unsupported control type {ControlType} for {ControlName}", control.GetType().Name, key);
                        break;
                }
            }

            _logger.LogInformation("Data cleared for {ControlCount} controls", _dynamicControls.Count);
        }

        public void RenderControls(Dictionary<string, (Label Label, Control Control)> controlsLayout)
        {
            //TODO - Cache static layouts

            _logger.LogDebug("Rendering {ControlCount} controls", controlsLayout.Count);
            tlpDynamicFields.Controls.Clear();
            _dynamicControls.Clear();
            tlpDynamicFields.ColumnStyles.Clear();
            tlpDynamicFields.RowStyles.Clear();

            const int MAX_COLUMN_PAIRS = 2;
            const int CONTROLS_PER_PAIR = 2;
            const int CONTROL_WIDTH = 150;
            const int LABEL_WIDTH = 100;
            const int CONTROL_HEIGHT_ESTIMATE = 55;
            int totalFields = controlsLayout.Count;
            int columnsPerRow = Math.Min(MAX_COLUMN_PAIRS, (totalFields + 1) / 2) * CONTROLS_PER_PAIR;
            int rowsNeeded = (int)Math.Ceiling((double)totalFields / (columnsPerRow / CONTROLS_PER_PAIR));

            tlpDynamicFields.ColumnCount = columnsPerRow;
            tlpDynamicFields.RowCount = rowsNeeded;

            for (int i = 0; i < columnsPerRow; i += CONTROLS_PER_PAIR)
            {
                tlpDynamicFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LABEL_WIDTH));
                tlpDynamicFields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CONTROL_WIDTH));
            }
            for (int i = 0; i < rowsNeeded; i++)
            {
                tlpDynamicFields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            int row = 0, col = 0;
            foreach ((string name, (Label label, Control control)) in controlsLayout)
            {
                tlpDynamicFields.Controls.Add(label, col, row);
                tlpDynamicFields.Controls.Add(control, col + 1, row);
                _dynamicControls[name] = control;

                col += CONTROLS_PER_PAIR;
                if (col >= columnsPerRow)
                {
                    col = 0;
                    row++;
                }
            }

            int requiredWidth = (columnsPerRow / 2 * LABEL_WIDTH) + (columnsPerRow / 2 * CONTROL_WIDTH) + 20;
            int requiredHeight = rowsNeeded * CONTROL_HEIGHT_ESTIMATE;
            tlpDynamicFields.AutoScroll = true;
            AutoSize = false;
            Size = new Size(requiredWidth, requiredHeight);
            _logger.LogInformation("Controls rendered: {ControlCount} controls in {RowCount} rows", controlsLayout.Count, rowsNeeded);
        }

        public Dictionary<string, Control> GetControls()
        {
            _logger.LogDebug("Returning {ControlCount} controls", _dynamicControls.Count);
            return _dynamicControls;
        }
    }
}
