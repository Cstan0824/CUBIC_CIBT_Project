<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmClientMaintenance.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmClientMaintenance" %>

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
			$("#ModalMessage").hide();
			$('#ErrorModalMessage').hide();
			$('#ConfirmationModalMessage').hide();
			$('#ModalConfirmation').hide();
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
			// Reinitiallize DataTable
			$('#DocMTable').DataTable();
		}

		function showErrorModal(status, message) {
			$("#ModalConfirmation").modal('hide');
			var title = document.getElementById('ErrorModalTitle');
			var content = document.getElementById('ErrorModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ErrorModalMessage").modal('show');
			// Reinitiallize DataTable
			$('#DocMTable').DataTable();
		}

		document.title = "Bank Statement Maintenance";

		$(document).ready(function () {
			$('#DocMTable').DataTable()
		});

		$('#DocMTable').DataTable({
			"paging": true,
			"searching": true,
			"info": true,
		});

		//$(document).click(function (e) {
		//    if ($(e.target).is(#ModalMessage)) {
		//        $('#ModalMessage').fadeOut(500);
		//    }
		//});

	</script>


	&nbsp;

    <asp:ScriptManager ID="ScriptManagerMain" runat="server"></asp:ScriptManager>
	<asp:UpdatePanel ID="UpdatePanelLotInfor" runat="server">
		<ContentTemplate>
			<div class="container-fluid" runat="server" ID="E_ClientM">
				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>
				<!-- Client -->
				<div class="card shadow mb-4">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Client Maintenance</h6>
					</div>

					<div class="card-body">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableClientScript="True" />

						<%-- row Select Mode/Client ID--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblClientddl" runat="server" Text="Select Mode" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="ddlClientMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlClientMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">Create new</asp:ListItem>
									<asp:ListItem Value="U">Update details</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator2" runat="server" Operator="Equal" ControlToValidate="ddlClientMode" ValueToCompare="" Errormessage="Please select a Mode" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblClientID" runat="server" Text="Client ID" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:DropDownList ID="DrpListClientID" Visible="false" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator1" runat="server" Operator="Equal" ControlToValidate="DrpListClientID" ValueToCompare="" Errormessage="Please select a Client" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>
							</div>
						</div>
						<%-- row Client Name/ Status--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblClientUsername" runat="server" Text="Client Username" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtClientUsername" runat="server" class="form-control input-text-cubic-14" ReadOnly="True" placeholder="Client Username"></asp:TextBox>

							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtClientUsername"  ErrorMessage="Please enter Client Username"  ForeColor="Red" BorderStyle="None"  >*</asp:RequiredFieldValidator>
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblStatus" runat="server" Text="Status " class="input-label-cubic"></asp:Label>
								<asp:RadioButtonList ID="rbStatus" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" class="input-radio-cubic-20">
									<asp:ListItem Selected="True" Value="True">Active</asp:ListItem>
									<asp:ListItem Value="False">Inactive</asp:ListItem>
								</asp:RadioButtonList>
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>

						<%-- row Save--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-6">
							</div>
							<div class="form-group col-md-6">
								<a data-bs-toggle="modal" runat="server" ID="DirectTarget" data-bs-target="#ModalConfirmation" data-bs-backdrop="static">
									<asp:Button ID="BtnSave" runat="server" class="btn-save" Text="Save" OnClick="BtnSave_Click"></asp:Button>
								</a>
							</div>

						</div>
						<%--  Submit button row row-margin-btm-cubic--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group form-group col-md-9">
								<asp:TextBox ID="txtAutoNumber" runat="server" class="form-control" Visible="false"></asp:TextBox>
								<asp:TextBox ID="txtAutoNumberIncreament" runat="server" class="form-control" Visible="false"></asp:TextBox>
								<asp:TextBox ID="txtRunningNo" runat="server" class="form-control" Visible="false"></asp:TextBox>
							</div>

							<div class="form-group col-md-3">
							</div>
						</div>

					</div>
				</div>
				</div>

				<hr class="cssContentHeaderLine" />
				<%-- Datagrid--%>

				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>

				<!-- DataTales Example -->
				<div class="card shadow mb-4" runat="server" ID="V_ClientM">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Client Table</h6>
					</div>
					<div class="card-body">

						<%--    add overflow so that when phone screen can see nicely--%>
						<%--    <div style="overflow-x: scroll; height:100%; min-width:350px;" >--%>



						<%-- <asp:Timer ID="TimerRefreshList" runat="server" Interval="3000" OnTick="TimerRefreshList_Tick"></asp:Timer>--%>

						<div class="row row-margin-btm-cubic">
							<div class="col">
								<div class="col-md-12">
									<div class="table-responsive">
										<main>
											<%--   <div class="card mb-4">
                            <div class="card-header">Employee Master DataTables</div>--%>
											<div class="card-body">
												<div style="overflow-x: scroll; height: 100%; min-width: 350px;">
													<div class="datatable">
														<div id="dataTable_wrapper" class="dataTables_wrapper dt-bootstrap4">
															<asp:Repeater ID="DocMRepeater" runat="server">
																<HeaderTemplate>
																	<table id="DocMTable" class="table table-bordered table-hover table-striped mydatatable " style="width: 100%">
																		<thead class="table table-success">
																			<th>Document No.</th>
																			<th>Document Name</th>
																			<th>Revision No</th>
																			<th>Status</th>
																			<th>Modified By</th>
																			<th>Modified Date</th>

																		</thead>
																</HeaderTemplate>

																<ItemTemplate>
																	<tr>
																		<td><%#Eval("DOC_CODE") %></td>
																		<td><%#Eval("DOC_NAME") %></td>
																		<td><%#Eval("REVISION_NO") %></td>
																		<td><%#Eval("DOC_STATUS") %></td>
																		<td><%#Eval("MODIFIED_BY") %></td>
																		<td><%#Eval("MODIFIED_DATE") %></td>
																	</tr>
																</ItemTemplate>
																<FooterTemplate>
																	</table>
																</FooterTemplate>
															</asp:Repeater>
														</div>
													</div>

												</div>
											</div>
											<%-- </div>--%>
										</main>
										<%--   <br />--%>
									</div>
								</div>
							</div>

							<!-- Modal-->
							<!-- Confirmation Modal-->
							<div class="modal fade" id="ModalConfirmation" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
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
											<%--<asp:Button ID="btnSave" runat="server" class="btn btn-primary" Text="Add" OnClick="btnSave_Click"/>--%>
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
						</div>
					</div>
				</div>

		</ContentTemplate>
	</asp:UpdatePanel>


	<br />


</asp:Content>
