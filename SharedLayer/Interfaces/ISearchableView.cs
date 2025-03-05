using StartSmartDeliveryForm.SharedLayer.EventArgs;

namespace StartSmartDeliveryForm.SharedLayer.Interfaces
{
    public interface ISearchableView
    {
        event EventHandler<SearchRequestEventArgs> SearchClicked;
    }
}
