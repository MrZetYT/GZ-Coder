import api from './client';

export const searchApi = {
  searchByQuery: async (query, topK = 5, minSimilarity = 0.3) => {
    return api.post('/search/query', { query, topK, minSimilarity });
  },
  getStats: async () => {
    return api.get('/search/stats');
  },
};
