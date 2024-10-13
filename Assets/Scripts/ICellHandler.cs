using System;
using UnityEngine;

public class ICellHandler : MonoBehaviour
{
    public event Action<int, int> Clicked;
    private void OnClicked() => Clicked?.Invoke(X, Y);

    public int X { get; private set; }
    public int Y { get; private set; }

    [SerializeField] private Material _highlightedMat;

    private bool _isAccessible;
    private Material _regularMat;
    private Renderer _renderer;
    private Light _light;

    private void Awake()
    {
        X = int.Parse(gameObject.name.Substring(4, 1));
        Y = int.Parse(gameObject.name.Substring(5, 1));
        _isAccessible = false;
        _renderer = GetComponent<Renderer>();
        _regularMat = _renderer.material;
        _light = transform.GetChild(0).GetComponent<Light>();
        _light.enabled = false;
    }

    private void OnMouseEnter()
    {
        if (_isAccessible)
        {
            _renderer.material = _highlightedMat;
        }
    }

    private void OnMouseExit()
    {
        _renderer.material = _regularMat;
    }

    private void OnMouseUp()
    {
        if (_isAccessible)
        {
            _renderer.material = _regularMat;
            OnClicked();
        }
    }

    public void Activate(Color color)
    {
        _isAccessible = true;
        _light.enabled = true;
        _light.color = color;
    }

    public void Inactivate()
    {
        _isAccessible = false;
        _light.enabled = false;
    }
}
