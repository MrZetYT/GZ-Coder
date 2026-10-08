import api from './client';

export const graphApi = {
    /**
     * Получить граф структуры проекта
     * @returns {Promise<{nodes: Array, edges: Array}>}
     */
    getProjectGraph: async () => {
        return api.get('/project/graph');
    },
};