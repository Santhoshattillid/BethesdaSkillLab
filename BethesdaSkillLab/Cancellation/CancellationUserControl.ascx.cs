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
                            var userProfile = profileManager.GetUserProfile(SPContext.Current.Web.CurrentUser.LoginName);
                            TxtContact.Text = userProfile.Properties.GetPropertyByName(PropertyConstants.WorkPhone) != null && userProfile[PropertyConstants.WorkPhone].Value != null ? userProfile[PropertyConstants.WorkPhone].Value.ToString() : "00000000000";

                            // getting list of skils and dates and times for the current user
                            var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now.AddDays(1));
                            using (var web = site.OpenWeb())
                            {
                                var list = web.Lists.TryGetList(Utilities.SkillLabListName);
                                if (list != null)
                                {
                                    var studentColumn = list.Fields[Utilities.StudentColumnName];
                                    var scheduleDate = list.Fields[Utilities.ScheduleDateColumnName];
                                    var query = new SPQuery
                                    {
                                        Query = @"<Where>
                                                    <And>
                                                        <Eq>
                                                            <FieldRef Name='" + studentColumn.InternalName + @"' />
                                                            <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                        </Eq>
                                                        <Geq>
                                                            <FieldRef Name='" + scheduleDate.InternalName + @"' />
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
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    LblError.Text = string.Empty;
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
                                        var studentColumn = list.Fields[Utilities.StudentColumnName];
                                        var scheduleDateColumn = list.Fields[Utilities.ScheduleDateColumnName];
                                        var skillColumn = list.Fields[Utilities.SkillColumnName];
                                        var query = new SPQuery
                                        {
                                            Query = @"<Where>
                                                        <And>
                                                            <And>
                                                                <Eq>
                                                                    <FieldRef Name='" + studentColumn.InternalName + @"' />
                                                                    <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                                </Eq>
                                                                <Eq>
                                                                    <FieldRef Name='" + skillColumn.InternalName + @"' />
                                                                    <Value Type='Text'>" + DdlSkill.SelectedValue + @"</Value>
                                                                </Eq>
                                                            </And>
                                                            <Geq>
                                                                <FieldRef Name='" + scheduleDateColumn.InternalName + @"' />
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
                        LblError.Text += "<br/>";
                        DdlDates.Items.Clear();
                        DdlTime.Items.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    LblError.Text = string.Empty;
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
                                        var studentColumn = list.Fields[Utilities.StudentColumnName];
                                        var scheduleDateColumn = list.Fields[Utilities.ScheduleDateColumnName];
                                        var skillColumn = list.Fields[Utilities.SkillColumnName];
                                        var query = new SPQuery
                                        {
                                            Query = @"<Where>
                                                        <And>
                                                            <And>
                                                                <Eq>
                                                                    <FieldRef Name='" + studentColumn.InternalName + @"' />
                                                                    <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                                </Eq>
                                                                <Eq>
                                                                     <FieldRef Name='" + skillColumn.InternalName + @"' />
                                                                    <Value Type='Text'>" + DdlSkill.SelectedValue + @"</Value>
                                                                </Eq>
                                                            </And>
                                                            <Eq>
                                                                <FieldRef Name='" + scheduleDateColumn.InternalName + @"' />
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
                        LblError.Text += "<br/>";
                    }
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                //var list = SPContext.Current.Web.Lists.TryGetList(Utilities.SkillLabListName);
                //Response.Redirect(list != null ? list.DefaultViewUrl : SPContext.Current.Web.Url);
                Response.Redirect(SPContext.Current.Web.Url);
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void BtnCancellation_Click(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    LblError.Text = string.Empty;

                    // validation starts here
                    if (DdlSkill.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select skill and fill all other fields.";
                        LblError.Text += "<br/>";
                        return;
                    }

                    if (DdlDates.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select date for cancellation";
                        LblError.Text += "<br/>";
                        return;
                    }

                    if (DdlTime.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select time slot for cancellation";
                        LblError.Text += "<br/>";
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
                                    var studentColumn = list.Fields[Utilities.StudentColumnName];
                                    var scheduleDateColumn = list.Fields[Utilities.ScheduleDateColumnName];
                                    var skillColumn = list.Fields[Utilities.SkillColumnName];
                                    var timeColumn = list.Fields[Utilities.TimeColumnName];
                                    var query = new SPQuery
                                    {
                                        Query = @"<Where>
                                                    <And>
                                                        <And>
                                                            <And>
                                                                <Eq>
                                                                    <FieldRef Name='" + timeColumn.InternalName + @"' />
                                                                    <Value Type='Text'>" + DdlTime.SelectedValue + @"</Value>
                                                                </Eq>
                                                                <Eq>
                                                                    <FieldRef Name='" + scheduleDateColumn.InternalName + @"' />
                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                </Eq>
                                                            </And>
                                                            <Eq>
                                                                <FieldRef Name='" + studentColumn.InternalName + @"' />
                                                                <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                            </Eq>
                                                        </And>
                                                        <Eq>
                                                            <FieldRef Name='" + skillColumn.InternalName + @"' />
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

                                    // removing the event from calendar
                                    var calendar = web.Lists.TryGetList(Utilities.CalendarListName);
                                    if (calendar != null)
                                    {
                                        var times = DdlTime.SelectedValue.Split('-');
                                        if (times.Length > 1)
                                        {
                                            var startTime = selectedDate;
                                            startTime = startTime.AddHours(startTime.Hour * -1);
                                            startTime = startTime.AddMinutes(startTime.Minute * -1);
                                            startTime = startTime.AddSeconds(startTime.Second * -1);

                                            var endTime = startTime;

                                            int hour = 0;

                                            if (times[0].IndexOf("AM", StringComparison.Ordinal) > 0)
                                                hour = Convert.ToInt16(times[0].Trim().Replace("AM", ""));
                                            else if (times[0].IndexOf("PM", StringComparison.Ordinal) > 0)
                                                hour = Convert.ToInt16(times[0].Trim().Replace("PM", "")) + 12;

                                            startTime = startTime.AddHours(hour);

                                            hour = 0;
                                            if (times[1].IndexOf("AM", StringComparison.Ordinal) > 0)
                                                hour = Convert.ToInt16(times[1].Trim().Replace("AM", ""));
                                            else if (times[1].IndexOf("PM", StringComparison.Ordinal) > 0)
                                                hour = Convert.ToInt16(times[1].Trim().Replace("PM", "")) + 12;

                                            endTime = endTime.AddHours(hour);

                                            query = new SPQuery
                                                        {
                                                            Query = @"<Where>
                                                                        <And>
                                                                            <And>
                                                                                <Eq>
                                                                                    <FieldRef Name='AssignedTo' />
                                                                                        <Value Type='User'>" + SPContext.Current.Web.CurrentUser.LoginName + @"</Value>
                                                                                </Eq>
                                                                                <Eq>
                                                                                    <FieldRef Name='EventDate' />
                                                                                        <Value IncludeTimeValue='TRUE' Type='DateTime'>" + SPUtility.CreateISO8601DateTimeFromSystemDateTime(startTime) + @"</Value>
                                                                                </Eq>
                                                                            </And>
                                                                            <Eq>
                                                                                <FieldRef Name='EndDate' />
                                                                                    <Value IncludeTimeValue='TRUE' Type='DateTime'>" + SPUtility.CreateISO8601DateTimeFromSystemDateTime(endTime) + @"</Value>
                                                                            </Eq>
                                                                        </And>
                                                                       </Where>"
                                                        };
                                            collection = calendar.GetItems(query);
                                            while (collection.Count > 0)
                                            {
                                                foreach (SPListItem listItem in collection)
                                                {
                                                    listItem.Delete();
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    web.AllowUnsafeUpdates = false;
                                    LblError.Text = "Your slot has been cancelled succesfully.";
                                    LblError.Text += "<br/>";

                                    // sending notification to faculty for skill lab cancellation
                                    if (SPUtility.IsEmailServerSet(web))
                                    {
                                        try
                                        {
                                            var skillLabConfigList = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                                            if (skillLabConfigList != null)
                                            {
                                                var startDateColumn = skillLabConfigList.Fields[Utilities.StartDateColumnName];
                                                var endDateColumn = skillLabConfigList.Fields[Utilities.EndDateColumnName];
                                                query = new SPQuery
                                                {
                                                    Query =
                                                        @"<Where>
                                                                            <And>
                                                                              <Or>
                                                                                 <Gt>
                                                                                    <FieldRef Name='" + startDateColumn.InternalName + @"' />
                                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" +
                                                                                convertedDate + @"</Value>
                                                                                 </Gt>
                                                                                 <Gt>
                                                                                    <FieldRef Name='" + endDateColumn.InternalName + @"' />
                                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" +
                                                                                convertedDate + @"</Value>
                                                                                 </Gt>
                                                                              </Or>
                                                                              <Eq>
                                                                                   <FieldRef Name='Title' />
                                                                                    <Value Type='Text'>" +
                                                                                DdlSkill.SelectedValue + @"</Value>
                                                                              </Eq>
                                                                            </And>
                                                                           </Where>"
                                                };
                                                foreach (SPListItem listItem in skillLabConfigList.GetItems(query))
                                                {
                                                    var createdBy = new SPFieldUserValue(web, listItem["Author"].ToString());
                                                    if (!string.IsNullOrEmpty(createdBy.User.Email))
                                                    {
                                                        var mailBody = "Here is the details of cancellation.";
                                                        mailBody += "</br> User: " + SPContext.Current.Web.CurrentUser.Name;
                                                        mailBody += "</br> Date: " + DdlDates.SelectedValue;
                                                        mailBody += "</br> Skill: " + DdlSkill.SelectedValue;
                                                        mailBody += "</br> Time-Slot: " + DdlTime.SelectedValue;
                                                        mailBody += "</br> Cancelled at: " + DateTime.Now.ToShortDateString();
                                                        SPUtility.SendEmail(web, true, true, createdBy.User.Email, "New skill lab test cancellation.", mailBody);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // write the error to event log
                                            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
                                        }
                                    }

                                    // else
                                    //   LblError.Text += "</br> The email notifications cannot be send due to settings not configured.";
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}