using UnityEngine;
using UnityEngine.EventSystems;

public class RotateModel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Transform model;
    private bool isRotate;
    private Vector3 startPoint;
    private Vector3 startAngle;
    [SerializeField] private float RotateScale = 1f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isRotate = true;
        startPoint = Input.mousePosition;
        startAngle = model.eulerAngles;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRotate)
        {
            Vector3 currentPoint = Input.mousePosition;
            float x = startPoint.x - currentPoint.x;
            model.eulerAngles = startAngle + new Vector3(0, RotateScale * x, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isRotate = false;
    }

    #region 恢复玩家的旋转角度
    private Vector3 originAngle;
    void Awake()
    {
        originAngle = model.transform.eulerAngles;
    }
    void OnDisable()
    {
        model.eulerAngles = originAngle;
    }
    #endregion
}