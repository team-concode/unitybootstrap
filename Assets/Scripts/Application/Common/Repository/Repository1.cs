using System;
using System.Collections.Generic;
using System.Linq;

public class Repository1<V> : Repository<V> where V : Table {
    public bool dirty { get; set; }

    private readonly Dictionary<int, V> table;
    private int maxPK;

    public Repository1() {
        this.table = new Dictionary<int, V>();
    }

    public void Create(V item) {
        table.Add(item.pk, item);
        if (maxPK < item.pk) {
            maxPK = item.pk;
        }
        this.dirty = true;
    }
    
    public V Read(int pk) {
        table.TryGetValue(pk, out V dao);
        return dao;
    }
    
    public List<V> ReadAll() {
        return table.Values.ToList();
    }

    public void CreateOrUpdate(V item) {
        table[item.pk] = item;
        this.dirty = true;
    }

    public void Delete(int pk) {
        table.Remove(pk);
        this.dirty = true;
    }

    public void DeleteIf(Func<V, bool> check) {
        var all = GetAll();
        all.ForEach(e => {
            if (check.Invoke(e)) {
                Delete(e.pk);
            }
        });
    }

    public int GetMaxPk() {
        return maxPK;
    }

    public List<V> GetAll() {
        return table.Values.ToList();
    } 

    public void Clear() {
        table.Clear();
        maxPK = 0;
    }
    
    public List<V> Export() {
        return ReadAll();
    }

    public void Import(List<V> data) {
        Clear();
        data?.ForEach(Create);
    }
}