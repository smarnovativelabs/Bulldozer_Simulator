#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Bulldozer.Services
{
    /// <summary>Bounded, asynchronous StoreKit proof; no receipt parsing or trust in UI callbacks.</summary>
    internal sealed class AppleTransactionVerifier : IDisposable
    {
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private const int TimeoutMilliseconds = 15000;
        private const int PollMilliseconds = 100;

        [DllImport("__Internal")] private static extern int QuarryAppleVerificationSupported();
        [DllImport("__Internal")] private static extern int QuarryAppleVerificationBegin(
            string productId, string transactionId, int nonConsumable);
        [DllImport("__Internal")] private static extern int QuarryAppleVerificationPoll(int request);
        [DllImport("__Internal")] private static extern void QuarryAppleVerificationRelease(int request);

        public static bool IsSupported => QuarryAppleVerificationSupported() == 1;

        public async Task<bool> VerifyAsync(string productId, string transactionId, bool nonConsumable)
        {
            int request = 0;
            try
            {
                if (lifetime.IsCancellationRequested) return false;
                // Native code allows at most four outstanding requests, including cancelled
                // StoreKit calls that have not returned. Repeated taps cannot grow a queue.
                request = QuarryAppleVerificationBegin(productId, transactionId, nonConsumable ? 1 : 0);
                if (request == 0) return false;
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token))
                {
                    timeout.CancelAfter(TimeoutMilliseconds);
                    while (!timeout.IsCancellationRequested)
                    {
                        int state = QuarryAppleVerificationPoll(request);
                        if (state != 0) return state == 1;
                        await Task.Delay(PollMilliseconds, timeout.Token);
                    }
                }
            }
            catch (Exception) { /* Fail closed; Unity can redeliver an unfinished purchase. */ }
            finally { if (request != 0) QuarryAppleVerificationRelease(request); }
            return false;
        }

        public void Dispose() { lifetime.Cancel(); }
    }
}
#endif

