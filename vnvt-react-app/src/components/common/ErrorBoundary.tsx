import { useRouteError, isRouteErrorResponse, Link } from 'react-router-dom';
import { AlertTriangle, Home, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui';

const ErrorBoundary = () => {
  const error = useRouteError();
  console.error(error);

  let errorMessage = 'Đã có lỗi không mong muốn xảy ra.';
  let errorTitle = 'Ôi hỏng!';

  if (isRouteErrorResponse(error)) {
    if (error.status === 404) {
      errorTitle = 'Không tìm thấy trang';
      errorMessage = 'Trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.';
    } else if (error.status === 401) {
      errorTitle = 'Chưa xác thực';
      errorMessage = 'Bạn cần đăng nhập để truy cập trang này.';
    } else if (error.status === 503) {
      errorTitle = 'Dịch vụ không khả dụng';
      errorMessage = 'Hệ thống đang bảo trì, vui lòng quay lại sau.';
    }
  } else if (error instanceof Error) {
    errorMessage = error.message;
  }

  return (
    <div className="min-h-screen flex flex-col items-center justify-center p-4 bg-gray-50 text-center">
      <div className="bg-white p-8 rounded-2xl shadow-xl max-w-md w-full border border-gray-100">
        <div className="w-16 h-16 bg-red-50 text-red-500 rounded-full flex items-center justify-center mx-auto mb-6">
          <AlertTriangle size={32} />
        </div>
        
        <h1 className="text-2xl font-bold text-gray-900 mb-2">{errorTitle}</h1>
        <p className="text-gray-600 mb-8">{errorMessage}</p>

        <div className="flex flex-col sm:flex-row gap-3 justify-center">
          <Button 
            variant="outline" 
            onClick={() => window.location.reload()}
            leftIcon={<RefreshCw size={18} />}
          >
            Tải lại trang
          </Button>
          
          <Link to="/">
            <Button leftIcon={<Home size={18} />}>
              Về trang chủ
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
};

export default ErrorBoundary;
