using System.Threading.Channels;

namespace e360_clone_api.Services
{
    public interface IExamNotificationQueue
    {
        void Enqueue(ExamNotificationMessage message);
        ValueTask<ExamNotificationMessage> DequeueAsync(CancellationToken cancellationToken);
    }

    public sealed class ExamNotificationQueue : IExamNotificationQueue
    {
        private readonly Channel<ExamNotificationMessage> _channel =
            Channel.CreateUnbounded<ExamNotificationMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        public void Enqueue(ExamNotificationMessage message)
        {
            if (!_channel.Writer.TryWrite(message))
            {
                throw new InvalidOperationException("Unable to enqueue exam notification.");
            }
        }

        public ValueTask<ExamNotificationMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }

    public sealed record ExamNotificationMessage(int ExamId, DateTime CreatedAtUtc);
}
