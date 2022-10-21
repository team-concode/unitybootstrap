using System;
using System.Collections.Generic;
using System.Linq;

public class Repository2<V, I> : Repository<V> where V : Table2<I> {
    public bool dirty { get; set; }

    private readonly Dictionary<int, V> table;
    private readonly Dictionary<I, HashSet<V>> idx1Table;
    private int maxPK;

    public Repository2() {
        this.table = new Dictionary<int, V>();
        this.idx1Table = new Dictionary<I, HashSet<V>>();
    }

    public void Create(V item) {
        table.Add(item.pk, item);
        
        if (!idx1Table.ContainsKey(item.index1)) {
            idx1Table[item.index1] = new HashSet<V>();
        }
        idx1Table[item.index1].Add(item);
        
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
    
    public List<V> ReadAllIndex1(I index) {
        if (idx1Table.TryGetValue(index, out HashSet<V> result)) {
            return result.ToList();
        }

        return new List<V>();
    }
    
    public void CreateOrUpdate(V item) {
        table[item.pk] = item;
        this.dirty = true;
    }

    public void Delete(int pk) {
        var item = Read(pk);
        if (item != null) {
            table.Remove(item.pk);
            idx1Table[item.index1].Remove(item);
            this.dirty = true;
        }
    }

    public void DeleteIf(Func<V, bool> check) {
        var all = ReadAll();
        all.ForEach(e => {
            if (check.Invoke(e)) {
                Delete(e.pk);
            }
        });
        
        this.dirty = true;
    }

    public void Clear() {
        table.Clear();
        idx1Table.Clear();
        maxPK = 0;
    }
    
    public int GetMaxPk() {
        return maxPK;
    }

    public List<V> Export() {
        return ReadAll();
    }

    public void Import(List<V> data) {
        Clear();
        if (data != null) {
            data.ForEach(Create);
        }
    }
}