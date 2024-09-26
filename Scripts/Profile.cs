using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable enable

// Simple class that can be inserted in specific code under investigation to output simplistic profiling data
class Profile : IDisposable
{
    private int _threadId;
    private string _fullName;
    private string? _appendName;
    private readonly Profile? _parent = null;
    private Stopwatch? _stopwatch;

    static object _lockA = new object();
    static object _lockB = new object();
    static Dictionary<int, Profile> _activeProfiles = new();
    static Dictionary<Tuple<int, string>, long> _milliseconds = new();
    static Dictionary<Tuple<int, string>, long> _instances = new();

    static internal void Dump()
    {
        lock (_lockB)
        {
            foreach (var threadId in _milliseconds.Keys.Select(a => a.Item1).Distinct())
            {
                Tuple<string, long>[] threadSpecificMilliseconds = _milliseconds.Where(a => a.Key.Item1 == threadId).Select(a => Tuple.Create(a.Key.Item2, a.Value)).OrderBy(a => a.Item2).ToArray();
                long totalMS = threadSpecificMilliseconds.Sum(a => a.Item2);
                Debug.Print($"Thread #{threadId}");
                foreach (Tuple<string, long> entry in threadSpecificMilliseconds)
                {
                    long count = _instances.Where(a => a.Key.Item1 == threadId && a.Key.Item2 == entry.Item1).Select(a => a.Value).First();
                    Debug.Print($"{100.0 * entry.Item2 / totalMS}%  count={count}  {entry.Item1}({entry.Item2}ms)");
                }
            }
        }
    }

    internal void AppendName(string v)
    {
        _appendName = v;
    }

    internal Profile(string name)
    {
        _threadId = System.Environment.CurrentManagedThreadId;
        lock (_lockA)
        {
            if (_activeProfiles.TryGetValue(_threadId, out Profile? currentProfile))
            {
                _parent = currentProfile;
                _fullName = $"{_parent._fullName}/{name}";
                if (_parent._stopwatch!.IsRunning == false)
                {
                    Debug.Print($"Can't create profile child({name}) on paused parent({_parent._fullName})");
                }
            }
            else
            {
                _parent = null;
                _fullName = name;
            }

            _activeProfiles[_threadId] = this;
        }

        _stopwatch = Stopwatch.StartNew();
    }

    internal void Pause()
    {
        _stopwatch!.Stop();
    }
    internal void Resume()
    {
        _stopwatch!.Start();
    }

    public void Dispose()
    {
        if (_stopwatch == null)
            return;

        _stopwatch.Stop();
        Tuple<int, string> key = (_appendName == null) ? Tuple.Create(_threadId, _fullName) : Tuple.Create(_threadId, $"{_fullName}{_appendName}");
        lock (_lockB)
        {
            if (_milliseconds.TryGetValue(key, out long total))
            {
                _milliseconds[key] = total + _stopwatch.ElapsedMilliseconds;
                _instances[key] += 1;
            }
            else
            {
                _milliseconds[key] = _stopwatch.ElapsedMilliseconds;
                _instances[key] = 1;
            }
        }

        if (_parent == null)
        {
            _activeProfiles.Remove(_threadId);
        }
        else
        {
            _activeProfiles[_threadId] = _parent;
        }
    }
}
