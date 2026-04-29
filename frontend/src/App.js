import React, { useState } from 'react';
import FileUpload from './components/FileUpload';
import Chat from './components/Chat';
import GraphViewer from './components/GraphViewer';
import './App.css';

function App() {
    const [activeTab, setActiveTab] = useState('upload'); // 'upload', 'chat', 'graph'

    return (
        <div className="App">
            <div className="app-container">
                <header className="app-header">
                    <h1>GZ-Coder</h1>
                    <p>RAG Code Assistant</p>
                </header>

                <nav className="app-nav">
                    <button
                        className={`nav-btn ${activeTab === 'upload' ? 'active' : ''}`}
                        onClick={() => setActiveTab('upload')}
                    >
                        📁 Загрузка проекта
                    </button>
                    <button
                        className={`nav-btn ${activeTab === 'chat' ? 'active' : ''}`}
                        onClick={() => setActiveTab('chat')}
                    >
                        🤖 AI Ассистент
                    </button>
                    <button
                        className={`nav-btn ${activeTab === 'graph' ? 'active' : ''}`}
                        onClick={() => setActiveTab('graph')}
                    >
                        📊 Граф проекта
                    </button>
                </nav>

                <main className="app-main">
                    {activeTab === 'upload' && <FileUpload />}
                    {activeTab === 'chat' && <Chat />}
                    {activeTab === 'graph' && <GraphViewer />}
                </main>

                <footer className="app-footer">
                    <p>GZ-Coder v2.0</p>
                </footer>
            </div>
        </div>
    );
}

export default App;