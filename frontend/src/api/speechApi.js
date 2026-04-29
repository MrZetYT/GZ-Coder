import api from './client';

export const speechApi = {
    recognizeSpeech: async (wavBlob) => {
        const formData = new FormData();
        formData.append('audio', wavBlob, 'recording.wav');

        // Прямой URL на бэкенд (порт 80, не 5275!)
        const apiUrl = 'http://localhost/api/speech/recognize';
        console.log(`Отправка запроса на: ${apiUrl}`);

        try {
            const response = await fetch(apiUrl, {
                method: 'POST',
                body: formData,
            });

            if (!response.ok) {
                let errorMessage = `HTTP ${response.status}`;
                try {
                    const errorData = await response.json();
                    errorMessage = errorData.message || errorData.transcript || errorMessage;
                } catch (e) {
                    const errorText = await response.text();
                    errorMessage = `${errorMessage}: ${errorText.substring(0, 100)}`;
                }
                throw new Error(errorMessage);
            }

            const data = await response.json();
            console.log('Распознанный текст:', data.transcript);
            return data;
        } catch (error) {
            console.error('Ошибка в speechApi.recognizeSpeech:', error);
            throw error;
        }
    },
};