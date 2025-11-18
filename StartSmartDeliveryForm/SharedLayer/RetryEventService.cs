namespace StartSmartDeliveryForm.SharedLayer
{
    public class RetryEventService
    {
        public delegate void RetryEventHandler(int attemptNumber, int maxRetries, TimeSpan retryDelay, string exceptionMessage);
        public event RetryEventHandler? RetryOccurred;

        public delegate void RetrySuccessEventHandler();
        public event RetrySuccessEventHandler RetrySucceeded;

        private bool _hasRetried = false;
        public void OnRetryOccurred(int attemptNumber, int maxRetries, TimeSpan retryDelay, string exceptionMessage)
        {
            _hasRetried = true;
            RetryOccurred?.Invoke(attemptNumber, maxRetries, retryDelay, exceptionMessage);
        }

        public void OnRetrySuccessOccurred()
        {
            if (_hasRetried)
            {
                RetrySucceeded?.Invoke();
                _hasRetried = false;
            }
        }
    }

}
