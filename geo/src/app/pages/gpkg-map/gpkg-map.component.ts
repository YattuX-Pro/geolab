import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import * as L from 'leaflet';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { GpkgService, GpkgFileDetail, GpkgFeatureCollection, GpkgLayer } from '../../services/gpkg.service';
import { toast } from 'ngx-sonner';

const iconRetinaUrl = 'assets/marker-icon-2x.png';
const iconUrl = 'assets/marker-icon.png';
const shadowUrl = 'assets/marker-shadow.png';
const iconDefault = L.icon({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  tooltipAnchor: [16, -28],
  shadowSize: [41, 41]
});
L.Marker.prototype.options.icon = iconDefault;

interface LayerState extends GpkgLayer {
  visible: boolean;
  color: string;
}

@Component({
  selector: 'app-gpkg-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './gpkg-map.component.html',
  styles: [`
    :host {
      display: block;
      height: 100%;
      overflow: hidden;
    }
  `]
})
export class GpkgMapComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private gpkgService = inject(GpkgService);

  private map!: L.Map;
  private geoJsonLayers: Map<string, L.GeoJSON> = new Map();
  private destroy$ = new Subject<void>();
  private currentFileId: string = '';

  fileDetail: GpkgFileDetail | null = null;
  features: GpkgFeatureCollection | null = null;
  layerStates: LayerState[] = [];
  isLoading = true;
  selectedFeature: any = null;
  loadedCount = 0;
  totalCount = 0;

  private colors = [
    '#3b82f6', '#ef4444', '#22c55e', '#f59e0b', '#8b5cf6',
    '#ec4899', '#06b6d4', '#84cc16', '#f97316', '#6366f1'
  ];

  get totalFeatures(): number {
    return this.fileDetail?.layers.reduce((sum, l) => sum + l.featureCount, 0) || 0;
  }

  ngOnInit(): void {
    const fileId = this.route.snapshot.paramMap.get('id');
    if (!fileId) {
      toast.error('ID de fichier manquant');
      this.router.navigate(['/files']);
      return;
    }

    setTimeout(() => {
      this.initMap();
      this.loadData(fileId);
    }, 0);
  }

  ngOnDestroy(): void {
    // Annuler toutes les subscriptions en cours
    this.destroy$.next();
    this.destroy$.complete();
    
    if (this.map) {
      this.map.remove();
    }
  }

  private initMap(): void {
    this.map = L.map('gpkg-map', {
      center: [0, 0],
      zoom: 2,
      minZoom: 2,
      maxZoom: 20
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19
    }).addTo(this.map);

    setTimeout(() => {
      this.map.invalidateSize();
    }, 100);
  }

  private loadData(fileId: string): void {
    this.isLoading = true;

    this.gpkgService.getFileById(fileId).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (detail) => {
        this.fileDetail = detail;
        this.currentFileId = fileId;
        this.layerStates = detail.layers.map((layer, index) => ({
          ...layer,
          visible: true,
          color: this.colors[index % this.colors.length]
        }));

        this.loadInitialFeatures(fileId);
      },
      error: (err) => {
        console.error('Erreur chargement détails:', err);
        toast.error('Erreur lors du chargement du fichier');
        this.isLoading = false;
      }
    });
  }

  private loadInitialFeatures(fileId: string): void {
    const maxFeatures = 60000; // Limiter à 500 features sur la map
    
    this.gpkgService.getFileFeaturesWithPagination(fileId, 1, maxFeatures).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        this.features = {
          type: 'FeatureCollection',
          features: response.features
        };
        this.loadedCount = response.features.length;
        this.totalCount = response.metadata.totalCount;
        
        console.log(`Chargé ${this.loadedCount}/${this.totalCount} features (limité à ${maxFeatures})`);
        
        this.displayFeatures();
        this.zoomToFeatures();
        this.isLoading = false;
        
        if (this.totalCount > maxFeatures) {
          toast.info(`Affichage de ${this.loadedCount} features sur ${this.totalCount} disponibles`);
        } else {
          toast.success(`${this.loadedCount} features chargées`);
        }
      },
      error: (err) => {
        console.error('Erreur chargement features:', err);
        toast.error('Erreur lors du chargement des features');
        this.isLoading = false;
      }
    });
  }

  private zoomToFeatures(): void {
    if (!this.features || this.features.features.length === 0) return;
    
    const geoJsonLayer = L.geoJSON(this.features as any);
    const bounds = geoJsonLayer.getBounds();
    
    if (bounds.isValid()) {
      this.map.fitBounds(bounds, { padding: [50, 50], maxZoom: 15 });
    }
  }

  private displayFeatures(): void {
    if (!this.features) return;

    this.geoJsonLayers.forEach(layer => this.map.removeLayer(layer));
    this.geoJsonLayers.clear();

    const allBounds: L.LatLngBounds[] = [];

    for (const layerState of this.layerStates) {
      const layerFeatures = this.features.features.filter(
        f => f.properties.layerId === layerState.id
      );

      if (layerFeatures.length === 0) continue;

      const geoJsonData = {
        type: 'FeatureCollection' as const,
        features: layerFeatures
      };

      const geoJsonLayer = L.geoJSON(geoJsonData as any, {
        style: () => ({
          color: layerState.color,
          weight: 2,
          fillOpacity: 0.3,
          fillColor: layerState.color
        }),
        pointToLayer: (feature, latlng) => {
          return L.circleMarker(latlng, {
            radius: 8,
            fillColor: layerState.color,
            color: '#fff',
            weight: 2,
            opacity: 1,
            fillOpacity: 0.8
          });
        },
        onEachFeature: (feature, layer) => {
          layer.on('click', () => {
            this.selectedFeature = feature;
          });
        }
      });

      if (layerState.visible) {
        geoJsonLayer.addTo(this.map);
      }

      this.geoJsonLayers.set(layerState.id, geoJsonLayer);

      try {
        const bounds = geoJsonLayer.getBounds();
        if (bounds.isValid()) {
          allBounds.push(bounds);
        }
      } catch (e) {
        // Ignore invalid bounds
      }
    }

    if (allBounds.length > 0) {
      let combinedBounds = allBounds[0];
      for (let i = 1; i < allBounds.length; i++) {
        combinedBounds = combinedBounds.extend(allBounds[i]);
      }
      this.map.fitBounds(combinedBounds, { padding: [50, 50] });
    }
  }

  toggleLayer(layer: LayerState): void {
    layer.visible = !layer.visible;
    const geoJsonLayer = this.geoJsonLayers.get(layer.id);
    
    if (geoJsonLayer) {
      if (layer.visible) {
        geoJsonLayer.addTo(this.map);
      } else {
        this.map.removeLayer(geoJsonLayer);
      }
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  }

  getFeatureProperties(): { key: string; value: string }[] {
    if (!this.selectedFeature?.properties) return [];
    
    return Object.entries(this.selectedFeature.properties)
      .filter(([key]) => key !== 'layerId')
      .map(([key, value]) => ({
        key,
        value: value !== null && value !== undefined ? String(value) : '-'
      }));
  }

  closeFeaturePanel(): void {
    this.selectedFeature = null;
  }

  goBack(): void {
    this.router.navigate(['/files']);
  }
}
