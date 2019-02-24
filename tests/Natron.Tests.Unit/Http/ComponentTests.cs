using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Natron.Http;
using NSubstitute;
using Xunit;

namespace Natron.Tests.Unit.Http
{
    public class ComponentTests
    {
        [Fact]
        public async Task Component_RunAsync()
        {
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var cts = new CancellationTokenSource();
            var config = new HttpConfig();
            config.Urls.Add("http://0.0.0.0:5002");
            config.Urls.Add("https://0.0.0.0:5003");
            var cmp = new Component(loggerFactory, config);
            var t = cmp.RunAsync(cts.Token);
            await Task.Delay(10, cts.Token);
            cts.Cancel();
            await Task.WhenAll(t);
            t.Status.Should().Be(TaskStatus.RanToCompletion);
        }
    }
}