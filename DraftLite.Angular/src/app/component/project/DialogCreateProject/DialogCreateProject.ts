import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProjectService } from '../../../services/project/project.service';
import { DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-dialog-create-project',
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule],
  template: `
    <form class="create-project-dialog" [formGroup]="titleForm" (ngSubmit)="submitTitle()">
      <h1>DraftLite</h1>
      <p class="create-project-dialog__subtitle">Créer un nouveau projet.</p>

      <label class="create-project-dialog__field">
        <span>Titre du projet</span>
        <input
          pInputText
          formControlName="title"
          type="text"
          autocomplete="off"
          placeholder="Mon nouveau projet"
          autofocus
        />
      </label>

      @if (titleError()) {
        <p class="create-project-dialog__error">{{ titleError() }}</p>
      }

      <div class="create-project-dialog__actions">
        <button
          pButton
          type="button"
          severity="secondary"
          label="Annuler"
          [disabled]="loading()"
          (click)="cancel()"
        ></button>
        <button
          pButton
          type="submit"
          label="Créer"
          icon="pi pi-plus"
          [loading]="loading()"
          [disabled]="titleForm.invalid || loading()"
        ></button>
      </div>
    </form>
  `,
  styleUrl: './DialogCreateProject.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DialogCreateProject {
  readonly titleForm = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly titleError = signal<string | null>(null);
  protected readonly loading = signal(false);
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly projectService = inject(ProjectService);

  async submitTitle() {
    if (this.titleForm.invalid) return;

    this.loading.set(true);
    this.titleError.set(null);

    try {
      const title = this.titleForm.controls.title.value.trim();

      if (!title) {
        this.titleError.set('Le titre du projet est obligatoire.');
        return;
      }

      const project = await this.projectService.createProject({ title });
      this.dialogRef.close(project);
    } catch (error) {
      this.titleError.set('Une erreur est survenue lors de la création du projet.');
      console.error(error);
    } finally {
      this.loading.set(false);
    }
  }

  protected cancel() {
    this.dialogRef.close(null);
  }
}
