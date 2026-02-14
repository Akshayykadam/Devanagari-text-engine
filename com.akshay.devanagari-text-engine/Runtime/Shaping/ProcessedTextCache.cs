// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System.Collections.Generic;

namespace DevanagariText.Shaping
{
    /// <summary>
    /// LRU cache for processed Devanagari text to avoid repeated processing operations.
    /// Thread-safe for concurrent access.
    /// </summary>
    public class ProcessedTextCache
    {
        private readonly Dictionary<string, string> _cache;
        private readonly LinkedList<string> _lruList;
        private readonly object _lock = new object();
        private int _maxSize;
        
        /// <summary>
        /// Gets or sets the maximum number of entries in the cache.
        /// </summary>
        public int MaxCacheSize
        {
            get => _maxSize;
            set
            {
                lock (_lock)
                {
                    _maxSize = value;
                    TrimToSize();
                }
            }
        }
        
        /// <summary>
        /// Gets the current number of entries in the cache.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _cache.Count;
                }
            }
        }
        
        /// <summary>
        /// Creates a new processed text cache with the specified maximum size.
        /// </summary>
        public ProcessedTextCache(int maxSize = 500)
        {
            _maxSize = maxSize;
            _cache = new Dictionary<string, string>(maxSize);
            _lruList = new LinkedList<string>();
        }
        
        /// <summary>
        /// Gets a processed version of the input text, using cache if available.
        /// </summary>
        public string GetOrProcess(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            lock (_lock)
            {
                if (_cache.TryGetValue(input, out string cached))
                {
                    _lruList.Remove(input);
                    _lruList.AddFirst(input);
                    return cached;
                }
            }
            
            string processed = DevanagariTextProcessor.Process(input);
            
            lock (_lock)
            {
                if (!_cache.ContainsKey(input))
                {
                    _cache[input] = processed;
                    _lruList.AddFirst(input);
                    TrimToSize();
                }
            }
            
            return processed;
        }
        
        /// <summary>
        /// Tries to get a cached processed version without processing if not found.
        /// </summary>
        public bool TryGet(string input, out string processed)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(input, out processed))
                {
                    _lruList.Remove(input);
                    _lruList.AddFirst(input);
                    return true;
                }
            }
            
            processed = null;
            return false;
        }
        
        /// <summary>
        /// Adds a processed text entry to the cache.
        /// </summary>
        public void Add(string input, string processed)
        {
            if (string.IsNullOrEmpty(input))
                return;
            
            lock (_lock)
            {
                if (_cache.ContainsKey(input))
                {
                    _cache[input] = processed;
                    _lruList.Remove(input);
                    _lruList.AddFirst(input);
                }
                else
                {
                    _cache[input] = processed;
                    _lruList.AddFirst(input);
                    TrimToSize();
                }
            }
        }
        
        /// <summary>
        /// Clears all entries from the cache.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
                _lruList.Clear();
            }
        }
        
        private void TrimToSize()
        {
            while (_cache.Count > _maxSize && _lruList.Count > 0)
            {
                var oldest = _lruList.Last;
                if (oldest != null)
                {
                    _cache.Remove(oldest.Value);
                    _lruList.RemoveLast();
                }
            }
        }
    }
    
    /// <summary>
    /// Global shared cache instance for common use cases.
    /// </summary>
    public static class GlobalProcessedTextCache
    {
        private static ProcessedTextCache _instance;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// Gets the global shared cache instance.
        /// </summary>
        public static ProcessedTextCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ProcessedTextCache(1000);
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Processes text using the global cache.
        /// </summary>
        public static string Process(string input)
        {
            return Instance.GetOrProcess(input);
        }
        
        /// <summary>
        /// Clears the global cache.
        /// </summary>
        public static void Clear()
        {
            Instance.Clear();
        }
    }
}
