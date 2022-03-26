using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;

namespace PMPageAddin
{
    [ComVisible(true)]
    [Guid("F9FB07A0-0632-4394-BD50-9C145498D7EC")]
    [Title("PMPageDemo")]
    public class SwAddin : SwAddInEx
    {

        public enum PMPageCmds
        {
            SelectionPage,
        }

        public override void OnConnect()
        {
            CommandManager.AddCommandGroup<PMPageCmds>().CommandClick += SwAddin_CommandClick;
        }

        private void SwAddin_CommandClick(PMPageCmds spec)
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
    }
}