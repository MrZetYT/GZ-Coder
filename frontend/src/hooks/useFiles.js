import { useState, useEffect, useCallback } from 'react';
import { fileApi } from '../api/fileApi';

export const useFiles = () => {
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [uploading, setUploading] = useState(false);

  const loadFiles = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fileApi.getFiles();
      setFiles(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  const uploadFiles = useCallback(async (fileList) => {
    setUploading(true);
    setError(null);
    try {
      const uploaded = await fileApi.uploadFiles(fileList);
      await loadFiles();
      return uploaded;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setUploading(false);
    }
  }, [loadFiles]);

  const deleteFile = useCallback(async (id) => {
    try {
      await fileApi.deleteFile(id);
      await loadFiles();
    } catch (err) {
      setError(err.message);
      throw err;
    }
  }, [loadFiles]);

  const deleteAllFiles = useCallback(async () => {
    try {
      await fileApi.deleteAllFiles();
      await loadFiles();
    } catch (err) {
      setError(err.message);
      throw err;
    }
  }, [loadFiles]);

  useEffect(() => {
    loadFiles();
  }, [loadFiles]);

  return { files, loading, error, uploading, uploadFiles, deleteFile, deleteAllFiles, refresh: loadFiles };
};
