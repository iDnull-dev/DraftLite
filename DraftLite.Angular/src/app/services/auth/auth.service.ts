import { Injectable } from '@angular/core';
import { ApiService } from '../api/api.service';
import { RegisterUserRequest, UserDto } from '../models/user.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private readonly api: ApiService) {}

  async fetchMe(): Promise<UserDto> {
    return this.api.get<UserDto>('/users');
  }

  async register(request: RegisterUserRequest): Promise<UserDto> {
    return this.api.post<UserDto>('/users/register', request);
  }

  async searchUsers(searchName: string): Promise<UserDto[]> {
    const encoded = encodeURIComponent(searchName);
    return this.api.get<UserDto[]>(`/users/${encoded}`);
  }
}

