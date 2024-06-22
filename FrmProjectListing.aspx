<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmProjectListing.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmProjectListing" %>

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

		.row-link {
			display: block;
			width: 100%;
			height: 100%;
			text-align: left;
		}

			.row-link table {
				width: 100%;
			}

				.row-link table tr td {
					padding: 10px;
				}

			.row-link:hover {
				background-color: #f0f0f0; /* Optional: Change background color on hover */
			}
	</style>




	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
	<script src="js/dataTables.min.js"></script>
	<script src="js/dataTables.bootstrap4.min.js"></script>
	<script src="assets/demo/datatables-demo.min.js"></script>

	<script>
		document.title = "Project Listing Report";
		$(document).ready(function () {
			var table = $('#ProjMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"responsive": true,
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
		Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
			$('#BSMTable').DataTable({
				"paging": true,
				"searching": true,
				"info": true,
				"destroy": true
			});
			document.title = "Project Listing Report";
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

				<!-- Project Listing Example -->
				<div class="card shadow mb-4">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Project Listing </h6>
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
											<div class="card-body">

												<div style="overflow-x: scroll; height: 100%; min-width: 350px;">
													<div class="datatable">
														<div id="dataTable_wrapper" class="dataTables_wrapper dt-bootstrap4">
															<asp:Repeater ID="ProjMRepeater" runat="server">
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

																	<table id="ProjMTable" class="table table-bordered table-hover table-striped" style="width: 100%">
																		<thead class="table table-success">
																			<th>Project No</th>
																			<th>Project Name</th>
																			<th>Date</th>
																			<th>Status</th>
																			<th></th>
																		</thead>
																</HeaderTemplate>
																<ItemTemplate>
																	<tr onclick="window.location.href='FrmProjectListingDet.aspx?ProjNo=<%#Eval("PROJ_NO") %>'">
																		<td><%#Eval("PROJ_NO") %></td>
																		<td><%#Eval("PROJ_NAME") %></td>
																		<td><%#Eval("PROJ_DATE","{0:yyyy/MM/dd}") %></td>
																		<td><%#Eval("PROJ_STATUS") %></td>
																		<td>
																			<button type="button" style="height: 20px; width: 20px; padding: 0;" class="btn btn-muted rounded-circle" data-bs-toggle="dropdown" onclick="event.stopPropagation();"><i class="fa-solid fa-ellipsis-vertical"></i></button>
																			<ul class="dropdown-menu">
																				<li>
																					<asp:Button class="dropdown-item" ID="DeleteProject" runat="server" Text="Delete" CommandArgument='<%# Eval("PROJ_NO") %>' OnCLick="DeleteProject_Click" /></li>
																				<li>
																					<asp:Button class="dropdown-item" ID="EditProject" runat="server" Text="Edit" CommandArgument='<%# Eval("PROJ_NO") %>' OnClick="EditProject_Click" /></li>
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
						</div>
					</div>
				</div>
			</div>
		</ContentTemplate>
	</asp:UpdatePanel>


	<br />


</asp:Content>
