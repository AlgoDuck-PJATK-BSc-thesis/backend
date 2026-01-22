using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Shared.Http.Result2;
using OneOf;

public readonly struct ErrorUnion<T1, T2> : IToActionResultable
    where T1 : IToActionResultable
    where T2 : IToActionResultable
{
    private readonly OneOf<T1, T2> _inner;
    
    private ErrorUnion(OneOf<T1, T2> inner) => _inner = inner;
    
    public static implicit operator ErrorUnion<T1, T2>(T1 t) => new(t);
    public static implicit operator ErrorUnion<T1, T2>(T2 t) => new(t);
    
    public bool IsErr1 => _inner.IsT0;
    public bool IsErr2 => _inner.IsT1;
    
    public T1? AsErr1 => _inner.IsT0 ? _inner.AsT0 : default;
    public T2? AsErr2 => _inner.IsT1 ? _inner.AsT1 : default;
    
    public IActionResult ToActionResult(string? message = null) => ((IToActionResultable)_inner.Value).ToActionResult(message);
    
    public TResult Match<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2) =>
        _inner.Match(f1, f2);
}

public readonly struct ErrorUnion<T1, T2, T3> : IToActionResultable
    where T1 : IToActionResultable
    where T2 : IToActionResultable
    where T3 : IToActionResultable
{
    private readonly OneOf<T1, T2, T3> _inner;
    
    private ErrorUnion(OneOf<T1, T2, T3> inner) => _inner = inner;
    
    public static implicit operator ErrorUnion<T1, T2, T3>(T1 t) => new(t);
    public static implicit operator ErrorUnion<T1, T2, T3>(T2 t) => new(t);
    public static implicit operator ErrorUnion<T1, T2, T3>(T3 t) => new(t);
    
    public bool IsErr1 => _inner.IsT0;
    public bool IsErr2 => _inner.IsT1;
    public bool IsErr3 => _inner.IsT2;
    
    public T1? AsErr1 => _inner.IsT0 ? _inner.AsT0 : default;
    public T2? AsErr2 => _inner.IsT1 ? _inner.AsT1 : default;
    public T3? AsErr3 => _inner.IsT2 ? _inner.AsT2 : default;
    
    public IActionResult ToActionResult(string? message = null) => ((IToActionResultable)_inner.Value).ToActionResult(message);
    
    public TResult Match<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2, Func<T3, TResult> f3) =>
        _inner.Match(f1, f2, f3);
}


public readonly struct ErrorUnion<T1, T2, T3, T4> : IToActionResultable
    where T1 : IToActionResultable
    where T2 : IToActionResultable
    where T3 : IToActionResultable
    where T4 : IToActionResultable
{
    private readonly OneOf<T1, T2, T3, T4> _inner;
    
    private ErrorUnion(OneOf<T1, T2, T3, T4> inner) => _inner = inner;
    
    public static implicit operator ErrorUnion<T1, T2, T3, T4>(T1 t) => new(t);
    public static implicit operator ErrorUnion<T1, T2, T3, T4>(T2 t) => new(t);
    public static implicit operator ErrorUnion<T1, T2, T3, T4>(T3 t) => new(t);
    public static implicit operator ErrorUnion<T1, T2, T3, T4>(T4 t) => new(t);
    
    public bool IsErr1 => _inner.IsT0;
    public bool IsErr2 => _inner.IsT1;
    public bool IsErr3 => _inner.IsT2;
    public bool IsErr4 => _inner.IsT3;
    
    public T1? AsErr1 => _inner.IsT0 ? _inner.AsT0 : default;
    public T2? AsErr2 => _inner.IsT1 ? _inner.AsT1 : default;
    public T3? AsErr3 => _inner.IsT2 ? _inner.AsT2 : default;
    public T4? AsErr4 => _inner.IsT3 ? _inner.AsT3 : default;
    
    public IActionResult ToActionResult(string? message = null) => ((IToActionResultable)_inner.Value).ToActionResult(message);
    
    public TResult Match<TResult>(Func<T1, TResult> f1, Func<T2, TResult> f2, Func<T3, TResult> f3, Func<T4, TResult> f4) =>
        _inner.Match(f1, f2, f3, f4);
}
