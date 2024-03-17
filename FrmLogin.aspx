<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FrmLogin.aspx.cs" Inherits="CUBIC_CIBT_Project.FrmLogin" %>

<%--//// css vs min.css 
after done css, can use cssminifier.com to do the convertion   
Where bootstrap.css is the development version and bootstrap.min.css is production.
The main purpose of using bootstrap.min.css is to reduce the size of the file(style) that boosts the website speed.
 if the css need run from server, then we need att runat="server, else the css will not able to link"
all JS and CSS is crazy important
 --%>


<!-- Custom styles for this template -->
<style>
    #txtUsername::placeholder {
        color: dimgrey;
    }

    #txtPassword::placeholder {
        color: dimgrey;
    }

    body {
        display: flex;
        flex-direction: column;
        min-height: 100vh;
    }

    .form-border {
        border: none; 
        background: #f8f9fa; 
        padding: 20px; 
        border-radius: 10px; 
        box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.5); 
    }

    .footer-admin {
        box-shadow: 0px 0px 10px rgba(0, 0, 0, 0.5); 
    }

    .footer-admin a {
        color: black; 
    }
    .card-body{
        width:80%;
        margin:auto;
    }
</style>

<script defer>
    document.title = "CubicSoftware Solution Login Page";
	function ShowPassword(element) {
		document.getElementById("txtPassword").type = (element.checked) ? "text" : "password";
	}
</script>


<!doctype html>
<html lang="en">

<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="description" content="">
    <meta name="author" content="">


    <link href="css/sb-admin-2-Pro.min.css" rel="stylesheet" />
    <link rel="icon" type="image/x-icon" href="assets/img/favicon.png" />
    <script data-search-pseudo-elements defer src="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.3.0/js/all.min.js" crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/feather-icons/4.29.0/feather.min.js" crossorigin="anonymous"></script>


    <title>Login - CIBT</title>
</head>
<body>


    <div class="container py-5 h-100">
        <div class="row d-flex align-items-center justify-content-center h-100">
            <div class="col-md-7 col-lg-5 col-xl-5 ">
                <div class="d-flex align-items-center justify-content-center mb-4">
                    <img src="Image/SmallLogo.png" class="img-fluid mr-2" alt="Phone image">
                </div>

                <div class="d-flex align-items-center justify-content-center mb-4">
                    <h2 style="font-size: 30px;">CIBT - Sign In</h2>
                </div>

                <div class="form-border">
                    <form method="post" class="form" runat="server">
                        <!-- Email input -->
                        <div class="form-outline mb-4">
                            <label class="form-label" for="txtUsername">User Name</label>
                            <asp:TextBox ID="txtUsername" placeholder="Enter Your Username" runat="server" class="form-control form-control-lg" Font-Size="14px"></asp:TextBox>
                        </div>

                        <!-- Password input -->
                        <div class="form-outline mb-4">
                            <label class="form-label" for="txtPassword">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" placeholder="Enter Your Password" Font-Size="14px" TextMode="Password" class="form-control form-control-lg"></asp:TextBox>
                            <label class="form-check-label" for="ChkShowPassword">Show Password</label>
                            <asp:CheckBox ID="ChkShowPassword" runat="server" OnClick="ShowPassword(this)" />
                        </div>

                        <div class="d-flex justify-content-start mb-4">
                            <!-- Checkbox -->
                            <div class="form-check ">
                                <asp:CheckBox ID="ChkRememberMe" runat="server" Checked="true" />
                                <label class="form-check-label" for="ChkRememberMe">Remember me </label>

                            </div>
                            <a href="#!" hidden>Forgot password?</a>
                        </div>

                        <!-- Submit button -->
                        <asp:Button ID="btnSubmit" runat="server" OnClick="btnLogin_Click" type="submit" Text="Sign In" class="btn btn-primary btn-lg btn-block w-100" style="background-color:#38d39f;border-color:#38d39f" />
                    </form>
                </div>
            </div>
        </div>
    </div>

    <footer class="footer-admin mt-auto">
        <div class="container-xl px-4">
            <div class="row">
                <div class="col-md-6 small">Copyright © CubicSoftware Solution. All Right Reserved. 2023 - <%: DateTime.Now.Year %> . v1.1.3</div>
                <div class="col-md-6 text-md-end small"">
                    <a href="#!">Privacy Policy</a>    
                    <a href="#!">Terms &amp; Conditions</a>
                </div>
            </div>
        </div>
    </footer>

</body>
</html>

