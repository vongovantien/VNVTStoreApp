using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq.Expressions;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Services
{
    public class ProductService : BaseService<Product, ProductDto>, IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
        {
            var result = await base.GetAllAsync(x => x.Category);
            return result;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetPagedProductsAsync(PagingParameters pagingParameters)
        {
            var pagedResult = await base.GetPagedAsync(pagingParameters, null, x => x.Category, x => x.ProductImages);
            return pagedResult;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductFilters(ProductFilter pagingParameters)
        {
            Expression<Func<Product, bool>> filter = x => !pagingParameters.CategoryId.HasValue || x.CategoryId == pagingParameters.CategoryId;

            var pagedResult = await base.GetPagedAsync(pagingParameters, filter, x => x.Category, x => x.ProductImages);
            return pagedResult;
        }

        public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
        {
            var pagedResult = await base.GetByIdAsync(id, x => x.ProductImages, x => x.Category, x => x.ProductImages);
            return pagedResult;
        }

        public async Task ImportProductsFromExcelAsync(IFormFile file)
        {
            // Bước 1: Phân tích file Excel để lấy danh sách sản phẩm
            var products = await ParseExcelFile(file);

            // Bước 2: Lặp qua từng sản phẩm và thực hiện thêm/cập nhật
            foreach (var product in products)
            {
                ValidateProduct(product);

                // Kiểm tra sản phẩm đã tồn tại trong CSDL chưa
                var existingProduct = await _unitOfWork.ProductRepository.FindAsync(x => x.Id == product.Id);

                if (existingProduct != null)
                {
                    // Cập nhật sản phẩm hiện có
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.Updateddate = DateTime.UtcNow;

                    _unitOfWork.ProductRepository.Update(existingProduct);
                }
                else
                {
                    // Thêm sản phẩm mới
                    product.Createddate = DateTime.UtcNow;
                    product.Updateddate = DateTime.UtcNow;
                    await _unitOfWork.ProductRepository.AddAsync(product);
                }
            }

            // Bước 3: Lưu các thay đổi vào cơ sở dữ liệu
            await _unitOfWork.CommitAsync();
        }

        private async Task<List<Product>> ParseExcelFile(IFormFile file)
        {
            var products = new List<Product>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Bỏ qua hàng tiêu đề
                    {
                        var product = new Product
                        {
                            Name = worksheet.Cells[row, 1].Text, // Cột 1: Tên sản phẩm
                            CategoryId = int.Parse(worksheet.Cells[row, 2].Text), // Cột 2: ID danh mục
                            Description = worksheet.Cells[row, 3].Text, // Cột 3: Mô tả
                            Price = decimal.Parse(worksheet.Cells[row, 4].Text), // Cột 4: Giá
                            StockQuantity = int.Parse(worksheet.Cells[row, 5].Text) // Cột 5: Số lượng tồn kho
                        };

                        products.Add(product);
                    }
                }
            }

            return products;
        }

        private void ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product Name cannot be empty.");
            }

            if (product.Price <= 0)
            {
                throw new ArgumentException("Product Price must be greater than zero.");
            }

            if (product.StockQuantity < 0)
            {
                throw new ArgumentException("Stock Quantity cannot be negative.");
            }
        }

        public async Task<MemoryStream> GenerateSalesReportAsync(ReportRequest reportRequest)
        {
            var products = await GetSalesDataAsync(reportRequest); // Implement data retrieval based on reportRequest

            using (var package = new ExcelPackage())
            {
                // Create worksheets
                CreateReportOverviewSheet(package.Workbook.Worksheets.Add("Report Overview"));
                CreateProductSalesSheet(package.Workbook.Worksheets.Add("Product Sales Data"), products);
                CreateMonthlySalesSheet(package.Workbook.Worksheets.Add("Monthly Sales Data"), products);
                CreateQuarterlySalesSheet(package.Workbook.Worksheets.Add("Quarterly Sales Data"), products);
                CreateYearlySalesSheet(package.Workbook.Worksheets.Add("Yearly Sales Data"), products);
                CreateCategorySalesSheet(package.Workbook.Worksheets.Add("Category Sales Data"), products);
                CreateCustomerSalesSheet(package.Workbook.Worksheets.Add("Customer Sales Data"), products);
                CreateConclusionSheet(package.Workbook.Worksheets.Add("Conclusion and Recommendations"));

                // Save to MemoryStream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }

        private async Task<List<ProductSalesData>> GetSalesDataAsync(ReportRequest reportRequest)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync();

            return products.Select(p => _mapper.Map<ProductSalesData>(p)).ToList();
        }

        private void CreateReportOverviewSheet(ExcelWorksheet sheet)
        {
            sheet.Cells[1, 1].Value = "Report Title";
            sheet.Cells[1, 2].Value = "Product Sales Report";
            sheet.Cells[2, 1].Value = "Report Period";
            sheet.Cells[2, 2].Value = DateTime.Now.ToString("MMMM yyyy");
            sheet.Cells[3, 1].Value = "Generated Date";
            sheet.Cells[3, 2].Value = DateTime.Now.ToString("MM/dd/yyyy");
        }

        private void CreateProductSalesSheet(ExcelWorksheet sheet, List<ProductSalesData> salesData)
        {
            sheet.Cells[1, 1].Value = "Product ID";
            sheet.Cells[1, 2].Value = "Product Name";
            sheet.Cells[1, 3].Value = "Category Name";
            sheet.Cells[1, 4].Value = "Unit Price";
            sheet.Cells[1, 5].Value = "Quantity Sold";
            sheet.Cells[1, 6].Value = "Total Sales";
            sheet.Cells[1, 7].Value = "Discount";
            sheet.Cells[1, 8].Value = "Net Sales";
            sheet.Cells[1, 9].Value = "Cost of Goods Sold";
            sheet.Cells[1, 10].Value = "Profit Margin";

            int row = 2;
            foreach (var data in salesData)
            {
                sheet.Cells[row, 1].Value = data.ProductId;
                sheet.Cells[row, 2].Value = data.ProductName;
                sheet.Cells[row, 3].Value = data.CategoryName;
                sheet.Cells[row, 4].Value = data.UnitPrice;
                sheet.Cells[row, 5].Value = data.QuantitySold;
                sheet.Cells[row, 6].Value = data.TotalSales;
                sheet.Cells[row, 7].Value = data.Discount;
                sheet.Cells[row, 8].Value = data.NetSales;
                sheet.Cells[row, 9].Value = data.CostOfGoodsSold;
                sheet.Cells[row, 10].Value = data.ProfitMargin;
                row++;
            }
        }

        private void CreateMonthlySalesSheet(ExcelWorksheet sheet, List<ProductSalesData> salesData)
        {
            // Create monthly sales data based on salesData
            sheet.Cells[1, 1].Value = "Month";
            sheet.Cells[1, 2].Value = "Total Quantity Sold";
            sheet.Cells[1, 3].Value = "Total Revenue";
            sheet.Cells[1, 4].Value = "Total Discounts";
            sheet.Cells[1, 5].Value = "Net Revenue";
            sheet.Cells[1, 6].Value = "Total Profit";

            // Dummy data example, replace with actual aggregation logic
            sheet.Cells[2, 1].Value = "January";
            sheet.Cells[2, 2].Value = salesData.Sum(d => d.QuantitySold);
            sheet.Cells[2, 3].Value = salesData.Sum(d => d.TotalSales);
            sheet.Cells[2, 4].Value = salesData.Sum(d => d.Discount);
            sheet.Cells[2, 5].Value = salesData.Sum(d => d.NetSales);
            sheet.Cells[2, 6].Value = salesData.Sum(d => d.ProfitMargin);
        }

        private void CreateQuarterlySalesSheet(ExcelWorksheet sheet, List<ProductSalesData> salesData)
        {
            // Create quarterly sales data based on salesData
            sheet.Cells[1, 1].Value = "Quarter";
            sheet.Cells[1, 2].Value = "Total Quantity Sold";
            sheet.Cells[1, 3].Value = "Total Revenue";
            sheet.Cells[1, 4].Value = "Total Discounts";
            sheet.Cells[1, 5].Value = "Net Revenue";
            sheet.Cells[1, 6].Value = "Total Profit";

            // Dummy data example, replace with actual aggregation logic
            sheet.Cells[2, 1].Value = "Q1";
            sheet.Cells[2, 2].Value = salesData.Sum(d => d.QuantitySold);
            sheet.Cells[2, 3].Value = salesData.Sum(d => d.TotalSales);
            sheet.Cells[2, 4].Value = salesData.Sum(d => d.Discount);
            sheet.Cells[2, 5].Value = salesData.Sum(d => d.NetSales);
            sheet.Cells[2, 6].Value = salesData.Sum(d => d.ProfitMargin);
        }

        private void CreateYearlySalesSheet(ExcelWorksheet sheet, List<ProductSalesData> salesData)
        {
            // Create yearly sales data based on salesData
            sheet.Cells[1, 1].Value = "Year";
            sheet.Cells[1, 2].Value = "Total Quantity Sold";
            sheet.Cells[1, 3].Value = "Total Revenue";
            sheet.Cells[1, 4].Value = "Total Discounts";
            sheet.Cells[1, 5].Value = "Net Revenue";
            sheet.Cells[1, 6].Value = "Total Profit";

            // Dummy data example, replace with actual aggregation logic
            sheet.Cells[2, 1].Value = DateTime.Now.Year;
            sheet.Cells[2, 2].Value = salesData.Sum(d => d.QuantitySold);
            sheet.Cells[2, 3].Value = salesData.Sum(d => d.TotalSales);
            sheet.Cells[2, 4].Value = salesData.Sum(d => d.Discount);
            sheet.Cells[2, 5].Value = salesData.Sum(d => d.NetSales);
            sheet.Cells[2, 6].Value = salesData.Sum(d => d.ProfitMargin);
        }

        private void CreateCategorySalesSheet(ExcelWorksheet sheet, List<ProductSalesData> salesData)
        {
            // Create category sales data based on salesData
            sheet.Cells[1, 1].Value = "Category Name";
            sheet.Cells[1, 2].Value = "Total Quantity Sold";
            sheet.Cells[1, 3].Value = "Total Revenue";
            sheet.Cells[1, 4].Value = "Total Discounts";
            sheet.Cells[1, 5].Value = "Net Revenue";
            sheet.Cells[1, 6].Value = "Total Profit";

            // Dummy data example, replace with actual aggregation logic
            var categories = salesData.GroupBy(d => d.CategoryName);
            int row = 2;
            foreach (var category in categories)
            {
                sheet.Cells[row, 1].Value = category.Key;
                sheet.Cells[row, 2].Value = category.Sum(d => d.QuantitySold);
                sheet.Cells[row, 3].Value = category.Sum(d => d.TotalSales);
                sheet.Cells[row, 4].Value = category.Sum(d => d.Discount);
                sheet.Cells[row, 5].Value = category.Sum(d => d.NetSales);
                sheet.Cells[row, 6].Value = category.Sum(d => d.ProfitMargin);
                row++;
            }
        }

        private void CreateCustomerSalesSheet(ExcelWorksheet sheet, List<ProductSalesData> salesData)
        {
            // Create customer sales data based on salesData
            sheet.Cells[1, 1].Value = "Customer ID";
            sheet.Cells[1, 2].Value = "Customer Name";
            sheet.Cells[1, 3].Value = "Total Quantity Sold";
            sheet.Cells[1, 4].Value = "Total Revenue";
            sheet.Cells[1, 5].Value = "Total Discounts";
            sheet.Cells[1, 6].Value = "Net Revenue";
            sheet.Cells[1, 7].Value = "Total Profit";

            // Dummy data example, replace with actual aggregation logic
            var customers = salesData.GroupBy(d => d.ProductId); // Assuming ProductId is used for customers
            int row = 2;
            foreach (var customer in customers)
            {
                sheet.Cells[row, 1].Value = customer.Key;
                sheet.Cells[row, 2].Value = "Customer Name"; // Replace with actual customer names
                sheet.Cells[row, 3].Value = customer.Sum(d => d.QuantitySold);
                sheet.Cells[row, 4].Value = customer.Sum(d => d.TotalSales);
                sheet.Cells[row, 5].Value = customer.Sum(d => d.Discount);
                sheet.Cells[row, 6].Value = customer.Sum(d => d.NetSales);
                sheet.Cells[row, 7].Value = customer.Sum(d => d.ProfitMargin);
                row++;
            }
        }

        private void CreateConclusionSheet(ExcelWorksheet sheet)
        {
            sheet.Cells[1, 1].Value = "Conclusion and Recommendations";
            sheet.Cells[2, 1].Value = "Focus on high-performing categories and explore opportunities to boost sales in underperforming areas.";
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetFrequentlyBoughtTogetherAsync(int productId)
        {
            try
            {
                // Get all order items that include the given product
                var orderItems = await _unitOfWork.OrderItemRepository.GetAllAsync(
                    oi => oi.ProductId == productId,
                    oi => oi.Order);

                var orderIds = orderItems.Select(oi => oi.OrderId).Distinct().ToList();

                if (!orderIds.Any())
                {
                    return new ApiResponse<IEnumerable<ProductDto>>(true, "No frequently bought products found.", Enumerable.Empty<ProductDto>(), 200);
                }

                // Fetch products that are frequently bought together
                var products = await _unitOfWork.OrderItemRepository.GetAllAsync(
                    oi => orderIds.Contains(oi.OrderId) && oi.ProductId != productId,
                    oi => oi.Product);

                var distinctProducts = products
                    .Select(oi => oi.Product)
                    .Distinct()
                    .ToList();

                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(distinctProducts);

                return new ApiResponse<IEnumerable<ProductDto>>(true, "Products fetched successfully.", productDtos, 200);
            }
            catch (Exception ex)
            {
                // Handle exception and return a response indicating failure
                return new ApiResponse<IEnumerable<ProductDto>>(false, $"Error: {ex.Message}", null, 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetTodaysRecommendationsAsync()
        {
            try
            {
                // Fetch today's recommended products
                var products = await _unitOfWork.ProductRepository.GetAllAsync();

                var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
              //  p => p.IsRecommended && p.RecommendationDate.Date == DateTime.Today.Date
                return new ApiResponse<IEnumerable<ProductDto>>(true, "Recommendations fetched successfully.", productDtos, 200);
            }
            catch (Exception ex)
            {
                // Handle exception and return a response indicating failure
                return new ApiResponse<IEnumerable<ProductDto>>(false, $"Error: {ex.Message}", null, 500);
            }
        }

    }
}