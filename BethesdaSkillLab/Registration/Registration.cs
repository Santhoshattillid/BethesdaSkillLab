using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

namespace BethesdaSkillLab.Registration
{
    [ToolboxItemAttribute(false)]
    public class Registration : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string AscxPath = @"~/_CONTROLTEMPLATES/BethesdaSkillLab/Registration/RegistrationUserControl.ascx";

        protected override void CreateChildControls()
        {
            Control control = Page.LoadControl(AscxPath);
            Controls.Add(control);
        }
    }
}