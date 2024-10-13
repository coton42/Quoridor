using System;
using UnityEngine;

public class IWallCellHandler : MonoBehaviour
{
    public event Action<int, int> Selected;
    private void OnSelected() => Selected?.Invoke(S, T);

    public int S { get; private set; }
    public int T { get; private set; }

    private Transform _wallTransform;
    private Material _wallMat;

    private void Awake()
    {
        S = int.Parse(gameObject.name.Substring(9, 1));
        T = int.Parse(gameObject.name.Substring(10, 1));
        _wallTransform = transform.GetChild(0);
        _wallMat = _wallTransform.GetComponent<Renderer>().material;
        SetTransparency(0f);
    }

    private void OnMouseEnter()
    {
        OnSelected();
    }

    public void ChangeDir()
    {
        _wallTransform.Rotate(0, 90, 0);
    }

    public void SetTransparency(float alpha)
    {
        var color = _wallMat.color;
        color.a = alpha;
        _wallMat.color = color;
    }

    public void Put()
    {
        SetTransparency(1f);
    }
}
