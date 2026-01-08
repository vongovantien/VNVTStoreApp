import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

/**
 * Generate page numbers to display with ellipsis
 */
function getPageNumbers(currentPage: number, totalPages: number): (number | string)[] {
  const pages: (number | string)[] = [];
  
  if (totalPages <= 7) {
    // Show all pages if 7 or less
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i);
    }
  } else {
    // Always show first page
    pages.push(1);
    
    if (currentPage > 3) {
      pages.push('...');
    }
    
    // Show pages around current
    const start = Math.max(2, currentPage - 1);
    const end = Math.min(totalPages - 1, currentPage + 1);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    if (currentPage < totalPages - 2) {
      pages.push('...');
    }
    
    // Always show last page
    pages.push(totalPages);
  }
  
  return pages;
}

export const Pagination = ({
  currentPage,
  totalPages,
  totalItems,
  pageSize,
  onPageChange,
}: PaginationProps): JSX.Element => {
  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalItems);
  const pages = getPageNumbers(currentPage, totalPages);

  return (
    <div className="flex items-center justify-between border-t border-gray-200 dark:border-gray-700 bg-primary px-4 py-3 sm:px-6">
      {/* Mobile pagination */}
      <div className="flex flex-1 justify-between sm:hidden">
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          className="relative inline-flex items-center rounded-md border border-gray-300 dark:border-gray-600 bg-primary px-4 py-2 text-sm font-medium text-secondary hover:bg-secondary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Trước
        </button>
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage >= totalPages}
          className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 dark:border-gray-600 bg-primary px-4 py-2 text-sm font-medium text-secondary hover:bg-secondary disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Sau
        </button>
      </div>

      {/* Desktop pagination */}
      <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
        <div>
          <p className="text-sm text-secondary">
            Hiển thị{' '}
            <span className="font-medium text-primary">{startItem}</span>
            {' '}đến{' '}
            <span className="font-medium text-primary">{endItem}</span>
            {' '}trong{' '}
            <span className="font-medium text-primary">{totalItems}</span>
            {' '}kết quả
          </p>
        </div>
        <div>
          <nav aria-label="Pagination" className="isolate inline-flex -space-x-px rounded-md shadow-sm">
            {/* Previous button */}
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage === 1}
              className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 dark:ring-gray-600 hover:bg-secondary focus:z-20 focus:outline-offset-0 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span className="sr-only">Trang trước</span>
              <ChevronLeft size={20} />
            </button>

            {/* Page numbers */}
            {pages.map((page, index) => {
              if (page === '...') {
                return (
                  <span
                    key={`ellipsis-${index}`}
                    className="relative inline-flex items-center px-4 py-2 text-sm font-semibold text-tertiary ring-1 ring-inset ring-gray-300 dark:ring-gray-600"
                  >
                    ...
                  </span>
                );
              }

              const pageNum = page as number;
              const isActive = pageNum === currentPage;

              return (
                <button
                  key={pageNum}
                  onClick={() => onPageChange(pageNum)}
                  aria-current={isActive ? 'page' : undefined}
                  className={
                    isActive
                      ? 'relative z-10 inline-flex items-center bg-indigo-600 px-4 py-2 text-sm font-semibold text-white focus:z-20 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600'
                      : 'relative inline-flex items-center px-4 py-2 text-sm font-semibold text-primary ring-1 ring-inset ring-gray-300 dark:ring-gray-600 hover:bg-secondary focus:z-20 focus:outline-offset-0'
                  }
                >
                  {pageNum}
                </button>
              );
            })}

            {/* Next button */}
            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage >= totalPages}
              className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 dark:ring-gray-600 hover:bg-secondary focus:z-20 focus:outline-offset-0 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span className="sr-only">Trang sau</span>
              <ChevronRight size={20} />
            </button>
          </nav>
        </div>
      </div>
    </div>
  );
};

export default Pagination;
