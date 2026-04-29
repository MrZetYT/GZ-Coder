import { useState, useRef, useCallback } from 'react';

export const useAudioRecorder = () => {
    const [isRecording, setIsRecording] = useState(false);
    const [recordingTime, setRecordingTime] = useState(0);
    const mediaRecorderRef = useRef(null);
    const audioChunksRef = useRef([]);
    const streamRef = useRef(null);
    const timerRef = useRef(null);

    // Конвертация WebM в WAV 16kHz mono
    const convertToWav = async (webmBlob) => {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = async () => {
                const arrayBuffer = reader.result;
                const audioContext = new (window.AudioContext || window.webkitAudioContext)();

                try {
                    const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
                    const targetSampleRate = 16000;

                    // Создаём offline контекст для ресемплинга до 16kHz
                    const offlineContext = new OfflineAudioContext(
                        1, // mono
                        audioBuffer.length * targetSampleRate / audioBuffer.sampleRate,
                        targetSampleRate
                    );

                    const source = offlineContext.createBufferSource();
                    source.buffer = audioBuffer;
                    source.connect(offlineContext.destination);
                    source.start();

                    const resampledBuffer = await offlineContext.startRendering();
                    const pcmData = resampledBuffer.getChannelData(0);

                    // Создаём WAV файл
                    const wavBlob = createWavBlob(pcmData, targetSampleRate);
                    resolve(wavBlob);
                } catch (err) {
                    reject(err);
                } finally {
                    audioContext.close();
                }
            };
            reader.onerror = reject;
            reader.readAsArrayBuffer(webmBlob);
        });
    };

    // Создание WAV заголовка и данных
    const createWavBlob = (samples, sampleRate) => {
        const numChannels = 1;
        const bitsPerSample = 16;
        const byteRate = sampleRate * numChannels * (bitsPerSample / 8);
        const blockAlign = numChannels * (bitsPerSample / 8);

        // Конвертируем Float32 в Int16
        const pcmData = new Int16Array(samples.length);
        for (let i = 0; i < samples.length; i++) {
            const s = Math.max(-1, Math.min(1, samples[i]));
            pcmData[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
        }

        const buffer = new ArrayBuffer(44 + pcmData.length * 2);
        const view = new DataView(buffer);

        // RIFF chunk
        writeString(view, 0, 'RIFF');
        view.setUint32(4, 36 + pcmData.length * 2, true);
        writeString(view, 8, 'WAVE');
        writeString(view, 12, 'fmt ');
        view.setUint32(16, 16, true);
        view.setUint16(20, 1, true); // PCM
        view.setUint16(22, numChannels, true);
        view.setUint32(24, sampleRate, true);
        view.setUint32(28, byteRate, true);
        view.setUint16(32, blockAlign, true);
        view.setUint16(34, bitsPerSample, true);
        writeString(view, 36, 'data');
        view.setUint32(40, pcmData.length * 2, true);

        // Запись PCM данных
        for (let i = 0; i < pcmData.length; i++) {
            view.setInt16(44 + i * 2, pcmData[i], true);
        }

        return new Blob([buffer], { type: 'audio/wav' });
    };

    const writeString = (view, offset, str) => {
        for (let i = 0; i < str.length; i++) {
            view.setUint8(offset + i, str.charCodeAt(i));
        }
    };

    const startRecording = useCallback(async () => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            streamRef.current = stream;

            // Используем WebM (браузер его точно поддерживает)
            const mimeType = MediaRecorder.isTypeSupported('audio/webm') ? 'audio/webm' : 'audio/mp4';
            const mediaRecorder = new MediaRecorder(stream, { mimeType });
            mediaRecorderRef.current = mediaRecorder;
            audioChunksRef.current = [];

            mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    audioChunksRef.current.push(event.data);
                }
            };

            mediaRecorder.start(100);
            setIsRecording(true);
            setRecordingTime(0);

            timerRef.current = setInterval(() => {
                setRecordingTime(prev => prev + 1);
            }, 1000);

        } catch (error) {
            console.error('Ошибка доступа к микрофону:', error);
            throw new Error('Не удалось получить доступ к микрофону');
        }
    }, []);

    const stopRecording = useCallback(async () => {
        return new Promise((resolve, reject) => {
            if (!mediaRecorderRef.current || mediaRecorderRef.current.state !== 'recording') {
                reject(new Error('Нет активной записи'));
                return;
            }

            if (timerRef.current) {
                clearInterval(timerRef.current);
                timerRef.current = null;
            }

            mediaRecorderRef.current.onstop = async () => {
                setIsRecording(false);

                if (streamRef.current) {
                    streamRef.current.getTracks().forEach(track => track.stop());
                    streamRef.current = null;
                }

                const webmBlob = new Blob(audioChunksRef.current, { type: 'audio/webm' });

                try {
                    // Конвертируем в WAV 16kHz
                    const wavBlob = await convertToWav(webmBlob);
                    resolve(wavBlob);
                } catch (err) {
                    console.error('Conversion error:', err);
                    reject(err);
                }
            };

            mediaRecorderRef.current.stop();
        });
    }, []);

    const cancelRecording = useCallback(() => {
        if (timerRef.current) {
            clearInterval(timerRef.current);
            timerRef.current = null;
        }

        if (mediaRecorderRef.current && mediaRecorderRef.current.state === 'recording') {
            mediaRecorderRef.current.onstop = () => {
                setIsRecording(false);
                if (streamRef.current) {
                    streamRef.current.getTracks().forEach(track => track.stop());
                    streamRef.current = null;
                }
            };
            mediaRecorderRef.current.stop();
        }
    }, []);

    return {
        isRecording,
        recordingTime,
        startRecording,
        stopRecording,
        cancelRecording,
    };
};