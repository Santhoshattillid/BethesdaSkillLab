using System;
using System.Web.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace BethesdaSkillLab.ViewRedirector
{
    public partial class ViewRedirectorUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    var group = SPContext.Current.Web.Groups[Utilities.StudentsGroupName];
                    if (group.ContainsCurrentUser)
                    {
                        var redirectView = SPContext.Current.List.Views[Utilities.StudentsViewName];
                        Response.Redirect("/" + redirectView.Url, true);
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}