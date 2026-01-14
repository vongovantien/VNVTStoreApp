import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { useTranslation } from 'react-i18next';
import { Upload, FileSpreadsheet, X, Download } from 'lucide-react';
import { Modal } from '@/components/ui/Modal'; // Adjust import based on your structure
import { Button } from '@/components/ui/Button'; // Adjust import
import { cn } from '@/utils/cn';

interface ImportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onImport: (file: File) => Promise<void>;
  title: string;
  templateUrl?: string;
}

export const ImportModal = ({
  isOpen,
  onClose,
  onImport,
  title,
  templateUrl,
}: ImportModalProps) => {
  const { t } = useTranslation();
  const [file, setFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      setFile(acceptedFiles[0]);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'application/vnd.ms-excel': ['.xls'],
    },
    maxFiles: 1,
  });

  const handleImport = async () => {
    if (!file) return;

    setIsUploading(true);
    try {
      await onImport(file);
      setFile(null);
      onClose();
    } catch (error) {
      console.error('Import failed', error);
      // Toast error should be handled by caller or global interceptor
    } finally {
      setIsUploading(false);
    }
  };

  const removeFile = (e: React.MouseEvent) => {
    e.stopPropagation();
    setFile(null);
  };

  // Custom Footer
  const footer = (
    <div className="flex justify-end gap-2">
      <Button variant="outline" onClick={onClose} disabled={isUploading}>
        {t('common.close', 'Close')}
      </Button>
      <Button 
        onClick={handleImport} 
        disabled={!file || isUploading}
        isLoading={isUploading}
        leftIcon={<Upload size={16} />}
      >
        {t('common.importData', 'Import Data')}
      </Button>
    </div>
  );

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={title}
      footer={footer}
      size="lg"
    >
      <div className="space-y-4">
        {/* Dropzone Area */}
        <div
          {...getRootProps()}
          className={cn(
            "border-2 border-dashed rounded-xl p-8 transition-colors flex flex-col items-center justify-center cursor-pointer min-h-[200px] text-center",
            isDragActive 
                ? "border-primary bg-primary/5" 
                : "border-slate-300 dark:border-slate-700 hover:border-primary hover:bg-slate-50 dark:hover:bg-slate-800/50"
          )}
        >
          <input {...getInputProps()} />
          
          {file ? (
             <div className="flex flex-col items-center gap-3">
                <div className="p-4 bg-green-50 dark:bg-green-900/20 rounded-full">
                    <FileSpreadsheet className="w-10 h-10 text-green-500" />
                </div>
                <div>
                    <p className="font-medium text-lg">{file.name}</p>
                    <p className="text-sm text-secondary">
                        {(file.size / 1024).toFixed(2)} KB
                    </p>
                </div>
                <Button 
                    variant="ghost" 
                    size="sm" 
                    onClick={removeFile}
                    className="text-red-500 hover:text-red-600 hover:bg-red-50"
                >
                    {t('common.removeFile', 'Remove File')}
                </Button>
             </div>
          ) : (
            <>
               <div className="p-4 bg-indigo-50 dark:bg-indigo-900/20 rounded-full mb-4">
                  <Upload className="w-10 h-10 text-primary" />
               </div>
               <h3 className="text-lg font-semibold mb-1">
                 {t('import.dragDropTitle', 'Drag and drop file here')}
               </h3>
               <p className="text-secondary text-sm mb-4">
                 {t('import.or', 'Or')}
               </p>
               <Button type="button" variant="outline">
                 {t('import.selectFile', 'Select File')}
               </Button>
               <p className="text-xs text-secondary mt-4">
                 {t('import.acceptedFormats', 'Accepted files: .xls, .xlsx')}
               </p>
            </>
          )}
        </div>

        {/* Note / Template */}
        <div className="bg-slate-50 dark:bg-slate-800/50 rounded-lg p-4 text-sm">
          <p className="font-semibold mb-1">{t('common.note', 'Note')}</p>
          <div className="space-y-1 text-secondary">
             <p>
                {t('import.noteDescription', 'To ensure accurate data import, please use the template file.')}
             </p>
             {templateUrl && (
                 <a 
                    href={templateUrl} 
                    download 
                    className="inline-flex items-center gap-1 text-primary hover:underline font-medium"
                    onClick={(e) => e.stopPropagation()} // Prevent modal close if inside generic container (not here but good practice)
                 >
                    <Download size={14} />
                    {t('import.downloadTemplate', 'Download Template')}
                 </a>
             )}
          </div>
          <p className="text-xs text-secondary italic mt-2">
             {t('import.rowMappingNote', 'Each row in the import file corresponds to 1 record.')}
          </p>
        </div>
      </div>
    </Modal>
  );
};
