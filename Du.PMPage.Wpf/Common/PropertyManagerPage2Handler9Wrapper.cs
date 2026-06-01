using System.Runtime.InteropServices;
using SolidWorks.Interop.swpublished;

namespace Du.PMPage.Wpf;

/// <summary>
/// A COM-visible proxy that wraps an <see cref="IPropertyManagerPage2Handler9"/> implementation
/// to avoid invalid cast exceptions during COM interop when the original handler uses generics
/// in its inheritance chain.
/// </summary>
/// <remarks>
/// <para>
/// Passing an object that inherits from a generic type as an <see langword="IDispatch"/> during
/// COM interop causes an invalid cast exception. This wrapper delegates every method of
/// <see cref="IPropertyManagerPage2Handler9"/> to the inner implementation, isolating the generic
/// inheritance chain from COM.
/// </para>
/// <para>
/// See: http://stackoverflow.com/questions/35438895/passing-an-object-in-c-sharp-that-inherits-from-a-generic-causes-a-cast-exceptio/35449486#35449486
/// </para>
/// <para>
/// All methods were generated to mirror the interface. There are NuGet packages that can
/// automate this kind of proxy generation, but the explicit approach was used for clarity
/// and maintainability.
/// </para>
/// </remarks>
[ComVisible(true)]
public class PropertyManagerPage2Handler9Wrapper(IPropertyManagerPage2Handler9 implementation)
    : IPropertyManagerPage2Handler9
{
    private readonly IPropertyManagerPage2Handler9 _Implementation = implementation;

    /// <inheritdoc />
    public void AfterActivation()
    {
        _Implementation.AfterActivation();
    }

    /// <inheritdoc />
    public void OnClose(int reason)
    {
        _Implementation.OnClose(reason);
    }

    /// <inheritdoc />
    public void AfterClose()
    {
        _Implementation.AfterClose();
    }

    /// <inheritdoc />
    public bool OnHelp()
    {
        return _Implementation.OnHelp();
    }

    /// <inheritdoc />
    public bool OnPreviousPage()
    {
        return _Implementation.OnPreviousPage();
    }

    /// <inheritdoc />
    public bool OnNextPage()
    {
        return _Implementation.OnNextPage();
    }

    /// <inheritdoc />
    public bool OnPreview()
    {
        return _Implementation.OnPreview();
    }

    /// <inheritdoc />
    public void OnWhatsNew()
    {
        _Implementation.OnWhatsNew();
    }

    /// <inheritdoc />
    public void OnUndo()
    {
        _Implementation.OnUndo();
    }

    /// <inheritdoc />
    public void OnRedo()
    {
        _Implementation.OnRedo();
    }

    /// <inheritdoc />
    public bool OnTabClicked(int id)
    {
        return _Implementation.OnTabClicked(id);
    }

    /// <inheritdoc />
    public void OnGroupExpand(int id, bool expanded)
    {
        _Implementation.OnGroupExpand(id, expanded);
    }

    /// <inheritdoc />
    public void OnGroupCheck(int id, bool Checked)
    {
        _Implementation.OnGroupCheck(id, Checked);
    }

    /// <inheritdoc />
    public void OnCheckboxCheck(int id, bool Checked)
    {
        _Implementation.OnCheckboxCheck(id, Checked);
    }

    /// <inheritdoc />
    public void OnOptionCheck(int id)
    {
        _Implementation.OnOptionCheck(id);
    }

    /// <inheritdoc />
    public void OnButtonPress(int id)
    {
        _Implementation.OnButtonPress(id);
    }

    /// <inheritdoc />
    public void OnTextboxChanged(int id, string text)
    {
        _Implementation.OnTextboxChanged(id, text);
    }

    /// <inheritdoc />
    public void OnNumberboxChanged(int id, double value)
    {
        _Implementation.OnNumberboxChanged(id, value);
    }

    /// <inheritdoc />
    public void OnComboboxEditChanged(int id, string text)
    {
        _Implementation.OnComboboxEditChanged(id, text);
    }

    /// <inheritdoc />
    public void OnComboboxSelectionChanged(int id, int item)
    {
        _Implementation.OnComboboxSelectionChanged(id, item);
    }

    /// <inheritdoc />
    public void OnListboxSelectionChanged(int id, int item)
    {
        _Implementation.OnListboxSelectionChanged(id, item);
    }

    /// <inheritdoc />
    public void OnSelectionboxFocusChanged(int id)
    {
        _Implementation.OnSelectionboxFocusChanged(id);
    }

    /// <inheritdoc />
    public void OnSelectionboxListChanged(int id, int count)
    {
        _Implementation.OnSelectionboxListChanged(id, count);
    }

    /// <inheritdoc />
    public void OnSelectionboxCalloutCreated(int id)
    {
        _Implementation.OnSelectionboxCalloutCreated(id);
    }

    /// <inheritdoc />
    public void OnSelectionboxCalloutDestroyed(int id)
    {
        _Implementation.OnSelectionboxCalloutDestroyed(id);
    }

    /// <inheritdoc />
    public bool OnSubmitSelection(int id, object selection, int selType, ref string itemText)
    {
        return _Implementation.OnSubmitSelection(id, selection, selType, ref itemText);
    }

    /// <inheritdoc />
    public int OnActiveXControlCreated(int id, bool status)
    {
        return _Implementation.OnActiveXControlCreated(id, status);
    }

    /// <inheritdoc />
    public void OnSliderPositionChanged(int id, double value)
    {
        _Implementation.OnSliderPositionChanged(id, value);
    }

    /// <inheritdoc />
    public void OnSliderTrackingCompleted(int id, double value)
    {
        _Implementation.OnSliderTrackingCompleted(id, value);
    }

    /// <inheritdoc />
    public bool OnKeystroke(int wparam, int message, int lparam, int id)
    {
        return _Implementation.OnKeystroke(wparam, message, lparam, id);
    }

    /// <inheritdoc />
    public void OnPopupMenuItem(int id)
    {
        _Implementation.OnPopupMenuItem(id);
    }

    /// <inheritdoc />
    public void OnPopupMenuItemUpdate(int id, ref int retval)
    {
        _Implementation.OnPopupMenuItemUpdate(id, ref retval);
    }

    /// <inheritdoc />
    public void OnGainedFocus(int id)
    {
        _Implementation.OnGainedFocus(id);
    }

    /// <inheritdoc />
    public void OnLostFocus(int id)
    {
        _Implementation.OnLostFocus(id);
    }

    /// <inheritdoc />
    public int OnWindowFromHandleControlCreated(int id, bool status)
    {
        return _Implementation.OnWindowFromHandleControlCreated(id, status);
    }

    /// <inheritdoc />
    public void OnListboxRMBUp(int id, int posX, int posY)
    {
        _Implementation.OnListboxRMBUp(id, posX, posY);
    }

    /// <inheritdoc />
    public void OnNumberBoxTrackingCompleted(int id, double value)
    {
        _Implementation.OnNumberBoxTrackingCompleted(id, value);
    }
}
