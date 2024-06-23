<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmDeliveryOrderMaintenance.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmDeliveryOrderMaintenance" %>

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
			$("#ModalConfirmation").modal('hide');
			var title = document.getElementById('MessageModalTitle');
			var content = document.getElementById('MessageModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ModalMessage").modal('show');
			// Reinitiallize DataTable
			$('#DOMTable').DataTable({
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
			$('#DOMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
		}
		function validation() {
			// Drop List Field
			var ddlDOMode = $('#<%= ddlDeliveryOrderMode.ClientID %>').val();
			var DrpListProjCode = $('#<%= DrpListProjectCode.ClientID %>').val();
			var DrpListRevisionNo = $('#<%= DrpListRevisionNo.ClientID %>').val();

			// Text Field
			var RevisionNo = $('#<%= txtRevisionNo.ClientID %>').val();
			var DeliveryOrderDate = $('#<%= txtDeliveryOrderDate.ClientID %>').val();
			var DeliveryAddress = $('#<%= txtDeliveryAddress.ClientID %>').val();
			var ArrivalDate = $('#<%= txtArrivalDate.ClientID %>').val();


			// Validate Invoice Mode
			if (!ddlDOMode || ddlDOMode.trim() === "") {
				showErrorModal('Failed', 'Please select the mode.');
				return;
			}
			else { // Update or Create Mode
				//Validate Project Code
				if (!DrpListProjCode || DrpListProjCode.trim() === "") {
					showErrorModal('Failed', 'Kindly select the Project Code.');
					return;
				}

				// Validate Revision No[Drop List] (only in Update Mode)
				if (ddlDOMode === "U" && (!DrpListRevisionNo || DrpListRevisionNo.trim() === "")) {
					showErrorModal('Failed', 'Kindly select the Revision No.');
					return;
				}

				//Validate Revision No[Text] (only in Create Mode)
				if (ddlDOMode === "C" && (!RevisionNo || RevisionNo.trim() === "")) {
					showErrorModal('Failed', 'Please enter the Revision No.');
					return;
				}

				// Validate Delivery Address
				if (!DeliveryAddress || DeliveryAddress.trim() === "") {
					showErrorModal('Failed', 'Please enter the Delivery Address.');
					return;
				}
				// Validate Delivery Order Date
				if (!DeliveryOrderDate || DeliveryOrderDate.trim() === "") {
					showErrorModal('Failed', 'Please enter the Delivery Order Date.');
					return;
				}
				// Validate Arrival Date
				if (!ArrivalDate || ArrivalDate.trim() === "") {
					showErrorModal('Failed', 'Please enter the Arrival Date.');
					return;
				}

				//Validate Delivery Address length
				if (DeliveryAddress.length > 500) {
					showErrorModal('Failed', 'Delivery Address should be 500 characters or fewer.');
					return;
				}

				//Validate Revision No length
				if (ddlDOMode === "C" && RevisionNo.length > 20) {
					showErrorModal('Failed', 'Revision No should be 20 characters or fewer.');
					return;
				}

				// Parse and Validate the logical of the Arrival Date 
				const inputArrivalDate = new Date(ArrivalDate);
				if (isNaN(inputArrivalDate.getTime())) {
					showErrorModal('Failed', 'Please enter a valid Arrival Date.');
					return;
				}

				// Parse and Validate the logical of the Delivery Order Date 
				const inputDeliveryOrderDate = new Date(DeliveryOrderDate);
				if (isNaN(inputDeliveryOrderDate.getTime())) {
					showErrorModal('Failed', 'Please enter a valid Delivery Order Date.');
					return;
				}

				//Current Date
				const currDate = new Date();

				// Calculate the date 100 years ago
				const minDate = new Date();
				minDate.setFullYear(minDate.getFullYear() - 100);

				// Validate the range (100 years ago < Arrival Date  <= Current Date)
				if (inputArrivalDate < currDate) {
					showErrorModal('Failed', 'The Arrival Date must be today or after.');
					return;
				}

				// Validate the range (100 years ago < Delivery Order Date  <= Current Date)
				if (inputDeliveryOrderDate > currDate || inputDeliveryOrderDate < minDate) {
					showErrorModal('Failed', 'The Delivery Order Date must be between today and 100 years ago.');
					return;
				}
			}
			return;
		}
		document.title = "Delivery Order Maintenance";

		//Data Table Filter
		$(document).ready(function () {
			var table = $('#DOMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			$("#FilterStatus").on("change", function () {
				table.columns(4).search($(this).val()).draw();
			});

			$('#filterDODate').on('click', function () {

				$.fn.dataTable.ext.search.push(
					function (settings, data, dataIndex) {
						var min = $('#DateDOFrom').val();
						var max = $('#DateDOTo').val();
						var date = data[3]; // Use data for the date column

						// Check if both min and max have values
						if (min && max) {
							min = new Date(min);
							min.setDate(min.getDate() - 1);
							max = new Date(max);
							var date = new Date(date);

							// Ensure the Date is in range
							var DateRangeValid = ((min <= date) && (max > date));
							return DateRangeValid;
						} else {
							return true;
						}
					}
				);
				table.draw();

			});

			$('#filterArrivalDate').on('click', function () {

				$.fn.dataTable.ext.search.push(
					function (settings, data, dataIndex) {
						var min = $('#DateArrivalFrom').val();
						var max = $('#DateArrivalTo').val();
						var date = data[3]; // Use data for the date column

						// Check if both min and max have values
						if (min && max) {
							min = new Date(min);
							min.setDate(min.getDate() - 1);
							max = new Date(max);
							var date = new Date(date);

							// Ensure the Date is in range
							var DateRangeValid = ((min <= date) && (max > date));
							return DateRangeValid;
						} else {
							return true;
						}
					}
				);
				table.draw();

			});
		});

		$("#form1").attr('enctype', 'multipart/form-data');

		//Ensures the DataTable and the title always displayed in expected result after PostBack
		//Script will be executed after the AJAX request from Update Panel has completed
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			$('#DOMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			document.title = "Delivery Order Maintenance";
		});

		var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
		var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
			return new bootstrap.Popover(popoverTriggerEl)
		})
	</script>


	&nbsp;

    <asp:ScriptManager ID="ScriptManagerMain" runat="server"></asp:ScriptManager>
	<asp:UpdatePanel ID="UpdatePanelLotInfor" runat="server">
		<ContentTemplate>
			<%--    This is another new design template--%>
			<div class="container-fluid">
				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>
				<!-- Delivery Order -->
				<div class="card shadow mb-4" runat="server" ID="E_DoM" Visible="false">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Delivery Order Maintenance</h6>
					</div>

					<div class="card-body">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableClientScript="True" />

						<%--  row Delivery Order ddl--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4" style="border: 0px solid">
								<asp:Label ID="lblDeliveryOrderddl" runat="server" Text="Select Mode" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="ddlDeliveryOrderMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlDeliveryOrderMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">--Create New--</asp:ListItem>
									<asp:ListItem Value="U">--Update Details--</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-4">
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<%-- row Project Code/Revision No --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblProjectCodeTxt" runat="server" Text="Project Code" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListProjectCode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="DrpListProjectCode_SelectedIndexChanged">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblRevisionNo" runat="server" Text="Revision No. " class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListRevisionNo" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" Visible="false" OnSelectedIndexChanged="DrpListRevisionNo_SelectedIndexChanged">
								</asp:DropDownList>
								<asp:TextBox runat="server" ID="txtRevisionNo" class="form-control input-textbox-cubic-16" ValidateRequestMode="Enabled" ReadOnly="true"></asp:TextBox>

							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<br />
						<%-- row Delivery Order Date/Arrival Date--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblDeliveryOrderDate" runat="server" Text="Delivery Order Date" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtDeliveryOrderDate" runat="server" TextMode="Date" class="form-control"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblArrivalDate" runat="server" Text="Arrival Date" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtArrivalDate" runat="server" TextMode="Date" class="form-control"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<br />
						<%-- Address --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-11">
								<asp:Label ID="lblDeliveryAddress" runat="server" Text="Deliver Address" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtDeliveryAddress" runat="server" class="form-control"></asp:TextBox>
							</div>
							<div class="form-group col-md-1">
							</div>
						</div>
						<br />
						<%--  row Delivery Order Status/Upload File--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblStatus" runat="server" Text="Status " class="input-label-cubic"></asp:Label>
								<asp:RadioButtonList ID="rbStatus" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" class="input-radio-cubic-20">
									<asp:ListItem Selected="True" Value="O">Active</asp:ListItem>
									<asp:ListItem Value="X">Inactive</asp:ListItem>
								</asp:RadioButtonList>
							</div>
							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblFileUpload" runat="server" Text="File Upload:" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:FileUpload ID="ChooseFileUpload" runat="server" Visible="false" />
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>

					</div>
					<br />
					<%-- row Save--%>
					<div class="row row-margin-btm-cubic">
						<div class="form-group col-md-6"></div>
						<div class="form-group col-md-6">
							<a data-bs-toggle="modal" data-bs-target="#ConfirmationModalMessage" data-bs-backdrop="static">
								<button class="btn-save" style="float: left;" onclick="validation()">Create</button>
							</a>
						</div>
					</div>
				</div>



				<hr class="cssContentHeaderLine" />
				<%-- Datagrid--%>

				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>

				<!-- DataTales -->
				<div class="card shadow mb-4" runat="server" ID="V_DoM" Visible="false">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Delivery Order Table</h6>
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
															<asp:Repeater ID="DOMRepeater" runat="server">
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
																		<!-- DO Date From -->
																		<div class="col-md-4">
																			<label for="DateFrom" class="form-label">DO Date From:</label>
																			<input id="DODateFrom" type="date" class="form-control" />
																		</div>

																		<!-- DO Date To -->
																		<div class="col-md-4">
																			<label for="DateTo" class="form-label">DO Date To:</label>
																			<input id="DODateTo" type="date" class="form-control" />
																		</div>

																		<!-- Filter Button -->
																		<div class="col-md-1 align-self-end">
																			<input id="filterDODate" type="button" class="btn btn-primary w-100" value="Filter" />
																		</div>
																	</div>
																	<div class="row mb-3">
																		<div class="col-md-3">
																		</div>
																		<!-- Arrival Date From -->
																		<div class="col-md-4">
																			<label for="DateFrom" class="form-label">Arrival Date From:</label>
																			<input id="ArrivalDateFrom" type="date" class="form-control" />
																		</div>

																		<!-- Arrival Date To -->
																		<div class="col-md-4">
																			<label for="DateTo" class="form-label">Arrival Date To:</label>
																			<input id="ArrivalDateTo" type="date" class="form-control" />
																		</div>

																		<!-- Filter Button -->
																		<div class="col-md-1 align-self-end">
																			<input id="filterArrivalDate" type="button" class="btn btn-primary w-100" value="Filter" />
																		</div>
																	</div>
																	<br />
																	<table id="DOMTable" class="table table-bordered table-hover table-striped mydatatable " style="width: 100%">
																		<thead class="table table-success">
																			<th>Project No</th>
																			<th>Revision No</th>
																			<th>Arrival Date</th>
																			<th>Date</th>
																			<th>Status</th>
																			<th></th>
																		</thead>
																</HeaderTemplate>
																<ItemTemplate>
																	<tr data-bs-toggle="popover" data-bs-trigger="hover" title="DO Address" data-bs-content="<%# Eval("DO_ADDRESS") %>">
																		<td><%#Eval("PROJ_NO") %></td>
																		<td><%#Eval("DOC_REVISION_NO") %></td>
																		<td><%#Eval("DO_ARRIVAL_DATE","{0:yyyy/MM/dd}") %></td>
																		<td><%#Eval("DOC_DATE","{0:yyyy/MM/dd}") %></td>
																		<td><%#Eval("DOC_STATUS") %></td>
																		<td>
																			<button type="button" style="height: 20px; width: 20px; padding: 0;" class="btn btn-muted rounded-circle" data-bs-toggle="dropdown"><i class="fa-solid fa-ellipsis-vertical"></i></button>
																			<ul class="dropdown-menu">
																				<li>
																					<asp:Button class="dropdown-item" ID="DeleteDoc" runat="server" Text="Delete" CommandArgument='<%# Eval("DOC_NO") %>' OnClick="DeleteDoc_Click" /></li>
																				<li>
																					<asp:Button class="dropdown-item" ID="EditDoc" runat="server" Text="Edit" CommandArgument='<%# Eval("DOC_NO") %>' OnClick="EditDoc_Click" /></li>
																				<li>

																					<asp:HyperLink class="dropdown-item" ID="DownloadDoc" runat="server" NavigateUrl='<%# F_GetDeliveryOrderFileUrl(Eval("DOC_NO").ToString()) %>' Text="View File" Target="_blank" Visible='<%# !F_ShowLink(Eval("DOC_UPL_PATH").ToString()) %>'></asp:HyperLink>
																					<span class="dropdown-item-text text-muted" runat="server" Visible='<%# F_ShowLink(Eval("DOC_UPL_PATH").ToString()) %>'>No File Available</span>

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
											<%-- </div>--%>
										</main>
										<%--   <br />--%>
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
			</div>
		</ContentTemplate>
		<Triggers>
			<asp:PostBackTrigger ControlID="BtnConfirmSave" />
		</Triggers>
	</asp:UpdatePanel>


	<br />


</asp:Content>



