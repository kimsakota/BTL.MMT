using System;

namespace ET4230.Core.Discovery;

public interface IElapsedClock
{
    TimeSpan Elapsed { get; }
}

public readonly record struct PeerKey(string FileId, string PeerId);

public sealed record PeerEndpoint(string PeerId, string Host, int Port);