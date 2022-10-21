using System;
using System.Collections.Generic;

public interface IRepository {
}

public interface Repository<V> : IRepository where V : Table {
    void Create(V item);
    V Read(int pk);
    List<V> ReadAll();
    void CreateOrUpdate(V item);
    void Delete(int pk);
    void DeleteIf(Func<V, bool> check);
    void Clear();
    int GetMaxPk();
    
    void Import(List<V> data);
    List<V> Export();
}
