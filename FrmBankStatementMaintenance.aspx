<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmBankStatementMaintenance.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmBankStatementMaintenance" %>

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
			$('#BSMTable').DataTable({
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
			$('#BSMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
		}

		function validation() {
			// Drop List Field
			var ddlBSMode = $('#<%= ddlBankStateMode.ClientID %>').val();
			var DrpListBankType = $('#<%= DrpListBankType.ClientID %>').val();
			var DrpListRemark = $('#<%= DrpListRemark.ClientID %>').val();

			// Text Field
			var StatementDate = $('#<%= txtStatementDate.ClientID %>').val();

			//Validate Bank Statement Mode
			if (!ddlBSMode || ddlBSMode.trim() === "") {
				showErrorModal('Failed', 'Please select the mode.');
				return;
			}
			else { // Update or Create Mode
				//Validate Bank Type
				if (!DrpListBankType || DrpListBankType.trim() === "") {
					showErrorModal('Failed', 'Please select the Bank Type.');
					return;
				}

				// Validate Statement Remark (only in Update Mode)
				if (ddlBSMode === "U" && (!DrpListRemark || DrpListRemark.trim() === "")) {
					showErrorModal('Failed', 'Kindly select the Statement Remark.');
					return;
				}

				//Validate Statement Date
				if (!StatementDate || StatementDate.trim() === "") {
					showErrorModal('Failed', 'Please enter the Statement Date.');
					return;
				}

				// Parse and validate the logical of the statement date
				const inputDate = new Date(StatementDate);
				if (isNaN(inputDate.getTime())) {
					showErrorModal('Failed', 'Please enter a valid Statement Date.');
					return;
				}

				//Current Date
				const currDate = new Date();

				// Calculate the date 100 years ago
				const minDate = new Date();
				minDate.setFullYear(minDate.getFullYear() - 100);

				// Validate the range (100 years ago < Statement Date <= Current Date)
				if (inputDate > currDate || inputDate < minDate) {
					showErrorModal('Failed', 'The Statement Date must be between today and 100 years ago.');
					return;
				}
			}
			return;
		}

		document.title = "Bank Statement Maintenance";

		//Data Table Filter
		$(document).ready(function () {
			var table = $('#BSMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true,
			});
			$("#FilterStatus").on("change", function () {
				table.columns(3).search($(this).val()).draw();
			});

			$('#filterDate').on('click', function () {

				$.fn.dataTable.ext.search.push(
					function (settings, data, dataIndex) {
						var min = $('#DateFrom').val();
						var max = $('#DateTo').val();
						var date = data[2]; // Use data for the date column

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
			$('#BSMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			document.title = "Bank Statement Maintenance";
		});
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
				<!-- Bank Statement -->
				<div class="card shadow mb-4" runat="server" ID="E_BankStateM" Visible="false">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Bank Statement Maintenance</h6>
					</div>

					<div class="card-body">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableClientScript="True" />
						<%-- Bank Statement Mode/Remark --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblBankStateddl" runat="server" Text="Select Code" class="input-label-cubic"></asp:Label>
								<%-- <div class="inputWithIcon">--%>
								<asp:DropDownList ID="ddlBankStateMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlBankStateMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">Create new</asp:ListItem>
									<asp:ListItem Value="U">Update details</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblRemark" runat="server" Text="Statement Remark" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:DropDownList ID="DrpListRemark" runat="server" AutoPostBack="true" class="form-control" Visible="false" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="DrpListRemark_SelectedIndexChanged">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>

						<%--  row Statement Status/Date --%>
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
								<asp:Label ID="lblStatementDate" runat="server" Text="Statement Date" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtStatementDate" ClientIDMode="Static" runat="server" ReadOnly="true" TextMode="Date" class="form-control" AutoPostBack="false"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
							</div>
						</div>
						<br />
						<%-- row Bank Type/Document Upload File--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblBankTypeDropList" runat="server" Text="Bank Type" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListBankType" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblFileUpload" runat="server" Text="File Upload:" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:FileUpload ID="ChooseFileUpload" class="fa-file" AutoPostBack="true" runat="server" />
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


				<hr class="cssContentHeaderLine" />
				<%-- Datagrid--%>

				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>

				<!-- DataTales -->
				<div class="card shadow mb-4" runat="server" ID="V_BankStateM" Visible="false">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Bank Statement Table</h6>
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
															<asp:Repeater ID="DocMRepeater" runat="server">
																<HeaderTemplate>
																	<%-- Filter --%>
																	<div class="row mb-3">
																		<!-- Status Filter -->
																		<div class="col-md-3">
																			<label for="FilterStatus" class="form-label">Status:</label>
																			<select id="FilterBSStatus" class="form-select">
																				<option value="">All</option>
																				<option value="O">Opening</option>
																				<option value="C">Closed</option>
																			</select>
																		</div>

																		<!-- Date From -->
																		<div class="col-md-4">
																			<label for="DateFrom" class="form-label">Date From:</label>
																			<input id="BSDateFrom" type="date" class="form-control" />
																		</div>

																		<!-- Date To -->
																		<div class="col-md-4">
																			<label for="DateTo" class="form-label">Date To:</label>
																			<input id="BSDateTo" type="date" class="form-control" />
																		</div>

																		<!-- Filter Button -->
																		<div class="col-md-1 align-self-end">
																			<input id="filterDate" type="button" class="btn btn-primary w-100" value="Filter" />
																		</div>
																	</div>
																	<table id="BSMTable" class="table table-bordered table-hover table-striped mydatatable " style="width: 100%">
																		<thead class="table table-success">
																			<th>Bank Statement No.</th>
																			<th>Bank Type</th>
																			<th>Date</th>
																			<th>Status</th>
																			<th></th>
																		</thead>
																</HeaderTemplate>
																<ItemTemplate>
																	<tr>
																		<td><%#Eval("BS_NO") %></td>
																		<td><%#F_DisplayBankType(int.Parse(Eval("BS_TYPE_ID").ToString())) %></td>
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
																					<asp:Button class="dropdown-item" ID="DownloadDoc" runat="server" Text="Download" CommandArgument='<%# Eval("DOC_NO") %>' OnClick="DownloadDoc_Click" /></li>
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
