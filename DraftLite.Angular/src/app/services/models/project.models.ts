export type ISODateString = string;

export interface ProjectDto {
  id: string;
  title: string;
  ownerId: string;
  ownerPseudo: string;
  createdAt: ISODateString;
  updatedAt: ISODateString;
  deletedAt?: ISODateString | null;
}

export interface CreateProjectRequest {
  title: string;
}

export interface UpdateProjectRequest {
  title: string;
}
