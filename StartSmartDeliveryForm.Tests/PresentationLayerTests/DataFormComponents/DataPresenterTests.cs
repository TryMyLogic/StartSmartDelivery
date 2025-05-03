using Microsoft.Extensions.Logging;
using StartSmartDeliveryForm.BusinessLogicLayer;
using StartSmartDeliveryForm.DataLayer.DTOs;
using StartSmartDeliveryForm.PresentationLayer.DataFormComponents;
using StartSmartDeliveryForm.SharedLayer.Enums;
using StartSmartDeliveryForm.Tests.SharedTestItems;
using Xunit.Abstractions;
using static StartSmartDeliveryForm.SharedLayer.TableDefinition;
using StartSmartDeliveryForm.DataLayer.Repositories;
using System.Windows.Forms;
using NSubstitute.ExceptionExtensions;
using NSubstitute;
using StartSmartDeliveryForm.SharedLayer.EventArgs;
using StartSmartDeliveryForm.SharedLayer.Interfaces;

namespace StartSmartDeliveryForm.Tests.PresentationLayerTests.DataFormComponents
{
    public class DataPresenterTests(DatabaseFixture fixture, ITestOutputHelper output) : IClassFixture<DatabaseFixture>
    {
        private readonly ILogger<IDataPresenter<DriversDTO>> _testLogger = SharedFunctions.CreateTestLogger<IDataPresenter<DriversDTO>>(output);
        private readonly IRepository<DriversDTO> _repository = fixture.DriversRepository;
        private DataForm? _dataForm;
        private DataPresenter<DriversDTO>? _dataPresenter;
        private DataModel<DriversDTO>? _dataModel;

        [Fact]
        public void Constructor_NullDataForm_ThrowsArgumentNullException()
        {
            // Arrange
            IDataModel<DriversDTO> mockDataModel = Substitute.For<IDataModel<DriversDTO>>();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new DataPresenter<DriversDTO>(null!, mockDataModel, _testLogger));
            Assert.Equal("dataForm", exception.ParamName);
        }

        [Fact]
        public void Constructor_NullDataModel_ThrowsArgumentNullException()
        {
            // Arrange
            IDataForm mockDataForm = Substitute.For<IDataForm>();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new DataPresenter<DriversDTO>(mockDataForm, null!, _testLogger));
            Assert.Equal("model", exception.ParamName);
        }

        [Theory]
        [InlineData(1, "John", "Doe", "EMP001", LicenseType.Code8, false)]
        [InlineData(2, "Jane", "Smith", "EMP002", LicenseType.Code8, true)]
        [InlineData(3, "Jim", "Brown", "EMP003", LicenseType.Code10, false)]
        [InlineData(4, "Jake", "White", "EMP004", LicenseType.Code10, true)]
        [InlineData(5, "Jill", "Green", "EMP005", LicenseType.Code14, false)]
        [InlineData(6, "Jack", "Black", "EMP006", LicenseType.Code14, true)]
        public void InitializeEditing_SetsFormValues_CallsMapToFormAndInitializeEditing(int DriverID, string Name, string Surname, string EmployeeNo, LicenseType LicenseType, bool Availability)
        {
            // Arrange
            _dataForm = new(null, new NoMessageBox());
            _dataModel = new(_repository);
            _dataPresenter = new(_dataForm, _dataModel);
            DriversDTO driver = new(DriverID, Name, Surname, EmployeeNo, LicenseType, Availability);
            Dictionary<string, object> expectedValues = new(){
            { "DriverID", DriverID },
            { "Name", Name },
            { "Surname", Surname },
            { "EmployeeNo", EmployeeNo },
            { "LicenseType", LicenseType },
            { "Availability", Availability }};

            _dataPresenter = new DataPresenter<DriversDTO>(_dataForm, _dataModel, _testLogger);

            // Act
            _dataPresenter.InitializeEditing(driver);

            // Assert
            DriversDTO entity = _dataModel.CreateFromForm(_dataForm.GetControls());

            Assert.Equal(DriverID, entity.DriverID);
            Assert.Equal(Name, entity.Name);
            Assert.Equal(Surname, entity.Surname);
            Assert.Equal(EmployeeNo, entity.EmployeeNo);
            Assert.Equal(LicenseType, entity.LicenseType);
            Assert.Equal(Availability, entity.Availability);
        }
    }
}


