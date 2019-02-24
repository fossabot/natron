using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Natron.Http;
using ValidDotNet;

namespace Natron
{
    public sealed class ServiceBuilder
    {
        private ILoggerFactory _loggerFactory;
        private CancellationTokenSource _cts;
        private readonly List<IComponent> _components;

        private ServiceBuilder()
        {
            _components = new List<IComponent>();
            _cts = new CancellationTokenSource();
        }

        public static ServiceBuilder Create(ILoggerFactory loggerFactory)
        {
            return new ServiceBuilder
            {
                _loggerFactory = loggerFactory.ThrowIfNull(nameof(loggerFactory))
            };
        }

        public ServiceBuilder ConfigureCancellationTokenSource(CancellationTokenSource source)
        {
            _cts = source.ThrowIfNull(nameof(source));
            return this;
        }

        public ServiceBuilder ConfigureHttp(HttpConfig config)
        {
            _components.Add(new Component(_loggerFactory, config.ThrowIfNull(nameof(config))));
            return this;
        }

        public ServiceBuilder ConfigureComponents(params IComponent[] components)
        {
            _components.AddRange(components.ThrowIfNull(nameof(components)));
            return this;
        }

        public Service Build()
        {
            return new Service(_loggerFactory, _cts, _components.ToArray());
        }
    }
}