using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartSmartDeliveryForm.PresentationLayer.DriverManagement.Views;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement.Presenters
{
    internal class DriverManagementFormPresenter
    {
        private readonly IDriverManagementForm _driverManagementForm;
        private readonly ILogger<DriverManagementFormPresenter> _logger;
        public DriverManagementFormPresenter(IDriverManagementForm driverManagementForm, ILogger<DriverManagementFormPresenter>? logger = null) {
            _driverManagementForm = driverManagementForm;
            _logger = logger ?? NullLogger<DriverManagementFormPresenter>.Instance;
        }
    }
}
