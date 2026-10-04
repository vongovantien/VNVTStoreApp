import { useState } from 'react';
import { QRCodeSVG } from 'qrcode.react';
import { Shield, Key, Copy, Check, AlertTriangle, X, ArrowRight } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { authService, type TwoFactorSetupResponse } from '@/services/authService';
import { useToast } from '@/store';

interface TwoFactorModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (enabled: boolean) => void;
  isCurrentlyEnabled: boolean;
}

export const TwoFactorModal = ({
  isOpen,
  onClose,
  onSuccess,
  isCurrentlyEnabled
}: TwoFactorModalProps) => {
  const { error: toastError, success: toastSuccess } = useToast();
  const [step, setStep] = useState<'init' | 'setup' | 'recovery' | 'disable'>('init');
  const [loading, setLoading] = useState(false);
  const [setupData, setSetupData] = useState<TwoFactorSetupResponse | null>(null);
  const [code, setCode] = useState('');
  const [password, setPassword] = useState('');
  const [recoveryCodes, setRecoveryCodes] = useState<string[]>([]);
  const [copiedKey, setCopiedKey] = useState(false);
  const [copiedCodes, setCopiedCodes] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  if (!isOpen) return null;

  // Start Setup Flow
  const startSetup = async () => {
    setLoading(true);
    setErrorMessage(null);
    try {
      const res = await authService.setupTwoFactor();
      if (res.success && res.data) {
        setSetupData(res.data);
        setStep('setup');
      } else {
        toastError(res.message || 'Không thể khởi tạo mã QR 2FA.');
      }
    } catch (err: unknown) {
      toastError((err as Error).message || 'Có lỗi xảy ra khi tạo mã 2FA.');
    } finally {
      setLoading(false);
    }
  };

  // Confirm and Enable 2FA
  const handleEnable2Fa = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!setupData || !code.trim()) return;

    setLoading(true);
    setErrorMessage(null);
    try {
      const res = await authService.enableTwoFactor({
        secretKey: setupData.secretKey,
        code: code.trim()
      });

      if (res.success && res.data) {
        setRecoveryCodes(res.data.recoveryCodes);
        setStep('recovery');
        toastSuccess('Kích hoạt xác thực 2 bước thành công!');
        onSuccess(true);
      } else {
        setErrorMessage(res.message || 'Mã xác thực 6 chữ số không hợp lệ.');
      }
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || 'Lỗi khi kích hoạt 2FA.');
    } finally {
      setLoading(false);
    }
  };

  // Disable 2FA
  const handleDisable2Fa = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!password || !code) return;

    setLoading(true);
    setErrorMessage(null);
    try {
      const res = await authService.disableTwoFactor({
        password,
        code: code.trim()
      });

      if (res.success) {
        toastSuccess('Đã tắt xác thực 2 bước thành công.');
        onSuccess(false);
        handleClose();
      } else {
        setErrorMessage(res.message || 'Mật khẩu hoặc mã xác thực không đúng.');
      }
    } catch (err: unknown) {
      setErrorMessage((err as Error).message || 'Lỗi khi tắt 2FA.');
    } finally {
      setLoading(false);
    }
  };

  const handleCopySecret = () => {
    if (setupData?.secretKey) {
      navigator.clipboard.writeText(setupData.secretKey);
      setCopiedKey(true);
      setTimeout(() => setCopiedKey(false), 2000);
    }
  };

  const handleCopyRecoveryCodes = () => {
    if (recoveryCodes.length > 0) {
      navigator.clipboard.writeText(recoveryCodes.join('\n'));
      setCopiedCodes(true);
      setTimeout(() => setCopiedCodes(false), 2000);
    }
  };

  const handleClose = () => {
    setStep('init');
    setSetupData(null);
    setCode('');
    setPassword('');
    setRecoveryCodes([]);
    setErrorMessage(null);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in">
      <div className="relative w-full max-w-md bg-white dark:bg-slate-900 rounded-3xl p-6 shadow-2xl border border-slate-200 dark:border-slate-800 animate-scale-in">
        {/* Close Button */}
        <button
          onClick={handleClose}
          className="absolute top-5 right-5 p-2 text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 rounded-full hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
        >
          <X size={20} />
        </button>

        {/* Modal Header */}
        <div className="flex items-center gap-3 mb-6">
          <div className="p-3 bg-primary/10 text-primary rounded-2xl">
            <Shield size={24} />
          </div>
          <div>
            <h3 className="text-xl font-bold text-slate-900 dark:text-white">
              {isCurrentlyEnabled ? 'Quản lý xác thực 2 bước (2FA)' : 'Thiết lập xác thực 2 bước (2FA)'}
            </h3>
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Google Authenticator / Microsoft Authenticator
            </p>
          </div>
        </div>

        {errorMessage && (
          <div className="mb-4 p-3 bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 text-xs rounded-xl flex items-center gap-2">
            <AlertTriangle size={16} className="shrink-0" />
            <span>{errorMessage}</span>
          </div>
        )}

        {/* STEP: Initial Choice (Enable vs Disable) */}
        {step === 'init' && (
          <div className="space-y-4">
            {isCurrentlyEnabled ? (
              <>
                <div className="p-4 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-800 rounded-2xl">
                  <p className="text-sm text-emerald-800 dark:text-emerald-300 font-medium">
                    ✓ Tài khoản của bạn hiện đang được bảo vệ bởi xác thực 2 bước.
                  </p>
                  <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">
                    Mỗi khi đăng nhập, hệ thống sẽ yêu cầu mã 6 số từ ứng dụng xác thực trên điện thoại.
                  </p>
                </div>
                <Button
                  variant="outline"
                  fullWidth
                  className="border-red-300 text-red-600 hover:bg-red-50 dark:hover:bg-red-950/50"
                  onClick={() => setStep('disable')}
                >
                  Tắt xác thực 2 bước
                </Button>
              </>
            ) : (
              <>
                <p className="text-sm text-slate-600 dark:text-slate-300 leading-relaxed">
                  Xác thực 2 bước bổ sung thêm một lớp bảo mật quan trọng. Bạn sẽ sử dụng ứng dụng <strong>Google Authenticator</strong> hoặc <strong>Microsoft Authenticator</strong> để quét mã QR và tạo mã đăng nhập ngẫu nhiên.
                </p>
                <Button
                  fullWidth
                  isLoading={loading}
                  onClick={startSetup}
                  rightIcon={<ArrowRight size={18} />}
                >
                  Bắt đầu quét mã QR
                </Button>
              </>
            )}
          </div>
        )}

        {/* STEP: Setup & Scan QR */}
        {step === 'setup' && setupData && (
          <form onSubmit={handleEnable2Fa} className="space-y-4">
            <div className="flex flex-col items-center justify-center p-4 bg-slate-50 dark:bg-slate-800/50 rounded-2xl border border-slate-200 dark:border-slate-700">
              <div className="p-3 bg-white rounded-xl shadow-sm border border-slate-100 mb-3">
                <QRCodeSVG value={setupData.qrCodeUri} size={160} level="M" />
              </div>
              <p className="text-xs text-slate-500 text-center mb-2">
                Quét mã này bằng ứng dụng Google Authenticator trên điện thoại
              </p>
              
              <div className="w-full flex items-center justify-between gap-2 p-2 bg-white dark:bg-slate-900 rounded-lg border border-slate-200 dark:border-slate-700 text-xs font-mono">
                <span className="truncate select-all">{setupData.secretKey}</span>
                <button
                  type="button"
                  onClick={handleCopySecret}
                  className="p-1.5 hover:bg-slate-100 dark:hover:bg-slate-800 rounded text-primary transition-colors shrink-0"
                  title="Sao chép khóa thủ công"
                >
                  {copiedKey ? <Check size={14} className="text-emerald-500" /> : <Copy size={14} />}
                </button>
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Nhập mã 6 chữ số từ ứng dụng để kích hoạt:
              </label>
              <input
                type="text"
                autoFocus
                maxLength={6}
                value={code}
                onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))}
                placeholder="000000"
                className="w-full text-center text-2xl font-mono tracking-widest py-2.5 px-4 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 focus:ring-2 focus:ring-primary focus:outline-none"
                required
              />
            </div>

            <Button
              type="submit"
              fullWidth
              isLoading={loading}
              disabled={code.length !== 6}
            >
              Xác nhận & Kích hoạt 2FA
            </Button>
          </form>
        )}

        {/* STEP: Recovery Codes */}
        {step === 'recovery' && (
          <div className="space-y-4">
            <div className="p-3 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-800 rounded-2xl text-xs text-amber-800 dark:text-amber-300">
              <strong className="block mb-1 font-bold">Lưu ý quan trọng:</strong>
              Hãy lưu lại các mã khôi phục dưới đây. Mỗi mã chỉ dùng được 1 lần để đăng nhập khi bạn bị mất quyền truy cập vào điện thoại.
            </div>

            <div className="grid grid-cols-2 gap-2 p-3 bg-slate-50 dark:bg-slate-800 rounded-xl border border-slate-200 dark:border-slate-700 font-mono text-xs text-center">
              {recoveryCodes.map((rc, idx) => (
                <div key={idx} className="p-1.5 bg-white dark:bg-slate-900 rounded border border-slate-100 dark:border-slate-800 select-all font-semibold">
                  {rc}
                </div>
              ))}
            </div>

            <div className="flex gap-2">
              <Button
                variant="outline"
                className="flex-1"
                onClick={handleCopyRecoveryCodes}
                leftIcon={copiedCodes ? <Check size={16} /> : <Copy size={16} />}
              >
                {copiedCodes ? 'Đã sao chép' : 'Sao chép tất cả'}
              </Button>
              <Button
                className="flex-1"
                onClick={handleClose}
              >
                Hoàn tất
              </Button>
            </div>
          </div>
        )}

        {/* STEP: Disable 2FA Form */}
        {step === 'disable' && (
          <form onSubmit={handleDisable2Fa} className="space-y-4">
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Nhập mật khẩu tài khoản và mã xác thực hiện tại để tắt bảo mật 2FA.
            </p>

            <Input
              label="Mật khẩu của bạn"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              leftIcon={<Key size={16} />}
            />

            <div>
              <label className="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1">
                Mã xác thực 6 số hoặc mã khôi phục:
              </label>
              <input
                type="text"
                value={code}
                onChange={(e) => setCode(e.target.value.toUpperCase())}
                placeholder="123456 hoặc XXXX-XXXX"
                className="w-full text-center text-lg font-mono py-2 px-3 rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 focus:ring-2 focus:ring-red-500 focus:outline-none"
                required
              />
            </div>

            <div className="flex gap-2 pt-2">
              <Button
                type="button"
                variant="outline"
                className="flex-1"
                onClick={() => setStep('init')}
              >
                Hủy
              </Button>
              <Button
                type="submit"
                className="flex-1 bg-red-600 hover:bg-red-700 text-white"
                isLoading={loading}
              >
                Xác nhận tắt 2FA
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
