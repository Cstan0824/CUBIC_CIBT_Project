<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmProjectListingDet.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmProjectListingDet" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
	<meta name="viewport" content="width=device-width, initial-scale=1" />
	<link href="Content/Modal.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />
	<link href="Content/SubSite.min.css" rel="stylesheet" type="text/css" media="screen" runat="server" />
	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
	<script src="js/dataTables.min.js"></script>
	<script src="js/dataTables.bootstrap4.min.js"></script>
	<script src="assets/demo/datatables-demo.min.js"></script>
	<style>
		.circle {
			width: 85px;
			height: 85px;
			font-size: 1.2em;
		}

		.DOC_card, .DOC_back_card {
			cursor: pointer;
		}

		.d-flex-card {
			display: flex;
		}
	</style>
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
			$('#DocMTable').DataTable({
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
		}
		document.title = "Project Listing Details Maintenance";

		$(document).ready(function () {
			var table = $('#DocMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			$("#FilterStatus").on("change", function () {
				table.columns(2).search($(this).val()).draw();
			});

			$('#filterDate').on('click', function () {

				$.fn.dataTable.ext.search.push(
					function (settings, data, dataIndex) {
						var min = $('#DateFrom').val();
						var max = $('#DateTo').val();
						var date = data[1]; // Use data for the date column

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

			// Retrieve values using jQuery
			var totalPaidAmountStr = $('#<%= hiddTotalPaidAmount.ClientID %>').val();
			var receivedAmountStr = $('#<%= hiddReceivedAmount.ClientID %>').val();
			var projStatus = $('#<%= hiddProjStatus.ClientID %>').val();

			console.log("ProjStatus: ", projStatus);
			console.log("Total Paid Amount: ", totalPaidAmountStr);
			console.log("Received Amount: ", receivedAmountStr);

			

			// Set title for Total Paid Amount
			$("#TotalPaidAmount").attr("title", "RM" + totalPaidAmountStr);

			// Parse the amounts
			var totalPaidAmount = parseFloat(totalPaidAmountStr);
			var receivedAmount = parseFloat(receivedAmountStr);

			console.log("Parsed Total Paid Amount: ", totalPaidAmount);
			console.log("Parsed Received Amount: ", receivedAmount);

			// Calculate Receivable Amount
			var receivableAmount = totalPaidAmount - receivedAmount;
			console.log("Receivable Amount: ", receivableAmount);

			// Calculate Percentages
			var receivablePercentage = totalPaidAmount !== 0 ? (receivableAmount / totalPaidAmount) * 100 : 0;
			var receivedPercentage = totalPaidAmount !== 0 ? (receivedAmount / totalPaidAmount) * 100 : 0;

			console.log("Receivable Percentage: ", receivablePercentage);
			console.log("Received Percentage: ", receivedPercentage);

			// Update progress bars
			$("#progress-ReceivableAmount").css("width", receivablePercentage + "%");
			$("#progress-ReceivedAmount").css("width", receivedPercentage + "%");
		});

		var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
		var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
			return new bootstrap.Tooltip(tooltipTriggerEl)
		})

		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			$('#DocMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			document.title = "Project Listing Details Maintenance";
			// Set project status color
			var projStatus = $('#<%= hiddProjStatus.ClientID %>').val();
			var statusClass = projStatus == "O" ? "text-success" : "text-danger";
			$("#ProjStatus").removeClass("text-success text-danger").addClass(statusClass);
		});

		
	</script>


	&nbsp;

    <asp:ScriptManager ID="ScriptManagerMain" runat="server"></asp:ScriptManager>
	<asp:UpdatePanel ID="UpdatePanelLotInfor" runat="server">
		<ContentTemplate>
			<%--    This is another new design template--%>
			<div class="container-fluid" runat="server" ID="V_ProjR" Visible="false">
				<%-- Datagrid--%>
				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>

				<!-- Project Details -->
				<div class="card shadow mb-4">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;<a class="link-info" href="FrmProjectListing.aspx">Project Listing</a> / Details</h6>
					</div>
					<div class="card-body">
						<div class="row row-margin-btm-cubic">
							<div class="col">
								<div class="col-md-12">
									<main>
										<div class="card-body row">
											<%-- Project Details --%>
											<div class="col-sm-8 row d-flex-card justify-content-around">
												<div class="col-sm-12">
													<div class="card shadow">
														<div class="card-body">
															<%-- ProjName/ProjNo/Status --%>
															<div class="card-title row">
																<div class="col-md-11 my-2">
																	<strong>
																		<asp:Label ID="lblProjectName" runat="server" Text="" class="sidenav-footer-title"></asp:Label></strong> - <small class="text-muted">
																			<asp:Label ID="lblProjectNo" runat="server" Text="" class="sidenav-footer-title"></asp:Label></small>
																</div>
																<asp:HiddenField runat="server" ID="hiddProjStatus" />
																<div class="col-md-1 my-2 float-right"><i class="fa-solid fa-circle" id="ProjStatus"></i></div>
															</div>
															<%-- ProjDate --%>
															<div class="card-text row">
																<div class="col-md-6 my-3">
																	Project Date:
																	<asp:Label ID="lblProjectDate" runat="server" Text="" class="sidenav-footer-title"></asp:Label>
																</div>
															</div>
															<%-- CustName --%>
															<div class="card-text row">
																<div class="col-md-6">
																	Customer:
																	<asp:Label ID="lblCustomerName" runat="server" Text="" class="sidenav-footer-title"></asp:Label>
																	<span class="text-muted">(<asp:Label ID="lblCustomerPhoneNumber" runat="server" Text="" class="sidenav-footer-title"></asp:Label>)</span>
																</div>
															</div>
															<%-- CreatedBy/CreatedDate --%>
															<div class="card-text row mt-4 me-3">
																<small class="text-muted">Create by
																<asp:Label ID="lblProjectCreatedBy" runat="server" Text="" class="sidenav-footer-title"></asp:Label>
																	- since
																<asp:Label ID="lblProjectCreatedDate" runat="server" Text="" class="sidenav-footer-title"></asp:Label></small>
															</div>
														</div>
													</div>
												</div>
											</div>
											<%-- Balances --%>
											<div class="col-sm-4 row d-flex-card justify-content-around">
												<div class="col-sm-12">
													<asp:hiddenfield ID="hiddTotalPaidAmount" runat="server" />
													<asp:hiddenfield ID="hiddReceivedAmount" runat="server" />
													<div class="card shadow" data-bs-toggle="tooltip" title="" id="TotalPaidAmount">
														<div class="card-body">
															<%-- Account Receiveable --%>
															<div class="card-text row">Account Receiveable:</div>
															<div class="card-text row ">
																<div class="col-md-12 my-2">
																	<div class="progress" style="height: 28px">
																		<div id="progress-ReceiableAmount" class="progress-bar bg-success text-lg-start" role="progressbar">
																			<asp:Label ID="lblReceiableAmount" runat="server" Text="" class="sidenav-footer-title ms-2"></asp:Label>
																		</div>
																	</div>

																</div>
															</div>
															<div class="card-text row">
																<div class="col-md-12 my-2">
																</div>
															</div>
															<div class="card-text row">
																<div class="col-md-12 my-2">
																</div>
															</div>
															<%-- Received Amount --%>
															<div class="card-text row">Received Amount:</div>
															<div class="card-text row">
																<div class="col-md-12 my-2">
																	<div class="progress" style="height: 25px">
																		<div id="progress-ReceivedAmount" class="progress-bar bg-danger text-lg-start" role="progressbar">
																			<asp:Label ID="lblReceivedAmount" runat="server" Text="" class="sidenav-footer-title ms-2"></asp:Label>
																		</div>
																	</div>
																</div>
															</div>
															<div class="card-text row">
																<div class="col-md-12 my-2">
																</div>
															</div>

															
														</div>
													</div>
												</div>
											</div>
										</div>
									</main>
								</div>
							</div>
						</div>
					</div>
				</div>
				<hr class="cssContentHeaderLine" />

				<!-- Document Details -->
				<div class="card shadow mb-4">
					<div class="card-header py-2">
						<nav class="m-0 page-heading-cubic navbar navbar-expand-sm">
							<div class="container-fluid">
								<asp:RadioButtonList class="navbar-nav input-radio-cubic-20" AutoPostBack="true" ID="DocRemark" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="DocRemark_SelectedIndexChanged">
									<asp:ListItem class="nav-item" Selected="True" Value="INV">Invoice</asp:ListItem>
									<asp:ListItem class="nav-item" Value="PO">Purchase Order</asp:ListItem>
									<asp:ListItem class="nav-item" Value="DO">Delivery Order</asp:ListItem>
									<asp:ListItem class="nav-item" Value="QO">Quotation</asp:ListItem>
								</asp:RadioButtonList>
							</div>
						</nav>
					</div>
					<div class="card-body">
						<%-- Table --%>
						<div class="datatable">
							<div id="dataTable_wrapper" class="dataTables_wrapper dt-bootstrap4">
								<asp:Repeater ID="DocMRepeater" runat="server">
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
											<!-- Date From -->
											<div class="col-md-4">
												<label for="DateFrom" class="form-label">Date From:</label>
												<input id="DateFrom" type="date" class="form-control" />
											</div>

											<!-- Date To -->
											<div class="col-md-4">
												<label for="DateTo" class="form-label">Date To:</label>
												<input id="DateTo" type="date" class="form-control" />
											</div>

											<!-- Filter Button -->
											<div class="col-md-1 align-self-end">
												<input id="filterDate" type="button" class="btn btn-primary w-100" value="Filter" />
											</div>
										</div>
										<table id="DocMTable" class="table table-bordered table-hover table-striped mydatatable " style="width: 100%">
											<thead class="table table-success">
												<th>Revision No</th>
												<th>Date</th>
												<th>Status</th>
												<th></th>
											</thead>
									</HeaderTemplate>
									<ItemTemplate>
										<tr>
											<td><%#Eval("DOC_REVISION_NO") %></td>
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
					</main>
				</div>
			</div>


		</ContentTemplate>
	</asp:UpdatePanel>


	<br />


</asp:Content>
