using System;
using System.Collections.Generic;

namespace ET4230.Core.Discovery;

public sealed class PeerRegistry
{
    private sealed record RegistryEntry(PeerEndpoint Endpoint, TimeSpan LastSeen);

    private readonly Dictionary<PeerKey, RegistryEntry> _entries = new();
    private readonly IElapsedClock _clock;
    private readonly TimeSpan _ttl;
    private readonly int _maxEntries;

    public PeerRegistry(IElapsedClock clock, TimeSpan ttl, int maxEntries)
    {
        if (ttl <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(ttl));
        if (maxEntries <= 0) throw new ArgumentOutOfRangeException(nameof(maxEntries));

        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _ttl = ttl;
        _maxEntries = maxEntries;
    }

    // Các hàm Register, Heartbeat, ListActive sẽ được viết tại đây
    public bool Register(string fileId, PeerEndpoint endpoint)
    {
    // 1. Chặn input sai quy tắc từ hợp đồng (TC05)
    if (string.IsNullOrWhiteSpace(fileId)) throw new ArgumentException("FileId không hợp lệ");
    if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
    if (endpoint.Port <= 0 || endpoint.Port > 65535) throw new ArgumentException("Port ngoài dải hợp lệ");
    if (endpoint.Host == "0.0.0.0") throw new ArgumentException("Không được công bố IP Wildcard 0.0.0.0"); //[cite: 2]

    var now = _clock.Elapsed;
    var key = new PeerKey(fileId, endpoint.PeerId);

    // 2. Dọn dẹp các peer đã hết hạn trước khi xét sức chứa (age >= TTL)
    var expiredKeys = new List<PeerKey>();
    foreach (var entry in _entries)
    {
        if (now - entry.Value.LastSeen >= _ttl)
        {
            expiredKeys.Add(entry.Key);
        }
    }
    foreach (var k in expiredKeys) _entries.Remove(k);

    // 3. Cập nhật (TC02) hoặc Thêm mới (TC06)
    if (_entries.ContainsKey(key))
    {
        // Đã tồn tại: ghi đè endpoint mới và làm mới thời gian
        _entries[key] = new RegistryEntry(endpoint, now);
        return true;
    }
    else
    {
        // Chưa tồn tại: kiểm tra sức chứa trước khi cho vào
        if (_entries.Count >= _maxEntries) return false; // Bị từ chối do đầy capacity
        
        _entries.Add(key, new RegistryEntry(endpoint, now));
        return true;
    }
    }
    public bool Heartbeat(string fileId, string peerId)
    {
        var now = _clock.Elapsed;
        var key = new PeerKey(fileId, peerId);

        if (_entries.TryGetValue(key, out var entry))
        {
            // Kiểm tra biên hết hạn (age >= TTL)
            if (now - entry.LastSeen >= _ttl)
            {
                _entries.Remove(key); // Dọn dẹp rác luôn
                return false; 
            }
            
            // Còn hiệu lực -> Cập nhật mốc thời gian báo sống
            _entries[key] = new RegistryEntry(entry.Endpoint, now);
            return true;
        }

        // Khóa lạ chưa từng Register -> Không tự tạo peer ảo, trả về false[cite: 3]
        return false;
    }

    public IReadOnlyList<PeerEndpoint> ListActive(string fileId)
    {
        var now = _clock.Elapsed;
        var activePeers = new List<PeerEndpoint>();

        foreach (var kvp in _entries)
        {
            // Chỉ lấy peer của đúng fileId này và chưa hết hạn (age < TTL)[cite: 3]
            if (kvp.Key.FileId == fileId && (now - kvp.Value.LastSeen < _ttl))
            {
                activePeers.Add(kvp.Value.Endpoint);
            }
        }

        // Trả về một List mới (snapshot). Caller có gọi list.Clear() 
        // thì dữ liệu _entries gốc cũng không bị ảnh hưởng (ngắt alias)
        return activePeers;
    }
}