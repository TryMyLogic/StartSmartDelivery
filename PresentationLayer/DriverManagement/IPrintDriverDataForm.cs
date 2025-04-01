﻿using System.Drawing.Printing;

namespace StartSmartDeliveryForm.PresentationLayer.DriverManagement
{
    public interface IPrintDriverDataForm
    {
        int CurrentPage { get; set; }
        int TotalPages { get; set; }
        void SetPrintDocument(PrintDocument document);
        void HideNavigationButtons();
        void UpdatePreviewPage(int pageIndex);

        event EventHandler PreviousClicked;
        event EventHandler NextClicked;
        event EventHandler SubmitClicked;
        void CloseForm();
    }
}
