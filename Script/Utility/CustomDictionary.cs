using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

/// <summary> 콜백이 존재하는 딕셔너리 </summary>
public class CustomDictionary<TKey, TValeu> : IDictionary<TKey, TValeu>
{
    Dictionary<TKey, TValeu> dic;
    public delegate void OnChangedValue(TKey key, TValeu value);
    public delegate void OnChanged(TKey key);
    public OnChanged onChangedValeu;

    /// <summary> 값이 바뀌기 직전에 호출. value는 바뀌게 될 값을 던져줌. (key, value) </summary>
    public OnChangedValue onPreChangeValue;
    public OnChanged onAdd;
    public OnChanged onRemove;



    /// <summary> 생성 </summary>
    public CustomDictionary()
    {
        dic = new Dictionary<TKey, TValeu>();
    }

    public void Add(TKey key, TValeu value)
    {
        dic.Add(key, value);

        if (onAdd != null)
            onAdd(key);

        if (onPreChangeValue != null)
            onPreChangeValue(key, value);

    }

    public Dictionary<TKey, TValeu>.ValueCollection Values1
    {
        get { return dic.Values; }
    }

    public bool ContainsKey(TKey key)
    {
        return dic.ContainsKey(key);
    }

    public new ICollection<TKey> Keys
    {
        get { return dic.Keys; }
    }

    public bool Remove(TKey key)
    {
        bool resut = dic.Remove(key);
        if (onRemove != null)
            onRemove(key);
        return resut;
    }

    public bool TryGetValue(TKey key, out TValeu value)
    {
        return dic.TryGetValue(key, out value);
    }

    public ICollection<TValeu> Values
    {
        get { return dic.Values; }
    }

    public TValeu this[TKey key]
    {
        get
        {
            return (this.dic[key]);
        }
        set
        {
            if (onPreChangeValue != null)
                onPreChangeValue(key, value);

            this.dic[key] = value;
            if(dic.ContainsKey(key))
            {
                if (onChangedValeu != null)
                    onChangedValeu(key);
            }            
            //Debug.Log("콜백" + key);
            
        }
    }

    private void ThrowItemReadOnlyException()
    {
        throw new NotImplementedException();
    }

    public new void Clear()
    {
        dic.Clear();
    }

    public void Add(KeyValuePair<TKey, TValeu> item)
    {

    }

    public bool Contains(KeyValuePair<TKey, TValeu> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<TKey, TValeu>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<TKey, TValeu> item)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<TKey, TValeu>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int Count
    {
        get { return dic.Count; }
    }

    public bool IsReadOnly
    {
        get
        {
            throw new NotImplementedException();
        }
    }
}
