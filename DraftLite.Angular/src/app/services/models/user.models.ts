export type ISODateString = string;

export interface UserDto {
  id: string;
  email: string;
  pseudo: string;
  theme: string;
  createdAt: ISODateString;
  isActive: boolean;
  banAt?: ISODateString | null;
  banReason?: string | null;
  roleName: string;
}

export interface RegisterUserRequest {
  email: string;
  pseudo: string;
  googleId?: string | null;
}

export interface UpdateMeRequest {
  email: string;
  pseudo: string;
}

