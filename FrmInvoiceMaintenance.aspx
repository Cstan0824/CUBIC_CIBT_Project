<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="FrmInvoiceMaintenance.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmInvoiceMaintenance" %>

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
			$(Invoice.body).removeClass('modal-open');
			Invoice.body.style.overflow = "scroll";
		}

		function showModal(status, message) {
			$("#ModalConfirmation").modal('hide');
			var title = Invoice.getElementById('MessageModalTitle');
			var content = Invoice.getElementById('MessageModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ModalMessage").modal('show');
			// Reinitiallize DataTable
			$('#DocMTable').DataTable();
		}

		function showErrorModal(status, message) {
			$("#ModalConfirmation").modal('hide');
			var title = Invoice.getElementById('ErrorModalTitle');
			var content = Invoice.getElementById('ErrorModalContent');
			title.innerHTML = status;
			content.innerHTML = message;
			$("#ErrorModalMessage").modal('show');
			// Reinitiallize DataTable
			$('#DocMTable').DataTable();
		}

		Invoice.title = "Invoice Maintenance";

		$(Invoice).ready(function () {
			$('#DocMTable').DataTable()
		});

		$('#DocMTable').DataTable({
			"paging": true,
			"searching": true,
			"info": true,
		});

		//$(Invoice).click(function (e) {
		//    if ($(e.target).is(#ModalMessage)) {
		//        $('#ModalMessage').fadeOut(500);
		//    }
		//});

	</script>


	&nbsp;

    <asp:ScriptManager ID="ScriptManagerMain" runat="server"></asp:ScriptManager>
	<asp:UpdatePanel ID="UpdatePanelLotInfor" runat="server">
		<ContentTemplate>
			<%--    This is another new design template--%>
			<div class="container-fluid" runat="server" ID="E_InvM">
				<!-- Page Heading --For Container untill card body-->
				<h1 class="h3 mb-2 text-gray-800"></h1>
				<p class="mb-4"></p>
				<!-- Invoice -->
				<div class="card shadow mb-4">
					<div class="card-header py-3">
						<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Invoice Maintenance</h6>
					</div>

					<div class="card-body">
						<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="#813838" ShowModelStateErrors="True" BackColor="#FEE2E1" Font-Names="Segoe UI Symbol" EnableTheming="True" EnableClientScript="True" />

						<%--  row Invoice ID--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4" style="border: 0px solid">
								<asp:Label ID="lblInvoiceddl" runat="server" Text="Select Mode" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="ddlInvoiceMode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;" OnSelectedIndexChanged="ddlInvoiceMode_SelectedIndexChanged">
									<asp:ListItem Value="">--Select One--</asp:ListItem>
									<asp:ListItem Value="C">--Create New--</asp:ListItem>
									<asp:ListItem Value="U">--Update Details--</asp:ListItem>
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator1" runat="server" Operator="NotEqual" ControlToValidate="ddlInvoiceMode" ValueToCompare="" Errormessage="Please select a Invoice Mode" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblInvoiceIDDrpList" runat="server" Text="Invoice ID" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:DropDownList ID="DrpListInvoiceID" runat="server" AutoPostBack="true" class="form-control" Visible="false" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator2" runat="server" Operator="NotEqual" ControlToValidate="DrpListInvoiceID" ValueToCompare="" Errormessage="Please select an Invoice ID" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>
							</div>
						</div>
						<%-- row Invoice Status/Date--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblStatus" runat="server" Text="Status " class="input-label-cubic"></asp:Label>
								<asp:RadioButtonList ID="rbStatus" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" class="input-radio-cubic-20">
									<asp:ListItem Selected="True" Value="True">Active</asp:ListItem>
									<asp:ListItem Value="False">Inactive</asp:ListItem>
								</asp:RadioButtonList>
							</div>
							<div class="form-group col-md-2"></div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblInvoiceDate" runat="server" Text="Invoice Date" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtInvoiceDate" ReadOnly="true" ClientIDMode="Static" runat="server" TextMode="Date" class="form-control input-textbox-cubic-16" AutoPostBack="false"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtInvoiceDate" ErrorMessage="Please Enter Invoice Date" ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>
						</div>
						<%-- row Invoice Upload File --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblFileUpload" runat="server" Text="Invoice File Upload(Only Exel File):" Visible="false" class="input-label-cubic"></asp:Label>
								<asp:FileUpload ID="ChooseFileUpload" runat="server" Visible="false" />
							</div>
							<div class="form-group col-md-2">
								<asp:RegularExpressionValidator ID="FileUploadValidator" runat="server" ControlToValidate="ChooseFileUpload"
									ErrorMessage="Please upload only Excel files." ValidationExpression="^.*\.(xls|xlsx)$"
									ForeColor="Red" Display="Dynamic" />
							</div>
							<div class="form-group col-md-6"></div>
						</div>
						<%-- row Paid/Balance Amount --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblPaidAmount" runat="server" Text="Paid Amount" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtPaidAmount" placeholder="Paid Amount" runat="server" ReadOnly="true" class="form-control input-textbox-cubic-16"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidatorPaidAmount" runat="server" ControlToValidate="txtPaidAmount" ErrorMessage="Please Enter Paid Amount" ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblBalanceAmount" runat="server" Text="Balance Amount" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtBalanceAmount" placeholder="Balance Amount" runat="server" class="form-control input-textbox-cubic-16" ReadOnly="true"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidatorBalanceAmount" runat="server" ControlToValidate="txtBalanceAmount" ErrorMessage="Please Enter Balance Amount" ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>
						</div>
						<br />
						<%-- row PO_NO/Amount --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblPONo" runat="server" Text="PO Number" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtPONo" runat="server" class="form-control input-textbox-cubic-16" ReadOnly="true" placeholder="PO Number" CssClass="form-control"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidatorPONo" runat="server" ControlToValidate="txtPONo" ErrorMessage="Please Enter PO Number" ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>
							<div class="form-group col-md-4">
								<asp:Label ID="lblPOAmount" runat="server" Text="PO Amount" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtPOAmount" runat="server" class="form-control input-textbox-cubic-16" placeholder="PO Amount" ReadOnly="true"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="RequiredFieldValidatorPOAmount" runat="server" ControlToValidate="txtPOAmount" ErrorMessage="Please Enter PO Amount" ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>
						</div>
						<%-- row PO File Upload *For Better Referencing in Future* --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-6">
								<asp:Label ID="lblPOFile" runat="server" Text="PO File Upload(If Got Any):" class="input-label-cubic" Visible="false"></asp:Label>
								<asp:FileUpload ID="filePO" runat="server" Visible="false" class="form-control-file" />
							</div>
						</div>
						<br />
						<%-- row Project Code/Remark --%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblProjectCodeTxt" runat="server" Text="Project Code" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListProjectCode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
								</asp:DropDownList>

							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator3" runat="server" Operator="Equal" ControlToValidate="DrpListProjectCode" ValueToCompare="" Errormessage="Please select a Project" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>
							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblRemark" runat="server" Text="Invoice Remark" class="input-label-cubic"></asp:Label>
								<asp:TextBox ID="txtRemark" runat="server" ReadOnly="true" class="form-control input-textbox-cubic-16" placeholder="Remark"></asp:TextBox>
							</div>
							<div class="form-group col-md-2">
								<asp:RequiredFieldValidator ID="vRevNo" runat="server" ControlToValidate="txtRemark" ErrorMessage="Please enter Revision No." ForeColor="Red" BorderStyle="None">*</asp:RequiredFieldValidator>
							</div>

						</div>

						<%--  row Customer/Supplier Code--%>
						<div class="row row-margin-btm-cubic">
							<div class="form-group col-md-4">
								<asp:Label ID="lblCustomerCodeTxt" runat="server" Text="Customer Code" class="input-label-cubic"></asp:Label>
								<%-- <div class="inputWithIcon">--%>
								<asp:DropDownList ID="DrpListCustomerCode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
								</asp:DropDownList>

							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator4" runat="server" Operator="Equal" ControlToValidate="DrpListCustomerCode" ValueToCompare="" Errormessage="Please select a Customer" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>

							</div>

							<div class="form-group col-md-4">
								<asp:Label ID="lblSupplierCodeTxt" runat="server" Text="Supplier Code" class="input-label-cubic"></asp:Label>
								<asp:DropDownList ID="DrpListSupplierCode" runat="server" AutoPostBack="true" class="form-control" ValidateRequestMode="Enabled" BackColor="white" Style="border: 1px solid; font-size: 16px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; min-width: 140px;">
								</asp:DropDownList>
							</div>
							<div class="form-group col-md-2">
								<asp:CompareValidator ID="CompareValidator5" runat="server" Operator="Equal" ControlToValidate="DrpListSupplierCode" ValueToCompare="" Errormessage="Please select a Supplier" ForeColor="Red" BorderStyle="None" Display="Dynamic">*</asp:CompareValidator>

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
			<div class="card shadow mb-4" runat="server" ID="V_InvM">
				<div class="card-header py-3">
					<h6 class="m-0 page-heading-cubic"><i class="fa fa-file"></i>&nbsp;&nbsp;Invoices Table</h6>
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
																		<th>Invoice No.</th>
																		<th>Invoice Name</th>
																		<th>Revision No</th>
																		<th>Status</th>
																		<th>Modified By</th>
																		<th>Modified Date</th>

																	</thead>
															</HeaderTemplate>

															<ItemTemplate>
																<tr>
																	<%--<td><%#Eval("DOC_CODE") %></td>
																		<td><%#Eval("DOC_NAME") %></td>
																		<td><%#Eval("REVISION_NO") %></td>
																		<td><%#Eval("DOC_STATUS") %></td>
																		<td><%#Eval("MODIFIED_BY") %></td>
																		<td><%#Eval("MODIFIED_DATE") %></td>--%>
																	<td></td>
																	<td></td>
																	<td></td>
																	<td></td>
																	<td></td>
																	<td></td>
																	<td></td>
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
							<div class="modal-dialog" role="Invoice">
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

						<!-- Fail Message Modal -->
						<div class="modal fade" id="ErrorModalMessage" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
							<div class="modal-dialog" role="Invoice">
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
