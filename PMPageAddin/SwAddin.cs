using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;

namespace PMPageAddin;

[ComVisible(true)]
[Guid("F9FB07A0-0632-4394-BD50-9C145498D7EC")]
[Title("PMPageDemo")]
public class SwAddin : SwAddInEx
{
    public SwAddin()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);
        if (name.Name.StartsWith("SolidWorks.Interop."))
        {
            var addinDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dllPath = Path.Combine(addinDir, name.Name + ".dll");
            if (File.Exists(dllPath))
            {
                return Assembly.LoadFrom(dllPath);
            }
        }
        return null;
    }

    public enum PMPageCmds
    {
        SelectionPage,
        MultiPage,
    }

    public override void OnConnect()
    {
        CommandManager.AddCommandGroup<PMPageCmds>().CommandClick += SwAddin_CommandClick;
    }

    private void SwAddin_CommandClick(PMPageCmds spec)
    {
        try
        {
            switch (spec)
            {
                case PMPageCmds.SelectionPage:
                    var page = new SelectionPage(Application.Sw);
                    page.ShowPage();
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Application.ShowMessageBox(ex.Message);
        }
    }
}
