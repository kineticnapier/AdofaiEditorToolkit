using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI.EditorToolkit;

internal static class Program
{
    private static int _passed;

    private static int Main()
    {
        Run("create converts values and enables properties", CreateConvertsValues);
        Run("query and remove", QueryAndRemove);
        Run("action and decoration collections", Collections);
        Run("detached level event scope", DetachedLevelEventScope);
        Run("commit refreshes and preserves selection", CommitRefreshes);
        Run("dispose rolls back", DisposeRollsBack);
        Run("failed commit rolls back", FailedCommitRollsBack);
        Run("snapshots reject a different backend", SnapshotRejectsDifferentBackend);

        Console.WriteLine($"All {_passed} tests passed.");
        return 0;
    }

    private static void CreateConvertsValues()
    {
        var backend = Configure();

        using (var tx = Editor.BeginTransaction())
        {
            var ev = Editor.Events.Create("OrbitDecoration", 12)
                .Set("duration", "1.25")
                .Set("ease", "OutSine")
                .Set("position", EventValues.Vector2(3f, 4f));

            Equal(typeof(float), ev.Get("duration").GetType());
            Equal(1.25f, ev.Get("duration"));
            Equal(FakeEase.OutSine, ev.Get("ease"));
            Equal(new FakeVector2(3f, 4f), ev.Get("position"));
            False(ev.IsDisabled("duration"));
            tx.Commit();
        }

        Equal(1, backend.Level.Events.Count);
    }

    private static void QueryAndRemove()
    {
        var backend = Configure();
        Editor.Events.Create("Twirl", 2);
        Editor.Events.Create("Twirl", 7);
        Editor.Events.Create("SetSpeed", 7);

        Equal(2, Editor.Events.Query("Twirl").Count);
        Equal(2, Editor.Events.Query(floor: 7).Count);
        Equal(1, Editor.Events.Remove("Twirl", 7));
        Equal(2, backend.Level.Events.Count);
    }

    private static void Collections()
    {
        var backend = Configure();
        var action = Editor.Events.Create("Twirl", 1);
        var decoration = Editor.Events.Create("AddObject", 1);

        Equal(EventCollection.Actions, action.Collection);
        Equal(EventCollection.Decorations, decoration.Collection);
        Equal(2, Editor.Events.Query().Count);
        Equal(1, Editor.Events.Query(new EventQuery { Collection = EventCollection.Decorations }).Count);
        Equal(1, backend.Level.Events.Count);
        Equal(1, backend.Level.Decorations.Count);
    }

    private static void DetachedLevelEventScope()
    {
        var backend = Configure();
        var detached = backend.Level.Clone();
        var events = Editor.Events.ForLevel(detached);

        var action = events.Create("Twirl", 3);
        var decoration = events.Create("AddObject", 4);

        Equal(0, backend.Level.Events.Count);
        Equal(0, backend.Level.Decorations.Count);
        Equal(1, detached.Events.Count);
        Equal(1, detached.Decorations.Count);
        Equal(EventCollection.Actions, action.Collection);
        Equal(EventCollection.Decorations, decoration.Collection);
        Equal(1, events.Query("Twirl").Count);
        Equal(1, events.Remove("Twirl", 3));
        Equal(0, detached.Events.Count);
        Equal(1, detached.Decorations.Count);
    }

    private static void CommitRefreshes()
    {
        var backend = Configure();
        backend.Selection.Clear();
        backend.Selection.Add(3);
        backend.Selection.Add(8);

        using (var tx = Editor.BeginTransaction())
        {
            Editor.Events.Create("Twirl", 5);
            backend.Selection.Clear();
            backend.Selection.Add(99);
            tx.Commit();
        }

        Equal(EditorRefreshOptions.All & ~EditorRefreshOptions.RestoreSelection, backend.LastRefresh);
        SequenceEqual(new[] { 99 }, backend.Selection);
    }

    private static void DisposeRollsBack()
    {
        var backend = Configure();
        backend.Selection.Clear();
        backend.Selection.Add(4);

        using (Editor.BeginTransaction())
        {
            Editor.Events.Create("Twirl", 10);
            backend.Selection.Clear();
            backend.Selection.Add(20);
        }

        Equal(0, backend.Level.Events.Count);
        SequenceEqual(new[] { 4 }, backend.Selection);
    }

    private static void FailedCommitRollsBack()
    {
        var backend = Configure();
        backend.Selection.Clear();
        backend.Selection.Add(6);

        var tx = Editor.BeginTransaction();
        Editor.Events.Create("Twirl", 11);
        backend.FailNextRefresh = true;

        Throws<EditorTransactionException>(() => tx.Commit());
        Equal(0, backend.Level.Events.Count);
        SequenceEqual(new[] { 6 }, backend.Selection);
    }

