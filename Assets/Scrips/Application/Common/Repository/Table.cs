using System;

public interface Table {
    int pk { get; }
}

public interface Table2<T> : Table {
    T index1 { get; }
}

public interface Table3<T, U> : Table {
    T index1 { get; }
    U index2 { get; }
}
