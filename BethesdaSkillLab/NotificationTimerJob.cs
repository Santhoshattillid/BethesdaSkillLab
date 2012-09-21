using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace BethesdaSkillLab
{
    public class NotificationTimerJob : SPJobDefinition
    {
        public NotificationTimerJob()
            : base()
        {
        }

        public NotificationTimerJob(string jobName, SPService service, SPServer server, SPJobLockType lockType)
            : base(jobName, service, server, lockType)
        {
            this.Title = jobName;
        }

        public NotificationTimerJob(string jobName, SPWebApplication webapp)
            : base(jobName, webapp, null, SPJobLockType.ContentDatabase)
        {
            this.Title = jobName;
        }

        public override void Execute(Guid targetInstanceId)
        {
            var webapp = this.Parent as SPWebApplication;
            if (webapp != null)
            {
                foreach (SPSite spSite in webapp.Sites)
                {
                    using (var web = spSite.OpenWeb())
                    {
                        if (SPUtility.IsEmailServerSet(web))
                        {
                            var skillLabTest = spSite.RootWeb.Lists.TryGetList(Utilities.SkillLabListName);
                            if (skillLabTest != null)
                            {
                                var scheduleDateField = skillLabTest.Fields[Utilities.ScheduleDateColumnName];
                                var convertedDate =
                                    SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now.AddDays(1));
                                var query = new SPQuery
                                                {
                                                    Query =
                                                        @"<Where>
                                                            <Eq>
                                                                <FieldRef Name='" +
                                                        scheduleDateField.InternalName + @"' />
                                                                <Value IncludeTimeValue='FALSE' Type='DateTime'>" +
                                                        convertedDate + @"</Value>
                                                            </Eq>
                                                       </Where>"
                                                };
                                foreach (SPListItem spListItem in skillLabTest.GetItems(query))
                                {
                                    // sending notification to studen for skill lab test reminder
                                    try
                                    {
                                        var student = new SPFieldUserValue(web,
                                                                           spListItem[Utilities.StudentColumnName].
                                                                               ToString());
                                        if (!string.IsNullOrEmpty(student.User.Email))
                                        {
                                            var mailBody = "Here is the details of registration.";
                                            mailBody += "</br> User: " + SPContext.Current.Web.CurrentUser.Name;
                                            mailBody += "</br> Date: " + DateTime.Now.AddDays(1).ToShortDateString();
                                            mailBody += "</br> Skill: " + spListItem[Utilities.SkillColumnName];
                                            mailBody += "</br> Time-Slot: " + spListItem[Utilities.TimeColumnName];
                                            SPUtility.SendEmail(web, true, true, student.User.Email,
                                                                "A reminder mail for your skill lab test.", mailBody);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // write the error to event log
                                        SPDiagnosticsService.Local.WriteTrace(0,
                                                                              new SPDiagnosticsCategory(
                                                                                  "BethesdaSkillLab",
                                                                                  TraceSeverity.Monitorable,
                                                                                  EventSeverity.Error),
                                                                              TraceSeverity.Monitorable, ex.Message,
                                                                              new object[] { ex.StackTrace });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}