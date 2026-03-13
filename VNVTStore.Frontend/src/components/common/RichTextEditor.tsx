import React, { useMemo } from 'react';
import ReactQuill from 'react-quill-new';
import 'react-quill-new/dist/quill.snow.css';

interface RichTextEditorProps {
    value: string;
    onChange: (value: string) => void;
    placeholder?: string;
    className?: string;
    minHeight?: string;
}

const RichTextEditor: React.FC<RichTextEditorProps> = ({
    value,
    onChange,
    placeholder = 'Nhập nội dung...',
    className = '',
    minHeight = '300px',
}) => {
    const modules = useMemo(() => ({
        toolbar: {
            container: [
                [{ header: [1, 2, 3, 4, 5, 6, false] }],
                [{ font: [] }],
                [{ size: ['small', false, 'large', 'huge'] }],
                ['bold', 'italic', 'underline', 'strike'],
                [{ color: [] }, { background: [] }],
                [{ script: 'sub' }, { script: 'super' }],
                [{ list: 'ordered' }, { list: 'bullet' }],
                [{ indent: '-1' }, { indent: '+1' }],
                [{ direction: 'rtl' }],
                [{ align: [] }],
                ['blockquote', 'code-block'],
                ['link', 'image', 'video'],
                ['clean'],
            ],
        },
        clipboard: {
            matchVisual: false,
        },
    }), []);

    const formats = [
        'header', 'font', 'size',
        'bold', 'italic', 'underline', 'strike',
        'color', 'background',
        'script',
        'list', 'indent', 'direction', 'align',
        'blockquote', 'code-block',
        'link', 'image', 'video',
    ];

    return (
        <div className={`rich-text-editor ${className}`}>
            <style>{`
                .rich-text-editor .ql-container {
                    min-height: ${minHeight};
                    font-size: 14px;
                    border-bottom-left-radius: 8px;
                    border-bottom-right-radius: 8px;
                }
                .rich-text-editor .ql-toolbar {
                    border-top-left-radius: 8px;
                    border-top-right-radius: 8px;
                    background: var(--bg-secondary, #f8fafc);
                }
                .rich-text-editor .ql-editor {
                    min-height: ${minHeight};
                }
                .rich-text-editor .ql-editor.ql-blank::before {
                    font-style: normal;
                    color: #94a3b8;
                }
                /* Dark mode support */
                .dark .rich-text-editor .ql-toolbar {
                    border-color: rgba(255,255,255,0.1);
                    background: rgba(255,255,255,0.05);
                }
                .dark .rich-text-editor .ql-container {
                    border-color: rgba(255,255,255,0.1);
                    color: #e2e8f0;
                }
                .dark .rich-text-editor .ql-editor.ql-blank::before {
                    color: rgba(255,255,255,0.3);
                }
                .dark .rich-text-editor .ql-toolbar .ql-stroke {
                    stroke: #94a3b8;
                }
                .dark .rich-text-editor .ql-toolbar .ql-fill {
                    fill: #94a3b8;
                }
                .dark .rich-text-editor .ql-toolbar .ql-picker-label {
                    color: #94a3b8;
                }
                .dark .rich-text-editor .ql-toolbar button:hover .ql-stroke,
                .dark .rich-text-editor .ql-toolbar .ql-picker-label:hover {
                    stroke: #e2e8f0;
                    color: #e2e8f0;
                }
            `}</style>
            <ReactQuill
                theme="snow"
                value={value}
                onChange={onChange}
                modules={modules}
                formats={formats}
                placeholder={placeholder}
            />
        </div>
    );
};

export default RichTextEditor;
