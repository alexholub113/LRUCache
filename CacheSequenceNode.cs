namespace LRUCache;

internal class CacheSequenceNode<TKey, TValue> where TKey : notnull
{
    public required TKey Key { get; set; }

    public required TValue Value { get; set; }

    public CacheSequenceNode<TKey, TValue>? Prev { get; set; }

    public CacheSequenceNode<TKey, TValue>? Next { get; set; }

    public void RemoveFromSequence()
    {
        if (Prev is not null && Next is not null)
        {
            Prev.Next = Next;
            Next.Prev = Prev;
        }
        else
        {
            if (Prev is not null)
            {
                Prev.Next = null;
            }

            if (Next is not null)
            {
                Next.Prev = null;
            }
        }

        Prev = null;
        Next = null;
    }

    public void SetNext(CacheSequenceNode<TKey, TValue> next)
    {
        Next = next;
        Next.Prev = this;
    }
}