    private static void SnapshotRejectsDifferentBackend()
    {
        Configure();
        var snapshot = Editor.Snapshot();
        Configure();
        Throws<InvalidOperationException>(() => Editor.Restore(snapshot));
    }

    private static FakeEditorBackend Configure()
    {
        var backend = new FakeEditorBackend();
        Editor.Configure(backend);
        return backend;
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            _passed++;
            Console.WriteLine("PASS " + name);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL " + name + ": " + ex);
            Environment.Exit(1);
        }
    }

    private static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new Exception($"Expected {expected}, got {actual}.");
    }

    private static void False(bool value)
    {
        if (value) throw new Exception("Expected false.");
    }

    private static void SequenceEqual(IEnumerable<int> expected, IEnumerable<int> actual)
    {
        if (!expected.SequenceEqual(actual))
            throw new Exception("Sequences differ.");
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        throw new Exception("Expected " + typeof(T).Name + ".");
    }
}

internal sealed class FakeEditorBackend : IEditorBackend
{
    private readonly IEventBackend _events;

    public FakeEditorBackend()
    {
        Level = new FakeLevel();
        Selection = new List<int>();
        var inner = new DelegateEventBackend(
            CreateEvent,
            (value, collection) => Add((FakeEvent)value, collection),
            collection => Enumerate(collection),
            value => Level.Decorations.Contains((FakeEvent)value)
                ? EventCollection.Decorations
                : EventCollection.Actions,
            value => ((FakeEvent)value).Name,
            value => ((FakeEvent)value).Floor,
            (value, key) => ((FakeEvent)value).Types.ContainsKey(key),
            (value, key) => ((FakeEvent)value).Types[key],
            (value, key) => ((FakeEvent)value).Values[key],
            (value, key, property) => ((FakeEvent)value).Values[key] = property,
            (value, key) => ((FakeEvent)value).Disabled[key],
            (value, key, disabled) => ((FakeEvent)value).Disabled[key] = disabled,
            value => Level.Events.Remove((FakeEvent)value) || Level.Decorations.Remove((FakeEvent)value));
        _events = new FakeScopedEventBackend(inner);
    }

    public FakeLevel Level { get; private set; }
    public List<int> Selection { get; }
    public bool FailNextRefresh { get; set; }
    public EditorRefreshOptions LastRefresh { get; private set; }

    public object CurrentEditor => this;
    public object CurrentLevel => Level;
    public IEventBackend Events => _events;

    public object CloneLevel() => Level.Clone();
    public void RestoreLevel(object snapshot) => Level = ((FakeLevel)snapshot).Clone();
    public IReadOnlyList<int> CaptureSelection() => Selection.ToArray();

    public void RestoreSelection(IReadOnlyList<int> selectedFloors)
    {
        Selection.Clear();
        Selection.AddRange(selectedFloors);
    }

    public void Refresh(EditorRefreshOptions options)
    {
        if (FailNextRefresh)
        {
            FailNextRefresh = false;
            throw new InvalidOperationException("Simulated refresh failure.");
        }
        LastRefresh = options;
    }

    private static object CreateEvent(string name, int floor)
    {
        var ev = new FakeEvent(name, floor);
        if (name == "OrbitDecoration")
        {
            ev.Define("duration", typeof(float), 0f);
            ev.Define("ease", typeof(FakeEase), FakeEase.Linear);
            ev.Define("position", typeof(FakeVector2), new FakeVector2(0f, 0f));
        }
        return ev;
    }

    private void Add(FakeEvent value, EventCollection collection)
    {
        if (collection == EventCollection.Auto)
            collection = value.Name == "AddObject"
                ? EventCollection.Decorations
                : EventCollection.Actions;
        GetList(collection).Add(value);
    }

    private List<FakeEvent> GetList(EventCollection collection)
    {
        if (collection == EventCollection.Actions) return Level.Events;
        if (collection == EventCollection.Decorations) return Level.Decorations;
        throw new ArgumentException(collection + " is not a writable collection.", nameof(collection));
    }

    private IEnumerable<object> Enumerate(EventCollection collection)
    {
        if (collection == EventCollection.Actions || collection == EventCollection.All)
            foreach (var ev in Level.Events) yield return ev;
        if (collection == EventCollection.Decorations || collection == EventCollection.All)
            foreach (var ev in Level.Decorations) yield return ev;
    }
}

internal sealed class FakeScopedEventBackend : IEventBackend, ILevelScopedEventBackend
{
    private readonly IEventBackend _inner;

    public FakeScopedEventBackend(IEventBackend inner)
    {
        _inner = inner;
    }

