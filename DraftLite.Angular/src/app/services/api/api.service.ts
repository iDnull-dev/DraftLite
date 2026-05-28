import { Injectable } from '@angular/core';
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import { environment } from '../../../environments/environment';

const AUTH_TOKEN_KEY = 'draftlite_token';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: environment.apiUrl,
      timeout: 15000,
    });

    // Attach Google ID token as Bearer for authenticated endpoints.
    this.client.interceptors.request.use((config) => {
      const token = sessionStorage.getItem(AUTH_TOKEN_KEY);
      if (token) {
        const anyConfig = config as any;
        anyConfig.headers = anyConfig.headers ?? {};
        anyConfig.headers['Authorization'] = `Bearer ${token}`;
      }
      return config;
    });
  }

  get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.client.get<T>(url, config).then((r) => r.data);
  }

  post<T>(url: string, body: unknown, config?: AxiosRequestConfig): Promise<T> {
    return this.client.post<T>(url, body, config).then((r) => r.data);
  }

  delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.client.delete<T>(url, config).then((r) => r.data);
  }
}

