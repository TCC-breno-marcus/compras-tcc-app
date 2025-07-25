import axios from 'axios';

export const apiClient = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = 'eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIyIiwidW5pcXVlX25hbWUiOiJBZG1pbiBkbyBTaXN0ZW1hIiwiZW1haWwiOiJhZG1pbkBzaXN0ZW1hLmNvbSIsInJvbGUiOiJBZG1pbiIsIm5iZiI6MTc1MzQ2MzQ3NiwiZXhwIjoxNzUzNDkyMjc2LCJpYXQiOjE3NTM0NjM0NzYsImlzcyI6ImJhY2tlbmQiLCJhdWQiOiJmcm9udGVuZCJ9.qLZxoqpf3zSYasx068B4bejxvQFAnNby02Hk5Og5aJtpFwsoFzpVL8LKRxgxxvkZ6433CiywdTqIRLLkl8uX0w';

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config; 
  },
  (error) => {
    return Promise.reject(error);
  }
);