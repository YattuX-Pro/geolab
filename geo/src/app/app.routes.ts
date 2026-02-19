import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'files',
    pathMatch: 'full'
  },
  {
    path: 'terrains',
    loadComponent: () => import('./pages/map/map.component').then(m => m.MapComponent)
  },
  {
    path: 'topography',
    loadComponent: () => import('./pages/topography/topography.component').then(m => m.TopographyComponent)
  },
  {
    path: 'upload',
    loadComponent: () => import('./pages/gpkg-upload/gpkg-upload.component').then(m => m.GpkgUploadComponent)
  },
  {
    path: 'files',
    loadComponent: () => import('./pages/gpkg-list/gpkg-list.component').then(m => m.GpkgListComponent)
  },
  {
    path: 'map/:id',
    loadComponent: () => import('./pages/gpkg-map/gpkg-map.component').then(m => m.GpkgMapComponent)
  }
];