    public object Create(string eventName, int floor) => _inner.Create(eventName, floor);
    public void Add(object levelEvent, EventCollection collection) => _inner.Add(levelEvent, collection);
    public IEnumerable<object> Enumerate(EventCollection collection) => _inner.Enumerate(collection);
    public EventCollection GetCollection(object levelEvent) => _inner.GetCollection(levelEvent);
    public string GetName(object levelEvent) => _inner.GetName(levelEvent);
    public int GetFloor(object levelEvent) => _inner.GetFloor(levelEvent);
    public bool HasProperty(object levelEvent, string key) => _inner.HasProperty(levelEvent, key);
    public Type GetPropertyType(object levelEvent, string key) => _inner.GetPropertyType(levelEvent, key);
    public object GetProperty(object levelEvent, string key) => _inner.GetProperty(levelEvent, key);
    public void SetProperty(object levelEvent, string key, object value) => _inner.SetProperty(levelEvent, key, value);
    public bool IsPropertyDisabled(object levelEvent, string key) => _inner.IsPropertyDisabled(levelEvent, key);
    public void SetPropertyDisabled(object levelEvent, string key, bool disabled) => _inner.SetPropertyDisabled(levelEvent, key, disabled);
    public bool Remove(object levelEvent) => _inner.Remove(levelEvent);

    public void AddToLevel(object level, object levelEvent, EventCollection collection)
    {
        var data = RequireLevel(level);
        var ev = (FakeEvent)levelEvent;
        if (collection == EventCollection.Auto)
            collection = ev.Name == "AddObject" ? EventCollection.Decorations : EventCollection.Actions;
        GetList(data, collection).Add(ev);
    }

    public IEnumerable<object> EnumerateFromLevel(object level, EventCollection collection)
    {
        var data = RequireLevel(level);
        var result = new List<object>();
        if (collection == EventCollection.Actions || collection == EventCollection.All)
            result.AddRange(data.Events.Cast<object>());
        if (collection == EventCollection.Decorations || collection == EventCollection.All)
            result.AddRange(data.Decorations.Cast<object>());
        return result;
    }

    public EventCollection GetCollectionFromLevel(object level, object levelEvent)
    {
        var data = RequireLevel(level);
        var ev = (FakeEvent)levelEvent;
        if (data.Events.Contains(ev)) return EventCollection.Actions;
        if (data.Decorations.Contains(ev)) return EventCollection.Decorations;
        throw new InvalidOperationException("Event is not attached to the requested fake level.");
    }

    public bool RemoveFromLevel(object level, object levelEvent)
    {
        var data = RequireLevel(level);
        var ev = (FakeEvent)levelEvent;
        return data.Events.Remove(ev) || data.Decorations.Remove(ev);
    }

    private static FakeLevel RequireLevel(object level)
    {
        var data = level as FakeLevel;
        if (data == null) throw new ArgumentException("Expected FakeLevel.", nameof(level));
        return data;
    }

    private static List<FakeEvent> GetList(FakeLevel level, EventCollection collection)
    {
        if (collection == EventCollection.Actions) return level.Events;
        if (collection == EventCollection.Decorations) return level.Decorations;
        throw new ArgumentException(collection + " is not a writable collection.", nameof(collection));
    }
}

internal sealed class FakeLevel
{
    public List<FakeEvent> Events { get; } = new List<FakeEvent>();
    public List<FakeEvent> Decorations { get; } = new List<FakeEvent>();

    public FakeLevel Clone()
    {
        var copy = new FakeLevel();
        copy.Events.AddRange(Events.Select(value => value.Clone()));
        copy.Decorations.AddRange(Decorations.Select(value => value.Clone()));
        return copy;
    }
}

internal sealed class FakeEvent
{
    public FakeEvent(string name, int floor)
    {
        Name = name;
        Floor = floor;
    }

    public string Name { get; }
    public int Floor { get; }
    public Dictionary<string, Type> Types { get; } = new Dictionary<string, Type>();
    public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();
    public Dictionary<string, bool> Disabled { get; } = new Dictionary<string, bool>();

    public void Define(string key, Type type, object value)
    {
        Types[key] = type;
        Values[key] = value;
        Disabled[key] = true;
    }

    public FakeEvent Clone()
    {
        var copy = new FakeEvent(Name, Floor);
        foreach (var pair in Types) copy.Types[pair.Key] = pair.Value;
        foreach (var pair in Values) copy.Values[pair.Key] = pair.Value;
        foreach (var pair in Disabled) copy.Disabled[pair.Key] = pair.Value;
        return copy;
    }
}

internal enum FakeEase
{
    Linear,
    OutSine
}

internal readonly struct FakeVector2 : IEquatable<FakeVector2>
{
    public FakeVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; }
    public float Y { get; }
    public bool Equals(FakeVector2 other) => X.Equals(other.X) && Y.Equals(other.Y);
    public override bool Equals(object obj) => obj is FakeVector2 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}
