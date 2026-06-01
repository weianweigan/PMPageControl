using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// Markup extension that creates a <see cref="List{T}"/> of <see cref="swSelectType_e"/>
/// for <see cref="SldSelectionBox.SwSelectTypes"/>.
/// </summary>
/// <remarks>
/// Usage in XAML:
/// <code><![CDATA[
/// SwSelectTypes="{controls:SwSelectTypes swSelEDGES, swSelFACES, swSelVERTICES}"
/// ]]></code>
/// <para>
/// Using a markup extension rather than a string-based <see cref="TypeConverter"/>
/// preserves IntelliSense / auto-completion for each <see cref="swSelectType_e"/> value
/// in the XAML editor.
/// </para>
/// </remarks>
public class SwSelectTypesExtension : MarkupExtension
{
    private readonly List<swSelectType_e> _types;

    /// <summary>
    /// Creates a new instance with a single <see cref="swSelectType_e"/> filter.
    /// </summary>
    /// <param name="type1">The allowed selection type.</param>
    public SwSelectTypesExtension(swSelectType_e type1)
    {
        _types = new List<swSelectType_e> { type1 };
    }

    /// <summary>
    /// Creates a new instance with two <see cref="swSelectType_e"/> filters.
    /// </summary>
    /// <param name="type1">The first allowed selection type.</param>
    /// <param name="type2">The second allowed selection type.</param>
    public SwSelectTypesExtension(swSelectType_e type1, swSelectType_e type2)
    {
        _types = new List<swSelectType_e> { type1, type2 };
    }

    /// <summary>
    /// Creates a new instance with three <see cref="swSelectType_e"/> filters.
    /// </summary>
    /// <param name="type1">The first allowed selection type.</param>
    /// <param name="type2">The second allowed selection type.</param>
    /// <param name="type3">The third allowed selection type.</param>
    public SwSelectTypesExtension(swSelectType_e type1, swSelectType_e type2, swSelectType_e type3)
    {
        _types = new List<swSelectType_e> { type1, type2, type3 };
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return _types;
    }
}
