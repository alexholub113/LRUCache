namespace LRUCache;

public class LruCacheManager<TKey, TValue>(int cacheMaxSize) : IDisposable
    where TKey : notnull
{
    private readonly IDictionary<TKey, CacheSequenceNode<TKey, TValue>> _cacheRegistry = new Dictionary<TKey, CacheSequenceNode<TKey, TValue>>();
    private CacheSequenceNode<TKey, TValue>? _first;
    private CacheSequenceNode<TKey, TValue>? _last;

    private int _currentCacheSize = 0;


    public bool Contains(TKey key)
    {
        return _cacheRegistry.ContainsKey(key);
    }

    public void Put(TKey key, TValue item)
    {
        if (_currentCacheSize == 0) return;

        lock (_cacheRegistry)
        {
            if (_cacheRegistry.TryGetValue(key, out var node))
            {
                if (_first is null)
                {
                    throw new InvalidOperationException("Cache Registry contains key but first node is not set.");
                }

                node.Value = item;
                if (node == _first && node == _last)
                {
                    return;
                }

                if (node == _last)
                {
                    _last = node.Prev;
                    _last.Next = null;
                }

                if (_first != node)
                {
                    node.RemoveFromSequence();
                    node.SetNext(_first);
                    _first = node;
                }
            }
            else
            {
                if (_currentCacheSize == cacheMaxSize)
                {
                    RemoveLeast();
                }

                var newNode = new CacheSequenceNode<TKey, TValue> { Key = key, Value = item };
                if (_first is not null)
                {
                    newNode.SetNext(_first);
                }
                else
                {
                    _last = newNode;
                }

                _first = newNode;
                _currentCacheSize++;
                _cacheRegistry.Add(key, newNode);
            }
        }
    }

    public TValue? Get(TKey key)
    {
        lock (_cacheRegistry)
        {
            if (_cacheRegistry.TryGetValue(key, out var node))
            {
                if (_first != node)
                {
                    if (_last == node)
                    {
                        _last.Prev.Next = null;
                        _last = _last.Prev;
                    }

                    node.RemoveFromSequence();
                    node.SetNext(_first);
                    _first = node;
                }

                return node.Value;
            }

            return default;
        }
    }

    public bool Delete(TKey key)
    {
        lock (_cacheRegistry)
        {
            if (_cacheRegistry.TryGetValue(key, out var node))
            {
                if (_last == node && _first == node)
                {
                    _last = null;
                    _first = null;
                }
                else if (_first == node)
                {
                    _first.Next.Prev = null;
                    _first = _first.Next;
                }
                else if (_last == node)
                {
                    _last.Prev.Next = null;
                    _last = _last.Prev;
                }

                _cacheRegistry.Remove(key);
                _currentCacheSize--;
                return true;
            }

            return false;
        }
    }

    public void Clear()
    {
        _cacheRegistry.Clear();
        _currentCacheSize = 0;
        _first = null;
        _last = null;
    }

    public void Dispose()
    {
        Clear();
    }

    private void RemoveLeast()
    {
        if (_last == null)
        {
            return;
        }

        _cacheRegistry.Remove(_last.Key);

        if (_first == _last)
        {
            _first = null;
            _last = null;
        }
        else
        {
            if (_last.Prev is null)
            {
                throw new InvalidOperationException("Last.Prev can't be null.");
            }

            _last.Prev.Next = null;
        }

        _currentCacheSize--;
    }
}