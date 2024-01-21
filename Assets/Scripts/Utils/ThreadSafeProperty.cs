namespace HologramPipe
{
  public class ThreadSafeProperty<T>
  {
    private object _locker = new();
    private T _value;

    public T value
    {
      get
      {
        lock (_locker)
        {
          return _value;
        }
      }
      set
      {
        lock (_locker)
        {
          _value = value;
        }
      }
    }
  }
}

