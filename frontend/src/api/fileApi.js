import api from './client';

export const fileApi = {
  uploadFiles: async (files) => {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    return api.post('/files/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  getFiles: async () => {
    return api.get('/files');
  },
  deleteFile: async (id) => {
    return api.delete(`/files/${id}`);
  },
  deleteAllFiles: async () => {
    return api.delete('/files/DeleteAll');
  },
};
