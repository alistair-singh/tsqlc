using System;

namespace tsqlc.Util
{
  // Inspired by Rx Disposable
  public sealed class Disposable : IDisposable
  {
    private  Action _action;

    private Disposable() { }

    public static IDisposable From(Action action)
    {
      return new Disposable { _action = action };
    }

    public void Dispose()
    {
      if(_action != null) _action();
    }
  }
}
