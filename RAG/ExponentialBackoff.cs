using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAG.Loader
{

    public class ExponentialBackoff
    {
        private readonly int _maxRetries;
        private readonly TimeSpan _initialDelay;
        private readonly TimeSpan _maxDelay;

        public ExponentialBackoff(int maxRetries, TimeSpan initialDelay, TimeSpan maxDelay)
        {
            _maxRetries = maxRetries;
            _initialDelay = initialDelay;
            _maxDelay = maxDelay;
        }

        public void Execute(Action action)
        {
            int retries = 0;
            TimeSpan delay = _initialDelay;
            bool retry = true;

            while (retry)
            {
                try
                {
                    action();
                    retry = false; // If action succeeds, no need to retry
                }
                catch (Exception ex)
                {
                    retries++;
                    if (retries > _maxRetries)
                    {
                        throw new Exception("Max retries exceeded", ex);
                    }
                    else
                    {
                        Console.WriteLine($"Retry #{retries}. Exception: {ex.Message}");
                        Thread.Sleep(delay);
                        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, _maxDelay.TotalMilliseconds));
                    }
                }
            }
        }
    }

}
