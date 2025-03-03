using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartSmartDeliveryForm.PresentationLayer.TemplateViews;

namespace StartSmartDeliveryForm.SharedLayer.EventArgs
{
    public static class CustomEventArgs
    {
        public class SubmissionCompletedEventArgs(object? data, FormMode mode) : System.EventArgs
        {
            public object? Data { get; } = data;
            public FormMode Mode { get; } = mode;

            public static new SubmissionCompletedEventArgs Empty => new(null, FormMode.Add);
        }
    }
}
