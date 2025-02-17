using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartSmartDeliveryForm.SharedLayer
{
    public class RetryEventService
    {
        public delegate void RetryEventHandler(int attemptNumber, int maxRetries, TimeSpan retryDelay, string exceptionMessage);
        public event RetryEventHandler? RetryOccurred;

        // Method to raise the event
        public void OnRetryOccurred(int attemptNumber, int maxRetries, TimeSpan retryDelay, string exceptionMessage)
        {
            RetryOccurred?.Invoke(attemptNumber, maxRetries, retryDelay, exceptionMessage);
        }
    }

}
