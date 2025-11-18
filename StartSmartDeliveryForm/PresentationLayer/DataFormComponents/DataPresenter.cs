using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.PresentationLayer.DataFormComponents
{
    public class DataPresenter<T> : IDisposable where T : class, new()
    {
        private IDataForm _dataForm;
        private readonly IDataModel<T> _model;
        private readonly ILogger<IDataPresenter<T>> _logger;
        private readonly ILogger<IDataForm> _dataFormLogger;
        public FormMode Mode { set; get; }

        private bool _disposedValue;

        public DataPresenter(
            IDataForm dataForm,
            IDataModel<T> model,
            ILogger<IDataPresenter<T>>? logger = null,
            ILogger<IDataForm>? dataFormLogger = null)
        {
            _dataForm = dataForm ?? throw new ArgumentNullException(nameof(dataForm));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _logger = logger ?? NullLogger<IDataPresenter<T>>.Instance;
            _dataFormLogger = dataFormLogger ?? NullLogger<IDataForm>.Instance;

            WireUpEvents();
            InitializeView();

            _logger.LogInformation("Created DataPresenter for entity {EntityType}", typeof(T).Name);
        }

        public event EventHandler<SubmissionCompletedEventArgs>? SubmissionCompleted;

        private void WireUpEvents()
        {
            _logger.LogDebug("Wiring up events for DataForm.SubmitClicked");
            _dataForm.SubmitClicked += HandleSubmitClicked;
        }

        private void InitializeView()
        {
            Dictionary<string, (Label Label, Control Control)> controlsLayout = _model.GenerateControls();
            _dataForm.RenderControls(controlsLayout);
            _logger.LogInformation("View initialized with {ControlCount} controls for entity {EntityType}", controlsLayout.Count, typeof(T).Name);
        }

        private async void HandleSubmitClicked(object? sender, SubmissionCompletedEventArgs e)
        {
            _logger.LogInformation("Handling form submission for entity {EntityType} in mode {Mode}", typeof(T).Name, Mode);
            try
            {
                (bool isValid, string? ErrorMessage) = await _model.ValidateFormAsync(_dataForm.GetControls());
                if (isValid)
                {
                    _logger.LogInformation("Form is valid");
                    T entity = _model.CreateFromForm(_dataForm.GetControls());
                    _logger.LogInformation("Submission entity: {entity}", entity);
                    SubmissionCompleted?.Invoke(this, new SubmissionCompletedEventArgs(entity, Mode));
                    if (Mode == FormMode.Edit && _dataForm is Form dataForm)
                    {
                        dataForm.Close();
                    }
                    _dataForm.ClearData(_model.GetDefaultValues());
                }
                else
                {
                    _logger.LogInformation("Form is not valid");
                    if (ErrorMessage != null)
                    {
                        _dataForm.ShowMessageBox(ErrorMessage, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        _dataForm.ShowMessageBox("Validation failed. Please check the form.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Submission failed: {Exception}", ex.Message);
                _dataForm.ShowMessageBox("Submission Failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeEditing(T entity)
        {
            Dictionary<string, object> values = _model.MapToForm(entity);
            _dataForm.InitializeEditing(values);
            _logger.LogInformation("Form initialized for editing entity {EntityType}", typeof(T).Name);
        }

        private HashSet<string>? _dataFormsExcludedColumns;
        public void SetExcludedColumns(HashSet<string> hashSet)
        {
            _dataFormsExcludedColumns = hashSet;
            _dataForm.SetExcludedColumns(hashSet);
            InitializeView();
        }

        public void SetMode(FormMode mode, T? entity = null)
        {
            _logger.LogDebug("Switching mode to {Mode}, DTO: {DtoType}", mode, typeof(T).Name);

            Mode = mode;
            _model.Mode = mode;
            DataForm form = _dataForm as DataForm ?? throw new InvalidOperationException("View provided for SetMode was not a dataform");

            // Each time the view is closed by the user or program, a new Instance is required. 
            if (form.IsDisposed)
            {
                // Remove old events
                _dataForm.SubmitClicked -= HandleSubmitClicked;

                _logger.LogWarning("Form is disposed, creating new instance");

                _dataForm = new DataForm(_dataFormLogger);
                form = _dataForm as DataForm ?? throw new InvalidOperationException("Failed to create new DataForm");
                _dataForm.SetExcludedColumns(_dataFormsExcludedColumns ?? []);
                WireUpEvents();
                InitializeView();
            }

            _logger.LogInformation("FormMode: {formMode}", Mode);
            if (mode == FormMode.Add)
            {
                _logger.LogDebug("Clearing data for Add mode for entity {EntityType}", typeof(T).Name);
                _dataForm.ClearData(_model.GetDefaultValues());
            }
            else if (mode == FormMode.Edit && entity != null)
            {
                _logger.LogDebug("Initializing editing for entity {EntityType} in Edit mode", typeof(T).Name);
                InitializeEditing(entity);
            }

            if (!form.Visible)
            {
                form.Show();
            }
            else
            {
                form.BringToFront();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _dataForm.SubmitClicked -= HandleSubmitClicked;

                    if (_dataForm is Form form && !form.IsDisposed)
                    {
                        form.Dispose();
                        _logger.LogDebug("Disposed DataForm during DataPresenter disposal, DTO: {DtoType}", typeof(T).Name);
                    }

                    if (_model is IDisposable disposableModel)
                    {
                        disposableModel.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        ~DataPresenter()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
