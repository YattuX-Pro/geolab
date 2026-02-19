import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpEventType } from '@angular/common/http';
import { GpkgService, GpkgUploadResponse } from '../../services/gpkg.service';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-gpkg-upload',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './gpkg-upload.component.html',
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `]
})
export class GpkgUploadComponent {
  private gpkgService = inject(GpkgService);
  private router = inject(Router);

  selectedFile: File | null = null;
  description = '';
  isDragging = false;
  isUploading = false;
  uploadProgress = 0;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }

  private handleFile(file: File): void {
    if (!file.name.toLowerCase().endsWith('.gpkg')) {
      toast.error('Format invalide', {
        description: 'Veuillez sélectionner un fichier .gpkg'
      });
      return;
    }
    this.selectedFile = file;
  }

  clearFile(): void {
    this.selectedFile = null;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  upload(): void {
    if (!this.selectedFile) return;

    this.isUploading = true;
    this.uploadProgress = 0;

    this.gpkgService.uploadFile(this.selectedFile, this.description).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.uploadProgress = Math.round((100 * event.loaded) / event.total);
        } else if (event.type === HttpEventType.Response) {
          const response = event.body as GpkgUploadResponse;
          toast.success('Import réussi!', {
            description: `${response.layerCount} couche(s) et ${response.totalFeatureCount} feature(s) importées`
          });
          this.isUploading = false;
          this.router.navigate(['/files']);
        }
      },
      error: (err) => {
        console.error('Erreur upload:', err);
        toast.error('Erreur lors de l\'import', {
          description: err.error?.error || 'Une erreur est survenue'
        });
        this.isUploading = false;
      }
    });
  }

  goToList(): void {
    this.router.navigate(['/files']);
  }
}
