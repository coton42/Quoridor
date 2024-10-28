using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IWallCellHandler : MonoBehaviour
{
    public event Action<int, int> Selected;

    public int S { get; private set; }
    public int T { get; private set; }

    private Transform _wallTransform;
    private Material _wallMat;

    public void ChangeDir()
    {
        _wallTransform.Rotate(0, 90, 0);
    }

    public void Deselect()
    {
        SetTransparency(0f);
    }

    public void Put()
    {
        SetTransparency(1f);
        Destroy(this);
    }

    public void SetActivation(bool isActivated)
    {
        this.enabled = isActivated;
    }

    private void Awake()
    {
        S = int.Parse(gameObject.name.Substring(9, 1));
        T = int.Parse(gameObject.name.Substring(10, 1));
        _wallTransform = transform.Find("Wall");
        _wallMat = _wallTransform.GetComponent<Renderer>().material;
        SetTransparency(0f);
    }

    private void OnMouseEnter()
    {
        // ポインターがGUI上（確定ボタンと回転ボタン）にあるときはreturnする
        #if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject()) return;
        #else
            // if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) でうまくいかない
            // 参考: https://discussions.unity.com/t/ispointerovereventsystemobject-always-returns-false-on-mobile/548758

            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Input.GetTouch(0).position;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            if (results.Count > 0) return;
        #endif

        if (this.enabled)
        {
            SetTransparency(.4f);
            Selected?.Invoke(S, T);
        }
    }

    private void SetTransparency(float alpha)
    {
        var color = _wallMat.color;
        color.a = alpha;
        _wallMat.color = color;
    }
}
