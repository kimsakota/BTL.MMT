using System;
using System.Collections.Generic;
using Xunit; 
using ET4230.Core.Discovery;

namespace ET4230.Core.Tests.Discovery;

// 1. Tạo Đồng hồ giả (Fake Clock) cho phép nhảy cóc thời gian
public sealed class FakeClock : IElapsedClock
{
    public TimeSpan Elapsed { get; set; }
    public void Advance(TimeSpan delta) => Elapsed += delta;
}

public class PeerRegistryTests
{
    [Fact]
    public void Register_CungPeerId_HaiFileId_KhongDeNhau()
    {
        var clock = new FakeClock();
        var registry = new PeerRegistry(clock, TimeSpan.FromSeconds(20), 10);
        
        registry.Register("file1", new PeerEndpoint("peerA", "192.168.1.1", 42301));
        registry.Register("file2", new PeerEndpoint("peerA", "192.168.1.1", 42302));

        // Khóa kép giúp 2 file độc lập, List ra phải có đủ[cite: 3]
        Assert.Single(registry.ListActive("file1"));
        Assert.Single(registry.ListActive("file2"));
    }

    [Fact]
    public void Heartbeat_HetHanDungBien_KhongTuTaoMoi()
    {
        var clock = new FakeClock();
        var registry = new PeerRegistry(clock, TimeSpan.FromSeconds(20), 10);
        registry.Register("file1", new PeerEndpoint("peerA", "192.168.1.1", 42301));

        // Nhảy vọt 20 giây, tới đúng biên age >= TTL[cite: 3, 4]
        clock.Advance(TimeSpan.FromSeconds(20)); 
        var result = registry.Heartbeat("file1", "peerA");

        // Bị từ chối, xóa khỏi danh sách, không tự tạo peer ảo[cite: 3]
        Assert.False(result); 
        Assert.Empty(registry.ListActive("file1"));
    }

    [Fact]
    public void Register_IPWildcard_BiTuChoi()
    {
        var clock = new FakeClock();
        var registry = new PeerRegistry(clock, TimeSpan.FromSeconds(20), 10);
        var ep = new PeerEndpoint("peerA", "0.0.0.0", 42301);

        // Ném lỗi ngay khi thấy IP 0.0.0.0[cite: 2]
        Assert.Throws<ArgumentException>(() => registry.Register("file1", ep));
    }

    [Fact]
    public void ListActive_TraVeSnapshot_KhongSuaDuocGoc()
    {
        var clock = new FakeClock();
        var registry = new PeerRegistry(clock, TimeSpan.FromSeconds(20), 10);
        registry.Register("file1", new PeerEndpoint("peerA", "1.1.1.1", 42301));

        // Lấy danh sách ra rồi cố tình xóa sạch[cite: 1]
        var list = (List<PeerEndpoint>)registry.ListActive("file1");
        list.Clear(); 

        // Dữ liệu gốc trong Registry vẫn còn nguyên vẹn 1 phần tử[cite: 1, 3]
        Assert.Single(registry.ListActive("file1"));
    }
}