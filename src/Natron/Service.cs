using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Natron.Http;
using ValidDotNet;

namespace Natron
{
    public sealed class Service
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IComponent[] _components;
        private readonly List<Task> _tasks;
        private readonly CancellationTokenSource _cancelTokenSource;
        private bool _cancelKeyPressed;

        internal Service(ILoggerFactory loggerFactory, CancellationTokenSource cts, IComponent[] components)
        {
            _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<Service>();
            _components = components.ThrowIfNullOrEmpty(nameof(components));
            _cancelTokenSource = cts.ThrowIfNull(nameof(cts));
            _tasks = new List<Task>();
            SetupCancelKeyPress();
        }

        public async Task RunAsync()
        {
            try
            {
                foreach (var component in _components)
                {
                    _tasks.Add(component.RunAsync(_cancelTokenSource.Token));
                }

                await Task.WhenAny(_tasks);

                GracefulShutdownComponents();

                await Task.WhenAll(_tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught, gracefully shutting down components.");
                GracefulShutdownComponents();
            }
        }

        private void SetupCancelKeyPress()
        {
            Console.CancelKeyPress += (s, ev) =>
            {
                _logger.LogInformation("Ctrl+C pressed.");
                _cancelTokenSource.Cancel();
                _cancelKeyPressed = true;
                ev.Cancel = true;
            };
        }

        private void GracefulShutdownComponents()
        {
            if (_cancelKeyPressed)
            {
                return;
            }

            _logger.LogWarning("Component returned unexpected. Canceling all components.");
            _cancelTokenSource.Cancel();
        }
    }
}