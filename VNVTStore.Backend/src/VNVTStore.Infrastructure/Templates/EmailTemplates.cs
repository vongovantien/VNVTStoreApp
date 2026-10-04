namespace VNVTStore.Infrastructure.Templates;

/// <summary>
/// HTML Email Templates với responsive design
/// </summary>
public static class EmailTemplates
{
    private const string BaseStyle = @"
        <style>
            body { margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; }
            .container { max-width: 600px; margin: 0 auto; background-color: #ffffff; }
            .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 20px; text-align: center; }
            .header h1 { color: #ffffff; margin: 0; font-size: 28px; font-weight: 600; }
            .content { padding: 40px 30px; color: #333333; line-height: 1.6; }
            .button { display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff !important; text-decoration: none; border-radius: 8px; font-weight: 600; margin: 20px 0; }
            .footer { background-color: #f8f9fa; padding: 30px; text-align: center; color: #6c757d; font-size: 14px; border-top: 1px solid #dee2e6; }
            .order-summary { background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin: 20px 0; }
            .order-item { display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #e9ecef; }
            .total { font-size: 18px; font-weight: 600; color: #667eea; margin-top: 15px; }
            @media only screen and (max-width: 600px) {
                .content { padding: 20px 15px; }
                .header h1 { font-size: 24px; }
            }
        </style>";

    /// <summary>
    /// Email xác nhận đăng ký tài khoản
    /// </summary>
    public static string EmailVerification(string userName, string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    {BaseStyle}
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Chào mừng đến với VNVTStore!</h1>
        </div>
        <div class=""content"">
            <h2 style=""color: #667eea; margin-top: 0;"">Xin chào {userName},</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại VNVTStore. Vui lòng xác nhận email của bạn để kích hoạt tài khoản.</p>
            <p style=""text-align: center;"">
                <a href=""{verificationLink}"" class=""button"">Xác nhận Email</a>
            </p>
            <p style=""color: #6c757d; font-size: 14px;"">
                Nếu bạn không thể click vào nút trên, copy link sau vào trình duyệt:<br>
                <span style=""word-break: break-all;"">{verificationLink}</span>
            </p>
            <p style=""margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6; color: #6c757d; font-size: 13px;"">
                Link xác nhận có hiệu lực trong 24 giờ. Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này.
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 5px 0;""><strong>VNVTStore</strong></p>
            <p style=""margin: 5px 0;"">Hotline: 1900 xxxx | Email: support@vnvtstore.com</p>
            <p style=""margin: 15px 0 5px 0; font-size: 12px;"">© 2026 VNVTStore. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Email reset mật khẩu
    /// </summary>
    public static string PasswordReset(string userName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    {BaseStyle}
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔒 Đặt lại mật khẩu</h1>
        </div>
        <div class=""content"">
            <h2 style=""color: #667eea; margin-top: 0;"">Xin chào {userName},</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Click vào nút bên dưới để tạo mật khẩu mới:</p>
            <p style=""text-align: center;"">
                <a href=""{resetLink}"" class=""button"">Đặt lại mật khẩu</a>
            </p>
            <p style=""color: #6c757d; font-size: 14px;"">
                Hoặc copy link sau vào trình duyệt:<br>
                <span style=""word-break: break-all;"">{resetLink}</span>
            </p>
            <p style=""margin-top: 30px; padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px; color: #856404; font-size: 14px;"">
                ⚠️ Link đặt lại mật khẩu có hiệu lực trong 1 giờ. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này và đảm bảo tài khoản của bạn an toàn.
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 5px 0;""><strong>VNVTStore</strong></p>
            <p style=""margin: 5px 0;"">Hotline: 1900 xxxx | Email: support@vnvtstore.com</p>
            <p style=""margin: 15px 0 5px 0; font-size: 12px;"">© 2026 VNVTStore. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Email xác nhận đơn hàng
    /// </summary>
    public static string OrderConfirmation(string customerName, string orderNumber, 
        decimal totalAmount, string orderDetailsHtml, string orderViewLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    {BaseStyle}
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✅ Đơn hàng đã được xác nhận</h1>
        </div>
        <div class=""content"">
            <h2 style=""color: #667eea; margin-top: 0;"">Xin chào {customerName},</h2>
            <p>Cảm ơn bạn đã đặt hàng tại VNVTStore! Đơn hàng của bạn đã được xác nhận và đang được xử lý.</p>
            
            <div style=""background-color: #f8f9fa; border-radius: 8px; padding: 20px; margin: 20px 0;"">
                <p style=""margin: 0 0 10px 0; font-size: 14px; color: #6c757d;"">Mã đơn hàng</p>
                <p style=""margin: 0; font-size: 24px; font-weight: 600; color: #667eea;"">{orderNumber}</p>
            </div>

            <div class=""order-summary"">
                <h3 style=""margin-top: 0; color: #333;"">Chi tiết đơn hàng</h3>
                {orderDetailsHtml}
                <div style=""border-top: 2px solid #667eea; margin-top: 15px; padding-top: 15px;"">
                    <div style=""display: flex; justify-content: space-between; font-size: 18px; font-weight: 600; color: #667eea;"">
                        <span>Tổng cộng:</span>
                        <span>{totalAmount:N0}₫</span>
                    </div>
                </div>
            </div>

            <p style=""text-align: center;"">
                <a href=""{orderViewLink}"" class=""button"">Xem chi tiết đơn hàng</a>
            </p>

            <p style=""margin-top: 30px; color: #6c757d; font-size: 14px;"">
                Chúng tôi sẽ thông báo cho bạn khi đơn hàng được giao đi. Cảm ơn bạn đã tin tựng và ủng hộ VNVTStore!
            </p>
        </div>
        <div class=""footer"">
            <p style=""margin: 5px 0;""><strong>VNVTStore</strong></p>
            <p style=""margin: 5px 0;"">Hotline: 1900 xxxx | Email: support@vnvtstore.com</p>
            <p style=""margin: 15px 0 5px 0; font-size: 12px;"">© 2026 VNVTStore. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Helper: Build order items HTML
    /// </summary>
    public static string BuildOrderItemsHtml(List<(string Name, int Quantity, decimal Price)> items)
    {
        var html = "";
        foreach (var item in items)
        {
            html += $@"
                <div style=""display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #e9ecef;"">
                    <div style=""flex: 1;"">
                        <strong>{item.Name}</strong><br>
                        <span style=""color: #6c757d; font-size: 14px;"">SL: {item.Quantity}</span>
                    </div>
                    <div style=""text-align: right; font-weight: 600;"">{item.Price:N0}₫</div>
                </div>";
        }
        return html;
    }
}
