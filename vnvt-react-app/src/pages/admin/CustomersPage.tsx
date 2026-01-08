import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Search, Filter, User, Mail, Phone, ShoppingBag, Eye } from 'lucide-react';
import { Button, Badge, Modal } from '@/components/ui';
import { mockUsers, mockOrders } from '@/data/mockData';
import { formatCurrency, formatDate } from '@/utils/format';

export const CustomersPage = () => {
  const { t } = useTranslation();
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCustomer, setSelectedCustomer] = useState<typeof mockUsers[0] | null>(null);

  // Mock customers with additional data
  const customers = [
    { ...mockUsers[1], orders: 5, totalSpent: 25000000 },
    { id: 'user-3', email: 'nguyenvana@email.com', name: 'Nguyễn Văn A', phone: '0901234567', role: 'customer' as const, createdAt: '2024-01-15', orders: 12, totalSpent: 85000000 },
    { id: 'user-4', email: 'tranthib@email.com', name: 'Trần Thị B', phone: '0912345678', role: 'customer' as const, createdAt: '2024-01-20', orders: 3, totalSpent: 15000000 },
  ].filter((c) =>
    c.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    c.email.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold">{t('admin.customers')}</h1>
        <p className="text-secondary">Tổng: {customers.length} khách hàng</p>
      </div>

      {/* Search */}
      <div className="bg-primary rounded-xl p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1 relative">
            <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary" />
            <input
              type="text"
              placeholder="Tìm theo tên, email..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border rounded-lg focus:outline-none focus:border-primary"
            />
          </div>
        </div>
      </div>

      {/* Table */}
      <div className="bg-primary rounded-xl overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[700px]">
            <thead className="bg-secondary">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-semibold">Khách hàng</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">Liên hệ</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">Đơn hàng</th>
                <th className="px-4 py-3 text-right text-sm font-semibold">Tổng chi tiêu</th>
                <th className="px-4 py-3 text-left text-sm font-semibold">Ngày tham gia</th>
                <th className="px-4 py-3 text-center text-sm font-semibold">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {customers.map((customer) => (
                <tr key={customer.id} className="border-b last:border-0 hover:bg-secondary/50 transition-colors">
                  <td className="px-4 py-4">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-full bg-gradient-to-r from-primary to-purple-500 flex items-center justify-center text-white font-bold">
                        {customer.name.charAt(0)}
                      </div>
                      <p className="font-medium">{customer.name}</p>
                    </div>
                  </td>
                  <td className="px-4 py-4">
                    <div className="space-y-1">
                      <p className="text-sm flex items-center gap-2">
                        <Mail size={14} className="text-tertiary" />
                        {customer.email}
                      </p>
                      {customer.phone && (
                        <p className="text-sm flex items-center gap-2 text-secondary">
                          <Phone size={14} className="text-tertiary" />
                          {customer.phone}
                        </p>
                      )}
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <span className="inline-flex items-center gap-1">
                      <ShoppingBag size={14} className="text-tertiary" />
                      {customer.orders}
                    </span>
                  </td>
                  <td className="px-4 py-4 text-right font-semibold text-success">
                    {formatCurrency(customer.totalSpent)}
                  </td>
                  <td className="px-4 py-4 text-secondary">{formatDate(customer.createdAt)}</td>
                  <td className="px-4 py-4">
                    <div className="flex items-center justify-center">
                      <button
                        className="p-2 hover:bg-secondary rounded-lg transition-colors"
                        onClick={() => setSelectedCustomer(customer)}
                        title="Xem chi tiết"
                      >
                        <Eye size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Customer Detail Modal */}
      <Modal
        isOpen={!!selectedCustomer}
        onClose={() => setSelectedCustomer(null)}
        title="Chi tiết khách hàng"
        size="md"
      >
        {selectedCustomer && (
          <div className="space-y-6">
            <div className="flex items-center gap-4">
              <div className="w-16 h-16 rounded-full bg-gradient-to-r from-primary to-purple-500 flex items-center justify-center text-white font-bold text-2xl">
                {selectedCustomer.name.charAt(0)}
              </div>
              <div>
                <h2 className="text-xl font-bold">{selectedCustomer.name}</h2>
                <p className="text-secondary">{selectedCustomer.email}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-secondary rounded-lg p-4 text-center">
                <p className="text-2xl font-bold">{selectedCustomer.orders}</p>
                <p className="text-sm text-secondary">Đơn hàng</p>
              </div>
              <div className="bg-secondary rounded-lg p-4 text-center">
                <p className="text-xl font-bold text-success">{formatCurrency(selectedCustomer.totalSpent)}</p>
                <p className="text-sm text-secondary">Tổng chi tiêu</p>
              </div>
            </div>

            <div className="space-y-3">
              <h3 className="font-semibold">Thông tin liên hệ</h3>
              <div className="space-y-2 text-secondary">
                <p className="flex items-center gap-2">
                  <Mail size={16} />
                  {selectedCustomer.email}
                </p>
                {selectedCustomer.phone && (
                  <p className="flex items-center gap-2">
                    <Phone size={16} />
                    {selectedCustomer.phone}
                  </p>
                )}
              </div>
            </div>

            <div className="flex gap-3">
              <Button fullWidth variant="outline">
                Xem đơn hàng
              </Button>
              <Button fullWidth>
                Gửi email
              </Button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

export default CustomersPage;
