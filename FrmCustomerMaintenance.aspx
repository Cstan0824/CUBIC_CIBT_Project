<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmCustomerMaintenance.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmCustomerMaintenance" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
	<meta name="viewport" content="width=device-width, initial-scale=1" />
	<link href="Content/SubSite.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />
	<link href="Content/Modal.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />


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
			$("#ModalConfirmation").modal('hide');
			var title = document.getElementById('MessageModalTitle');
			var content = document.getElementById('MessageModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ModalMessage").modal('show');
			// Reinitiallize DataTable
			$('#CustMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
		}

		function showErrorModal(status, message) {
			$("#ModalConfirmation").modal('hide');
			var title = document.getElementById('ErrorModalTitle');
			var content = document.getElementById('ErrorModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ErrorModalMessage").modal('show');
			// Reinitiallize DataTable
			$('#CustMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
		}

		function validation() {
			// Drop List Field
			var ddlCustMode = $('#<%= ddlCustomerMode.ClientID %>').val();
			var DrpListCustID = $('#<%= DrpListCustomerID.ClientID %>').val();

			// Text Field
			var CustUsername = $('#<%= txtCustomerUsername.ClientID %>').val();
			var CustPhoneNumber = $('#<%= txtCustomerPhoneNumber.ClientID %>').val();


			//Validate Bank Statement Mode
			if (!ddlCustMode || ddlCustMode.trim() === "") {
				showErrorModal('Failed', 'Please select the mode.');
				return;
			}
			else { // Update or Create Mode

				// Validate Customer ID (only in Update Mode)
				if (ddlCustMode === "U" && (!DrpListCustID || DrpListCustID.trim() === "")) {
					showErrorModal('Failed', 'Kindly select the Customer ID.');
					return;
				}
				// Validate Username
				if (!CustUsername || CustUsername.trim() === "") {
					showErrorModal('Failed', 'Please enter the Customer Username.');
					return;
				}
				// Validate Phone Number
				if (!CustPhoneNumber || CustPhoneNumber.trim() === "") {
					showErrorModal('Failed', 'Please enter the Customer Phone Number.');
					return;
				}
				//Validate Username length
				if (CustUsername.length > 50) {
					showErrorModal('Failed', 'Customer Username should be 50 characters or fewer.');
					return;
				}
				//Validate Phone Number format (+60XX XXX-XXXX)
				const phonePattern = /^\+60\d{2} \d{3}-\d{4}$/;
				if (!phonePattern.test(CustPhoneNumber)) {
					showErrorModal('Failed', 'Phone number must be in the format +60XX XXX-XXXX.');
					return;
				}
			}
			return;
		}

		document.title = "Customer Maintenance";

		//Data Table Filter
		$(document).ready(function () {
			var table = $('#CustMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			$("#FilterStatus").on("change", function () {
				table.columns(3).search($(this).val()).draw();
			});
		});

		//Ensures the DataTable and the title always displayed in expected result after PostBack
		//Script will be executed after the AJAX request from Update Panel has completed
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			$('#CustMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			document.title = "Customer Maintenance";
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
			<div class="container-fluid">
				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>
				<!-- Customer -->
				<div class="card shadow mb-4" runat="server" ID="E_CustomerM" Visible="false">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Customer Maintenance</h6>
					</div>

					<div class="card-body">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableCustomerScript="True" />

						<%-- row Select Mode/Customer ID--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblCustomerddl" runat="server" Text="Select Mode" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="ddlCustomerMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlCustomerMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">Create new</asp:ListItem>
									<asp:ListItem Value="U">Update details</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblCustomerID" runat="server" Text="Customer ID" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:DropDownList ID="DrpListCustomerID" Visible="false" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="DrpListCustomerID_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator1" runat="server" Operator="Equal" ControlToValidate="DrpListCustomerID" ValueToCompare="" Errormessage="Please select a Customer" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>
							</div>
						</div>
						<%-- row Customer Name/Phone Number --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblCustomerUsername" runat="server" Text="Customer Username" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtCustomerUsername" runat="server" class="form-control input-text-cubic-14" ReadOnly="True" placeholder="Customer Username"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtCustomerUsername" ErrorMessage="Please enter Customer Username" ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblCustomerPhoneNumber" runat="server" Text="Customer Phone Number" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtCustomerPhoneNumber" runat="server" class="form-control input-text-cubic-14" ReadOnly="True" placeholder="+60XX XXX-XXXX"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RegularExpressionValidator runat="server" ID="RegularExpressionValidator1" ControlToValidate="txtCustomerPhoneNumber" ValidationExpression="^\+60\d{2} \d{3}-\d{4}$" ErrorMessage="Please follow the format (+60XX XXX-XXXX)" ForeColor="Red" BorderStyle="None">*</asp:RegularExpressionValidator>
							</div>
						</div>
						<%-- row Customer Status--%>
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
						<%--  Submit button row row-margin-btm-cubic--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group form-group col-md-9">
							</div>

							<div class="form-group col-md-3">
							</div>
						</div>

					</div>
				</div>


				<hr class="cssContentHeaderLine" />
				<%-- Datagrid--%>

				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>

				<!-- DataTales -->

				<div class="card shadow mb-4" runat="server" ID="V_CustomerM" Visible="false">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Customer Table</h6>
					</div>
					<div class="card-body">
						<div class="row row-margin-btm-cubic">
							<div class="col">
								<div class="col-md-12">
									<div class="table-responsive">
										<main>

											<div class="card-body">
												<div style="overflow-x: scroll; height: 100%; min-width: 350px;">
													<div class="datatable">
														<div id="dataTable_wrapper" class="dataTables_wrapper dt-bootstrap4">
															<asp:Repeater ID="CustMRepeater" runat="server">
																<HeaderTemplate>
																	<%-- Filter --%>
																	<div class="row mb-3">
																		<!-- Status Filter -->
																		<div class="col-md-3">
																			<label for="FilterStatus" class="form-label">Status:</label>
																			<select id="FilterStatus" class="form-select">
																				<option value="">All</option>
																				<option value="O">Opening</option>
																				<option value="C">Closed</option>
																			</select>
																		</div>
																		<div class="col-md-9">
																		</div>
																	</div>
																	<table id="CustMTable" class="table table-bordered table-hover table-striped" style="width: 100%">
																		<thead class="table table-success">
																			<th>Customer Name</th>
																			<th>Customer Phone Number</th>
																			<th>Status</th>
																			<th></th>
																		</thead>
																</HeaderTemplate>
																<ItemTemplate>
																	<tr>
																		<td><%#Eval("CUST_NAME") %></td>
																		<td><%#Eval("CUST_PHONE_NUMBER") %></td>
																		<td><%#Eval("CUST_STATUS") %></td>
																		<td>
																			<button type="button" style="height: 20px; width: 20px; padding: 0;" class="btn btn-muted rounded-circle" data-bs-toggle="dropdown"><i class="fa-solid fa-ellipsis-vertical"></i></button>
																			<ul class="dropdown-menu">
																				<li>
																					<asp:Button class="dropdown-item" ID="DeleteCustomer" runat="server" Text="Delete" CommandArgument='<%# Eval("CUST_NO") %>' OnClick="DeleteCustomer_Click" />
																				</li>
																				<li>
																					<asp:Button class="dropdown-item" ID="EditCustomer" runat="server" Text="Edit" CommandArgument='<%# Eval("CUST_NO") %>' OnClick="EditCustomer_Click" />
																				</li>
																			</ul>
																		</td>
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
											
										</main>
									
									</div>
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
						</div>
					</div>
				</div>
			</div>
		</ContentTemplate>
		<Triggers>
			<asp:PostBackTrigger ControlID="BtnConfirmSave" />
		</Triggers>
	</asp:UpdatePanel>


	<br />


</asp:Content>
