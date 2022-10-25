public delegate void EventBusHandler();
public delegate void EventBusHandlerT1<T1>(T1 param1);
public delegate void EventBusHandlerT2<T1, T2>(T1 param1, T2 param2);
public delegate void EventBusHandlerT3<T1, T2, T3>(T1 param1, T2 param2, T3 param3);

public class EventBusTopic {
    public event EventBusHandler handler;

    public void Fire() {
        handler?.Invoke();
    }
}

public class EventBusTopic<T> {
    public event EventBusHandlerT1<T> handler;

    public void Fire(T param) {
        handler?.Invoke(param);
    }
}

public class EventBusTopic<T1, T2> {
    public event EventBusHandlerT2<T1, T2> handler;

    public void Fire(T1 param1, T2 param2) {
        handler?.Invoke(param1, param2);
    }
}

public class EventBusTopic<T1, T2, T3> {
    public event EventBusHandlerT3<T1, T2, T3> handler;

    public void Fire(T1 param1, T2 param2, T3 param3) {
        handler?.Invoke(param1, param2, param3);
    }
}