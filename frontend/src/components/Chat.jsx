import React, { useState, useRef, useEffect } from 'react';
import { speechApi } from '../api/speechApi';
import { useStreamingChat } from '../hooks/useStreamingChat';
import { useAudioRecorder } from '../hooks/useAudioRecorder';
import SourcesBlock from './SourcesBlock';
import './Chat.css';

const Chat = () => {
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState('');
    const [isProcessingVoice, setIsProcessingVoice] = useState(false);
    const messagesEndRef = useRef(null);

    const { loading: streamingLoading, sendQuestionStream, cancelStream } = useStreamingChat();
    const { isRecording, startRecording, stopRecording, cancelRecording } = useAudioRecorder();

    // Загрузка истории
    useEffect(() => {
        const saved = localStorage.getItem('chatMessages');
        if (saved) {
            try {
                setMessages(JSON.parse(saved));
            } catch (e) { }
        }
    }, []);

    // Сохранение истории
    useEffect(() => {
        if (messages.length) {
            localStorage.setItem('chatMessages', JSON.stringify(messages));
        }
    }, [messages]);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    const clearHistory = () => {
        if (window.confirm('Очистить всю историю чата?')) {
            setMessages([]);
            localStorage.removeItem('chatMessages');
        }
    };

    const handleVoiceRecord = async () => {
        if (isRecording) {
            try {
                const audioBlob = await stopRecording();
                setIsProcessingVoice(true);
                const result = await speechApi.recognizeSpeech(audioBlob);
                const transcript = result.transcript;
                if (transcript?.trim()) {
                    setInput(transcript);           
                         
                }
            } catch (error) {
                console.error('Voice recognition failed:', error);
                const errorMsg = {
                    id: Date.now(),
                    type: 'assistant',
                    content: `❌ Ошибка распознавания голоса: ${error.message}`,
                    isError: true,
                    timestamp: new Date(),
                };
                setMessages(prev => [...prev, errorMsg]);
            } finally {
                setIsProcessingVoice(false);
            }
        } else {
            await startRecording();
        }
    };

    const handleSend = async (textToSend = null) => {
        const question = textToSend ?? input;
        if (!question.trim()) return;
        if (streamingLoading) cancelStream();

        const userMessage = { id: Date.now(), type: 'user', content: question, timestamp: new Date() };
        setMessages(prev => [...prev, userMessage]);
        setInput('');

        const assistantMsgId = Date.now() + 1;
        setMessages(prev => [...prev, {
            id: assistantMsgId,
            type: 'assistant',
            content: '',
            streaming: true,
            sources: [],
            timestamp: new Date(),
        }]);

        await sendQuestionStream(
            question, 5,
            (token, fullAnswer) => setMessages(prev => prev.map(msg =>
                msg.id === assistantMsgId ? { ...msg, content: fullAnswer } : msg
            )),
            (finalAnswer, sources) => setMessages(prev => prev.map(msg =>
                msg.id === assistantMsgId ? { ...msg, content: finalAnswer, streaming: false, sources } : msg
            )),
            (error) => setMessages(prev => prev.map(msg =>
                msg.id === assistantMsgId ? { ...msg, content: `❌ Ошибка: ${error.message}`, streaming: false, isError: true } : msg
            ))
        );
    };

    const handleKeyPress = (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    const formatTime = (date) => new Date(date).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });

    const formatMessage = (text) => {
        if (!text) return { __html: '' };
        return { __html: text.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>').replace(/\n/g, '<br/>') };
    };

    return (
        <div className="chat-container">
            <div className="chat-header">
                <h2>🤖 AI-ассистент</h2>
                <p>Задайте вопрос о загруженном проекте</p>
                <div className="chat-controls">
                    <button onClick={clearHistory} className="clear-history-btn" title="Очистить историю">
                        🗑️ Очистить историю
                    </button>
                </div>
            </div>

            <div className="messages-container">
                {messages.length === 0 && (
                    <div className="welcome-message">
                        <div className="welcome-icon">🤖</div>
                        <h3>AI-ассистент готов к работе</h3>
                        <p>Загрузите файлы проекта и задайте вопрос</p>
                    </div>
                )}

                {messages.map((msg) => (
                    <div key={msg.id} className={`message ${msg.type}`}>
                        <div className="message-header">
                            <span>{msg.type === 'user' ? '👤 Вы' : '🤖 AI-ассистент'}</span>
                            <span>{formatTime(msg.timestamp)}</span>
                        </div>
                        <div className={`message-content ${msg.isError ? 'error' : ''}`}>
                            {msg.type === 'assistant' && !msg.isError ? (
                                <>
                                    <div dangerouslySetInnerHTML={formatMessage(msg.content)} />
                                    {msg.streaming && <span className="streaming-cursor">▊</span>}
                                    {msg.sources?.length > 0 && <SourcesBlock sources={msg.sources} />}
                                </>
                            ) : (
                                msg.content
                            )}
                        </div>
                    </div>
                ))}

                {(streamingLoading && !messages.find(m => m.streaming)) && (
                    <div className="message assistant loading">
                        <div className="message-content">
                            <div className="typing-indicator"><span></span><span></span><span></span></div>
                            <span> AI думает...</span>
                        </div>
                    </div>
                )}

                <div ref={messagesEndRef} />
            </div>

            <div className="input-area">
                <div className="input-wrapper">
                    <textarea
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        onKeyPress={handleKeyPress}
                        placeholder="Спросите о проекте..."
                        rows={3}
                        disabled={streamingLoading || isProcessingVoice}
                    />
                    <button
                        className={`voice-btn ${isRecording ? 'recording' : ''}`}
                        onMouseDown={handleVoiceRecord}
                        onMouseUp={() => isRecording && handleVoiceRecord()}
                        onMouseLeave={() => isRecording && cancelRecording()}
                        disabled={streamingLoading || isProcessingVoice}
                    >
                        {isRecording ? '🔴' : '🎤'}
                    </button>
                </div>
                <button
                    onClick={() => handleSend()}
                    disabled={streamingLoading || !input.trim() || isProcessingVoice}
                    className="send-btn"
                >
                    ✈️
                </button>
            </div>
        </div>
    );
};

export default Chat;