export interface ProjectDto {
    id: string;
    title: string;
    description: string;
    updatedAt: Date;
    collaborators?: number;
    tag?: string;
  }
  