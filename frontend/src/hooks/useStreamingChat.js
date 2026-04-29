import { useState, useCallback, useRef } from 'react';

export const useStreamingChat = () => {
    const [loading, setLoading] = useState(false);
    const [streamingAnswer, setStreamingAnswer] = useState('');
    const [foundBlocks, setFoundBlocks] = useState([]);
    const abortControllerRef = useRef(null);

    const sendQuestionStream = useCallback(async (question, topK = 5, onToken, onComplete, onError) => {
        // Отменяем предыдущий запрос, если есть
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }

        const controller = new AbortController();
        abortControllerRef.current = controller;

        setLoading(true);
        setStreamingAnswer('');
        setFoundBlocks([]);

        let accumulatedAnswer = '';
        let blocksData = null;

        try {
            const response = await fetch('http://localhost/api/query/stream', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ question, topK }),
                signal: controller.signal,
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const reader = response.body.getReader();
            const decoder = new TextDecoder();

            while (true) {
                const { done, value } = await reader.read();
                if (done) break;

                const chunk = decoder.decode(value);
                const lines = chunk.split('\n');

                for (const line of lines) {
                    if (line.startsWith('data: ')) {
                        const data = line.slice(6); // убираем 'data: '

                        if (data === '[DONE]') {
                            // Стриминг завершён
                            onComplete?.(accumulatedAnswer, blocksData?.foundBlocks || []);
                            break;
                        }

                        // Пробуем распарсить JSON (если это foundBlocks)
                        try {
                            const parsed = JSON.parse(data);
                            if (parsed.foundBlocks) {
                                blocksData = parsed;
                                setFoundBlocks(parsed.foundBlocks);
                                // Не показываем блоки в ответе
                                continue;
                            }
                        } catch (e) {
                            // Это обычный текст
                            accumulatedAnswer += data;
                            setStreamingAnswer(accumulatedAnswer);
                            onToken?.(data, accumulatedAnswer);
                        }
                    }
                }
            }

        } catch (error) {
            if (error.name !== 'AbortError') {
                console.error('Streaming error:', error);
                onError?.(error);
            }
        } finally {
            setLoading(false);
            setStreamingAnswer('');
            abortControllerRef.current = null;
        }
    }, []);

    const cancelStream = useCallback(() => {
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
            abortControllerRef.current = null;
            setLoading(false);
            setStreamingAnswer('');
        }
    }, []);

    return {
        loading,
        streamingAnswer,
        foundBlocks,
        sendQuestionStream,
        cancelStream,
        setStreamingAnswer,
        setFoundBlocks,
    };
};