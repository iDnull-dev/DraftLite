import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  inject,
  signal,
  computed,
  effect,
  input,
  InputFunction,
  WritableSignal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, Observable, of } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { SkeletonModule } from 'primeng/skeleton';
import { ChipModule } from 'primeng/chip';
import { DynamicDialogModule, DialogService } from 'primeng/dynamicdialog';

import type { UserDto } from '../../services/models/user.models';
import{AuthStore} from '../../stores/auth.store';
import { SideNavStore } from '../../stores/sideNav.store';

import { ProjectService } from '../../services/project/project.service';
import { ProjectDto, CreateProjectRequest, UpdateProjectRequest } from '../../services/models/project.models';
import { DialogCreateProject } from '../project/DialogCreateProject/DialogCreateProject';

@Component({
  selector: 'app-project-nav',
  imports: [CommonModule,
    ButtonModule,
    RippleModule,
    AvatarModule,
    BadgeModule,
    TooltipModule,
    DividerModule,
    SkeletonModule,
    ChipModule,
    DynamicDialogModule,],
  providers: [DialogService],
  template: `  <aside
  class="dl-sidebar"
  [class.dl-sidebar--visible]="isSidebarVisible()"
  aria-label="Projects panel"
  role="complementary"
>
  <!-- Sidebar Header -->
  <div class="dl-sidebar__header">
    <span class="dl-sidebar__title">Projects</span>
    <button
      pButton
      pRipple
      icon="pi pi-times"
      class="p-button-text p-button-rounded p-button-sm dl-sidebar__close"
      (click)="toggleSidebar()"
      aria-label="Close sidebar"
    ></button>
  </div>

  <p-divider styleClass="dl-sidebar__divider"></p-divider>

  <!-- Own Projects ────────────────────────────────────────────────── -->
  <section class="dl-sidebar__section" aria-labelledby="own-projects-label">
    <header class="dl-sidebar__section-header">
      <i class="pi pi-folder dl-sidebar__section-icon"></i>
      <h3 id="own-projects-label" class="dl-sidebar__section-title">My projects</h3>
      <p-badge
        [value]="ownProjects().length.toString()"
        styleClass="dl-sidebar__badge"
      ></p-badge>
    </header>

    <ul class="dl-sidebar__list" role="list">
      @for (project of ownProjects(); track project.id) {
        <li class="dl-sidebar__item" role="listitem">
          <i class="pi pi-file dl-sidebar__item-icon"></i>
          <div class="dl-sidebar__item-content">
            <span class="dl-sidebar__item-title">{{ project.title }}</span>
            <span class="dl-sidebar__item-meta">{{ project.updatedAt | date:'MMM d' }}</span>
          </div>
        </li>
      } @empty {
        <li class="dl-sidebar__empty" role="listitem">
          <i class="pi pi-inbox"></i>
          <span>No projects yet</span>
        </li>
      }
    </ul>

    <button
      pButton
      pRipple
      label="New project"
      (click)="createProject()"
      icon="pi pi-plus"
      class="p-button-text p-button-sm dl-sidebar__add-btn"
    ></button>
  </section>

  <p-divider styleClass="dl-sidebar__divider"></p-divider>

  <!-- Shared Projects ─────────────────────────────────────────────── -->
  <section class="dl-sidebar__section" aria-labelledby="shared-projects-label">
    <header class="dl-sidebar__section-header">
      <i class="pi pi-users dl-sidebar__section-icon"></i>
      <h3 id="shared-projects-label" class="dl-sidebar__section-title">Shared with me</h3>
      <p-badge
        [value]="sharedProjects().length.toString()"
        styleClass="dl-sidebar__badge"
      ></p-badge>
    </header>

    <ul class="dl-sidebar__list" role="list">
      @for (project of sharedProjects(); track project.id) {
        <li class="dl-sidebar__item" role="listitem">
          <i class="pi pi-share-alt dl-sidebar__item-icon"></i>
          <div class="dl-sidebar__item-content">
            <span class="dl-sidebar__item-title">{{ project.title }}</span>
            <span class="dl-sidebar__item-meta">{{ project.ownerPseudo }} collaborators</span>
          </div>
        </li>
      } @empty {
        <li class="dl-sidebar__empty" role="listitem">
          <i class="pi pi-inbox"></i>
          <span>Nothing shared yet</span>
        </li>
      }
    </ul>
  </section>

  <!-- Sidebar Footer ───────────────────────────────────────────────── -->
  <div class="dl-sidebar__footer">
    <button
      pButton
      pRipple
      label="Invite collaborator"
      icon="pi pi-user-plus"
      class="p-button-outlined p-button-sm dl-sidebar__invite-btn"
    ></button>
  </div>

</aside>`,
  styleUrl: './projectNav.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})

export class ProjectNavComponent {
  protected readonly authStore = inject(AuthStore);
  protected readonly projectService = inject(ProjectService);
  protected readonly sideNav = inject(SideNavStore);
  private readonly dialogService = inject(DialogService);
  protected readonly isSidebarVisible = computed(
    () => this.sideNav.isSidebarVisible(),
  );
  protected toggleSidebar = this.sideNav.toggleSidebar;

  // ─── Derived state (computed) ─────────────────────────────────────────────
 
  /** True when the user has successfully authenticated. */
  protected readonly isAuthenticated = computed(
    () => this.authStore.status() === 'authenticated',
  );
 
  /** Convenience shortcut to the current UserDto. */
  protected readonly currentUser = computed(() => this.authStore.user());
 
  /** Display initials for the avatar — falls back to 'U'. */
  protected readonly avatarInitials = computed(() => {
    const pseudo = this.authStore.user()?.pseudo ?? '';
    return (
      pseudo
        .split(' ')
        .slice(0, 2)
        .map((w) => w[0]?.toUpperCase() ?? '')
        .join('') || 'U'
    );
  });

 // ─── Reactive side-effects ────────────────────────────────────────────────
 
  /**
   * Reacts to auth status changes:
   *  - Opens sidebar and loads project lists on login.
   *  - Clears everything on logout / anonymous state.
   */
  private readonly updateProjectsEffect = effect(() => {
    if (this.isSidebarVisible() && this.isAuthenticated() ) {   
      this.getOwnProjects().then(projects => this.ownProjects.set(projects));
    }
  });
  
  // ─── Local UI state ───────────────────────────────────────────────────────
 
  protected readonly ownProjects      = signal<ProjectDto[]>([]);
  protected readonly sharedProjects   = signal<ProjectDto[]>([]);
 
  // ─── Private Data Methods ─────────────────────────────────────────────────
 
  /** Returns projects owned by the current user. Replace with HTTP call when ready. */
  private async getOwnProjects(): Promise<ProjectDto[]> {
    return this.projectService.fetchProjects();
  }
 
  /** Returns projects shared with the current user. Replace with HTTP call when ready. */
  private getSharedProjects(): Observable<ProjectDto[]> {
    return of([]);
  }

  // ─── Event Handlers ───────────────────────────────────────────────────────

  // Show the dialog to create a new project
  protected createProject() { 
    const dialogRef = this.dialogService.open(DialogCreateProject, {
      header: 'Créer un projet',
      modal: true,
      closable: true,
      dismissableMask: true,
      styleClass: 'dl-auth-dialog',
      width: '28rem',
      breakpoints: {
        '640px': 'calc(100vw - 2rem)',
      },
    });

    if ( dialogRef != null) 
      dialogRef.onClose.subscribe((project?: ProjectDto | null) => {
        if (!project) return;
        this.ownProjects.update((projects) => [project, ...projects]);
      });
  }
}
