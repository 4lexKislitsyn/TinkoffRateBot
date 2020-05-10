using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TinkoffRateBot.Background
{
    /// <summary>
    /// Service that triggered handle action by timer.
    /// </summary>
    public abstract class TimedHostedService: IHostedService, IDisposable
    {
        protected readonly ILogger _logger;
        private readonly int _seconds;
        private readonly int _dueTime;
        private int executionCount = 0;
        private Timer _timer;
        /// <summary>
        /// Create an instance of <see cref="TimedHostedService"/>.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="seconds">The period of raising handle callback.</param>
        /// <param name="dueTime">The amount of time to delay before raise handle callback.</param>
        public TimedHostedService(ILogger logger, int seconds, int dueTime = 0)
        {
            _logger = logger;
            _seconds = seconds;
            _dueTime = dueTime;
        }
        /// <inheritdoc/>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{GetType().Name} is running.");
            _timer = new Timer(TickHandle, null, TimeSpan.FromSeconds(_dueTime), TimeSpan.FromSeconds(_seconds));
            return Task.CompletedTask;
        }
        /// <inheritdoc/>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{GetType().Name} is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            _timer?.Dispose();
        }
        /// <summary>
        /// Timer tick handler.
        /// </summary>
        /// <returns></returns>
        protected abstract Task TickAsync();

        private async void TickHandle(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation($"{GetType().Name} is working. Count: {count}");
            try
            {
                await TickAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error was occurred while handle tick.");
            }
        }
    }
}
