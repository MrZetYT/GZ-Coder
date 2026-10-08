import React, { useState } from 'react';
import { fileApi } from '../api/fileApi';
import './FileUpload.css';
import FileList from './FileList';

const FileUpload = () => {
    const [selectedFiles, setSelectedFiles] = useState([]);
    const [uploading, setUploading] = useState(false);
    const [message, setMessage] = useState('');
    const [messageType, setMessageType] = useState('');

    const handleFileChange = (e) => {
        const files = Array.from(e.target.files);
        setSelectedFiles(files);
        setMessage('');
    };

    const handleUpload = async () => {
        if (selectedFiles.length === 0) {
            setMessage('Пожалуйста, выберите файлы');
            setMessageType('error');
            return;
        }

        setUploading(true);
        setMessage('');

        try {
            const result = await fileApi.uploadFiles(selectedFiles);
            setMessage(`✅ Успешно загружено ${result.length || selectedFiles.length} файлов`);
            setMessageType('success');
            setSelectedFiles([]);
            const fileInput = document.getElementById('file-input');
            if (fileInput) fileInput.value = '';
        } catch (error) {
            console.error('Upload error:', error);
            setMessage(`❌ Ошибка загрузки: ${error.message}`);
            setMessageType('error');
        } finally {
            setUploading(false);
        }
    };

    const handleDragOver = (e) => {
        e.preventDefault();
        e.stopPropagation();
    };

    const handleDrop = (e) => {
        e.preventDefault();
        e.stopPropagation();
        const files = Array.from(e.dataTransfer.files);
        setSelectedFiles(files);
        setMessage('');
    };

    return (
        <div className="file-upload">
            <div className="upload-header">
                <div className="upload-icon">🚀</div>
                <h2>Загрузка проекта</h2>
                <p>Загрузите файлы с кодом для анализа AI-ассистентом</p>
            </div>

            <div
                className="drop-zone"
                onDragOver={handleDragOver}
                onDrop={handleDrop}
            >
                <div className="drop-zone-icon">📂</div>
                <p className="drop-text">Перетащите файлы сюда</p>
                <p className="drop-or">или</p>
                <input
                    id="file-input"
                    type="file"
                    multiple
                    onChange={handleFileChange}
                    accept=".txt,.md,.cs,.py,.js,.ts,.java,.cpp,.c,.rs,.php,.html,.css,.pdf,.docx,.rtf"
                />
                <label htmlFor="file-input" className="file-label">
                    Выбрать файлы
                </label>
            </div>

            {selectedFiles.length > 0 && (
                <div className="selected-files">
                    <div className="selected-header">
                        <h3>📋 Выбрано файлов: {selectedFiles.length}</h3>
                        <button className="clear-btn" onClick={() => setSelectedFiles([])}>
                            Очистить
                        </button>
                    </div>
                    <ul>
                        {selectedFiles.map((file, idx) => (
                            <li key={idx}>
                                <span className="file-icon">📄</span>
                                <span className="file-name">{file.name}</span>
                                <span className="file-size">({(file.size / 1024).toFixed(1)} KB)</span>
                            </li>
                        ))}
                    </ul>
                    <button
                        onClick={handleUpload}
                        disabled={uploading}
                        className="upload-btn"
                    >
                        {uploading ? (
                            <>
                                <span className="spinner"></span>
                                Загрузка...
                            </>
                        ) : (
                            <>
                                🚀 Загрузить {selectedFiles.length} файл(ов)
                            </>
                        )}
                    </button>
                </div>
            )}

            {message && (
                <div className={`upload-message ${messageType}`}>
                    {message}
                </div>
            )}

            <div className="supported-formats">
                <h4>📋 Поддерживаемые форматы</h4>
                <div className="format-grid">
                    <div className="format-category">
                        <span className="format-badge">💻 Код</span>
                        .cs, .py, .js, .ts, .java, .cpp, .c, .rs, .php
                    </div>
                    <div className="format-category">
                        <span className="format-badge">🌐 Веб</span>
                        .html, .css
                    </div>
                    <div className="format-category">
                        <span className="format-badge">📄 Документы</span>
                        .pdf, .docx, .md, .txt, .rtf
                    </div>
                </div>
                <p className="note">⚠️ Максимальный размер файла: 50 MB</p>
            </div>

            <FileList />
        </div>
    );
};

export default FileUpload;