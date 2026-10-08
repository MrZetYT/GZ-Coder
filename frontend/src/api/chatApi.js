import api from './client';

export const chatApi = {
    
    askQuestion: async (question, topK = 5, minSimilarity = 0.5) => {
        return api.post('/query', {
            Question: question,       
            TopK: topK,
            MinSimilarity: minSimilarity
        });
    },
};