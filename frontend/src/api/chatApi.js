import api from './client';

export const chatApi = {
  askQuestion: async (question, topK = 5, minSimilarity = 0.5) => {
    return api.post('/explanation/ask', { question, topK, minSimilarity });
  },
  askQuestionSimple: async (question, topK = 5, minSimilarity = 0.5) => {
    return api.get('/explanation/ask', { 
      params: { q: question, topK, minSimilarity } 
    });
  },
};
