﻿<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmAccessControl.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmAccessControl" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
	<meta name="viewport" content="width=device-width, initial-scale=1" />
	<link href="Content/SubSite.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />
	<link href="Content/Modal.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />

	<style>
		.CheckBoxBackgrounds {
			background-color: transparent;
		}

			.CheckBoxBackgrounds input[type="checkbox"] {
				margin-top: 0px;
				margin-bottom: 0px;
				margin-left: 10px;
				margin-right: 10px;
			}

		.inputWithIcon input[type=text] {
			padding-left: 40px;
		}

		.inputLabel {
			border: 0px solid;
			font-size: 14px;
			font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
		}

		.d-grid {
			display: grid;
			grid-template-columns: 50% 50%;
		}

		@media only screen and (max-width: 670px) {
			.d-grid {
				grid-template-columns: 100%;
			}
		}

		input[type="radio"] {
			margin: auto 10px;
		}
	</style>
	<script>
		document.title = "Admin Access Form";

		function closeModal() {
			$('#ModalMessage').modal('hide');
			$('#ErrorModalMessage').modal('hide');
			$('#ConfirmationModalMessage').modal('hide');
			$('#ModalConfirmation').modal('hide');
			$('.modal-backdrop').remove();
			$(document.body).removeClass('modal-open');
			document.body.style.overflow = "scroll";
		}

		function showModal(status, message) {
			$("#ModalConfirmation").modal('hide');
			var title = document.getElementById('MessageModalTitle');
			var content = document.getElementById('MessageModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ModalMessage").modal('show');
		}

		function showErrorModal(status, message) {
			$("#ModalConfirmation").modal('hide');
			var title = document.getElementById('ErrorModalTitle');
			var content = document.getElementById('ErrorModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ErrorModalMessage").modal('show');
		}

		function validation() {
			// Drop List Field
			var ddlEmpMode = $('#<%= ddlEmpMode.ClientID %>').val();
			var DrpListEmpID = $('#<%= EmpIDDrpList.ClientID %>').val();

			// Text Field
			var Username = $('#<%= txtEmpUsername.ClientID %>').val();
			var Password = $('#<%= txtPassword.ClientID %>').val();

			// Validate Employee Mode
			if (!ddlEmpMode || ddlEmpMode.trim() === "") {
				showErrorModal('Failed', 'Please select the mode.');
				return;
			}

			// Validate Employee ID (only in Update Mode)
			if (ddlEmpMode === "U" && (!DrpListEmpID || DrpListEmpID.trim() === "")) {
				showErrorModal('Failed', 'Please select the Employee ID.');
				return;
			}

			// Validate Username
			if (!Username || Username.trim() === "") {
				showErrorModal('Failed', 'Please enter the Employee Username.');
				return;
			}

			// Validate Password (only in Create mode)
			if (ddlEmpMode === "C" && (!Password || Password.trim() === "")) {
				showErrorModal('Failed', 'Please enter the Employee password.');
				return;
			}

			//Validate Username length
			if (Username.length > 50) {
				showErrorModal('Failed', 'Employee Username should be 50 characters or fewer.');
				return;
			}
			// Validate Password length (only in Create mode)
			if (ddlEmpMode === "C" && (Password.length > 14 || Password.length < 8)) {
				showErrorModal('Failed', 'Password length should be between 8 to 14.');
				return;
			}

			return;
		}

		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			document.title = "Admin Access Form";
		});
	</script>

	&nbsp;
     
    <%--    This is another new design template--%>
	<div class="container-fluid" runat="server" ID="E_AccessC" Visible="false">
		<!-- Page Heading --For Container untill card body-->
		<h1 class="h3 mb-2 text-gray-800"></h1>
		<p class="mb-4"></p>
		<!-- Access Control -->
		<div class="card shadow mb-4">
			<div class="card-header py-3">
				<h6 class="m-0 page-heading-cubic"><i class="fa fa-user-shield"></i>&nbsp;&nbsp; Access Control</h6>
			</div>

			<div class="card-body">

				<asp:ScriptManager ID="ScriptManagerMain" runat="server"></asp:ScriptManager>

				<asp:UpdatePanel ID="UpdatePanel0" runat="server">
					<ContentTemplate>

						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableClientScript="True" />


						<%--  Row  Select Mode--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4" style="border: 0px solid">
								<asp:Label ID="lblEmpddl" runat="server" Text="Select Mode" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="ddlEmpMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" style="border: 1px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlEmpMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">--Create New--</asp:ListItem>
									<asp:ListItem Value="U">--Update Details--</asp:ListItem>
								</asp:DropDownList>
							</div>

							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-6">
							</div>
						</div>

						<%--  Row  Employee ID--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4" style="border: 0px solid">
								<asp:Label ID="lblEmpIDtxt" runat="server" Text="Employee ID" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:DropDownList ID="EmpIDDrpList" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" style="border: #C5CCD6 1px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" Visible="false" OnSelectedIndexChanged="EmpIDDrpList_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator1" runat="server" Operator="NotEqual" ControlToValidate="EmpIDDrpList" ValueToCompare="" Errormessage="Please select an Employee ID" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>

							</div>

						</div>

						<%--  Row  Employee Name--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4" style="border: 0px solid">
								<asp:Label ID="lblEmpUsername" runat="server" Text="Employee Username" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtEmpUsername" runat="server" class="form-control input-text-cubic-14" ReadOnly="True" placeholder="Employee Username"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<%-- Row Employee Status --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblStatus" runat="server" Text="Status " class="input-label-cubic"></asp:Label>
								<asp:RadioButtonList ID="rbStatus" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" class="input-radio-cubic-20">
									<asp:ListItem Selected="True" Value="O">Active</asp:ListItem>
									<asp:ListItem Value="C">Inactive</asp:ListItem>
								</asp:RadioButtonList>
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<%-- Row Employee Password--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblPassword" runat="server" Text="Password" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtPassword" runat="server" class="form-control input-text-cubic-14" ReadOnly="True" placeholder="Enter Password"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:Button ID="btnGeneratePassword" runat="server" CausesValidation="false" style="margin-top: 2.25em; float: left; background-color: #FFFFFF; color: #00A8B0; border-radius: 5px; font-size: 15px; font-weight: 600;" Text="Generate Password" Visible="false" Width="180px" min-Width="140px" Height="40" CssClass="form-control" BorderColor="#00A8B0" BorderStyle="Solid" OnClick="btnGeneratePassword_Click" />
							</div>

							<div class="form-group col-md-2">
							</div>
						</div>


						<hr class="cssContentHeaderLine" />

						<%--Row  Admin Setting--%>
						<div class="form-row">
							<div class="form-group col-md-12">
								<h5 style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 15px; font-style: oblique; font-weight: 600"><i class="fa fa-user-shield"></i>&nbsp;&nbsp; Admin Setting</h5>
							</div>
						</div>
						<div class="d-grid" style="border: 0px solid;">
							<div class="form-group" style="border: 0px solid;">
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:Label ID="Label1" runat="server" Text="Editable Access " class="input-label-cubic"></asp:Label>
									</div>
								</div>

								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:CheckBoxList ID="ChkEditAccessAdmin" runat="server" CssClass="CheckBoxBackgrounds" AutoPostBack="true" RepeatLayout="table" style="border: 0px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding-top: 10px">
											<asp:ListItem Value="E_CustomerM">Customer</asp:ListItem>
											<asp:ListItem Value="E_ProjM">Project Maintenance</asp:ListItem>
											<asp:ListItem Value="E_AccessC">Access Control</asp:ListItem>
										</asp:CheckBoxList>
									</div>
								</div>
							</div>
							<div class="form-group" style="border: 0px solid;">
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:Label ID="Label2" runat="server" Text="View Access " class="input-label-cubic"></asp:Label>
									</div>
								</div>

								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:CheckBoxList ID="ChkViewAccessAdmin" runat="server" CssClass="CheckBoxBackgrounds" AutoPostBack="true" RepeatLayout="table" style="border: 0px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding-top: 10px">
											<asp:ListItem Value="V_CustomerM">Customer</asp:ListItem>
										</asp:CheckBoxList>
									</div>
								</div>
							</div>
						</div>

						<br />

						<%--Row  Maintenance Setting--%>
						<div class="form-row">
							<div class="form-group col-md-12">
								<h5 style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 15px; font-style: oblique; font-weight: 600"><i class="fa fa-archive"></i>&nbsp;&nbsp; Maintenance</h5>
							</div>
						</div>
						<div class="d-grid" style="border: 0px solid;">
							<div class="form-group" style="border: 0px solid;">
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:Label ID="lblEditAccessMaintenance" runat="server" Text="Editable Access " class="input-label-cubic"></asp:Label>
									</div>

								</div>
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="col-md-12 checkbox-column form-group" style="border: 0px solid;">
										<asp:CheckBoxList ID="ChkEditAccessMaintenance" runat="server" class="col-md-6" CssClass="CheckBoxBackgrounds" AutoPostBack="true" RepeatLayout="Table" style="border: 0px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding-top: 10px">
											<asp:ListItem Value="E_BankStateM">Bank Statement</asp:ListItem>
											<asp:ListItem Value="E_InvM">Invoice</asp:ListItem>
											<asp:ListItem Value="E_PoM">Purchase Order</asp:ListItem>
											<asp:ListItem Value="E_DoM">Delivery Order</asp:ListItem>
											<asp:ListItem Value="E_QoM">Quotation</asp:ListItem>
										</asp:CheckBoxList>
									</div>
								</div>
							</div>
							<div class="form-group" style="border: 0px solid;">
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:Label ID="lblViewAccessMaintenance" runat="server" Text="View Access" class="input-label-cubic"></asp:Label>
									</div>
								</div>

								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="col-md-12 form-group checkbox-column" style="border: 0px solid;">
										<asp:CheckBoxList ID="ChkViewAccessMaintenance" runat="server" AutoPostBack="True" CssClass="CheckBoxBackgrounds" CellPadding="3" CellSpacing="3" RepeatLayout="Flow" style="border: 0px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding-top: 10px">
											<asp:ListItem Value="V_BankStateM">Bank Statement</asp:ListItem>
											<asp:ListItem Value="V_InvM">Invoice</asp:ListItem>
											<asp:ListItem Value="V_PoM">Purchase Order</asp:ListItem>
											<asp:ListItem Value="V_DoM">Delivery Order</asp:ListItem>
											<asp:ListItem Value="V_QoM">Quotation</asp:ListItem>
										</asp:CheckBoxList>
									</div>
								</div>
							</div>
						</div>

						<br />

						<%--Row  Report Setting--%>
						<div class="form-row">
							<div class="form-group col-md-12">
								<h5 style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 15px; font-style: oblique; font-weight: 600"><i class="fa fa-file-text"></i>&nbsp;&nbsp; Report</h5>
							</div>
						</div>
						<div class="d-grid">
							<div class="form-group" style="border: 0px solid;">
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="form-group col-md-12" style="border: 0px solid;">
										<asp:Label ID="Label3" runat="server" Text="View Access " class="input-label-cubic"></asp:Label>
									</div>

								</div>
								<div class="form-row" style="border: 0px solid; width: 100%">
									<div class="col-md-12 checkbox-column form-group" style="border: 0px solid;">
										<asp:CheckBoxList ID="ChkViewAccessReport" runat="server" class="col-md-6" CssClass="CheckBoxBackgrounds" AutoPostBack="true" RepeatLayout="Table" style="border: 0px solid; font-size: 14px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding-top: 10px">
											<asp:ListItem Value="V_ProjR">Project Report</asp:ListItem>
										</asp:CheckBoxList>
									</div>
								</div>

							</div>
						</div>

						<br />
						<%--  Submit button row--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-6">
							</div>
							<div class="form-group col-md-6">

								<a data-bs-toggle="modal" data-bs-target="#ConfirmationModalMessage" data-bs-backdrop="static">
									<button class="btn-save" style="float: left;" onclick="validation()">Create</button>
								</a>

							</div>
						</div>

						<!-- Modal-->
						<!-- Confirmation Modal-->
						<div class="modal fade" id="ConfirmationModalMessage" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
							<div class="modal-dialog" role="document">
								<div class="modal-content">
									<div class="modal-header Modal-Confirmation-Cubic">
										<h5 class="modal-title" id="ConfirmationModalTitle">Confirmation</h5>
										<button class="close" type="button" data-bs-dismiss="modal" aria-label="Close" aria-hidden="true" hidden>
											<span aria-hidden="true">×</span>
										</button>
									</div>
									<div class="modal-body" id="ConfirmationModalContent">Are You Sure Want To Save?</div>
									<div class="modal-footer">
										<button class="btn-cancel" type="button" data-bs-dismiss="modal">Cancel</button>
										<asp:Button ID="BtnConfirmSave" runat="server" class="btn-save" Text="Save" OnClick="BtnConfirmSave_Click" />
									</div>
								</div>
							</div>
						</div>

						<!-- Success/Fail Message Modal-->
						<div class="modal fade" id="ModalMessage" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
							<div class="modal-dialog" role="document">
								<div class="modal-content">
									<div class="modal-header">
										<h5 class="modal-title" id="MessageModalTitle">Success</h5>
										<button class="close" type="button" data-bs-dismiss="modal" aria-label="Close" aria-hidden="true" hidden>
											<span aria-hidden="true">x</span>
										</button>
									</div>
									<div class="modal-body" id="MessageModalContent"></div>
									<div class="modal-footer">
										<button class="btn-cancel" type="button" onclick="closeModal()">Close</button>
									</div>
								</div>
							</div>
						</div>

						<!-- Fail Message Modal -->
						<div class="modal fade" id="ErrorModalMessage" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
							<div class="modal-dialog" role="document">
								<div class="modal-content">
									<div class="modal-header Modal-Error-Cubic">
										<h5 class="modal-title" id="ErrorModalTitle">Error</h5>
										<button class="close" type="button" data-bs-dismiss="modal" aria-label="Close" aria-hidden="true" hidden>
											<span aria-hidden="true">x</span>
										</button>
									</div>
									<div class="modal-body" id="ErrorModalContent">Please fill in required field</div>
									<div class="modal-footer">
										<button class="btn-cancel" type="button" onclick="closeModal()">Close</button>
									</div>
								</div>
							</div>
						</div>
						<%-- Error --%>
					</ContentTemplate>
					<Triggers>
						<asp:PostBackTrigger ControlID="BtnConfirmSave" />
					</Triggers>
				</asp:UpdatePanel>
			</div>
		</div>

		<br />
	</div>

</asp:Content>
