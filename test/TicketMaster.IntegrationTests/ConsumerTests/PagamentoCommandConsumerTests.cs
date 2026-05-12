using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TicketMaster.Application.Commands.ConfirmarPagamento;
using TicketMaster.Application.Messages;
using TicketMaster.Domain.Common;
using TicketMaster.Web.Consumers;

namespace TicketMaster.IntegrationTests.ConsumerTests;

public sealed class PagamentoCommandConsumerTests
{
    [Fact]
    public async Task Consume_QuandoPagamentoValido_DeveProcessarConfirmacao()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<ConfirmarPagamentoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        await using var provider = new ServiceCollection()
            .AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning))
            .AddSingleton(mediatorMock.Object)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PagamentoCommandConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            var eventId = Guid.NewGuid();
            var usuarioId = Guid.NewGuid();

            // Act — publica um comando de pagamento
            await harness.Bus.Publish(new PagamentoCommand("A1", usuarioId, eventId));

            // Assert — verifica se o consumidor recebeu a mensagem
            var consumed = await harness.Consumed.Any<PagamentoCommand>();
            Assert.True(consumed, "O consumidor deveria ter processado o comando de pagamento.");

            // Verifica se o MediatR foi chamado
            mediatorMock.Verify(
                m => m.Send(It.IsAny<ConfirmarPagamentoCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
