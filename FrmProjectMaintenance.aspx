<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmProjectMaintenance.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmProjectMaintenance" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
	<meta name="viewport" content="width=device-width, initial-scale=1" />
	<link href="Content/Modal.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />
	<link href="Content/SubSite.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />


	<style>
		.inputWithIcon input[type=text] {
			padding-left: 40px;
		}

		/*.inputddl{
          border: 1px solid;
          font-size:14px;
          font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
          min-width:140px;
      }*/
	</style>


	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
	<script src="js/dataTables.min.js"></script>
	<script src="js/dataTables.bootstrap4.min.js"></script>
	<script src="assets/demo/datatables-demo.min.js"></script>

	<script>
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
			$("#ConfirmationModalMessage").modal('hide');
			var title = document.getElementById('MessageModalTitle');
			var content = document.getElementById('MessageModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ModalMessage").modal('show');
		}

		function showErrorModal(status, message) {
			$("#ConfirmationModalMessage").modal('hide');
			var title = document.getElementById('ErrorModalTitle');
			var content = document.getElementById('ErrorModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ErrorModalMessage").modal('show');
		}

		function validation() {
			// Drop List Field
			var ddlProjMode = $('#<%= ddlProjectMode.ClientID %>').val();
			var DrpListProjCode = $('#<%= DrpListProjectCode.ClientID %>').val();
			var DrpListCustCode = $('#<%= DrpListCustomerCode.ClientID %>').val();

			// Text Field
			var ProjectName = $('#<%= txtProjectName.ClientID %>').val();
			var ProjectDate = $('#<%= txtProjectDate.ClientID %>').val();


			// Validate Project Mode
			if (!ddlProjMode || ddlProjMode.trim() === "") {
				showErrorModal('Failed', 'Please select the mode.');
				return;
			}
			else { // Update or Create Mode
				//Validate Project Code (only in Update Mode)
				if (ddlProjMode === "U" && (!DrpListProjCode || DrpListProjCode.trim() === "")) {
					showErrorModal('Failed', 'Kindly select the Project Code.');
					return;
				}

				// Validate Project Date
				if (!ProjectDate || ProjectDate.trim() === "") {
					showErrorModal('Failed', 'Please enter the Project Date.');
					return;
				}

				// Validate Project Name
				if (!ProjectName || ProjectName.trim() === "") {
					showErrorModal('Failed', 'Please enter the Project Name.');
					return;
				}

				// Validate Project Name length
				if (ProjectName.length > 50) {
					showErrorModal('Failed', 'Customer Username should be 50 characters or fewer.');
					return;
				}

				// Parse and Validate the logical of the Project Date
				const inputProjectDate = new Date(ProjectDate);
				if (isNaN(inputProjectDate.getTime())) {
					showErrorModal('Failed', 'Please enter a valid Project Date.');
					return;
				}

				//Current Date
				const currDate = new Date();

				// Calculate the date 100 years ago
				const minDate = new Date();
				minDate.setFullYear(minDate.getFullYear() - 100);

				// Validate the range (100 years ago < Project Date  <= Current Date)
				if (inputProjectDate > currDate || inputProjectDate < minDate) {
					showErrorModal('Failed', 'The Project Date must be between today and 100 years ago.');
					return;
				}
			}
			return;
		}

		document.title = "Project Maintenance";



		//$(document).click(function (e) {
		//	if ($(e.target).is(#ModalMessage)) {
		//		$('#ModalMessage').fadeOut(500);
		//	}
		//});

	</script>


	&nbsp;

    <asp:ScriptManager ID="ScriptManagerMain" runat="server"></asp:ScriptManager>
	<asp:UpdatePanel ID="UpdatePanelLotInfor" runat="server">
		<ContentTemplate>
			<%--    This is another new design template--%>
			<div class="container-fluid" runat="server" ID="E_ProjM" Visible="false">
				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>
				<!-- Project -->
				<div class="card shadow mb-4">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Project Maintenance</h6>
					</div>

					<div class="card-body">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableProjectScript="True" />

						<%-- row Project Mode/Name --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblProjectModeddl" runat="server" Text="Select Mode" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="ddlProjectMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlProjectMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">Create new</asp:ListItem>
									<asp:ListItem Value="U">Update Details</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2"></div>
							<div class="form-group col-md-4">
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<%--  row Project Code/Customer Code --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblCustomerCodeTxt" runat="server" Text="Customer Code" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListCustomerCode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblProjectCodeTxt" runat="server" Text="Project Code" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListProjectCode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ProjectCodeDrpList_SelectedIndexChanged">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>

						</div>
						<%-- row Project Name/Date--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="Label1" runat="server" Text="Project ID" class="input-label-cubic">Project Name: </asp:Label>
								<asp:TextBox ID="txtProjectName" ProjectIDMode="Static" runat="server" ReadOnly="false" class="form-control" AutoPostBack="false"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblProjectDate" runat="server" Text="Project Date" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtProjectDate" ProjectIDMode="Static" runat="server" TextMode="Date" ReadOnly="false" class="form-control" AutoPostBack="false"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
							</div>

						</div>

						<%--  row Project Status --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblStatus" runat="server" Text="Status " class="input-label-cubic"></asp:Label>
								<asp:RadioButtonList ID="rbStatus" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" class="input-radio-cubic-20">
									<asp:ListItem Selected="True" Value="O">Active</asp:ListItem>
									<asp:ListItem Value="C">Inactive</asp:ListItem>
								</asp:RadioButtonList>
							</div>
							<div class="form-group col-md-2"></div>

							<div class="form-group col-md-4">
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<%-- row Save--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-6">
							</div>
							<div class="form-group col-md-6">
								<a data-bs-toggle="modal" data-bs-target="#ConfirmationModalMessage" data-bs-backdrop="static">
									<button class="btn-save" style="float: left;" onclick="validation()">Create</button>
								</a>
							</div>
						</div>
					</div>
				</div>
			</div>
			<!-- Modal-->
			<!-- Confirmation Modal-->
			<div class="modal fade" id="ConfirmationModalMessage" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
				<div class="modal-dialog" role="document">
					<div class="modal-content">
						<div class="modal-header Modal-Confirmation-Cubic">
							<h5 class="modal-title" id="ConfirmationMessageModalTitle">Confirmation</h5>
							<button class="close" type="button" data-bs-dismiss="modal" aria-label="Close" aria-hidden="true" hidden>
								<span aria-hidden="true">×</span>
							</button>
						</div>
						<div class="modal-body" id="ConfirmationMessageModalContent">Are You Sure Want To Save?</div>
						<div class="modal-footer">
							<button class="btn-cancel" type="button" data-bs-dismiss="modal">Cancel</button>
							<asp:Button ID="BtnConfirmSave" runat="server" class="btn-save" Text="Create/Update" OnClick="BtnConfirmSave_Click" />
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


	<br />


</asp:Content>
