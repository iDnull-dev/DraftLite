import { Injectable } from '@angular/core';
import { ApiService } from '../api/api.service';
import { ProjectDto, CreateProjectRequest, UpdateProjectRequest } from '../models/project.models';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  constructor(private readonly api: ApiService) {}

  async fetchProjects(): Promise<ProjectDto[]> {
    return this.api.get<ProjectDto[]>('/projects');
  }

  async createProject(request: CreateProjectRequest): Promise<ProjectDto> {
    return this.api.post<ProjectDto>('/projects', request);
  }
}

