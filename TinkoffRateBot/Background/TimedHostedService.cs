using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TinkoffRateBot.Background
{
    public abstract class TimedHostedService: IHostedService, IDisposable
    {
        protected readonly ILogger _logger;
        private readonly int _seconds;
        private readonly int _dueTime;
        private int executionCount = 0;
        private Timer _timer;

        public TimedHostedService(ILogger logger, int seconds, int dueTime = 0)
        {
            _logger = logger;
            _seconds = seconds;
            _dueTime = dueTime;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{GetType().Name} is running.");
            _timer = new Timer(TickHandle, null, TimeSpan.FromSeconds(_dueTime), TimeSpan.FromSeconds(_seconds));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{GetType().Name} is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        protected abstract Task TickAsync();

        private async void TickHandle(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            _logger.LogInformation($"{GetType().Name} is working. Count: {count}");
            await TickAsync();
        }
    }
}
