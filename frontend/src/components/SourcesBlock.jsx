import React from 'react';
import './SourcesBlock.css';

const SourcesBlock = ({ sources }) => {
    if (!sources || sources.length === 0) return null;

    return (
        <div className="sources-block">
            <div className="sources-header">
                📚 Источники ({sources.length})
            </div>
            <div className="sources-list">
                {sources.map((source, idx) => (
                    <div key={idx} className="source-item">
                        <div className="source-meta">
                            <span className="source-file">📄 {source.fileName || source.file}</span>
                            {source.similarity && (
                                <span className="source-similarity">
                                    ✨ {(source.similarity * 100).toFixed(1)}%
                                </span>
                            )}
                            {source.type && (
                                <span className={`source-type source-type-${source.type}`}>
                                    {source.type === 'class' ? '🏛️ Класс' :
                                        source.type === 'method' ? '🔧 Метод' : '📄 Файл'}
                                </span>
                            )}
                        </div>
                        {source.content && (
                            <pre className="source-content">
                                {source.content.length > 300
                                    ? source.content.slice(0, 300) + '...'
                                    : source.content}
                            </pre>
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
};

export default SourcesBlock;