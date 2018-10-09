using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomList<T> : List<T> {
    //public event EventHandler OnAdd;

    public delegate void ListDelegate(T item);

    /// <summary> 리스트에 새로운 항목이 추가된 직후 발생. 추가된 item을 반환함 </summary>
    public ListDelegate onAdd;

    /// <summary> 리스트 항목이 제거되기 직전에 발생. 제거될 item을 반환함 </summary>
    public ListDelegate onRemove;

    /// <summary> 리스트에서 항목이 제거된 후 발생. 제거된 item을 반환함 </summary>
    public ListDelegate onRemovePost;

    /// <summary> 리스트가 클리어 될 때. clear직전에 발생 </summary>
    public SimpleDelegate onClear;

    public void Add(T item)
    {
        base.Add(item);

        if (null != onAdd)
            onAdd(item);
    }

    public void Remove(T item)
    {
        if (Contains(item) && null != onRemove)
            onRemove(item);

        bool successToRemove = base.Remove(item);

        if (successToRemove && onRemovePost != null)
            onRemovePost(item);
    }

    public void RemoveAt(int index)
    {
        T item = this[index];

        if (null != onRemove)
            onRemove(item);

        base.RemoveAt(index);

        if (onRemovePost != null)
            onRemovePost(item);
    }


    public void Clear()
    {
        if (null != onClear)
            onClear();

        base.Clear();


    }
}
