using ChatAPI.Domain.Entities;
using System.Diagnostics;
using System.Text;

namespace ChatAPI.Infrastructure.Services.Abstraction;

public interface IMessageRepository
{
    ValueTask<IEnumerable<Message>> GetMessagesAsync(
        RoomIdentifier identifier,
        DateTimeOffset since,
        CancellationToken cancellationToken,
        int count = 20);
    ValueTask<Guid> SendMessage(Guid receiverId, string content, CancellationToken cancellationToken);
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct RoomIdentifier
{
    public RoomIdentifier(Span<Guid> participants)
    {
        _identifier = InitialiseIdentifier(participants);
    }
    public RoomIdentifier(string[] participants)
    {
        Span<Guid> guids = new Guid[participants.Length];

        for (int index = 0; index < participants.Length; index++)
        {
            var idString = participants[index];
            guids[index] = Guid.Parse(idString);
        }

        _identifier = InitialiseIdentifier(guids);
    }

    private readonly string[] _participants = Array.Empty<string>();

    private byte[] _identifier;
    public string Identifier { get => Convert.ToBase64String(_identifier); }

    private static byte[] InitialiseIdentifier(Span<Guid> participants)
    {
        var result = new byte[16 * participants.Length];
        var buffer = result.AsSpan();

        participants.Sort();

        for (int index = 0; index < participants.Length; index++)
        {
            var identifier = participants[index];
            var bufferTarget = buffer[(index * 16)..];

            identifier.TryWriteBytes(bufferTarget);
        }

        return result;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is RoomIdentifier identifier
            && identifier._identifier.SequenceEqual(_identifier);
    }

    public static bool operator ==(RoomIdentifier left, RoomIdentifier right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoomIdentifier left, RoomIdentifier right)
    {
        return !(left == right);
    }

    private string GetDebuggerDisplay()
    {
        var builder = new StringBuilder();
        Span<byte> identifierSpan = _identifier;

        for (var index = 0; index < _identifier.Length; index++)
        {
            var startIndex = (index * 16);
            var data = identifierSpan.Slice(startIndex, startIndex + 16);
            var guid = new Guid(data);
            builder.Append($"{guid};");
        }

        return builder.ToString();
    }
}
