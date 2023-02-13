using System.Collections;

namespace aeglite;

public class CircularBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _startIndex;

    public CircularBuffer(int size)
    {
        _buffer = new T[size];
    }

    public int Count { get; private set; }

    private int GetOffset(int index)
        => (_startIndex + index) % _buffer.Length;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _buffer[GetOffset(index)];
        }
    }

    public void Append(T item)
    {
        _buffer[GetOffset(Count)] = item;
        if (Count == _buffer.Length)
        {
            _startIndex = GetOffset(1);
        }
        else
        {
            Count++;
        }
    }

    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _startIndex = 0;
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _buffer[GetOffset(i)];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}