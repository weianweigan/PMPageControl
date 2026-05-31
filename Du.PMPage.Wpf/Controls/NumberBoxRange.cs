using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

public class NumberBoxRange
{
    /// <summary>
    /// 单位
    /// </summary>
    public swNumberboxUnitType_e Units { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    public double Minimum { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public double Maximum { get; set; }

    /// <summary>
    /// true表示包括此范围，false表示在此范围外
    /// </summary>
    public bool Inclusive { get; set; } = true;

    /// <summary>
    /// 每次增长的值
    /// </summary>
    public double Increment { get; set; } = 1;

    /// <summary>
    /// 鼠标快速滚动增长的值
    /// </summary>
    public double FastIncr { get; set; } = 1;

    /// <summary>
    /// 鼠标慢速滚动增长的值
    /// </summary>
    public double SlowIncr { get; set; } = 1;
}
