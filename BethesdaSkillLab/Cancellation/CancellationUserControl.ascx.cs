using System;
using System.Collections.Generic;
using System.Web.UI;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BethesdaSkillLab.Cancellation
{
    public partial class CancellationUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack && SPContext.Current != null)
                {
                    Txtname.Text = SPContext.Current.Web.CurrentUser.Name;
                    Txtmail.Text = SPContext.Current.Web.CurrentUser.Email;
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        using (var site = new SPSite(SPContext.Current.Site.Url))
                        {
                            var context = SPServiceContext.GetContext(site);
                            var profileManager = new UserProfileManager(context);
                            var userProfile =
                                profileManager.GetUserProfile(
                                    SPContext.Current.Web.CurrentUser.LoginName);
                            TxtContact.Text = userProfile.Properties.GetPropertyByName(PropertyConstants.WorkPhone) != null && userProfile[PropertyConstants.WorkPhone].Value != null ? userProfile[PropertyConstants.WorkPhone].Value.ToString() : "00000000000";

                            // getting list of skils and dates and times for the current user
                            var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now.AddDays(1));
                            using (var web = site.OpenWeb())
                            {
                                var list = web.Lists.TryGetList(Utilities.SkillLabListName);
                                if (list != null)
                                {
                                    var query = new SPQuery
                                    {
                                        Query = @"<Where>
                                                    <And>
                                                        <Eq>
                                                            <FieldRef Name='Student' />
                                                            <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                        </Eq>
                                                        <Geq>
                                                            <FieldRef Name='Schedule_x0020_Date' />
                                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                        </Geq>
                                                    </And>
                                                  </Where>"
                                    };
                                    var skills = new List<string>();
                                    foreach (SPListItem listItem in list.GetItems(query))
                                    {
                                        if (!skills.Contains(listItem[Utilities.SkillColumnName].ToString()))
                                            skills.Add(listItem[Utilities.SkillColumnName].ToString());
                                    }
                                    DdlSkill.Items.Clear();
                                    DdlDates.Items.Clear();
                                    DdlTime.Items.Clear();
                                    DdlSkill.Items.Add("Select Skill");
                                    foreach (string skill in skills)
                                    {
                                        DdlSkill.Items.Add(skill);
                                    }
                                    DdlSkill.SelectedIndex = 0;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    if (DdlSkill.SelectedIndex > 0)
                    {
                        // getting list of skils and dates and times for the current user
                        var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now.AddDays(1));
                        SPSecurity.RunWithElevatedPrivileges(delegate
                        {
                            using (var site = new SPSite(SPContext.Current.Site.Url))
                            {
                                using (var web = site.OpenWeb())
                                {
                                    var list = web.Lists.TryGetList(Utilities.SkillLabListName);
                                    if (list != null)
                                    {
                                        var query = new SPQuery
                                        {
                                            Query = @"<Where>
                                                        <And>
                                                            <And>
                                                                <Eq>
                                                                    <FieldRef Name='Student' />
                                                                    <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                                </Eq>
                                                                <Eq>
                                                                    <FieldRef Name='Skill' />
                                                                    <Value Type='Text'>" + DdlSkill.SelectedValue + @"</Value>
                                                                </Eq>
                                                            </And>
                                                            <Geq>
                                                                <FieldRef Name='Schedule_x0020_Date' />
                                                                <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                            </Geq>
                                                        </And>
                                                   </Where>"
                                        };
                                        DdlDates.Items.Clear();
                                        DdlTime.Items.Clear();
                                        DdlDates.Items.Add("Select date");
                                        foreach (SPListItem listItem in list.GetItems(query))
                                        {
                                            DdlDates.Items.Add(Convert.ToDateTime(listItem[Utilities.ScheduleDateColumnName]).ToString(Utilities.DateFormatString));
                                        }
                                        DdlDates.SelectedIndex = 0;
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        LblError.Text = "Please select skill";
                        DdlDates.Items.Clear();
                        DdlTime.Items.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    if (DdlSkill.SelectedIndex > 0 && DdlDates.SelectedIndex > 0)
                    {
                        var selectedDate = Convert.ToDateTime(DdlDates.SelectedValue);
                        var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(selectedDate);
                        SPSecurity.RunWithElevatedPrivileges(delegate
                        {
                            using (var site = new SPSite(SPContext.Current.Site.Url))
                            {
                                using (var web = site.OpenWeb())
                                {
                                    var list = web.Lists.TryGetList(Utilities.SkillLabListName);
                                    if (list != null)
                                    {
                                        var query = new SPQuery
                                        {
                                            Query = @"<Where>
                                                        <And>
                                                            <And>
                                                                <Eq>
                                                                    <FieldRef Name='Student' />
                                                                    <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                                </Eq>
                                                                <Eq>
                                                                     <FieldRef Name='Skill' />
                                                                    <Value Type='Text'>" + DdlSkill.SelectedValue + @"</Value>
                                                                </Eq>
                                                            </And>
                                                            <Eq>
                                                                <FieldRef Name='Schedule_x0020_Date' />
                                                                <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                            </Eq>
                                                        </And>
                                                   </Where>"
                                        };
                                        DdlTime.Items.Clear();
                                        DdlTime.Items.Add("Select slot time");
                                        foreach (SPListItem listItem in list.GetItems(query))
                                        {
                                            DdlTime.Items.Add(listItem[Utilities.TimeColumnName].ToString());
                                        }
                                        DdlTime.SelectedIndex = 0;
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        DdlTime.Items.Clear();
                        LblError.Text = "Please select slot date";
                    }
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                var list = SPContext.Current.Web.Lists.TryGetList(Utilities.SkillLabListName);
                Response.Redirect(list != null ? list.DefaultViewUrl : SPContext.Current.Web.Url);
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void BtnCancellation_Click(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    // validation starts here
                    if (DdlSkill.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select skill and fill all other fields.";
                        return;
                    }

                    if (DdlDates.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select date for cancellation";
                        return;
                    }

                    if (DdlTime.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select time slot for cancellation";
                        return;
                    }

                    var selectedDate = Convert.ToDateTime(DdlDates.SelectedValue);
                    var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(selectedDate);
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        using (var site = new SPSite(SPContext.Current.Site.Url))
                        {
                            using (var web = site.OpenWeb())
                            {
                                var list = web.Lists.TryGetList(Utilities.SkillLabListName);
                                if (list != null)
                                {
                                    var query = new SPQuery
                                    {
                                        Query = @"<Where>
                                                    <And>
                                                        <And>
                                                            <And>
                                                                <Eq>
                                                                    <FieldRef Name='Time' />
                                                                    <Value Type='Text'>" + DdlTime.SelectedValue + @"</Value>
                                                                </Eq>
                                                                <Eq>
                                                                    <FieldRef Name='Schedule_x0020_Date' />
                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                </Eq>
                                                            </And>
                                                            <Eq>
                                                                <FieldRef Name='Student' />
                                                                <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                            </Eq>
                                                        </And>
                                                        <Eq>
                                                            <FieldRef Name='Skill' />
                                                            <Value Type='Text'>" + DdlSkill.SelectedValue + @"</Value>
                                                        </Eq>
                                                    </And>
                                                   </Where>"
                                    };
                                    var collection = list.GetItems(query);
                                    bool isDeletedAllRecords = true;
                                    web.AllowUnsafeUpdates = true;
                                    while (isDeletedAllRecords)
                                    {
                                        isDeletedAllRecords = false;
                                        foreach (SPListItem spListItem in collection)
                                        {
                                            spListItem.Delete();
                                            isDeletedAllRecords = true;
                                            break;
                                        }
                                    }
                                    web.AllowUnsafeUpdates = false;
                                    LblError.Text = "Your slot has been cancelled succesfully.";
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}