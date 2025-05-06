namespace Notification.Service.Resilience
{
    public class SimpleCircuitBreaker
    {
        private int _failureCount = 0;
        private readonly int _failureThreshold = 3;
        private readonly TimeSpan _openDuration = TimeSpan.FromSeconds(15);
        private DateTime? _lastFailureTime;

        public bool CanExecute()
        {
            if (_lastFailureTime == null) return true;
            if (DateTime.UtcNow - _lastFailureTime > _openDuration)
            {
                _failureCount = 0;
                _lastFailureTime = null;
                Console.WriteLine("[CIRCUIT BREAKER] Closed. Ready to retry.");
                return true;
            }
            return false;
        }

        public void RecordSuccess()
        {
            _failureCount = 0;
            _lastFailureTime = null;
        }

        public void RecordFailure()
        {
            _failureCount++;
            if (_failureCount >= _failureThreshold)
            {
                _lastFailureTime = DateTime.UtcNow;
                Console.WriteLine("[CIRCUIT BREAKER] Opened due to repeated failures.");
            }
        }
    }

}
