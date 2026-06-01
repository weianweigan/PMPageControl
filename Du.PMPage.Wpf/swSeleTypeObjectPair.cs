using System;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf;


/// <summary>
/// Key-value pair combining a selection type and the selected object.
/// </summary>
public class SwSeleTypeObjectPair : IEquatable<SwSeleTypeObjectPair>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwSeleTypeObjectPair"/> class.
    /// </summary>
    /// <param name="index">The ordinal position in the selection list.</param>
    /// <param name="selectType">The type of the selected entity.</param>
    /// <param name="mark">The SolidWorks selection mark.</param>
    /// <param name="selectedObject">The native SolidWorks object.</param>
    /// <param name="name">The display name for the selection list UI.</param>
    /// <param name="point">The pick point coordinates.</param>
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
    /// The ordinal position of this selection in the selection list.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The SolidWorks selection mark identifying this selection entry.
    /// </summary>
    public int Mark { get; private set; }

    /// <summary>
    /// The type of the selected entity (e.g. edge, face, vertex, component).
    /// </summary>
    public swSelectType_e SelectType { get; private set; }

    /// <summary>
    /// The native SolidWorks object representing the selected entity.
    /// </summary>
    public object SelectedObject { get; private set; }

    /// <summary>
    /// The display name shown in the selection list UI.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// An arbitrary tag for tracking or user-defined data.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// The pick point where the user clicked to make the selection.
    /// </summary>
    public double[] Point { get; set; }

    /// <summary>
    /// Temporary storage for a persist reference ID (Base64-encoded).
    /// Set this before calling <see cref="ResolveFromPID"/> to restore a
    /// persisted selection reference.
    /// </summary>
    public string PID { get; set; }

    /// <summary>
    /// Resolves the <see cref="PID"/> into a live SolidWorks object and updates
    /// <see cref="SelectedObject"/>.
    /// </summary>
    /// <param name="doc">The model document used to resolve the persist reference.</param>
    /// <returns>The resolution status code.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="PID"/> is null or empty.
    /// </exception>
    public swPersistReferencedObjectStates_e ResolveFromPID(IModelDoc2 doc)
    {
        if (string.IsNullOrEmpty(PID))
        {
            throw new ArgumentNullException(
                nameof(PID),
                "The PID property must be set before calling ResolveFromPID.");
        }

        var byteId = Convert.FromBase64String(PID);

        SelectedObject = doc.Extension.GetObjectByPersistReference3(byteId, out int errorCode);
        return (swPersistReferencedObjectStates_e)errorCode;
    }

    /// <summary>
    /// For edge, face, or vertex selections, replaces <see cref="SelectedObject"/>
    /// with a safe (persist-capable) copy of the entity to avoid
    /// <see cref="System.Runtime.InteropServices.COMException"/>
    /// ("The called object has been disconnected from its clients").
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
            if (entity != null && !entity.IsSafe)
                SelectedObject = entity.GetSafeEntity();
        }
    }

    #region Equality — compared by Name

    /// <inheritdoc />
    public bool Equals(SwSeleTypeObjectPair other)
    {
        if (other is null) return false;
        return string.Equals(Name, other.Name);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return Equals(obj as SwSeleTypeObjectPair);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="SwSeleTypeObjectPair"/>
    /// instances have the same <see cref="Name"/>.
    /// </summary>
    public static bool operator ==(SwSeleTypeObjectPair left, SwSeleTypeObjectPair right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value indicating whether two <see cref="SwSeleTypeObjectPair"/>
    /// instances have different <see cref="Name"/> values.
    /// </summary>
    public static bool operator !=(SwSeleTypeObjectPair left, SwSeleTypeObjectPair right)
    {
        return !(left == right);
    }

    #endregion

    /// <inheritdoc />
    public override string ToString()
    {
        return Name ?? base.ToString();
    }
}
