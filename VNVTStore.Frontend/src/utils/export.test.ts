import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { exportToExcel } from './export';
import ExcelJS from 'exceljs';

// Define mocks using vi.hoisted
const mocks = vi.hoisted(() => ({
    addWorksheet: vi.fn(),
    addRow: vi.fn(),
    writeBuffer: vi.fn(),
    getRow: vi.fn(),
}));

vi.mock('exceljs', () => {
    return {
        default: {
            Workbook: vi.fn(function () {
                return {
                    addWorksheet: mocks.addWorksheet,
                    xlsx: { writeBuffer: mocks.writeBuffer }
                };
            })
        }
    };
});

describe('exportToExcel', () => {
    // DOM Mocks
    const mockCreateElement = vi.fn();
    const mockAppendChild = vi.fn();
    const mockRemoveChild = vi.fn();
    const mockClick = vi.fn();
    const mockCreateObjectURL = vi.fn();
    const mockRevokeObjectURL = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();

        // Setup Mock Returns
        mocks.addWorksheet.mockReturnValue({
            columns: [],
            addRow: mocks.addRow,
            getRow: mocks.getRow.mockReturnValue({
                font: {},
                fill: {},
                commit: vi.fn()
            }),
            eachCell: vi.fn() // Add eachCell mock as it's used in auto-fit
        });
        mocks.writeBuffer.mockResolvedValue(new ArrayBuffer(8));

        // Global DOM setup
        global.URL.createObjectURL = mockCreateObjectURL;
        global.URL.revokeObjectURL = mockRevokeObjectURL;

        document.createElement = mockCreateElement;
        document.body.appendChild = mockAppendChild;
        document.body.removeChild = mockRemoveChild;

        const mockLink = {
            setAttribute: vi.fn(),
            style: {},
            click: mockClick,
        };
        mockCreateElement.mockReturnValue(mockLink as any);
        mockCreateObjectURL.mockReturnValue('blob:url');
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    it('should not export if data is empty', async () => {
        const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(() => { });
        await exportToExcel([], 'test');
        expect(consoleSpy).toHaveBeenCalledWith('No data to export');
        expect(ExcelJS.Workbook).not.toHaveBeenCalled();
    });

    it('should create workbook and worksheet with correct data', async () => {
        const data = [{ id: 1, name: 'Test' }];
        await exportToExcel(data, 'test_file');

        expect(ExcelJS.Workbook).toHaveBeenCalled();
        expect(mocks.addWorksheet).toHaveBeenCalledWith('Data');

        // Validate rows added
        expect(mocks.addRow).toHaveBeenCalledWith({ id: 1, name: 'Test' });

        // Validate download flow
        expect(mocks.writeBuffer).toHaveBeenCalled();
        expect(mockCreateElement).toHaveBeenCalledWith('a');
        expect(mockClick).toHaveBeenCalled();
    });
});
