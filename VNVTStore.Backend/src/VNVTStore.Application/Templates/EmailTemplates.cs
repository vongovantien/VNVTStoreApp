namespace VNVTStore.Application.Templates;

/// <summary>
/// Responsive, premium HTML Email Templates for VNVT Store
/// </summary>
public static class EmailTemplates
{
    private const string BaseStyle = @"
        <style>
            body { margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; color: #1e293b; line-height: 1.6; }
            .wrapper { width: 100%; table-layout: fixed; background-color: #f1f5f9; padding: 40px 0; }
            .container { max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.05); }
            .header { background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); padding: 36px 30px; text-align: center; }
            .header h1 { color: #ffffff; margin: 0; font-size: 26px; font-weight: 700; letter-spacing: -0.5px; }
            .header p { color: rgba(255,255,255,0.9); margin: 8px 0 0; font-size: 14px; }
            .content { padding: 36px 30px; }
            .greeting { font-size: 18px; font-weight: 600; color: #0f172a; margin-top: 0; margin-bottom: 16px; }
            .btn-container { text-align: center; margin: 30px 0; }
            .btn { display: inline-block; padding: 14px 32px; background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: #ffffff !important; text-decoration: none; border-radius: 10px; font-weight: 600; font-size: 15px; box-shadow: 0 4px 14px rgba(79,70,229,0.3); }
            .card { background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; margin: 24px 0; }
            .alert-warning { background-color: #fffbeb; border: 1px solid #fde68a; border-radius: 12px; padding: 16px; margin: 24px 0; color: #92400e; font-size: 14px; }
            .table-items { width: 100%; border-collapse: collapse; margin-top: 12px; }
            .table-items th { text-align: left; padding: 10px 0; border-bottom: 2px solid #e2e8f0; font-size: 13px; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
            .table-items td { padding: 12px 0; border-bottom: 1px solid #f1f5f9; font-size: 14px; }
            .total-row { display: flex; justify-content: space-between; align-items: center; padding-top: 16px; margin-top: 12px; border-top: 2px solid #e2e8f0; font-size: 18px; font-weight: 700; color: #4f46e5; }
            .footer { background-color: #f8fafc; padding: 28px 30px; text-align: center; color: #64748b; font-size: 13px; border-top: 1px solid #e2e8f0; }
            .footer-links a { color: #4f46e5; text-decoration: none; margin: 0 8px; font-weight: 500; }
            .url-box { word-break: break-all; font-size: 12px; color: #64748b; background-color: #f1f5f9; padding: 10px; border-radius: 8px; margin-top: 10px; }
            @media only screen and (max-width: 600px) {
                .container { width: 100% !important; border-radius: 0; }
                .content { padding: 24px 18px; }
                .header { padding: 28px 18px; }
            }
        </style>";

    /// <summary>
    /// Email xác nhận đăng ký tài khoản
    /// </summary>
    public static string EmailVerification(string userName, string verificationLink)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Xác nhận Email - VNVT Store</title>
    {BaseStyle}
</head>
<body>
    <div class=""wrapper"">
        <div class=""container"">
            <div class=""header"">
                <h1>VNVT STORE</h1>
                <p>Nền tảng mua sắm công nghệ hiện đại</p>
            </div>
            <div class=""content"">
                <h2 class=""greeting"">Xin chào {userName},</h2>
                <p>Cảm ơn bạn đã lựa chọn đăng ký tài khoản tại <strong>VNVT Store</strong>. Để bắt đầu trải nghiệm mua sắm an toàn và nhận nhiều ưu đãi, vui lòng bấm nút xác nhận email bên dưới:</p>
                
                <div class=""btn-container"">
                    <a href=""{verificationLink}"" class=""btn"" target=""_blank"">Xác nhận tài khoản ngay</a>
                </div>

                <div class=""card"">
                    <p style=""margin: 0; font-size: 13px; color: #64748b;"">
                        Nếu nút bấm trên không hoạt động, bạn có thể copy và dán liên kết sau vào trình duyệt:
                    </p>
                    <div class=""url-box"">{verificationLink}</div>
                </div>

                <p style=""font-size: 13px; color: #94a3b8; margin-top: 24px;"">
                    * Liên kết xác thực này có hiệu lực trong 24 giờ. Nếu bạn không yêu cầu tạo tài khoản tại VNVT Store, vui lòng bỏ qua thư này.
                </p>
            </div>
            <div class=""footer"">
                <p style=""margin: 0 0 8px 0; font-weight: 600; color: #334155;"">VNVT Store Việt Nam</p>
                <p style=""margin: 0 0 12px 0;"">Hotline hỗ trợ: 1900 1234 | Email: support@vnvtstore.com</p>
                <div class=""footer-links"">
                    <a href=""#"">Cửa hàng</a> • <a href=""#"">Chính sách bảo mật</a> • <a href=""#"">Hỗ trợ khách hàng</a>
                </div>
                <p style=""margin: 16px 0 0 0; font-size: 11px; color: #94a3b8;"">© 2026 VNVT Store. Tất cả các quyền được bảo lưu.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Email đặt lại mật khẩu
    /// </summary>
    public static string PasswordReset(string userName, string resetLink)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Đặt lại mật khẩu - VNVT Store</title>
    {BaseStyle}
</head>
<body>
    <div class=""wrapper"">
        <div class=""container"">
            <div class=""header"">
                <h1>🔒 ĐẶT LẠI MẬT KHẨU</h1>
                <p>Bảo mật tài khoản VNVT Store của bạn</p>
            </div>
            <div class=""content"">
                <h2 class=""greeting"">Xin chào {userName},</h2>
                <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản VNVT Store của bạn. Vui lòng bấm nút bên dưới để tiến hành tạo mật khẩu mới:</p>
                
                <div class=""btn-container"">
                    <a href=""{resetLink}"" class=""btn"" target=""_blank"">Tạo mật khẩu mới</a>
                </div>

                <div class=""alert-warning"">
                    <strong>⚠️ Lưu ý quan trọng:</strong> Liên kết đặt lại mật khẩu này chỉ có hiệu lực trong vòng <strong>60 phút</strong>. Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email và kiểm tra lại an toàn bảo mật tài khoản.
                </div>

                <div class=""card"">
                    <p style=""margin: 0; font-size: 13px; color: #64748b;"">
                        Nếu nút bấm không hoạt động, vui lòng copy liên kết bên dưới vào trình duyệt:
                    </p>
                    <div class=""url-box"">{resetLink}</div>
                </div>
            </div>
            <div class=""footer"">
                <p style=""margin: 0 0 8px 0; font-weight: 600; color: #334155;"">VNVT Store Việt Nam</p>
                <p style=""margin: 0 0 12px 0;"">Hỗ trợ khẩn cấp: support@vnvtstore.com | Hotline: 1900 1234</p>
                <p style=""margin: 16px 0 0 0; font-size: 11px; color: #94a3b8;"">© 2026 VNVT Store. Tất cả các quyền được bảo lưu.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Email xác nhận đơn hàng chuyên nghiệp
    /// </summary>
    public static string OrderConfirmation(
        string customerName, 
        string orderNumber, 
        decimal totalAmount, 
        List<(string Name, int Quantity, decimal Price)> items, 
        string orderViewLink)
    {
        var itemsRows = "";
        foreach (var item in items)
        {
            var lineTotal = item.Quantity * item.Price;
            itemsRows += $@"
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #f1f5f9;"">
                        <strong style=""color: #1e293b;"">{item.Name}</strong><br>
                        <span style=""color: #64748b; font-size: 13px;"">Số lượng: {item.Quantity}</span>
                    </td>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #f1f5f9; text-align: right; font-weight: 600; color: #334155;"">
                        {lineTotal:N0}₫
                    </td>
                </tr>";
        }

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Xác nhận đơn hàng #{orderNumber}</title>
    {BaseStyle}
</head>
<body>
    <div class=""wrapper"">
        <div class=""container"">
            <div class=""header"">
                <h1>✅ ĐƠN HÀNG ĐÃ ĐƯỢC ĐẶT</h1>
                <p>Cảm ơn bạn đã tin tưởng mua sắm tại VNVT Store</p>
            </div>
            <div class=""content"">
                <h2 class=""greeting"">Xin chào {customerName},</h2>
                <p>Đơn hàng của bạn đã được tiếp nhận thành công và đang được bộ phận vận hành chuẩn bị đóng gói để chuyển đi sớm nhất.</p>
                
                <div class=""card"" style=""text-align: center; padding: 20px;"">
                    <span style=""font-size: 13px; text-transform: uppercase; letter-spacing: 1px; color: #64748b; font-weight: 600;"">Mã đơn hàng</span>
                    <h3 style=""margin: 6px 0 0 0; font-size: 26px; color: #4f46e5; letter-spacing: 1px;"">#{orderNumber}</h3>
                </div>

                <div class=""card"">
                    <h3 style=""margin-top: 0; font-size: 16px; color: #0f172a; border-bottom: 1px solid #e2e8f0; padding-bottom: 10px;"">
                        Chi tiết sản phẩm
                    </h3>
                    <table class=""table-items"">
                        <thead>
                            <tr>
                                <th>Sản phẩm</th>
                                <th style=""text-align: right;"">Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsRows}
                        </tbody>
                    </table>

                    <div class=""total-row"">
                        <span>Tổng thanh toán:</span>
                        <span>{totalAmount:N0}₫</span>
                    </div>
                </div>

                <div class=""btn-container"">
                    <a href=""{orderViewLink}"" class=""btn"" target=""_blank"">Xem chi tiết đơn hàng</a>
                </div>

                <p style=""font-size: 13px; color: #64748b; text-align: center;"">
                    Chúng tôi sẽ gửi email thông báo trạng thái đơn hàng khi shipper lấy hàng thành công.
                </p>
            </div>
            <div class=""footer"">
                <p style=""margin: 0 0 8px 0; font-weight: 600; color: #334155;"">VNVT Store - Đơn vị công nghệ hàng đầu</p>
                <p style=""margin: 0 0 12px 0;"">Cần hỗ trợ đơn hàng? Liên hệ hotline: 1900 1234</p>
                <p style=""margin: 16px 0 0 0; font-size: 11px; color: #94a3b8;"">© 2026 VNVT Store. Tất cả các quyền được bảo lưu.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Email thông báo yêu cầu báo giá mới cho Admin
    /// </summary>
    public static string QuoteNotification(
        string quoteCode, 
        string customerName, 
        string customerEmail, 
        string? note, 
        DateTime date, 
        string adminReviewLink)
    {
        var effectiveNote = string.IsNullOrWhiteSpace(note) ? "Không có ghi chú thêm" : note;
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Yêu cầu báo giá mới #{quoteCode}</title>
    {BaseStyle}
</head>
<body>
    <div class=""wrapper"">
        <div class=""container"">
            <div class=""header"" style=""background: linear-gradient(135deg, #0284c7 0%, #0369a1 100%);"">
                <h1>📋 YÊU CẦU BÁO GIÁ MỚI</h1>
                <p>Khách hàng gửi yêu cầu báo giá B2B</p>
            </div>
            <div class=""content"">
                <h2 class=""greeting"">Xin chào Quản trị viên,</h2>
                <p>Hệ thống vừa tiếp nhận một yêu cầu báo giá mới từ khách hàng doanh nghiệp:</p>
                
                <div class=""card"">
                    <table style=""width: 100%; border-collapse: collapse;"">
                        <tr>
                            <td style=""padding: 8px 0; color: #64748b; width: 140px;"">Mã báo giá:</td>
                            <td style=""padding: 8px 0; font-weight: 700; color: #0284c7;"">{quoteCode}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; color: #64748b;"">Khách hàng:</td>
                            <td style=""padding: 8px 0; font-weight: 600;"">{customerName}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; color: #64748b;"">Email liên hệ:</td>
                            <td style=""padding: 8px 0;""><a href=""mailto:{customerEmail}"" style=""color: #0284c7;"">{customerEmail}</a></td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; color: #64748b;"">Thời gian gửi:</td>
                            <td style=""padding: 8px 0;"">{date:dd/MM/yyyy HH:mm:ss}</td>
                        </tr>
                        <tr>
                            <td style=""padding: 8px 0; color: #64748b; vertical-align: top;"">Ghi chú yêu cầu:</td>
                            <td style=""padding: 8px 0; font-style: italic; color: #334155;"">{effectiveNote}</td>
                        </tr>
                    </table>
                </div>

                <div class=""btn-container"">
                    <a href=""{adminReviewLink}"" class=""btn"" style=""background: linear-gradient(135deg, #0284c7 0%, #0369a1 100%);"" target=""_blank"">
                        Xử lý báo giá trong Admin
                    </a>
                </div>
            </div>
            <div class=""footer"">
                <p style=""margin: 0;"">Hệ thống thông báo nội bộ VNVT Store Admin Portal</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }
}
