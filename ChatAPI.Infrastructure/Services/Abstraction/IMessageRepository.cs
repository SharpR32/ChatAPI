using ChatAPI.Domain.Entities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace ChatAPI.Infrastructure.Services.Abstraction;

public interface IMessageRepository
{
    ValueTask<ImmutableArray<Message>> GetMessagesAsync(
        RoomIdentifier identifier,
        DateTimeOffset since,
        CancellationToken cancellationToken,
        int count = 20);
    ValueTask<Guid> SendMessage(Guid receiverId, string content, CancellationToken cancellationToken);
}

public readonly ref struct RoomIdentifier
{
    public RoomIdentifier(params Guid[] participants) : this(participants.Select(x => x.ToString()).ToArray()) { }
    public RoomIdentifier(params string[] participants)
    {
        Identifier = new(() =>
        {
            Array.Sort(participants);
            var pairedParticipants = string.Join(':', participants);
            Span<byte> bytes = Encoding.UTF8.GetBytes(pairedParticipants);
            var result = Convert.ToBase64String(bytes);

            // Removed when applying optimisations
            Debug.WriteLine($"Chat room identifier: {result}");

            return result;
        });
    }

    private readonly string[] _participants = Array.Empty<string>();

    public readonly Lazy<string> Identifier;
}
