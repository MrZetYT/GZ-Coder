import axios from 'axios';

const API_BASE_URL = 'http://localhost/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    timeout: 30000,
});

api.interceptors.response.use(
    (response) => response.data,
    (error) => {
        console.error('API Error:', error);

        // Получаем понятное сообщение об ошибке
        let message = 'Неизвестная ошибка';

        if (error.response) {
            // Сервер ответил с ошибкой
            const data = error.response.data;
            if (typeof data === 'string') {
                message = data;
            } else if (data?.message) {
                message = data.message;
            } else if (data?.error) {
                message = data.error;
            } else if (data?.title) {
                message = data.title;
            } else {
                message = `Ошибка сервера: ${error.response.status}`;
            }
        } else if (error.request) {
            // Запрос был отправлен, но ответа нет
            message = 'Сервер не отвечает. Проверьте, запущен ли бэкенд.';
        } else {
            // Ошибка при настройке запроса
            message = error.message || 'Ошибка запроса';
        }

        throw new Error(message);
    }
);

export default api;