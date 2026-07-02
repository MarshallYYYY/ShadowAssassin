using UnityEngine;
using UnityEngine.EventSystems;

public class RotateModel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform model;
    [SerializeField] private Camera characterShowCamera;
    #region 打开菜单时摄像机跳到正面 / 关闭时恢复模型旋转
    /// <summary>
    /// 打开菜单时的模型，关闭时恢复
    /// </summary>
    private Vector3 originEulerAngle;
    /// <summary>
    /// 摄像机在模型本地空间的偏移位置
    /// </summary>
    private Vector3 localCameraPositionOffset;
    /// <summary>
    /// 摄像机在模型本地空间的偏移旋转
    /// </summary>
    private Quaternion localCameraRotationOffset;

    public void Init()
    {
        model = GameObject.FindWithTag("Player").transform;
        // if (previewCamera == null)
        // {
        //     previewCamera = GameObject.FindWithTag("CharacterShowCamera").GetComponent<Camera>();
        // }

        // 将你在场景中摆好的摄像机位置/旋转，转成模型本地空间偏移（之后随模型朝向自动适配）

        // 把摄像机在世界空间里的坐标，转成"以模型为原点、以模型朝向为坐标轴"的本地坐标。
        // 这个本地坐标不随模型转向而变——不管模型面朝哪，"正后方 5 米、上方 2 米"这个相对关系永远是对的。
        // A. 世界摄像机位置 → InverseTransformPoint → 本地偏移 (存起来)
        localCameraPositionOffset = model.InverseTransformPoint(characterShowCamera.transform.position);

        // 模型的"反旋转"。用反旋转去乘摄像机的世界旋转，得到摄像机的本地旋转。
        // B. 世界摄像机旋转 → Inverse(模型) × 摄像机 → 本地旋转 (存起来)
        localCameraRotationOffset = Quaternion.Inverse(model.rotation) * characterShowCamera.transform.rotation;
    }

    void OnEnable()
    {
        // ① 保存打开菜单时模型的当前朝向（关闭菜单时恢复到此朝向）
        originEulerAngle = model.eulerAngles;

        // ② 摄像机跳到模型正前方的固定偏移位置（无论模型此刻面朝哪）
        // C. 本地偏移 → TransformPoint → 新世界位置 (自动适配新朝向)
        // D. 本地旋转 → 模型.rotation × 本地旋转 → 新世界旋转
        // E. SetPositionAndRotation 一次性应用
        characterShowCamera.transform.SetPositionAndRotation(
            // 把第一步存的本地偏移，按模型当前的世界位置和朝向，重新算回世界坐标。
            // 这就是"无论模型面朝哪，摄像机永远在模型的同一个相对位置上"。
            model.TransformPoint(localCameraPositionOffset),
            model.rotation * localCameraRotationOffset);
    }

    void OnDisable()
    {
        // 关闭菜单：模型回到打开菜单前的旋转角度
        model.eulerAngles = originEulerAngle;
    }
    #endregion

    #region 拖拽旋转模型
    /// <summary>
    /// 拖拽开始时的鼠标位置
    /// </summary>
    private Vector3 startDragMousePosition;
    /// <summary>
    /// 拖拽开始时的模型欧拉角（基值）
    /// </summary>
    private Vector3 startDragModelEulerAngle;
    [SerializeField] private float RotateScale = 1f;

    public void OnBeginDrag(PointerEventData eventData)
    {
        startDragMousePosition = Input.mousePosition;
        startDragModelEulerAngle = model.eulerAngles;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaX = startDragMousePosition.x - Input.mousePosition.x;
        model.eulerAngles = startDragModelEulerAngle + new Vector3(0, RotateScale * deltaX, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }
    #endregion
}