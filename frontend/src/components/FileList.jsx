import React, { useState, useEffect } from 'react';
import { fileApi } from '../api/fileApi';
import './FileList.css';

const FileList = () => {
    const [files, setFiles] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    const loadFiles = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await fileApi.getFiles();
            setFiles(data);
        } catch (err) {
            console.error('Failed to load files:', err);
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    const handleDeleteFile = async (id, fileName) => {
        if (!window.confirm(`Удалить файл "${fileName}"?`)) return;
        
        try {
            await fileApi.deleteFile(id);
            await loadFiles();
        } catch (err) {
            console.error('Failed to delete file:', err);
            alert(`Ошибка удаления: ${err.message}`);
        }
    };

    const handleDeleteAll = async () => {
        if (!window.confirm('Удалить ВСЕ файлы?')) return;
        
        try {
            await fileApi.deleteAllFiles();
            await loadFiles();
        } catch (err) {
            console.error('Failed to delete all files:', err);
            alert(`Ошибка удаления: ${err.message}`);
        }
    };

    useEffect(() => {
        loadFiles();
    }, []);

    const getStatusText = (status) => {
        switch(status) {
            case 0: return { text: 'Загружен', class: 'status-uploaded' };
            case 1: return { text: 'Обработка', class: 'status-processing' };
            case 2: return { text: 'Ошибка', class: 'status-error' };
            case 3: return { text: 'Готов', class: 'status-ready' };
            default: return { text: 'Неизвестно', class: '' };
        }
    };

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        return date.toLocaleString('ru-RU');
    };

    if (loading && files.length === 0) {
        return <div className="file-list-loading">Загрузка файлов...</div>;
    }

    return (
        <div className="file-list-container">
            <div className="file-list-header">
                <h3>📁 Загруженные файлы</h3>
                {files.length > 0 && (
                    <button onClick={handleDeleteAll} className="delete-all-btn">
                        🗑️ Удалить все
                    </button>
                )}
            </div>

            {error && (
                <div className="file-list-error">
                    ❌ Ошибка: {error}
                    <button onClick={loadFiles}>Повторить</button>
                </div>
            )}

            {files.length === 0 && !loading && (
                <div className="file-list-empty">
                    <p>📂 Нет загруженных файлов</p>
                    <p className="hint">Загрузите файлы на вкладке "Загрузка проекта"</p>
                </div>
            )}

            {files.length > 0 && (
                <div className="file-list-table-wrapper">
                    <table className="file-list-table">
                        <thead>
                            <tr>
                                <th>Имя файла</th>
                                <th>Тип</th>
                                <th>Статус</th>
                                <th>Дата загрузки</th>
                                <th>Действия</th>
                            </tr>
                        </thead>
                        <tbody>
                            {files.map(file => {
                                const status = getStatusText(file.status);
                                return (
                                    <tr key={file.id}>
                                        <td className="file-name-cell">
                                            <span className="file-icon">📄</span>
                                            {file.fileName}
                                        </td>
                                        <td>{file.fileType || '—'}</td>
                                        <td>
                                            <span className={`status-badge ${status.class}`}>
                                                {status.text}
                                            </span>
                                        </td>
                                        <td>{formatDate(file.uploadedAt)}</td>
                                        <td>
                                            <button
                                                onClick={() => handleDeleteFile(file.id, file.fileName)}
                                                className="delete-file-btn"
                                                title="Удалить"
                                            >
                                                🗑️
                                            </button>
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default FileList;