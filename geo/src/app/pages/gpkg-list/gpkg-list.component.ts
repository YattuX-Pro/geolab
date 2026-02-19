import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { GpkgService, GpkgFile } from '../../services/gpkg.service';
import { toast } from 'ngx-sonner';

@Component({
  selector: 'app-gpkg-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './gpkg-list.component.html',
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `]
})
export class GpkgListComponent implements OnInit {
  private gpkgService = inject(GpkgService);
  private router = inject(Router);

  files: GpkgFile[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadFiles();
  }

  loadFiles(): void {
    this.isLoading = true;
    this.gpkgService.getAllFiles().subscribe({
      next: (files) => {
        this.files = files;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erreur chargement fichiers:', err);
        toast.error('Erreur lors du chargement des fichiers');
        this.isLoading = false;
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  viewOnMap(fileId: string): void {
    this.router.navigate(['/map', fileId]);
  }

  goToUpload(): void {
    this.router.navigate(['/upload']);
  }

  deleteFile(file: GpkgFile): void {
    if (!confirm(`Êtes-vous sûr de vouloir supprimer "${file.fileName}" ?`)) {
      return;
    }

    this.gpkgService.deleteFile(file.id).subscribe({
      next: () => {
        toast.success('Fichier supprimé');
        this.loadFiles();
      },
      error: (err) => {
        console.error('Erreur suppression:', err);
        toast.error('Erreur lors de la suppression');
      }
    });
  }
}
