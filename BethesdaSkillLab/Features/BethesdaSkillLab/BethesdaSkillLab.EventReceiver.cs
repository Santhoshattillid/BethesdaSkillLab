using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace BethesdaSkillLab.Features.BethesdaSkillLab
{
    [Guid("560949b7-c3d2-49cd-ab21-074365f3d058")]
    public class BethesdaSkillLabEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                var site = (SPSite)properties.Feature.Parent;
                using (var web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;

                    var skillLabList = web.Lists.TryGetList(Utilities.SkillLabListName);
                    if (skillLabList == null)
                    {
                        //creating new list
                        var listUID = web.Lists.Add(Utilities.SkillLabListName, string.Empty, SPListTemplateType.GenericList);
                        skillLabList = web.Lists[listUID];
                        skillLabList.OnQuickLaunch = true;
                        skillLabList.Update();
                    }

                    SPField textField;
                    SPFieldDateTime scheduleDateField = null;
                    SPFieldDateTime dateField = null;
                    SPFieldUser studentUserField = null;

                    // adding columns to the list
                    if (!skillLabList.Fields.ContainsField(Utilities.SkillColumnName))
                    {
                        skillLabList.Fields.Add(Utilities.SkillColumnName, SPFieldType.Text, true);
                        textField = skillLabList.Fields[Utilities.SkillColumnName];
                        textField.StaticName = Utilities.SkillColumnName;
                        textField.Update();
                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.SkillColumnName]);
                    }

                    if (!skillLabList.Fields.ContainsField(Utilities.StudentColumnName))
                    {
                        skillLabList.Fields.Add(Utilities.StudentColumnName, SPFieldType.User, true);
                        studentUserField = (SPFieldUser)skillLabList.Fields[Utilities.StudentColumnName];
                        if (studentUserField != null)
                        {
                            studentUserField.AllowMultipleValues = false;
                            studentUserField.Presence = true;
                            studentUserField.SelectionMode = SPFieldUserSelectionMode.PeopleOnly;
                            studentUserField.StaticName = Utilities.StudentColumnName;
                            studentUserField.Update();
                        }
                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.StudentColumnName]);
                    }
                    else
                        studentUserField = (SPFieldUser)skillLabList.Fields[Utilities.StudentColumnName];

                    if (!skillLabList.Fields.ContainsField(Utilities.ScheduleDateColumnName))
                    {
                        skillLabList.Fields.Add(Utilities.ScheduleDateColumnName, SPFieldType.DateTime, true);
                        scheduleDateField = (SPFieldDateTime)skillLabList.Fields[Utilities.ScheduleDateColumnName];
                        if (scheduleDateField != null)
                        {
                            scheduleDateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                            scheduleDateField.StaticName = Utilities.ScheduleDateColumnName;
                            scheduleDateField.Update();
                        }
                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.ScheduleDateColumnName]);
                    }
                    else
                        scheduleDateField = (SPFieldDateTime)skillLabList.Fields[Utilities.ScheduleDateColumnName];

                    if (!skillLabList.Fields.ContainsField(Utilities.TimeColumnName))
                    {
                        skillLabList.Fields.Add(Utilities.TimeColumnName, SPFieldType.Text, true);
                        textField = skillLabList.Fields[Utilities.TimeColumnName];
                        textField.StaticName = Utilities.TimeColumnName;
                        textField.Update();
                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.TimeColumnName]);
                    }

                    try
                    {
                        // creating the view for students
                        var viewFound = skillLabList.Views.Cast<SPView>().Any(spView => spView.Title.ToLower().Trim() == Utilities.StudentsViewName.ToLower().Trim());
                        if (!viewFound)
                        {
                            var stringCollection = new StringCollection
                                                       {
                                                           "Title",
                                                           Utilities.SkillColumnName,
                                                           Utilities.ScheduleDateColumnName,
                                                           Utilities.TimeColumnName
                                                       };
                            if (scheduleDateField != null && studentUserField != null)
                            {
                                string query = @"
                                <Where>
                                    <And>
                                        <Gt>
                                            <FieldRef Name='" +
                                               scheduleDateField.InternalName + @"' />
                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>[Today]</Value>
                                        </Gt>
                                        <Eq>
                                            <FieldRef Name='" +
                                               studentUserField.InternalName + @"' /><Value Type='User'>[Me]</Value>
                                        </Eq>
                                    </And>
                                </Where>";
                                skillLabList.Views.Add(Utilities.StudentsViewName, stringCollection, query, 50, true,
                                                       true);
                            }
                        }

                        // finally update the list for saving view
                        skillLabList.Update();
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    // now need to create skill lab config list
                    var skillLabConfiglist = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                    if (skillLabConfiglist == null)
                    {
                        //create list first
                        var listUid = web.Lists.Add(Utilities.SkillLabConfigListName, string.Empty,
                                                    SPListTemplateType.GenericList);
                        skillLabConfiglist = web.Lists[listUid];
                        skillLabConfiglist.OnQuickLaunch = true;
                        skillLabConfiglist.Update();
                    }

                    // adding fields here
                    var titleField = (SPFieldText)skillLabConfiglist.Fields["Title"];
                    titleField.StaticName = "Skill";
                    titleField.Update();

                    if (!skillLabConfiglist.Fields.ContainsField(Utilities.StartDateColumnName))
                    {
                        skillLabConfiglist.Fields.Add(Utilities.StartDateColumnName, SPFieldType.DateTime, true);
                        dateField = (SPFieldDateTime)skillLabConfiglist.Fields[Utilities.StartDateColumnName];
                        dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                        dateField.StaticName = Utilities.StartDateColumnName;
                        dateField.Update();
                        AddFieldOnView(skillLabConfiglist, dateField);
                    }

                    if (!skillLabConfiglist.Fields.ContainsField(Utilities.EndDateColumnName))
                    {
                        skillLabConfiglist.Fields.Add(Utilities.EndDateColumnName, SPFieldType.DateTime, true);
                        dateField = (SPFieldDateTime)skillLabConfiglist.Fields[Utilities.EndDateColumnName];
                        dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                        dateField.StaticName = Utilities.EndDateColumnName;
                        dateField.Update();
                        AddFieldOnView(skillLabConfiglist, dateField);
                    }

                    if (!skillLabConfiglist.Fields.ContainsField(Utilities.TimeSlotColumnName))
                    {
                        skillLabConfiglist.Fields.Add(Utilities.TimeSlotColumnName, SPFieldType.Text, true);
                        textField = skillLabConfiglist.Fields[Utilities.TimeSlotColumnName];
                        textField.StaticName = Utilities.TimeSlotColumnName;
                        textField.Update();
                        AddFieldOnView(skillLabConfiglist, skillLabConfiglist.Fields[Utilities.TimeSlotColumnName]);
                    }

                    skillLabConfiglist.Update();

                    // creating user groups here
                    try
                    {
                        web.SiteGroups.Add(Utilities.StudentsGroupName, web.AllUsers[0], web.AllUsers[0], string.Empty);
                        web.SiteGroups.Add(Utilities.FacultyGroupName, web.AllUsers[0], web.AllUsers[0], string.Empty);
                        web.Update();
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    // modifying list permissions here
                    try
                    {
                        ModifyListPermissions(skillLabList);
                        ModifyListPermissions(skillLabConfiglist);
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    // Creating Calendar view for viewing schedules
                    var calendar = web.Lists.TryGetList(Utilities.CalendarListName);
                    if (calendar == null)
                    {
                        var uid = web.Lists.Add(Utilities.CalendarListName, string.Empty, SPListTemplateType.Events);
                        calendar = web.Lists[Utilities.CalendarListName];
                        calendar.Fields.Add(Utilities.EventOwnerColumnName, SPFieldType.User, true);
                        var userField = (SPFieldUser)calendar.Fields[Utilities.EventOwnerColumnName];
                        userField.AllowMultipleValues = false;
                        userField.SelectionMode = SPFieldUserSelectionMode.PeopleOnly;
                        userField.Update();
                        calendar.Update();
                    }

                    web.AllowUnsafeUpdates = false;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        // Uncomment the method below to handle the event raised before a feature is deactivated.
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
        }

        private void ModifyListPermissions(SPList list)
        {
            // Modifying the permissions for the list to restrict the students for direct list items operations
            try
            {
                if (!list.HasUniqueRoleAssignments)
                {
                    var web = list.ParentWeb;

                    list.BreakRoleInheritance(false, false);
                    var group = web.SiteGroups[Utilities.StudentsGroupName];
                    var roleAssignment = new SPRoleAssignment(group);
                    SPRoleDefinition spRole = web.RoleDefinitions["Read"];
                    roleAssignment.RoleDefinitionBindings.Add(spRole);
                    list.RoleAssignments.Add(roleAssignment);

                    group = web.SiteGroups[Utilities.FacultyGroupName];
                    roleAssignment = new SPRoleAssignment(group);
                    spRole = web.RoleDefinitions["Full Control"];
                    roleAssignment.RoleDefinitionBindings.Add(spRole);
                    list.RoleAssignments.Add(roleAssignment);

                    list.Update();
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        private static void AddFieldOnView(SPList list, SPField spField)
        {
            try
            {
                list.DefaultView.ViewFields.Add(spField);
                list.DefaultView.Update();
                for (int i = 0; i < list.Views.Count; i++)
                {
                    SPView view = list.Views[i];
                    view.ViewFields.Add(spField);
                    view.Update();
                }
                list.Update();
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}