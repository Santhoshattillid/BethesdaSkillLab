<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages"
    Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RegistrationUserControl.ascx.cs"
    Inherits="BethesdaSkillLab.Registration.RegistrationUserControl" %>
<link href="/_layouts/BethesdaSkillLab/SkillLabStyles.css" rel="stylesheet" type="text/css" />
<div class="Container">
    <ul>
        <li>
            <h3>
                Student Registration Form</h3>
        </li>
        <li>
            <div class="left">
                <label>
                    Student name:</label>
            </div>
            <div class="right">
                <asp:TextBox runat="server" ID="Txtname" CssClass="text readOnly" ReadOnly="True"></asp:TextBox>
            </div>
            <span class="clear"></span></li>
        <li>
            <div class="left">
                <label>
                    Email:</label>
            </div>
            <div class="right">
                <asp:TextBox runat="server" ID="Txtmail" CssClass="text readOnly" ReadOnly="True"></asp:TextBox>
            </div>
            <span class="clear"></span></li>
        <li>
            <div class="left">
                <label>
                    Phone:
                </label>
            </div>
            <div class="right">
                <asp:TextBox runat="server" ID="TxtContact" CssClass="text readOnly" ReadOnly="True"></asp:TextBox>
            </div>
            <span class="clear"></span></li>
        <li>
            <div class="clear">
            </div>
        </li>
        <li>
            <div class="left">
                <label>
                    Skill:</label></div>
            <div class="right">
                <asp:DropDownList runat="server" ID="DdlSkill" CssClass="listbox" AutoPostBack="true">
                </asp:DropDownList>
            </div>
            <span class="clear"></span></li>
        <li>
            <div class="left">
                <label>
                    Date:
                </label>
            </div>
            <div class="right">
                <asp:DropDownList ID="DdlDates" runat="server" CssClass="listbox" OnSelectedIndexChanged="DdlDates_SelectedIndexChanged"
                    AutoPostBack="True">
                </asp:DropDownList>
            </div>
            <span class="clear"></span></li>
        <li>
            <div class="left">
                <label>
                    Time:
                </label>
            </div>
            <div class="right">
                <asp:DropDownList ID="DdlTime" runat="server" CssClass="listbox">
                </asp:DropDownList>
            </div>
            <span class="clear"></span></li>
        <li>
            <div class="left">
            </div>
            <div class="right">
                <asp:Label runat="server" ID="LblError" CssClass="ErrorInfo"></asp:Label>
            </div>
            <div class="clear">
            </div>
        </li>
        <li>
            <div class="controls">
                <asp:Button runat="server" Text="Register" ID="BtnRegister" OnClick="BtnRegister_Click" />
                <asp:Button runat="server" Text="Close" ID="BtnCancel"
                    onclick="BtnCancel_Click" />
            </div>
        </li>
    </ul>
</div>