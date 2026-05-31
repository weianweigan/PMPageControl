using System;
using System.Collections.Generic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf
{

/// <summary>
/// 选择类型和选择对象的键值对
/// </summary>
public class SwSeleTypeObjectPair : IEqualityComparer<SwSeleTypeObjectPair>
{
    public SwSeleTypeObjectPair(
        int index,
        swSelectType_e selectType,
        int mark,
        object selectedObject,
        string name,
        double[] point
    )
    {
        SelectType = selectType;
        Mark = mark;
        Index = index;
        Point = point;
        SelectedObject = selectedObject;
        Name = name;
    }

    /// <summary>
    /// 序号
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// 标记
    /// </summary>
    public int Mark { get; private set; }

    /// <summary>
    /// 选择类型
    /// </summary>
    public swSelectType_e SelectType { get; private set; }

    /// <summary>
    /// 选择对象
    /// </summary>
    public object SelectedObject { get; private set; }

    /// <summary>
    /// 在列表中显示的名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 用来追踪用的标记
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// 用户选择的点
    /// </summary>
    public double[] Point { get; set; }

    /// <summary>
    /// 用来临时存储PID的属性，默认为null
    /// </summary>
    public string PID { get; set; }

    public swPersistReferencedObjectStates_e ReSolveFormPID(IModelDoc2 doc)
    {
        if (string.IsNullOrEmpty(PID))
        {
            throw new ArgumentNullException("请先对属性 PID 赋值");
        }

        var byteId = Convert.FromBase64String(PID);

        SelectedObject = doc.Extension.GetObjectByPersistReference3(byteId, out int errorCode);
        return (swPersistReferencedObjectStates_e)errorCode;
    }

    public bool Equals(SwSeleTypeObjectPair x, SwSeleTypeObjectPair y)
    {
        return x?.Name == y?.Name;
    }

    public int GetHashCode(SwSeleTypeObjectPair obj)
    {
        return obj?.Name.GetHashCode() ?? -1;
    }

    /// <summary>
    /// 在边面顶点的时候，为了保存对象的持久引用，防止引发 <see cref="System.Runtime.InteropServices.COMException"/>被调用的对象已与其客户端断开连接
    /// </summary>
    public void GetSafeEntity()
    {
        if (
            SelectType == swSelectType_e.swSelEDGES
            || SelectType == swSelectType_e.swSelFACES
            || SelectType == swSelectType_e.swSelVERTICES
        )
        {
            var entity = SelectedObject as IEntity;
            if (!entity.IsSafe)
                SelectedObject = entity.GetSafeEntity();
        }
    }

    public override string ToString()
    {
        return Name ?? base.ToString();
    }
}
}
