using System;
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

                        // adding columns to the list

                        skillLabList.Fields.Add(Utilities.SkillColumnName, SPFieldType.Text, true);
                        var textField = skillLabList.Fields[Utilities.SkillColumnName];
                        textField.StaticName = Utilities.SkillColumnName;
                        textField.Update();

                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.SkillColumnName]);

                        skillLabList.Fields.Add(Utilities.StudentColumnName, SPFieldType.User, true);
                        var userField = (SPFieldUser)skillLabList.Fields[Utilities.StudentColumnName];
                        if (userField != null)
                        {
                            userField.AllowMultipleValues = false;
                            userField.Presence = true;
                            userField.SelectionMode = SPFieldUserSelectionMode.PeopleOnly;
                            userField.StaticName = Utilities.StudentColumnName;
                            userField.Update();
                        }
                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.StudentColumnName]);

                        skillLabList.Fields.Add(Utilities.ScheduleDateColumnName, SPFieldType.DateTime, true);
                        var dateField = (SPFieldDateTime)skillLabList.Fields[Utilities.ScheduleDateColumnName];
                        if (dateField != null)
                        {
                            dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                            dateField.StaticName = Utilities.ScheduleDateColumnName;
                            dateField.Update();
                        }

                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.ScheduleDateColumnName]);

                        skillLabList.Fields.Add(Utilities.TimeColumnName, SPFieldType.Text, true);
                        textField = skillLabList.Fields[Utilities.TimeColumnName];
                        textField.StaticName = Utilities.TimeColumnName;
                        textField.Update();

                        AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.TimeColumnName]);

                        skillLabList.Update();
                    }

                    // now need to create skill lab config list
                    var skillLabConfiglist = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                    if (skillLabConfiglist == null)
                    {
                        //create list first
                        var listUid = web.Lists.Add(Utilities.SkillLabConfigListName, string.Empty, SPListTemplateType.GenericList);
                        skillLabConfiglist = web.Lists[listUid];
                        skillLabConfiglist.OnQuickLaunch = true;
                        skillLabConfiglist.Update();

                        // adding fields here
                        var titleField = (SPFieldText)skillLabConfiglist.Fields["Title"];
                        titleField.StaticName = "Skill";
                        titleField.Update();

                        skillLabConfiglist.Fields.Add(Utilities.StartDateColumnName, SPFieldType.DateTime, true);
                        var dateField = (SPFieldDateTime)skillLabConfiglist.Fields[Utilities.StartDateColumnName];
                        dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                        dateField.StaticName = Utilities.StartDateColumnName;
                        dateField.Update();

                        AddFieldOnView(skillLabConfiglist, dateField);

                        skillLabConfiglist.Fields.Add(Utilities.EndDateColumnName, SPFieldType.DateTime, true);
                        dateField = (SPFieldDateTime)skillLabConfiglist.Fields[Utilities.EndDateColumnName];
                        dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                        dateField.StaticName = Utilities.EndDateColumnName;
                        dateField.Update();

                        AddFieldOnView(skillLabConfiglist, dateField);

                        skillLabConfiglist.Fields.Add(Utilities.TimeSlotColumnName, SPFieldType.Text, true);
                        var textField = skillLabConfiglist.Fields[Utilities.TimeSlotColumnName];
                        textField.StaticName = Utilities.TimeSlotColumnName;
                        textField.Update();

                        AddFieldOnView(skillLabConfiglist, skillLabConfiglist.Fields[Utilities.TimeSlotColumnName]);

                        skillLabConfiglist.Update();
                    }

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
                    ModifyListPermissions(skillLabList);
                    ModifyListPermissions(skillLabConfiglist);
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
    }
}