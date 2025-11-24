using System.Collections.ObjectModel;

namespace Domain.DTOs;

public class PageDto<T>
{
    public int Offset { get; init; }
    public int Count { get; init; }
    public long Total { get; init; }
    public ICollection<T> Items { get; init; } = [];
}
